using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

/// <summary>
/// 적 기본 클래스
/// - HP 관리 및 피격 처리
/// - HP 바 UI 연동
/// - NavMeshAgent 기반 플레이어 추적 (에이전트끼리 자동 분리)
/// - 충돌 시 데미지 + 넉백
/// </summary>
public class Enemy : MonoBehaviour
{
    #region Serialized Fields
    [Header("Stats")]
    [SerializeField] int _maxHp = 30;
    [SerializeField] float _moveSpeed = 2f;
    [SerializeField] float _stopDistance = 1f;
    [SerializeField] int _damage = 10;
    [SerializeField] float _attackCooldown = 1f;
    [SerializeField] float _knockbackDistance = 1.5f;

    [Header("UI")]
    [SerializeField] Image _hpBarFill;
    #endregion

    #region Private Fields
    int _currentHp;
    Transform _player;
    float _attackTimer;
    bool _isKnockedBack;
    Coroutine _knockbackCoroutine;
    NavMeshAgent _agent;
    Collider _collider;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _currentHp = _maxHp;
        _agent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();

        _agent.speed = _moveSpeed;
        _agent.stoppingDistance = _stopDistance;
        _agent.updateUpAxis = false;

        UpdateHPBar();
    }

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (_player == null) return;

        if (0 < _attackTimer)
            _attackTimer -= Time.deltaTime;

        if (!_isKnockedBack)
            _agent.SetDestination(_player.position);
    }
    #endregion

    #region Collision
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // 데미지는 쿨타임 체크, 넉백은 항상 적용
        if (_attackTimer <= 0)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(_damage);

            _attackTimer = _attackCooldown;
        }

        Knockback(other.transform.position);
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_attackTimer > 0 || _isKnockedBack) return;

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

        if (_knockbackCoroutine != null)
            StopCoroutine(_knockbackCoroutine);
        _knockbackCoroutine = StartCoroutine(KnockbackRoutine(knockbackDirection));
    }

    IEnumerator KnockbackRoutine(Vector3 direction)
    {
        _isKnockedBack = true;
        _agent.enabled = false;

        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            float ratio = 1f - (elapsed / duration);
            transform.position += direction * _knockbackDistance * ratio * Time.deltaTime / duration;
            elapsed += Time.deltaTime;
            yield return null;
        }

        _agent.enabled = true;
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
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
            waveManager.OnEnemyDied();

        Destroy(gameObject);
    }
    #endregion
}
