using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

/// <summary>
/// Firebase 싱글톤 매니저
/// - 초기화, 익명 인증, Firestore 데이터 동기화
/// - PlayerPrefs를 오프라인 캐시로 유지
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public bool IsInitialized { get; private set; }
    public bool IsDataLoaded { get; private set; }
    public event Action OnDataLoaded;

    FirebaseAuth _auth;
    FirebaseFirestore _db;
    string _userId;

    #region User Data (캐시)
    int _totalGold;
    Dictionary<StatType, int> _upgradeLevels = new Dictionary<StatType, int>();
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
    }
    #endregion

    #region Firebase 초기화
    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result != DependencyStatus.Available)
            {
                Debug.LogError($"[Firebase] 의존성 해결 실패: {task.Result}");
                LoadFromPlayerPrefs();
                return;
            }

            _auth = FirebaseAuth.DefaultInstance;
            _db = FirebaseFirestore.DefaultInstance;
            IsInitialized = true;

            SignInAnonymously();
        });
    }
    #endregion

    #region 인증
    void SignInAnonymously()
    {
        // 이미 로그인된 사용자가 있으면 재사용
        if (_auth.CurrentUser != null)
        {
            _userId = _auth.CurrentUser.UserId;
            LoadUserData();
            return;
        }

        _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError($"[Firebase] 익명 인증 실패: {task.Exception}");
                LoadFromPlayerPrefs();
                return;
            }

            _userId = task.Result.User.UserId;
            Debug.Log($"[Firebase] 익명 인증 성공: {_userId}");
            LoadUserData();
        });
    }
    #endregion

    #region Firestore 데이터 로드
    void LoadUserData()
    {
        DocumentReference docRef = _db.Collection("users").Document(_userId);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[Firebase] 데이터 로드 실패: {task.Exception}");
                LoadFromPlayerPrefs();
                return;
            }

            DocumentSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();

                _totalGold = data.ContainsKey("totalGold")
                    ? Convert.ToInt32(data["totalGold"]) : 0;

                foreach (StatType stat in Enum.GetValues(typeof(StatType)))
                {
                    string key = $"upgrade_{stat}";
                    _upgradeLevels[stat] = data.ContainsKey(key)
                        ? Convert.ToInt32(data[key]) : 0;
                }

                SyncToPlayerPrefs();
            }
            else
            {
                // 신규 유저: PlayerPrefs에서 마이그레이션
                MigrateFromPlayerPrefs();
            }

            IsDataLoaded = true;
            OnDataLoaded?.Invoke();
        });
    }
    #endregion

    #region Firestore 데이터 저장
    public void SaveTotalGold(int gold)
    {
        _totalGold = gold;
        PlayerPrefs.SetInt("TotalGold", gold);
        PlayerPrefs.Save();

        if (!IsInitialized || string.IsNullOrEmpty(_userId)) return;

        _db.Collection("users").Document(_userId)
            .UpdateAsync("totalGold", gold)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError($"[Firebase] 골드 저장 실패: {task.Exception}");
            });
    }

    public void SaveUpgradeLevel(StatType type, int level)
    {
        _upgradeLevels[type] = level;
        PlayerPrefs.SetInt($"Upgrade_{type}", level);
        PlayerPrefs.Save();

        if (!IsInitialized || string.IsNullOrEmpty(_userId)) return;

        _db.Collection("users").Document(_userId)
            .UpdateAsync($"upgrade_{type}", level)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError($"[Firebase] 강화 저장 실패: {task.Exception}");
            });
    }
    #endregion

    #region 캐시 데이터 접근
    public int GetTotalGold() => _totalGold;

    public int GetUpgradeLevel(StatType type)
    {
        return _upgradeLevels.ContainsKey(type) ? _upgradeLevels[type] : 0;
    }
    #endregion

    #region PlayerPrefs 폴백 & 마이그레이션
    void LoadFromPlayerPrefs()
    {
        _totalGold = PlayerPrefs.GetInt("TotalGold", 0);

        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            _upgradeLevels[stat] = PlayerPrefs.GetInt($"Upgrade_{stat}", 0);

        IsDataLoaded = true;
        OnDataLoaded?.Invoke();
    }

    void SyncToPlayerPrefs()
    {
        PlayerPrefs.SetInt("TotalGold", _totalGold);
        foreach (var pair in _upgradeLevels)
            PlayerPrefs.SetInt($"Upgrade_{pair.Key}", pair.Value);
        PlayerPrefs.Save();
    }

    void MigrateFromPlayerPrefs()
    {
        _totalGold = PlayerPrefs.GetInt("TotalGold", 0);

        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            _upgradeLevels[stat] = PlayerPrefs.GetInt($"Upgrade_{stat}", 0);

        // Firestore에 초기 데이터 생성
        var data = new Dictionary<string, object>
        {
            { "totalGold", _totalGold },
            { "createdAt", FieldValue.ServerTimestamp },
            { "lastLoginAt", FieldValue.ServerTimestamp }
        };

        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            data[$"upgrade_{stat}"] = _upgradeLevels[stat];

        _db.Collection("users").Document(_userId)
            .SetAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError($"[Firebase] 마이그레이션 실패: {task.Exception}");
                else
                    Debug.Log("[Firebase] PlayerPrefs → Firestore 마이그레이션 완료");
            });
    }
    #endregion
}
