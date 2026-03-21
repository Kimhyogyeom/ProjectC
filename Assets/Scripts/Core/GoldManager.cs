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
    int _gold = 0;          // 인게임 세션 골드 (매판 초기화)
    static int _totalGold = 0;  // 누적 골드 (앱 실행 중 유지)
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }
    #endregion

    #region Public API
    public int Gold => _gold;
    public static int TotalGold => _totalGold;
    public event Action<int> OnGoldChanged;

    /// <summary>로비로 나갈 때 호출 — 인게임 골드를 누적 골드에 더함</summary>
    public void SaveSessionGold()
    {
        _totalGold += _gold;
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
        OnGoldChanged?.Invoke(_gold);
    }
    #endregion
}
