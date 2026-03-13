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
    #endregion

    #region Unity Lifecycle
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
        transform.position = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
        transform.LookAt(_target.position);
    }
    #endregion
}
