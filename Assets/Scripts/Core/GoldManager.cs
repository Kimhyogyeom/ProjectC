using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 골드 싱글톤 매니저
/// - 골드 수치 관리
/// - AddGold() 호출 시 UI 코인 날리기 + OnGoldChanged 이벤트
/// </summary>
public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    #region Serialized Fields
    [Header("UI")]
    [SerializeField] GameObject _goldCoinFlyPrefab;  // GoldCoinFly UI 프리팹
    [SerializeField] RectTransform _goldIconTarget;  // 골드 UI 아이콘 위치
    [SerializeField] Canvas _canvas;
    #endregion

    #region Private Fields
    const string GOLD_PREFS_KEY = "TotalGold";
    int _gold = 0;          // 인게임 세션 골드 (매판 초기화)
    static int _totalGold = 0;  // 누적 골드
    static bool _loaded = false;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        LoadGold();
    }

    void OnApplicationQuit()
    {
        if (_gold > 0) SaveSessionGold();
    }

    void OnDestroy()
    {
        if (Instance == this && _gold > 0) SaveSessionGold();
    }
    #endregion

    #region Public API
    public int Gold => _gold;
    public static int TotalGold
    {
        get
        {
            if (!_loaded) LoadGold();
            return _totalGold;
        }
    }
    public event Action<int> OnGoldChanged;

    /// <summary>로비로 나갈 때 호출 — 인게임 골드를 누적 골드에 더함</summary>
    public void SaveSessionGold()
    {
        if (_gold <= 0) return;
        _totalGold += _gold;
        _gold = 0;
        SaveGold();
    }

    /// <summary>누적 골드 직접 설정 (디버그용)</summary>
    public static void SetTotalGold(int amount)
    {
        _totalGold = amount;
        SaveGold();
    }

    /// <summary>골드 차감 (강화 등)</summary>
    public static bool SpendGold(int amount)
    {
        if (_totalGold < amount) return false;
        _totalGold -= amount;
        PlayerPrefs.SetInt(GOLD_PREFS_KEY, _totalGold);
        PlayerPrefs.Save();
        return true;
    }

    static void LoadGold()
    {
        _totalGold = PlayerPrefs.GetInt(GOLD_PREFS_KEY, 0);
        _loaded = true;
    }

    static void SaveGold()
    {
        PlayerPrefs.SetInt(GOLD_PREFS_KEY, _totalGold);
        PlayerPrefs.Save();
    }

    /// <summary>골드 추가 + 코인 날리기</summary>
    public void AddGold(int amount, Vector3 worldPos)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxCoin();

        if (_goldCoinFlyPrefab != null && _goldIconTarget != null && _canvas != null)
            StartCoroutine(SpawnFlyingCoins(amount, worldPos));
        else
        {
            _gold += amount;
            GameStats.AddGold(amount);
            OnGoldChanged?.Invoke(_gold);
        }
    }
    #endregion

    #region Coin Fly
    IEnumerator SpawnFlyingCoins(int amount, Vector3 worldPos)
    {
        int coinCount = Mathf.Clamp(amount / 5, 1, 5);
        int goldPerCoin = amount / coinCount;
        int remainder = amount - goldPerCoin * coinCount;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        for (int i = 0; i < coinCount; i++)
        {
            GameObject coinObj = Instantiate(_goldCoinFlyPrefab, _canvas.transform);
            RectTransform coinRect = coinObj.GetComponent<RectTransform>();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.GetComponent<RectTransform>(), screenPos, _canvas.worldCamera, out Vector2 localPos);
            coinRect.localPosition = localPos + UnityEngine.Random.insideUnitCircle * 30f;

            int coinGold = (i == coinCount - 1) ? goldPerCoin + remainder : goldPerCoin;
            GoldCoinFly fly = coinObj.GetComponent<GoldCoinFly>();
            fly.Init(_goldIconTarget, coinGold, OnCoinArrived);

            yield return new WaitForSeconds(0.05f);
        }
    }

    void OnCoinArrived(int amount)
    {
        _gold += amount;
        GameStats.AddGold(amount);
        OnGoldChanged?.Invoke(_gold);
    }
    #endregion
}
