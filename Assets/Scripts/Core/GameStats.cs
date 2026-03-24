/// <summary>
/// 인게임 세션 통계 (매 판 초기화)
/// - 킬 카운트, 도달 웨이브, 획득 골드 추적
/// - 게임 결과 화면에서 참조
/// </summary>
public static class GameStats
{
    public static int KillCount { get; private set; }
    public static int BossKillCount { get; private set; }
    public static int MaxWave { get; private set; }
    public static int GoldEarned { get; private set; }

    public static void Reset()
    {
        KillCount = 0;
        BossKillCount = 0;
        MaxWave = 0;
        GoldEarned = 0;
    }

    public static void AddKill(bool isBoss)
    {
        KillCount++;
        if (isBoss) BossKillCount++;
    }

    public static void SetWave(int wave)
    {
        if (wave > MaxWave) MaxWave = wave;
    }

    public static void AddGold(int amount)
    {
        GoldEarned += amount;
    }
}
