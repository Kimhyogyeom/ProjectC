using UnityEditor;
using UnityEngine;

public static class DebugMenu
{
    [MenuItem("Debug/골드 10000 지급")]
    static void SetGold10000()
    {
        int current = GoldManager.TotalGold;
        GoldManager.SetTotalGold(current + 10000);
        Debug.Log($"[Debug] 골드 지급 완료: {current} → {current + 10000}");
    }

    [MenuItem("Debug/골드 초기화")]
    static void ResetGold()
    {
        GoldManager.SetTotalGold(0);
        Debug.Log("[Debug] 골드 초기화 완료");
    }

    [MenuItem("Debug/강화 레벨 초기화")]
    static void ResetUpgrades()
    {
        PlayerPrefs.DeleteKey("Upgrade_Damage");
        PlayerPrefs.DeleteKey("Upgrade_Hp");
        PlayerPrefs.DeleteKey("Upgrade_MoveSpeed");
        PlayerPrefs.DeleteKey("Upgrade_AttackSpeed");
        PlayerPrefs.Save();
        Debug.Log("[Debug] 강화 레벨 초기화 완료");
    }

    [MenuItem("Debug/전체 초기화 (골드 + 강화)")]
    static void ResetAll()
    {
        GoldManager.SetTotalGold(0);
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[Debug] 전체 초기화 완료 (골드 + 강화)");
    }
}
