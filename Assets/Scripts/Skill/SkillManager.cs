using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 플레이어 스킬 보유 및 적용 관리
/// - 스킬 획득/레벨업 처리
/// - 각 스킬 효과를 관련 컴포넌트에 적용
/// </summary>
public class SkillManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("Skill Data")]
    [SerializeField] SkillData[] _allSkills;    // 모든 스킬 데이터 (Inspector에서 연결)
    #endregion

    #region Private Fields
    Dictionary<SkillType, int> _acquiredSkills = new Dictionary<SkillType, int>();

    // 컴포넌트 캐싱
    PlayerAttack _playerAttack;
    PlayerController _playerController;
    PlayerHealth _playerHealth;
    PlayerLevel _playerLevel;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _playerAttack = GetComponent<PlayerAttack>();
        _playerController = GetComponent<PlayerController>();
        _playerHealth = GetComponent<PlayerHealth>();
        _playerLevel = GetComponent<PlayerLevel>();
    }

    #if UNITY_EDITOR
    void Update()
    {
        // 디버그: 숫자 키 1~7로 스킬 즉시 적용
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        if (kb.digit1Key.wasPressedThisFrame) AcquireSkill(SkillType.MultiShuriken);
        if (kb.digit2Key.wasPressedThisFrame) AcquireSkill(SkillType.PoisonBlade);
        if (kb.digit3Key.wasPressedThisFrame) AcquireSkill(SkillType.ShadowClone);
        if (kb.digit4Key.wasPressedThisFrame) AcquireSkill(SkillType.Gale);
        if (kb.digit5Key.wasPressedThisFrame) AcquireSkill(SkillType.SmokeBomb);
        if (kb.digit6Key.wasPressedThisFrame) AcquireSkill(SkillType.SpinningStar);
        if (kb.digit7Key.wasPressedThisFrame) AcquireSkill(SkillType.Magnet);
    }
    #endif
    #endregion

    #region Public API
    /// <summary>스킬 획득 또는 레벨업 처리</summary>
    public void AcquireSkill(SkillType skillType)
    {
        // 최대 레벨 체크
        SkillData data = GetSkillData(skillType);
        int currentLevel = GetSkillLevel(skillType);
        if (data != null && currentLevel >= data.MaxLevel) return;

        if (_acquiredSkills.ContainsKey(skillType))
            _acquiredSkills[skillType]++;
        else
            _acquiredSkills[skillType] = 1;

        int level = _acquiredSkills[skillType];
        ApplySkillEffect(skillType, level);

        Debug.Log($"스킬 획득: {skillType} Lv.{level}");
    }

    /// <summary>현재 스킬 레벨 반환 (없으면 0)</summary>
    public int GetSkillLevel(SkillType skillType)
    {
        return _acquiredSkills.ContainsKey(skillType) ? _acquiredSkills[skillType] : 0;
    }

    /// <summary>선택 가능한 랜덤 스킬 3개 반환</summary>
    public List<SkillData> GetRandomSkillChoices(int count = 3)
    {
        // 최대 레벨 미만인 스킬만 추출
        List<SkillData> available = new List<SkillData>();
        foreach (SkillData skill in _allSkills)
        {
            int currentLevel = GetSkillLevel(skill.skillType);
            if (currentLevel < skill.MaxLevel)
                available.Add(skill);
        }

        // 랜덤 셔플
        for (int i = 0; i < available.Count; i++)
        {
            int rand = Random.Range(i, available.Count);
            (available[i], available[rand]) = (available[rand], available[i]);
        }

        // count개 반환 (부족하면 있는 만큼)
        int returnCount = Mathf.Min(count, available.Count);
        return available.GetRange(0, returnCount);
    }
    #endregion

    #region Skill Effects
    void ApplySkillEffect(SkillType skillType, int level)
    {
        switch (skillType)
        {
            case SkillType.MultiShuriken:
                ApplyMultiShuriken(level);
                break;
            case SkillType.PoisonBlade:
                ApplyPoisonBlade(level);
                break;
            case SkillType.ShadowClone:
                ApplyShadowClone(level);
                break;
            case SkillType.Gale:
                ApplyGale(level);
                break;
            case SkillType.SmokeBomb:
                ApplySmokeBomb(level);
                break;
            case SkillType.SpinningStar:
                ApplySpinningStar(level);
                break;
            case SkillType.Magnet:
                ApplyMagnet(level);
                break;
        }
    }

    // 다중 표창: 발사 수 증가 (Lv5 유니크: 관통 추가)
    void ApplyMultiShuriken(int level)
    {
        if (_playerAttack == null) return;
        bool isPierce = level >= 5;
        _playerAttack.SetMultiShot(level + 1, isPierce);  // Lv1=2발, Lv2=3발 ...
    }

    // 독 묻히기: 투사체에 독 데미지 부여
    void ApplyPoisonBlade(int level)
    {
        if (_playerAttack == null) return;
        SkillData data = GetSkillData(SkillType.PoisonBlade);
        float poisonDamage = data != null ? data.GetValue(level) : level * 2f;
        float duration = level >= 5 ? 999f : 3f + level;  // Lv5: 전염 (duration 플래그로 사용)
        _playerAttack.SetPoison(poisonDamage, duration, level >= 5);
    }

    // 분신술: 분신 수 및 데미지 배율 설정
    void ApplyShadowClone(int level)
    {
        ShadowCloneSkill shadowClone = GetOrAddComponent<ShadowCloneSkill>();
        SkillData data = GetSkillData(SkillType.ShadowClone);
        int cloneCount = level <= 2 ? 1 : 2 + (level >= 5 ? 1 : 0);
        float damageRatio = data != null ? data.GetValue(level) : 0.5f + (level - 1) * 0.25f;
        shadowClone.SetClone(cloneCount, damageRatio, level >= 5);
    }

    // 질풍: 이동속도 증가 + 이동 중 공격 가능
    void ApplyGale(int level)
    {
        if (_playerController == null || _playerAttack == null) return;
        SkillData data = GetSkillData(SkillType.Gale);
        float speedBonus = data != null ? data.GetValue(level) : 1f + level * 0.1f;
        _playerController.SetSpeedMultiplier(speedBonus);
        _playerAttack.SetCanAttackWhileMoving(true, level >= 5);
    }

    // 연막탄: 피격 회피 확률
    void ApplySmokeBomb(int level)
    {
        if (_playerHealth == null) return;
        SkillData data = GetSkillData(SkillType.SmokeBomb);
        float dodgeChance = data != null ? data.GetValue(level) : level * 0.05f;
        _playerHealth.SetDodgeChance(dodgeChance, level >= 5);
    }

    // 수리검 회전: 회전 수리검 추가/강화
    void ApplySpinningStar(int level)
    {
        SpinningStarSkill spinningStar = GetOrAddComponent<SpinningStarSkill>();
        int count = Mathf.Min(level, 3);
        bool isPierce = level >= 5;
        float sizeMultiplier = isPierce ? 2f : 1f;
        spinningStar.SetStar(count, isPierce, sizeMultiplier);
    }

    // 자석: 경험치 구슬 흡수 범위 확장 (Lv5: 맵 전체)
    void ApplyMagnet(int level)
    {
        if (_playerLevel == null) return;
        SkillData data = GetSkillData(SkillType.Magnet);
        float radiusMultiplier = data != null ? data.GetValue(level) : 1f + level * 0.2f;
        bool isGlobal = level >= 5;
        _playerLevel.SetOrbRadiusMultiplier(radiusMultiplier, isGlobal);
    }
    #endregion

    #region Helpers
    SkillData GetSkillData(SkillType skillType)
    {
        foreach (SkillData skill in _allSkills)
            if (skill.skillType == skillType) return skill;
        return null;
    }

    T GetOrAddComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();
        return component;
    }
    #endregion
}
