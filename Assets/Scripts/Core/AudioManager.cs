using UnityEngine;

/// <summary>
/// 오디오 싱글톤 매니저
/// - BGM/SFX ON/OFF 상태 관리 (PlayerPrefs 저장)
/// - SFX 재생 (PlayOneShot)
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    #region Serialized Fields
    [Header("SFX - 전투")]
    [SerializeField] AudioClip _sfxShoot;
    [SerializeField, Range(0f, 1f)] float _volShoot = 1f;
    [SerializeField] AudioClip _sfxHit;
    [SerializeField, Range(0f, 1f)] float _volHit = 1f;
    [SerializeField] AudioClip _sfxPlayerHit;
    [SerializeField, Range(0f, 1f)] float _volPlayerHit = 1f;
    [SerializeField] AudioClip _sfxEnemyDie;
    [SerializeField, Range(0f, 1f)] float _volEnemyDie = 1f;

    [Header("SFX - 성장")]
    [SerializeField] AudioClip _sfxExpOrb;
    [SerializeField, Range(0f, 1f)] float _volExpOrb = 1f;
    [SerializeField] AudioClip _sfxLevelUp;
    [SerializeField, Range(0f, 1f)] float _volLevelUp = 1f;
    [SerializeField] AudioClip _sfxSkillSelect;
    [SerializeField, Range(0f, 1f)] float _volSkillSelect = 1f;
    [SerializeField] AudioClip _sfxHeal;
    [SerializeField, Range(0f, 1f)] float _volHeal = 1f;
    [SerializeField] AudioClip _sfxCoin;
    [SerializeField, Range(0f, 1f)] float _volCoin = 1f;

    [Header("SFX - 이벤트")]
    [SerializeField] AudioClip _sfxBoss;
    [SerializeField, Range(0f, 1f)] float _volBoss = 1f;
    [SerializeField] AudioClip _sfxWaveClear;
    [SerializeField, Range(0f, 1f)] float _volWaveClear = 1f;
    [SerializeField] AudioClip _sfxGameOver;
    [SerializeField, Range(0f, 1f)] float _volGameOver = 1f;

    [Header("SFX - UI")]
    [SerializeField] AudioClip _sfxButton;
    [SerializeField, Range(0f, 1f)] float _volButton = 1f;

    [Header("BGM")]
    [SerializeField] AudioClip _bgmMain;
    [SerializeField, Range(0f, 1f)] float _bgmVolume = 0.5f;

    [Header("SFX Volume")]
    [SerializeField, Range(0f, 1f)] float _sfxVolume = 1f;
    #endregion

    #region Private Fields
    const string PREF_BGM = "Audio_BGM";
    const string PREF_SFX = "Audio_SFX";

    bool _isBgmOn = true;
    bool _isSfxOn = true;

    AudioSource _sfxSource;
    AudioSource _bgmSource;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _isBgmOn = PlayerPrefs.GetInt(PREF_BGM, 1) == 1;
        _isSfxOn = PlayerPrefs.GetInt(PREF_SFX, 1) == 1;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.volume = _sfxVolume;

        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.playOnAwake = false;
        _bgmSource.loop = true;
        _bgmSource.volume = _bgmVolume;
    }

    void Start()
    {
        PlayBgm();
    }
    #endregion

    #region Public API
    public bool IsBgmOn => _isBgmOn;
    public bool IsSfxOn => _isSfxOn;

    public void SetBgm(bool isOn)
    {
        _isBgmOn = isOn;
        PlayerPrefs.SetInt(PREF_BGM, isOn ? 1 : 0);
        PlayerPrefs.Save();
        _bgmSource.mute = !isOn;
    }

    public void SetSfx(bool isOn)
    {
        _isSfxOn = isOn;
        PlayerPrefs.SetInt(PREF_SFX, isOn ? 1 : 0);
        PlayerPrefs.Save();
        _sfxSource.mute = !isOn;
    }

    public void PlaySfxShoot()       => PlaySfx(_sfxShoot, _volShoot);
    public void PlaySfxHit()         => PlaySfx(_sfxHit, _volHit);
    public void PlaySfxPlayerHit()   => PlaySfx(_sfxPlayerHit, _volPlayerHit);
    public void PlaySfxEnemyDie()    => PlaySfx(_sfxEnemyDie, _volEnemyDie);
    public void PlaySfxExpOrb()      => PlaySfx(_sfxExpOrb, _volExpOrb);
    public void PlaySfxLevelUp()     => PlaySfx(_sfxLevelUp, _volLevelUp);
    public void PlaySfxSkillSelect() => PlaySfx(_sfxSkillSelect, _volSkillSelect);
    public void PlaySfxHeal()        => PlaySfx(_sfxHeal, _volHeal);
    public void PlaySfxCoin()        => PlaySfx(_sfxCoin, _volCoin);
    public void PlaySfxBoss()        => PlaySfx(_sfxBoss, _volBoss);
    public void PlaySfxWaveClear()   => PlaySfx(_sfxWaveClear, _volWaveClear);
    public void PlaySfxGameOver()    => PlaySfx(_sfxGameOver, _volGameOver);
    public void PlaySfxButton()      => PlaySfx(_sfxButton, _volButton);
    #endregion

    #region Private
    void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (!_isSfxOn || clip == null) return;
        _sfxSource.PlayOneShot(clip, _sfxVolume * volume);
    }

    void PlayBgm()
    {
        if (_bgmMain == null) return;
        _bgmSource.clip = _bgmMain;
        _bgmSource.mute = !_isBgmOn;
        _bgmSource.Play();
    }
    #endregion
}
