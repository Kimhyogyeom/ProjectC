using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적 기본 클래스
/// - HP 관리 및 피격 처리
/// - HP 바 UI 연동
/// - 플레이어 추적 AI
/// - 충돌 시 데미지 + 넉백
/// </summary>
public class Enemy : MonoBehaviour
{
    #region Serialized Fields
    [Header("Stats")]
    [SerializeField] int _maxHp = 30;
    [SerializeField] float _moveSpeed = 2f;
    [SerializeField] float _pushForce = 5f;       // 플레이어에게 밀려나는 힘
    [SerializeField] float _stopDistance = 1f;    // 플레이어와 유지할 거리
    [SerializeField] int _damage = 10;            // 플레이어에게 주는 데미지
    [SerializeField] float _attackCooldown = 1f;  // 재접촉 후 데미지 쿨타임
    [SerializeField] float _knockbackDistance = 1.5f; // 충돌 후 넉백 거리

    [Header("UI")]
    [SerializeField] Image _hpBarFill;
    #endregion

    #region Private Fields
    int _currentHp;
    Transform _player;
    float _attackTimer;
    bool _isKnockedBack;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _currentHp = _maxHp;
        UpdateHPBar();
    }

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (_player == null) return;

        // 쿨타임 감소
        if (0 < _attackTimer)
            _attackTimer -= Time.deltaTime;

        if (!_isKnockedBack)
            ChasePlayer();
    }
    #endregion

    #region AI
    void ChasePlayer()
    {
        float distance = Vector3.Distance(transform.position, _player.position);

        // 정지 거리 이내면 이동 안 함
        if (distance <= _stopDistance) return;

        // 플레이어 방향으로 이동
        Vector3 direction = (_player.position - transform.position).normalized;
        direction.y = 0f;

        transform.position += direction * _moveSpeed * Time.deltaTime;

        // 플레이어 방향으로 회전
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }
    #endregion

    #region Collision
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 쿨타임 중이면 데미지 없음
        if (0 < _attackTimer) return;

        // 플레이어 데미지
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(_damage);

        // 쿨타임 초기화
        _attackTimer = _attackCooldown;

        // 넉백 (플레이어 반대 방향으로)
        Knockback(other.transform.position);
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 겹침 방지 - 플레이어 반대 방향으로 밀어냄 (Rigidbody 없이 처리)
        Vector3 pushDirection = (transform.position - other.transform.position).normalized;
        pushDirection.y = 0f;
        transform.position += pushDirection * _pushForce * Time.deltaTime;

        // 쿨타임마다 데미지
        if (_attackTimer > 0) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(_damage);

        _attackTimer = _attackCooldown;
        Knockback(other.transform.position);
    }
    #endregion

    #region Knockback
    void Knockback(Vector3 playerPosition)
    {
        Vector3 knockbackDirection = (transform.position - playerPosition).normalized;
        knockbackDirection.y = 0f;
        StartCoroutine(KnockbackRoutine(knockbackDirection));
    }

    IEnumerator KnockbackRoutine(Vector3 direction)
    {
        _isKnockedBack = true;

        float elapsed = 0f;
        float duration = 0.15f; // 넉백 지속 시간 (초)

        while (elapsed < duration)
        {
            float ratio = 1f - (elapsed / duration); // 점점 느려지는 효과
            transform.position += direction * _knockbackDistance * ratio * Time.deltaTime / duration;
            elapsed += Time.deltaTime;
            yield return null;
        }

        _isKnockedBack = false;
    }
    #endregion

    #region Public API
    /// <summary>데미지를 받아 HP 감소, 0 이하 시 사망 처리</summary>
    public void TakeDamage(int damage)
    {
        _currentHp -= damage;
        UpdateHPBar();

        if (_currentHp <= 0)
            Die();
    }
    #endregion

    #region HP Bar
    void UpdateHPBar()
    {
        if (_hpBarFill == null) return;
        _hpBarFill.fillAmount = (float)_currentHp / _maxHp;
    }
    #endregion

    #region Death
    void Die()
    {
        // TODO: 사망 이펙트, 경험치 드랍 등 추가 예정
        Destroy(gameObject);
    }
    #endregion
}
