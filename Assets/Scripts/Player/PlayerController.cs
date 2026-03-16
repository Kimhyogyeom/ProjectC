using UnityEngine;

/// <summary>
/// 플레이어 이동 및 회전을 담당하는 컨트롤러
/// - DynamicJoystick으로부터 입력을 받아 이동
/// - 멈추면 PlayerAttack에서 자동 공격 활성화
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Movement")]
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _rotationSpeed = 720f;

    [Header("References")]
    [SerializeField] DynamicJoystick _joystick;
    #endregion

    #region Private Fields
    CharacterController _controller;
    Vector3 _moveDirection;
    bool _isMoving;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
    }
    #endregion

    #region Movement
    void HandleMovement()
    {
        // 조이스틱 입력을 3D 방향으로 변환 (Y축 무시 - 탑다운)
        Vector2 input = _joystick.InputVector;
        _moveDirection = new Vector3(input.x, 0f, input.y);

        _isMoving = 0.01f < _moveDirection.sqrMagnitude;

        if (_isMoving)
        {
            _controller.Move(_moveDirection.normalized * _moveSpeed * Time.deltaTime);
        }
    }

    void HandleRotation()
    {
        if (!_isMoving) return;

        // 이동 방향으로 부드럽게 회전
        Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }
    #endregion

    #region Public API
    /// <summary>현재 이동 중인지 여부 (PlayerAttack에서 사용)</summary>
    public bool IsMoving => _isMoving;
    #endregion
}
