using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 일시정지 UI
/// - 우상단 일시정지 버튼 → 팝업 표시
/// - BGM/SFX 토글, 이어하기, 재시작, 로비 이동
/// </summary>
public class PauseUI : MonoBehaviour
{
    #region Serialized Fields
    [Header("Buttons")]
    [SerializeField] Button _pauseButton;       // 우상단 일시정지 버튼
    [SerializeField] Button _resumeButton;      // 이어하기
    [SerializeField] Button _restartButton;     // 재시작
    [SerializeField] Button _lobbyButton;       // 로비로 가기

    [Header("Toggles")]
    [SerializeField] Toggle _bgmToggle;         // BGM ON/OFF
    [SerializeField] Toggle _sfxToggle;         // 효과음 ON/OFF

    [Header("Panel")]
    [SerializeField] GameObject _pausePanel;    // 일시정지 팝업 패널
    #endregion

    #region Private Fields
    bool _isPaused;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _pausePanel.SetActive(false);

        _pauseButton.onClick.AddListener(OnPauseClicked);
        _resumeButton.onClick.AddListener(OnResumeClicked);
        _restartButton.onClick.AddListener(OnRestartClicked);
        _lobbyButton.onClick.AddListener(OnLobbyClicked);

        _bgmToggle.onValueChanged.AddListener(OnBgmToggled);
        _sfxToggle.onValueChanged.AddListener(OnSfxToggled);
    }

    void Start()
    {
        // 저장된 설정 불러오기 (AudioManager.Awake 이후 실행 보장)
        _bgmToggle.isOn = AudioManager.Instance.IsBgmOn;
        _sfxToggle.isOn = AudioManager.Instance.IsSfxOn;
    }
    #endregion

    #region Public API
    /// <summary>현재 일시정지 상태인지 여부</summary>
    public bool IsPaused => _isPaused;

    /// <summary>외부에서 강제로 일시정지 해제 (게임오버 전환 등)</summary>
    public void ForceClose()
    {
        _isPaused = false;
        _pausePanel.SetActive(false);
    }
    #endregion

    #region Button Callbacks
    void OnPauseClicked()
    {
        if (_isPaused) return;
        if (Time.timeScale == 0f) return; // 스킬 선택, 게임오버 등 이미 정지 상태면 무시

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        _isPaused = true;
        _pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void OnResumeClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        _isPaused = false;
        _pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void OnRestartClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        _isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnLobbyClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        _isPaused = false;
        Time.timeScale = 1f;

        // TODO: 로비 씬 추가 후 아래 주석 해제
        // SceneManager.LoadScene("Lobby");

        // 임시: 현재 씬 재로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion

    #region Toggle Callbacks
    void OnBgmToggled(bool isOn)
    {
        AudioManager.Instance.SetBgm(isOn);
    }

    void OnSfxToggled(bool isOn)
    {
        AudioManager.Instance.SetSfx(isOn);
    }
    #endregion
}
