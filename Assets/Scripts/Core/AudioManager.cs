using UnityEngine;

/// <summary>
/// 오디오 싱글톤 매니저
/// - BGM/SFX ON/OFF 상태 관리 (PlayerPrefs 저장)
/// - 실제 오디오 재생은 추후 구현
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    #region Private Fields
    const string PREF_BGM = "Audio_BGM";
    const string PREF_SFX = "Audio_SFX";

    bool _isBgmOn = true;
    bool _isSfxOn = true;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // PlayerPrefs에서 설정 불러오기 (기본값: ON)
        _isBgmOn = PlayerPrefs.GetInt(PREF_BGM, 1) == 1;
        _isSfxOn = PlayerPrefs.GetInt(PREF_SFX, 1) == 1;
    }
    #endregion

    #region Public API
    public bool IsBgmOn => _isBgmOn;
    public bool IsSfxOn => _isSfxOn;

    /// <summary>BGM ON/OFF 설정</summary>
    public void SetBgm(bool isOn)
    {
        _isBgmOn = isOn;
        PlayerPrefs.SetInt(PREF_BGM, isOn ? 1 : 0);
        PlayerPrefs.Save();

        // TODO: 실제 BGM AudioSource 뮤트 처리
        // if (_bgmSource != null) _bgmSource.mute = !isOn;
    }

    /// <summary>SFX ON/OFF 설정</summary>
    public void SetSfx(bool isOn)
    {
        _isSfxOn = isOn;
        PlayerPrefs.SetInt(PREF_SFX, isOn ? 1 : 0);
        PlayerPrefs.Save();

        // TODO: 실제 SFX 볼륨 처리
        // AudioListener 또는 개별 SFX 소스 제어
    }

    /// <summary>SFX 재생 (추후 구현)</summary>
    public void PlaySfx(AudioClip clip)
    {
        if (!_isSfxOn || clip == null) return;

        // TODO: AudioSource.PlayOneShot 구현
    }
    #endregion
}
