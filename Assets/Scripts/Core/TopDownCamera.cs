using UnityEngine;

/// <summary>
/// Archero 스타일 탑다운(쿼터뷰) 카메라
/// - 플레이어를 부드럽게 추적
/// - Offset으로 시점 각도 조절
/// </summary>
public class TopDownCamera : MonoBehaviour
{
    #region Serialized Fields
    [Header("Target")]
    [SerializeField] Transform _target;

    [Header("Camera Settings")]
    [SerializeField] Vector3 _offset = new Vector3(0f, 10f, -6f);
    [SerializeField] float _smoothSpeed = 10f;

    [Header("Boundary")]
    [SerializeField] BoxCollider _boundaryCollider;  // 맵 경계 콜라이더
    #endregion

    #region Private Fields
    Camera _cam;
    Quaternion _fixedRotation;
    Vector3 _shakeOffset;
    float _shakeDuration;
    float _shakeTimer;
    float _shakeMagnitude;
    #endregion

    #region Static Instance
    public static TopDownCamera Instance { get; private set; }
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        Instance = this;
        _cam = GetComponent<Camera>();
    }

    void Start()
    {
        // 시작 시 카메라 각도 고정 (offset 방향을 바라보는 회전)
        if (_target != null)
        {
            transform.position = _target.position + _offset;
            transform.LookAt(_target.position);
        }
        _fixedRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (_target == null) return;

        FollowTarget();
        ApplyShake();
    }
    #endregion

    #region Camera
    void FollowTarget()
    {
        Vector3 desiredPosition = _target.position + _offset;
        Vector3 smoothed = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);

        // 경계 콜라이더가 있으면, 카메라 시야를 계산해서 바닥 밖이 안 보이게 클램프
        if (_boundaryCollider != null)
        {
            Bounds b = _boundaryCollider.bounds;

            // 카메라가 바닥(y=0)에서 보이는 영역 절반 크기 계산
            float camHeight = smoothed.y;
            float halfHeight = camHeight * Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * _cam.aspect;

            // X축 클램프
            float minX = b.min.x + halfWidth;
            float maxX = b.max.x - halfWidth;
            if (minX < maxX)
                smoothed.x = Mathf.Clamp(smoothed.x, minX, maxX);

            // Z축 클램프 (카메라 기울기 보정)
            float minZ = b.min.z + halfHeight + _offset.z;
            float maxZ = b.max.z - halfHeight + _offset.z;
            if (minZ < maxZ)
                smoothed.z = Mathf.Clamp(smoothed.z, minZ, maxZ);
        }

        transform.position = smoothed;
        transform.rotation = _fixedRotation;
    }
    #endregion

    #region Shake
    void ApplyShake()
    {
        if (_shakeTimer <= 0f) return;

        _shakeTimer -= Time.deltaTime;
        float t = _shakeTimer / _shakeDuration;  // 1→0 감쇠
        float magnitude = _shakeMagnitude * t;

        _shakeOffset = new Vector3(
            Random.Range(-magnitude, magnitude),
            Random.Range(-magnitude, magnitude) * 0.5f,
            Random.Range(-magnitude, magnitude)
        );

        transform.position += _shakeOffset;

        if (_shakeTimer <= 0f)
            _shakeOffset = Vector3.zero;
    }

    /// <summary>카메라 셰이크 (magnitude: 흔들림 세기, duration: 지속 시간)</summary>
    public void Shake(float magnitude = 0.15f, float duration = 0.2f)
    {
        // 더 강한 셰이크가 진행 중이면 무시
        if (_shakeTimer > 0f && _shakeMagnitude > magnitude) return;

        _shakeMagnitude = magnitude;
        _shakeDuration = duration;
        _shakeTimer = duration;
    }
    #endregion
}
