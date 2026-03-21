using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 씬 전환 페이드 인/아웃
/// - 싱글톤, DontDestroyOnLoad
/// - FadeOut → 씬 로드 → FadeIn
/// </summary>
public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] Image _fadeImage;
    [SerializeField] float _fadeDuration = 0.4f;
    [SerializeField] float _waitDuration = 2f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    /// <summary>페이드 아웃 후 씬 전환</summary>
    public void LoadScene(string sceneName)
    {
        StartCoroutine(TransitionRoutine(sceneName));
    }

    IEnumerator TransitionRoutine(string sceneName)
    {
        yield return StartCoroutine(FadeOut());

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // 로드 완료 대기
        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;

        // 씬 완전히 로드된 후 2초 대기
        yield return new WaitForSecondsRealtime(_waitDuration);

        yield return StartCoroutine(FadeIn());
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color c = _fadeImage.color;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Clamp01(elapsed / _fadeDuration);
            _fadeImage.color = c;
            yield return null;
        }
        c.a = 1f;
        _fadeImage.color = c;
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color c = _fadeImage.color;
        c.a = 1f;
        _fadeImage.color = c;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = 1f - Mathf.Clamp01(elapsed / _fadeDuration);
            _fadeImage.color = c;
            yield return null;
        }
        c.a = 0f;
        _fadeImage.color = c;
    }
}
