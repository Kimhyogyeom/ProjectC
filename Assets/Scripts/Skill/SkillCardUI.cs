using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 선택 카드 UI 1개
/// - 스킬 이름, 현재 레벨, 설명 표시
/// - 등장 시 회전 애니메이션 (도는 중 선택 불가)
/// - 클릭 시 SkillSelectionUI에 선택 전달
/// </summary>
public class SkillCardUI : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] TMP_Text _skillNameText;
    [SerializeField] TMP_Text _levelText;
    [SerializeField] TMP_Text _descriptionText;
    [SerializeField] Image _iconImage;
    [SerializeField] Image _cardAccent;                                      // 하단 컬러 바
    [SerializeField] Color _newSkillColor = new Color(0.2f, 0.6f, 1f);     // 새 스킬: 파란색
    [SerializeField] Color _upgradeColor = new Color(0.2f, 0.8f, 0.2f);    // 업그레이드: 초록색
    [SerializeField] Color _uniqueColor = new Color(1f, 0.8f, 0f);         // 유니크: 골드색
    #endregion

    #region Private Fields
    SkillData _skillData;
    int _currentLevel;
    SkillSelectionUI _selectionUI;
    Button _button;
    CanvasGroup _canvasGroup;
    RectTransform _rectTransform;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _button = GetComponent<Button>();
        _rectTransform = GetComponent<RectTransform>();

        // CanvasGroup이 없으면 추가 (카드 내용 숨김 제어용)
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    #endregion

    #region Public API
    /// <summary>카드 초기화</summary>
    public void Setup(SkillData skillData, int currentLevel, SkillSelectionUI selectionUI)
    {
        _skillData = skillData;
        _currentLevel = currentLevel;
        _selectionUI = selectionUI;

        int nextLevel = currentLevel + 1;
        bool isNew = currentLevel == 0;
        bool isUnique = nextLevel >= skillData.MaxLevel;

        // 스킬 이름
        _skillNameText.text = skillData.skillName;

        // 레벨 표시
        if (isNew)
            _levelText.text = "NEW";
        else if (isUnique)
            _levelText.text = $"Lv.{currentLevel} → <color=#FFD700>UNIQUE</color>";
        else
            _levelText.text = $"Lv.{currentLevel} → {nextLevel}";

        // 설명
        _descriptionText.text = skillData.GetDescription(nextLevel);

        // 아이콘
        if (_iconImage != null && skillData.icon != null)
            _iconImage.sprite = skillData.icon;

        // 하단 컬러 바
        if (_cardAccent != null)
        {
            if (isUnique) _cardAccent.color = _uniqueColor;
            else if (isNew) _cardAccent.color = _newSkillColor;
            else _cardAccent.color = _upgradeColor;
        }
    }

    /// <summary>등장 애니메이션 재생 (SkillSelectionUI에서 호출)</summary>
    public Coroutine PlayEntranceAnimation(float delay)
    {
        return StartCoroutine(EntranceAnimationRoutine(delay));
    }

    /// <summary>버튼 상호작용 설정</summary>
    public void SetInteractable(bool interactable)
    {
        if (_button != null)
            _button.interactable = interactable;
    }

    /// <summary>카드 클릭 시 호출 (Button OnClick에 연결)</summary>
    public void OnCardClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxSkillSelect();
        _selectionUI.OnSkillSelected(_skillData.skillType);
    }
    #endregion

    #region Animation
    IEnumerator EntranceAnimationRoutine(float delay)
    {
        const float SPIN_DURATION = 0.35f;
        const float SETTLE_DURATION = 0.15f;

        // 초기 상태: 숨김 + 축소
        _canvasGroup.alpha = 0f;
        _rectTransform.localScale = Vector3.zero;
        _rectTransform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        // 딜레이 대기 (timeScale=0이므로 Realtime 사용)
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        // Phase 1: 스핀 — 뒷면에서 앞면으로 회전하며 커짐
        float elapsed = 0f;
        while (elapsed < SPIN_DURATION)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / SPIN_DURATION);
            float easeT = EaseOutBack(t);

            // Y축 180° → 0° 회전
            float yAngle = Mathf.Lerp(180f, 0f, t);
            _rectTransform.localRotation = Quaternion.Euler(0f, yAngle, 0f);

            // 스케일: 0 → 1.15 (살짝 오버슈트)
            float scale = Mathf.Lerp(0f, 1.15f, easeT);
            _rectTransform.localScale = new Vector3(scale, scale, 1f);

            // 알파: 회전 초반에 빠르게 표시 (뒷면일 때부터 보이도록)
            _canvasGroup.alpha = Mathf.Clamp01(t * 3f);

            yield return null;
        }

        // Phase 2: 안착 — 오버슈트에서 정확한 크기로
        elapsed = 0f;
        while (elapsed < SETTLE_DURATION)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / SETTLE_DURATION);
            float easeT = EaseOutCubic(t);

            float scale = Mathf.Lerp(1.15f, 1f, easeT);
            _rectTransform.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        // 최종 상태 보정
        _rectTransform.localScale = Vector3.one;
        _rectTransform.localRotation = Quaternion.identity;
        _canvasGroup.alpha = 1f;
    }

    // EaseOutBack: 살짝 넘어갔다 돌아오는 느낌
    float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // EaseOutCubic: 부드러운 감속
    float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
    #endregion
}
