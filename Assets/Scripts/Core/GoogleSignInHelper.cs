#if UNITY_ANDROID
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

/// <summary>
/// Google Play Games 로그인 래퍼
/// - 인증 코드를 받아서 FirebaseManager에 전달
/// </summary>
public class GoogleSignInHelper : MonoBehaviour
{
    public static GoogleSignInHelper Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        PlayGamesPlatform.Activate();
    }

    /// <summary>Google Play Games 로그인 → Firebase 연동</summary>
    public void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status == SignInStatus.Success)
            {
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    if (!string.IsNullOrEmpty(code))
                    {
                        Debug.Log("[GooglePlayGames] 인증 코드 획득 성공");
                        FirebaseManager.Instance?.LinkWithGoogle(code);
                    }
                    else
                    {
                        Debug.LogError("[GooglePlayGames] 인증 코드 획득 실패");
                        FirebaseManager.Instance?.NotifyGoogleLoginResult(false);
                    }
                });
            }
            else
            {
                Debug.LogError($"[GooglePlayGames] 로그인 실패: {status}");
                FirebaseManager.Instance?.NotifyGoogleLoginResult(false);
            }
        });
    }

    public void SignOut()
    {
        // Google Play Games v2는 SignOut을 지원하지 않음
        // Firebase 로그아웃만 처리
        FirebaseManager.Instance?.SignOut();
    }
}
#endif
