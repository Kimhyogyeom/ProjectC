using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 웨이브 UI
/// - 상단 웨이브 번호 표시 (항상)
/// - 중앙 웨이브 시작/클리어 알림 (잠깐 표시 후 페이드아웃)
/// </summary>
public class WaveUI : MonoBehaviour
{
    #region Serialized Fields
    [Header("References")]
    [SerializeField] WaveManager _waveManager;

    [Header("UI")]
    [SerializeField] TMP_Text _waveCounterText;     // 상단 상시 표시 (예: "Wave 1")
    [SerializeField] TMP_Text _announcementText;    // 중앙 알림 (예: "Wave 1 Start!")

    [Header("Announcement Settings")]
    [SerializeField] float _displayDuration = 1.5f; // 알림 표시 시간
    [SerializeField] float _fadeDuration = 0.5f;    // 페이드아웃 시간
    #endregion

    #region Private Fields
    Coroutine _announcementCoroutine;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        if (_announcementText != null)
            _announcementText.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        _waveManager.OnWaveStarted += HandleWaveStarted;
        _waveManager.OnWaveCleared += HandleWaveCleared;
    }

    void OnDisable()
    {
        _waveManager.OnWaveStarted -= HandleWaveStarted;
        _waveManager.OnWaveCleared -= HandleWaveCleared;
    }
    #endregion

    #region Event Handlers
    void HandleWaveStarted(int wave)
    {
        if (_waveCounterText != null)
            _waveCounterText.text = $"Wave {wave}";

        ShowAnnouncement($"Wave {wave} Start!");
    }

    void HandleWaveCleared()
    {
        ShowAnnouncement("Wave Clear!");
    }
    #endregion

    #region Announcement
    void ShowAnnouncement(string message)
    {
        if (_announcementText == null) return;

        if (_announcementCoroutine != null)
            StopCoroutine(_announcementCoroutine);

        _announcementCoroutine = StartCoroutine(AnnouncementRoutine(message));
    }

    IEnumerator AnnouncementRoutine(string message)
    {
        _announcementText.text = message;
        _announcementText.gameObject.SetActive(true);

        // 알림 유지
        Color color = _announcementText.color;
        color.a = 1f;
        _announcementText.color = color;
        yield return new WaitForSeconds(_displayDuration);

        // 페이드아웃
        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            color.a = 1f - (elapsed / _fadeDuration);
            _announcementText.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }

        _announcementText.gameObject.SetActive(false);
    }
    #endregion
}
