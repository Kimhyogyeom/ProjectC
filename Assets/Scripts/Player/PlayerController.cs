using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 이동 및 회전을 담당하는 컨트롤러
/// - Input System의 "Move" 액션과 자동 연결 (Send Messages)
/// - 멈추면 자동 공격 (추후 구현)
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Movement")]
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _rotationSpeed = 720f;
    #endregion

    #region Private Fields
    CharacterController _controller;
    Vector2 _inputVector;
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
        // 2D 입력을 3D 방향으로 변환 (Y축 무시 - 탑다운)
        _moveDirection = new Vector3(_inputVector.x, 0f, _inputVector.y);

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

    #region Input Callbacks
    // Input System "Move" 액션 콜백 (Player Input 컴포넌트가 자동 호출)
    void OnMove(InputValue value)
    {
        _inputVector = value.Get<Vector2>();
    }
    #endregion

    #region Public API
    /// <summary>현재 이동 중인지 여부 (공격 시스템에서 사용 예정)</summary>
    public bool IsMoving => _isMoving;
    #endregion
}
