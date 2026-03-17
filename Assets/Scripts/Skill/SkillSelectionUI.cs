using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 선택 UI 패널
/// - 레벨업 시 활성화, 3개 카드 표시
/// - 선택 완료 시 게임 재개
/// </summary>
public class SkillSelectionUI : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] GameObject _panel;                 // 전체 패널
    [SerializeField] SkillCardUI[] _skillCards;         // 카드 3개
    #endregion

    #region Private Fields
    SkillManager _skillManager;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _panel.SetActive(false);
    }
    #endregion

    #region Public API
    /// <summary>스킬 선택 UI 표시 (PlayerLevel에서 호출)</summary>
    public void Show(SkillManager skillManager)
    {
        _skillManager = skillManager;

        List<SkillData> choices = skillManager.GetRandomSkillChoices(3);

        // 카드 설정
        for (int i = 0; i < _skillCards.Length; i++)
        {
            if (i < choices.Count)
            {
                _skillCards[i].gameObject.SetActive(true);
                int currentLevel = skillManager.GetSkillLevel(choices[i].skillType);
                _skillCards[i].Setup(choices[i], currentLevel, this);
            }
            else
            {
                // 선택지가 3개 미만이면 나머지 카드 숨김
                _skillCards[i].gameObject.SetActive(false);
            }
        }

        _panel.SetActive(true);
        Time.timeScale = 0f;    // 게임 일시정지
    }

    /// <summary>카드 선택 시 호출 (SkillCardUI에서 호출)</summary>
    public void OnSkillSelected(SkillType skillType)
    {
        _skillManager.AcquireSkill(skillType);
        _panel.SetActive(false);
        Time.timeScale = 1f;    // 게임 재개
    }
    #endregion
}
