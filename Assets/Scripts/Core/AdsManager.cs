using UnityEngine;
using Unity.Services.LevelPlay;
using System;

/// <summary>
/// LevelPlay(ironSource) 광고 매니저
/// - 싱글톤, DontDestroyOnLoad
/// - 리워드 광고 초기화 및 표시
/// </summary>
public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    #region Serialized Fields
    [Header("LevelPlay Settings")]
    [SerializeField] string _appKey = "25a9f2bad";
    [SerializeField] string _rewardedAdUnitId = "rvvyulrirp1o4rd4";
    #endregion

    #region Private Fields
    LevelPlayRewardedAd _rewardedAd;
    Action _onRewardGranted;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        LevelPlay.OnInitSuccess += OnSdkInitialized;
        LevelPlay.OnInitFailed += reason => Debug.LogWarning($"[Ads] 초기화 실패: {reason}");
        LevelPlay.Init(_appKey);
    }
    #endregion

    #region Init
    void OnSdkInitialized(LevelPlayConfiguration config)
    {
        LoadRewardedAd();
    }

    void LoadRewardedAd()
    {
        _rewardedAd = new LevelPlayRewardedAd(_rewardedAdUnitId);
        _rewardedAd.OnAdRewarded += (ad, reward) => HandleReward();
        _rewardedAd.OnAdClosed += ad => _rewardedAd.LoadAd();  // 닫히면 다음 광고 미리 로드
        _rewardedAd.LoadAd();
    }
    #endregion

    #region Public API
    /// <summary>리워드 광고 표시. 보상 지급 시 onRewardGranted 콜백 호출</summary>
    public void ShowRewardedAd(Action onRewardGranted)
    {
#if UNITY_EDITOR
        // 에디터에서는 광고 없이 바로 보상 지급
        onRewardGranted?.Invoke();
        return;
#endif
        if (_rewardedAd == null || !_rewardedAd.IsAdReady())
        {
            return;
        }

        _onRewardGranted = onRewardGranted;
        _rewardedAd.ShowAd();
    }

    public bool IsRewardedAdReady() => _rewardedAd != null && _rewardedAd.IsAdReady();
    #endregion

    #region Reward
    void HandleReward()
    {
        _onRewardGranted?.Invoke();
        _onRewardGranted = null;
    }
    #endregion
}
