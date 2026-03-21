using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 경험치 및 레벨 관리
/// - 경험치 구슬 획득 시 경험치 누적
/// - 일정 경험치 도달 시 레벨업
/// </summary>
public class PlayerLevel : MonoBehaviour
{
    #region Serialized Fields
    [Header("Level Settings")]
    [SerializeField] int _baseExpRequired = 10;     // 1레벨업에 필요한 기본 경험치
    [SerializeField] float _expGrowthRate = 1.3f;   // 레벨업마다 필요 경험치 증가율

    [Header("Orb Settings")]
    [SerializeField] float _orbPickupRadius = 1.5f; // 경험치 구슬 흡수 반경

    [Header("UI")]
    [SerializeField] Image _expBarFill;
    [SerializeField] TMP_Text _levelText;

    [Header("References")]
    [SerializeField] SkillSelectionUI _skillSelectionUI;
    #endregion

    #region Private Fields
    int _currentLevel = 1;
    int _currentExp = 0;
    int _expRequired;
    float _orbRadiusMultiplier = 1f;    // 자석 스킬 배율
    bool _isGlobalMagnet = false;       // 자석 Lv5: 맵 전체 흡수
    #endregion

    #region Debug (Inspector 실시간 확인용)
    [Header("Debug (ReadOnly)")]
    [SerializeField, ReadOnly] int _debugLevel = 1;
    [SerializeField, ReadOnly] int _debugCurrentExp = 0;
    [SerializeField, ReadOnly] int _debugExpRequired = 0;
    #endregion

    #region Properties
    public float OrbPickupRadius => _orbPickupRadius * _orbRadiusMultiplier;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _expRequired = _baseExpRequired;
        UpdateExpBar();
    }

    void Update()
    {
        // 주변 경험치 구슬 감지 및 흡수 (자석 Lv5: 매우 큰 반경)
        float radius = _isGlobalMagnet ? 999f : OrbPickupRadius;
        Collider[] orbs = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider orb in orbs)
        {
            ExpOrb expOrb = orb.GetComponent<ExpOrb>();
            if (expOrb != null)
                expOrb.Attract();
        }
    }
    #endregion

    #region Public API
    /// <summary>경험치 획득 및 레벨업 체크</summary>
    public void GainExp(int amount)
    {
        _currentExp += amount;

        // 레벨업 체크 (연속 레벨업 가능)
        while (_currentExp >= _expRequired)
        {
            _currentExp -= _expRequired;
            LevelUp();
        }

        UpdateExpBar();
    }
    #endregion

    #region Level Up
    void LevelUp()
    {
        _currentLevel++;
        _expRequired = Mathf.RoundToInt(_baseExpRequired * Mathf.Pow(_expGrowthRate, _currentLevel - 1));

        Debug.Log($"레벨업! 현재 레벨: {_currentLevel} / 다음 레벨까지: {_expRequired}");

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxLevelUp();

        // 레벨업 이펙트
        LevelUpEffect.Create(transform.position);

        // 스킬 선택 UI 호출
        SkillManager skillManager = GetComponent<SkillManager>();
        if (_skillSelectionUI != null && skillManager != null)
            _skillSelectionUI.Show(skillManager);
    }
    #endregion

    #region Public API
    /// <summary>자석 스킬: 흡수 범위 배율 설정</summary>
    public void SetOrbRadiusMultiplier(float multiplier, bool isGlobal)
    {
        _orbRadiusMultiplier = multiplier;
        _isGlobalMagnet = isGlobal;
    }
    #endregion

    #region UI
    void UpdateExpBar()
    {
        // Inspector 디버그 값 업데이트
        _debugLevel = _currentLevel;
        _debugCurrentExp = _currentExp;
        _debugExpRequired = _expRequired;

        if (_expBarFill != null)
            _expBarFill.fillAmount = (float)_currentExp / _expRequired;

        if (_levelText != null)
            _levelText.text = $"Lv.{_currentLevel}";
    }
    #endregion
}
