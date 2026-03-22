using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 선택 UI 패널
/// - 레벨업 시 활성화, 3개 카드 표시
/// - 카드 등장 애니메이션 후 선택 가능
/// - 선택 완료 시 게임 재개
/// </summary>
public class SkillSelectionUI : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] GameObject _panel;                 // 전체 패널
    [SerializeField] SkillCardUI[] _skillCards;         // 카드 3개
    [SerializeField] float _cardDelay = 0.12f;          // 카드 간 등장 딜레이
    #endregion

    #region Private Fields
    SkillManager _skillManager;
    bool _isAnimating;
    System.Action _onCompleted;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _panel.SetActive(false);
    }
    #endregion

    #region Public API
    /// <summary>스킬 선택 UI 표시 (PlayerLevel 또는 WaveManager에서 호출)</summary>
    public void Show(SkillManager skillManager, System.Action onCompleted = null)
    {
        _skillManager = skillManager;
        _onCompleted = onCompleted;

        List<SkillData> choices = skillManager.GetRandomSkillChoices(3);

        // 카드 설정 + 버튼 비활성
        int activeCount = 0;
        for (int i = 0; i < _skillCards.Length; i++)
        {
            if (i < choices.Count)
            {
                _skillCards[i].gameObject.SetActive(true);
                int currentLevel = skillManager.GetSkillLevel(choices[i].skillType);
                _skillCards[i].Setup(choices[i], currentLevel, this);
                _skillCards[i].SetInteractable(false);  // 애니메이션 중 선택 불가
                activeCount++;
            }
            else
            {
                // 선택지가 3개 미만이면 나머지 카드 숨김
                _skillCards[i].gameObject.SetActive(false);
            }
        }

        _panel.SetActive(true);
        Time.timeScale = 0f;    // 게임 일시정지

        // 등장 애니메이션 시작
        StartCoroutine(PlayCardsEntranceRoutine(activeCount));
    }

    /// <summary>카드 선택 시 호출 (SkillCardUI에서 호출)</summary>
    public void OnSkillSelected(SkillType skillType)
    {
        if (_isAnimating) return;   // 애니메이션 중 선택 방지

        _skillManager.AcquireSkill(skillType);
        _panel.SetActive(false);
        Time.timeScale = 1f;    // 게임 재개
        _onCompleted?.Invoke();
        _onCompleted = null;
    }
    #endregion

    #region Animation
    IEnumerator PlayCardsEntranceRoutine(int activeCount)
    {
        _isAnimating = true;

        // 각 카드 순차적으로 등장 애니메이션 실행
        Coroutine lastCard = null;
        for (int i = 0; i < activeCount; i++)
        {
            lastCard = _skillCards[i].PlayEntranceAnimation(i * _cardDelay);
        }

        // 마지막 카드 애니메이션 완료 대기
        if (lastCard != null)
            yield return lastCard;

        // 모든 카드 선택 가능하도록 활성화
        for (int i = 0; i < activeCount; i++)
        {
            _skillCards[i].SetInteractable(true);
        }

        _isAnimating = false;
    }
    #endregion
}
