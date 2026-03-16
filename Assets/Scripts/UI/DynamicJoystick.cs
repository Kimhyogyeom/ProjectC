using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 다이나믹 조이스틱
/// - 화면 터치 시 해당 위치에 조이스틱 생성
/// - 드래그 방향/크기로 이동 입력 제공
/// - 손가락 떼면 조이스틱 숨김
/// </summary>
public class DynamicJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    #region Serialized Fields
    [Header("Joystick Settings")]
    [SerializeField] float _handleRange = 1f;       // 핸들 이동 범위 배율
    [SerializeField] float _deadZone = 0f;           // 입력 무시 최소값

    [Header("UI References")]
    [SerializeField] RectTransform _background;      // 조이스틱 외부 원
    [SerializeField] RectTransform _handle;          // 조이스틱 내부 볼
    #endregion

    #region Private Fields
    RectTransform _baseRect;
    Canvas _canvas;
    Camera _camera;

    Vector2 _input = Vector2.zero;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _baseRect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _camera = _canvas.renderMode == RenderMode.ScreenSpaceCamera ? _canvas.worldCamera : null;

        // 시작 시 조이스틱 숨김
        _background.gameObject.SetActive(false);
    }
    #endregion

    #region Touch Input Handlers
    public void OnPointerDown(PointerEventData eventData)
    {
        // 터치 위치에 조이스틱 표시
        _background.anchoredPosition = ScreenToAnchoredPosition(eventData.position);
        _background.gameObject.SetActive(true);

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position = RectTransformUtility.WorldToScreenPoint(_camera, _background.position);
        Vector2 radius = _background.sizeDelta * 0.5f;

        // 터치 위치를 -1 ~ 1 범위로 정규화
        _input = (eventData.position - position) / (radius * _canvas.scaleFactor);

        // 핸들 범위 제한 (원 밖으로 못 나가도록)
        if (_deadZone < _input.magnitude)
        {
            _input = _input.magnitude < 1f ? _input : _input.normalized;
        }
        else
        {
            _input = Vector2.zero;
        }

        // 핸들 위치 업데이트
        _handle.anchoredPosition = _input * radius * _handleRange;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 손가락 떼면 초기화 및 숨김
        _input = Vector2.zero;
        _handle.anchoredPosition = Vector2.zero;
        _background.gameObject.SetActive(false);
    }
    #endregion

    #region Public API
    /// <summary>현재 조이스틱 입력값 (-1 ~ 1, PlayerController에서 사용)</summary>
    public Vector2 InputVector => _input;
    #endregion

    #region Helpers
    Vector2 ScreenToAnchoredPosition(Vector2 screenPosition)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _baseRect, screenPosition, _camera, out localPoint);
        return localPoint;
    }
    #endregion
}
