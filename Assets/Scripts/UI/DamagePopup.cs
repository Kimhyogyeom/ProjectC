using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 데미지 숫자 팝업 (오브젝트 풀링)
/// - 월드 스페이스에서 위로 떠오르며 페이드 아웃
/// - 색상으로 타입 구분 (흰색: 일반, 빨강: 플레이어 피격, 초록: 독)
/// </summary>
public class DamagePopup : MonoBehaviour
{
    #region Pool
    public static TMP_FontAsset SharedFont;

    static Queue<DamagePopup> _pool = new Queue<DamagePopup>();
    static Transform _poolParent;
    const int POOL_PRELOAD = 15;

    static void EnsurePool()
    {
        if (_poolParent == null)
        {
            _poolParent = new GameObject("[DamagePopupPool]").transform;
            Object.DontDestroyOnLoad(_poolParent.gameObject);

            // 미리 생성
            for (int i = 0; i < POOL_PRELOAD; i++)
                _pool.Enqueue(CreateInstance());
        }
    }

    static DamagePopup CreateInstance()
    {
        GameObject popupObj = new GameObject("DamagePopup");
        popupObj.transform.SetParent(_poolParent);

        DamagePopup popup = popupObj.AddComponent<DamagePopup>();

        // TextMeshPro 생성
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(popupObj.transform, false);
        TMP_Text tmpText = textObj.AddComponent<TextMeshPro>();
        if (SharedFont != null) tmpText.font = SharedFont;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.fontStyle = FontStyles.Bold;
        popup._text = tmpText;

        popupObj.SetActive(false);
        return popup;
    }

    static DamagePopup GetFromPool()
    {
        EnsurePool();

        DamagePopup popup;
        if (_pool.Count > 0)
        {
            popup = _pool.Dequeue();
        }
        else
        {
            popup = CreateInstance();
        }

        popup.gameObject.SetActive(true);
        return popup;
    }

    void ReturnToPool()
    {
        gameObject.SetActive(false);
        transform.SetParent(_poolParent);
        _pool.Enqueue(this);
    }
    #endregion

    #region Private Fields
    TMP_Text _text;
    float _duration = 0.8f;
    float _elapsed = 0f;
    float _riseSpeed = 1.5f;
    Color _color;
    Camera _mainCamera;
    #endregion

    #region Unity Lifecycle
    void OnEnable()
    {
        _elapsed = 0f;
        _mainCamera = Camera.main;
    }

    void Update()
    {
        _elapsed += Time.deltaTime;

        // 위로 떠오름
        transform.position += Vector3.up * _riseSpeed * Time.deltaTime;

        // 빌보드 (카메라 바라보기)
        if (_mainCamera != null)
            transform.forward = _mainCamera.transform.forward;

        // 페이드 아웃
        float alpha = 1f - (_elapsed / _duration);
        if (_text != null)
        {
            Color c = _color;
            c.a = alpha;
            _text.color = c;
        }

        // 스케일 애니메이션 (처음에 커졌다가 줄어듦)
        float scale = _elapsed < 0.1f ? Mathf.Lerp(0.5f, 1.2f, _elapsed / 0.1f) :
                      _elapsed < 0.2f ? Mathf.Lerp(1.2f, 1f, (_elapsed - 0.1f) / 0.1f) : 1f;
        transform.localScale = Vector3.one * scale;

        if (_elapsed >= _duration)
            ReturnToPool();
    }
    #endregion

    #region Public API
    /// <summary>데미지 팝업 생성 (풀링)</summary>
    public static void Create(Vector3 position, string text, Color color, float fontSize = 5f)
    {
        DamagePopup popup = GetFromPool();

        // 랜덤 오프셋으로 겹침 방지
        Vector3 offset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.5f, 1f), 0f);
        popup.transform.SetParent(null);
        popup.transform.position = position + offset;
        popup.transform.localScale = Vector3.one;

        popup._color = color;
        popup._text.text = text;
        popup._text.color = color;
        popup._text.fontSize = fontSize;
    }
    #endregion
}
