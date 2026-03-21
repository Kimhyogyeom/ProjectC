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

    [Header("UI - 하단 버튼")]
    [SerializeField] Button _shopButton;
    [SerializeField] Button _gemShopButton;
    [SerializeField] Button _rewardButton;
    [SerializeField] Button _settingsButton;
    #endregion

    #region Private Fields
    int _currentIndex = 0;
    Coroutine _slideCoroutine;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _leftButton.onClick.AddListener(OnLeftClicked);
        _rightButton.onClick.AddListener(OnRightClicked);
        _startButton.onClick.AddListener(OnStartClicked);

        if (_shopButton != null) _shopButton.onClick.AddListener(OnShopClicked);
        if (_gemShopButton != null) _gemShopButton.onClick.AddListener(OnGemShopClicked);
        if (_rewardButton != null) _rewardButton.onClick.AddListener(OnRewardClicked);
        if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    void Start()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBgmLobby();
        UpdateUI();
        UpdateGoldUI();
    }
    #endregion

    #region UI
    void UpdateGoldUI()
    {
        if (_goldText == null) return;
        _goldText.text = GoldManager.TotalGold.ToString("N0");
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

        Vector2 centerPos = _stageCard.anchoredPosition;
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
    }

    void OnStartClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        SceneTransition.Instance.LoadScene(_stages[_currentIndex].sceneName);
    }

    void OnShopClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        // TODO: 상점 UI 열기
    }

    void OnGemShopClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        // TODO: 보석 상점 UI 열기
    }

    void OnRewardClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        // TODO: 광고 보상 UI 열기
    }

    void OnSettingsClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        // TODO: 설정 UI 열기
    }
    #endregion
}
