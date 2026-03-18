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
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
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
}
