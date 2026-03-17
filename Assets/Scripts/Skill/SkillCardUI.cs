using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 선택 카드 UI 1개
/// - 스킬 이름, 현재 레벨, 설명 표시
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

    /// <summary>카드 클릭 시 호출 (Button OnClick에 연결)</summary>
    public void OnCardClicked()
    {
        _selectionUI.OnSkillSelected(_skillData.skillType);
    }
    #endregion
}
