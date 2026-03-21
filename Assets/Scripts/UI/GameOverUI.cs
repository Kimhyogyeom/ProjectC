using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 게임 오버 UI
/// - 플레이어 사망 시 패널 표시
/// - 다시 시작 버튼으로 씬 재로드
/// </summary>
public class GameOverUI : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] PlayerHealth _playerHealth;
    [SerializeField] PauseUI _pauseUI;

    [Header("UI")]
    [SerializeField] GameObject _gameOverPanel;
    [SerializeField] Button _retryButton;
    [SerializeField] Button _lobbyButton;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _gameOverPanel.SetActive(false);
        _retryButton.onClick.AddListener(OnRetryClicked);
        if (_lobbyButton != null) _lobbyButton.onClick.AddListener(OnLobbyClicked);
    }

    void OnEnable()
    {
        _playerHealth.OnPlayerDied += HandlePlayerDied;
    }

    void OnDisable()
    {
        _playerHealth.OnPlayerDied -= HandlePlayerDied;
    }
    #endregion

    #region Event Handlers
    void HandlePlayerDied()
    {
        // 일시정지 중이면 먼저 해제
        if (_pauseUI != null && _pauseUI.IsPaused)
        {
            // PauseUI 패널은 직접 닫아주고 게임오버로 전환
            _pauseUI.ForceClose();
        }

        _gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxGameOver();
    }
    #endregion

    #region Button Callbacks
    void OnRetryClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        Time.timeScale = 1f;
        SceneTransition.Instance.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnLobbyClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        if (GoldManager.Instance != null) GoldManager.Instance.SaveSessionGold();
        Time.timeScale = 1f;
        SceneTransition.Instance.LoadScene("LobbyScene");
    }
    #endregion
}
