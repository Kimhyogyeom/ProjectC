using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 로비 매니저
/// - 스테이지 선택 (좌우 화살표)
/// - 잠금 스테이지 처리
/// - 게임 씬 전환
/// </summary>
public class LobbyManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("Stage Settings")]
    [SerializeField] StageData[] _stages;

    [Header("UI - 스테이지 선택")]
    [SerializeField] TMP_Text _stageNameText;
    [SerializeField] Image _stageImage;
    [SerializeField] GameObject _lockIcon;
    [SerializeField] Button _startButton;
    [SerializeField] Button _leftButton;
    [SerializeField] Button _rightButton;
    [SerializeField] RectTransform _stageCard;
    [SerializeField] float _slideDistance = 300f;
    [SerializeField] float _slideDuration = 0.2f;

    [Header("UI - 골드")]
    [SerializeField] TMP_Text _goldText;

    [Header("UI - 보너스 카드")]
    [SerializeField] GameObject _bonusCardIndicator;  // Start 버튼 위 Card+1 표시

    [Header("UI - 하단 버튼")]
    [SerializeField] Button _gemShopButton;
    [SerializeField] Button _rewardButton;
    [SerializeField] Button _soundButton;

    [Header("UI - 로그인")]
    [SerializeField] Button _loginButton;
    [SerializeField] TMP_Text _loginButtonText;
    [SerializeField] GameObject _loginImage;
    #endregion

    #region Private Fields
    int _currentIndex = 0;
    Coroutine _slideCoroutine;
    Vector2 _cardCenterPos;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _leftButton.onClick.AddListener(OnLeftClicked);
        _rightButton.onClick.AddListener(OnRightClicked);
        _startButton.onClick.AddListener(OnStartClicked);

        if (_gemShopButton != null) _gemShopButton.onClick.AddListener(OnGemShopClicked);
        if (_rewardButton != null) _rewardButton.onClick.AddListener(OnRewardClicked);
        if (_soundButton != null) _soundButton.onClick.AddListener(OnSoundClicked);
        if (_loginButton != null) _loginButton.onClick.AddListener(OnLoginClicked);
    }

    void Start()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBgmLobby();
        if (_stageCard != null) _cardCenterPos = _stageCard.anchoredPosition;
        UpdateUI();
        UpdateGoldUI();
        UpdateBonusCardUI();

        // Firebase 데이터 로드 완료 시 골드 UI 갱신
        if (FirebaseManager.Instance != null && !FirebaseManager.Instance.IsDataLoaded)
            FirebaseManager.Instance.OnDataLoaded += UpdateGoldUI;

        if (FirebaseManager.Instance != null)
            FirebaseManager.Instance.OnGoogleLoginResult += OnGoogleLoginResult;

        UpdateLoginUI();
    }
    #endregion

    #region UI
    void Update()
    {
        UpdateGoldUI();
    }

    void UpdateGoldUI()
    {
        if (_goldText == null) return;
        _goldText.text = GoldManager.TotalGold.ToString("N0");
    }

    void UpdateBonusCardUI()
    {
        bool hasBonusCard = GameSessionData.HasBonusSkillCard;
        if (_bonusCardIndicator != null)
            _bonusCardIndicator.SetActive(hasBonusCard);
        if (_rewardButton != null)
            _rewardButton.interactable = !hasBonusCard;
    }

    void UpdateUI()
    {
        if (_stages == null || _stages.Length == 0) return;

        StageData stage = _stages[_currentIndex];

        _stageNameText.text = stage.stageName;

        if (_stageImage != null)
            _stageImage.sprite = stage.stageImage;

        if (_lockIcon != null)
            _lockIcon.SetActive(stage.isLocked);

        _startButton.interactable = !stage.isLocked;

        // 화살표 버튼 (끝에서 비활성화)
        _leftButton.interactable = _currentIndex > 0;
        _rightButton.interactable = _currentIndex < _stages.Length - 1;
    }
    #endregion

    #region Button Callbacks
    void OnLeftClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        _currentIndex = Mathf.Max(0, _currentIndex - 1);
        SlideAndUpdate(1);
    }

    void OnRightClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        _currentIndex = Mathf.Min(_stages.Length - 1, _currentIndex + 1);
        SlideAndUpdate(-1);
    }

    void SlideAndUpdate(int direction)
    {
        if (_slideCoroutine != null) StopCoroutine(_slideCoroutine);
        _slideCoroutine = StartCoroutine(SlideRoutine(direction));
    }

    System.Collections.IEnumerator SlideRoutine(int direction)
    {
        if (_stageCard == null) { UpdateUI(); yield break; }

        _leftButton.interactable = false;
        _rightButton.interactable = false;

        Vector2 centerPos = _cardCenterPos;
        Vector2 outPos = centerPos + new Vector2(-direction * _slideDistance, 0f);
        Vector2 inPos = centerPos + new Vector2(direction * _slideDistance, 0f);
        float elapsed = 0f;

        // 슬라이드 아웃
        while (elapsed < _slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _slideDuration);
            _stageCard.anchoredPosition = Vector2.Lerp(centerPos, outPos, t);
            yield return null;
        }

        // UI 업데이트 + 반대편 배치
        UpdateUI();
        _stageCard.anchoredPosition = inPos;
        elapsed = 0f;

        // 슬라이드 인
        while (elapsed < _slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _slideDuration);
            _stageCard.anchoredPosition = Vector2.Lerp(inPos, centerPos, t);
            yield return null;
        }

        _stageCard.anchoredPosition = centerPos;
        UpdateUI();
    }

    void OnStartClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        SceneTransition.Instance.LoadScene(_stages[_currentIndex].sceneName);
    }

    void OnGemShopClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
    }

    void OnSoundClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
    }

    void OnRewardClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        if (AdsManager.Instance != null)
            AdsManager.Instance.ShowRewardedAd(OnRewardGranted);
    }

    void OnRewardGranted()
    {
        GameSessionData.HasBonusSkillCard = true;
        UpdateBonusCardUI();
    }

    void OnLoginClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();

        // TODO: Google Play 승인 후 임시 로그인 제거하고 원래 코드 복원
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsGoogleLinked)
        {
#if UNITY_ANDROID
            if (GoogleSignInHelper.Instance != null)
                GoogleSignInHelper.Instance.SignOut();
#endif
            FirebaseManager.Instance.SignOut();
            UpdateLoginUI();
            UpdateGoldUI();
        }
        else
        {
            Debug.Log("[Login] 임시 로그인 처리");
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.SetTempLogin("테스트유저");
                OnGoogleLoginResult(true);
            }
        }
    }

    void OnGoogleLoginResult(bool success)
    {
        UpdateLoginUI();
        if (success) UpdateGoldUI();
    }

    void UpdateLoginUI()
    {
        if (_loginButton == null || _loginButtonText == null) return;

        bool isLoggedIn = FirebaseManager.Instance != null && FirebaseManager.Instance.IsGoogleLinked;

        if (_loginImage != null)
            _loginImage.SetActive(!isLoggedIn);

        if (isLoggedIn)
        {
            _loginButtonText.text = FirebaseManager.Instance.DisplayName ?? "로그인됨";
        }
        else
        {
            _loginButtonText.text = "Google 로그인";
        }
    }

    #endregion
}
