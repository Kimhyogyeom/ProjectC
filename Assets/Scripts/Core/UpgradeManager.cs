using UnityEngine;

public enum StatType { Damage, Hp, MoveSpeed, AttackSpeed }

/// <summary>
/// 영구 강화 시스템 매니저
/// - PlayerPrefs로 강화 레벨 저장/로드
/// - 비용: 1000 * 2^level (무한 레벨)
/// - 스탯 증가량: 소량 고정
/// </summary>
public static class UpgradeManager
{
    const int BASE_COST = 1000;

    // 레벨당 증가량
    const int DAMAGE_PER_LEVEL = 1;         // 기본 10
    const int HP_PER_LEVEL = 5;             // 기본 100
    const float SPEED_PER_LEVEL = 0.1f;     // 기본 5
    const float ATK_SPD_PER_LEVEL = 0.01f;  // 기본 0.5s

    static string GetPrefsKey(StatType type) => $"Upgrade_{type}";

    public static int GetLevel(StatType type)
    {
        return PlayerPrefs.GetInt(GetPrefsKey(type), 0);
    }

    public static int GetUpgradeCost(StatType type)
    {
        int level = GetLevel(type);
        return BASE_COST * (1 << level); // 1000 * 2^level
    }

    public static bool TryUpgrade(StatType type)
    {
        int cost = GetUpgradeCost(type);
        if (!GoldManager.SpendGold(cost)) return false;

        int newLevel = GetLevel(type) + 1;
        PlayerPrefs.SetInt(GetPrefsKey(type), newLevel);
        PlayerPrefs.Save();
        return true;
    }

    #region Bonus Getters
    public static int GetBonusDamage() => GetLevel(StatType.Damage) * DAMAGE_PER_LEVEL;
    public static int GetBonusHp() => GetLevel(StatType.Hp) * HP_PER_LEVEL;
    public static float GetBonusMoveSpeed() => GetLevel(StatType.MoveSpeed) * SPEED_PER_LEVEL;
    public static float GetBonusAttackSpeed() => GetLevel(StatType.AttackSpeed) * ATK_SPD_PER_LEVEL;
    #endregion

    #region Stat Display (기본값 + 보너스)
    public static int GetTotalDamage() => 10 + GetBonusDamage();
    public static int GetTotalHp() => 100 + GetBonusHp();
    public static float GetTotalMoveSpeed() => 5f + GetBonusMoveSpeed();
    public static float GetTotalAttackSpeed() => Mathf.Max(0.1f, 0.5f - GetBonusAttackSpeed());
    #endregion
}
