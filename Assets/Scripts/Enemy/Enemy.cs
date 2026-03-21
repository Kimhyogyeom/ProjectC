using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections.Generic;

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

    [Header("Wave Scaling")]
    [SerializeField] float _scalePerWave = 0.01f;        // 웨이브당 스케일 증가량

    [Header("Drop")]
    [SerializeField] GameObject _expOrbPrefab;      // 경험치 구슬 프리팹
    [SerializeField] GameObject _healOrbPrefab;     // HP 회복 아이템 프리팹
    [SerializeField] float _healDropChance = 0.1f;  // 회복 아이템 드롭 확률 (10%)
    [SerializeField] int _goldDrop = 5;             // 사망 시 지급 골드

    [Header("UI")]
    [SerializeField] Image _hpBarFill;
    #endregion

    #region Private Fields
    int _currentHp;
    Transform _player;
    float _attackTimer;
    bool _isKnockedBack;
    bool _isDead = false;
    bool _isBoss = false;
    Coroutine _knockbackCoroutine;
    NavMeshAgent _agent;
    Collider _collider;
    Animator _animator;
    System.Action<float> _onHpChanged;   // 보스 HP바 UI 연동용
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _currentHp = _maxHp;
        _agent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();
        _animator = GetComponentInChildren<Animator>();

        _agent.speed = _moveSpeed;
        _agent.stoppingDistance = _stopDistance;
        _agent.updateUpAxis = false;

        UpdateHPBar();
    }

    /// <summary>웨이브 난이도에 따라 스탯 조정 (WaveManager에서 스폰 직후 호출)</summary>
    public void ApplyWaveDifficulty(int wave)
    {
        // 웨이브 1은 기본값, 이후 점진적 강화
        float hpMultiplier = 1f + (wave - 1) * 0.25f;       // 웨이브당 HP +25%
        float dmgMultiplier = 1f + (wave - 1) * 0.15f;      // 웨이브당 데미지 +15%
        float speedMultiplier = 1f + (wave - 1) * 0.05f;    // 웨이브당 이동속도 +5%

        _maxHp = Mathf.RoundToInt(_maxHp * hpMultiplier);
        _currentHp = _maxHp;
        _damage = Mathf.RoundToInt(_damage * dmgMultiplier);
        _moveSpeed *= speedMultiplier;

        if (_agent != null)
            _agent.speed = _moveSpeed;

        // 웨이브마다 스케일 증가
        float scaleMultiplier = 1f + (wave - 1) * _scalePerWave;
        transform.localScale = Vector3.one * scaleMultiplier;

        UpdateHPBar();
    }

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (_player == null || _isDead) return;

        if (0 < _attackTimer)
            _attackTimer -= Time.deltaTime;

        if (!_isKnockedBack && _agent.enabled && _agent.isOnNavMesh)
            _agent.SetDestination(_player.position);

        // 이동 애니메이션
        if (_animator != null)
        {
            bool isMoving = _agent.velocity.sqrMagnitude > 0.1f;
            _animator.SetBool("Moving", isMoving);
            Vector3 localVel = transform.InverseTransformDirection(_agent.velocity.normalized);
            _animator.SetFloat("Velocity X", localVel.x);
            _animator.SetFloat("Velocity Z", localVel.z);
        }
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
    public bool IsDead => _isDead;
    public bool IsBoss => _isBoss;
    public Transform Player => _player;

    /// <summary>보스로 설정 (WaveManager에서 호출)</summary>
    public void SetAsBoss(int wave, System.Action<float> onHpChanged)
    {
        _isBoss = true;
        _onHpChanged = onHpChanged;

        // 보스 스탯 배율 (웨이브 난이도 위에 추가 적용)
        _maxHp = Mathf.RoundToInt(_maxHp * 10f);
        _currentHp = _maxHp;
        _damage = Mathf.RoundToInt(_damage * 2f);
        _moveSpeed *= 0.7f;
        _goldDrop *= 10;

        if (_agent != null)
            _agent.speed = _moveSpeed;

        // 보스 스케일
        transform.localScale *= 2.5f;

        // 넉백 안 받게
        _knockbackDistance = 0f;

        UpdateHPBar();
        _onHpChanged?.Invoke(1f);
    }

    /// <summary>데미지를 받아 HP 감소, 0 이하 시 사망 처리</summary>
    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        _currentHp -= damage;
        UpdateHPBar();

        // 데미지 숫자 팝업 (흰색)
        DamagePopup.Create(transform.position + Vector3.up * 1.5f, damage.ToString(), Color.white);

        // 피격 빨간 깜빡임
        StartCoroutine(HitFlashRoutine());

        if (_currentHp <= 0)
            Die();
    }

    /// <summary>독 적용 (독 묻히기 스킬)</summary>
    public void ApplyPoison(float damagePerSecond, float duration, bool isSpreading)
    {
        StartCoroutine(PoisonRoutine(damagePerSecond, duration, isSpreading));
    }
    #endregion

    #region Poison
    Coroutine _poisonCoroutine;

    IEnumerator PoisonRoutine(float damagePerSecond, float duration, bool isSpreading)
    {
        float elapsed = 0f;
        float tick = 1f;
        float nextTick = tick;

        while (elapsed < duration && _currentHp > 0)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= nextTick)
            {
                int poisonDmg = Mathf.RoundToInt(damagePerSecond);
                _currentHp -= poisonDmg;
                UpdateHPBar();

                // 독 데미지 팝업 (초록색)
                DamagePopup.Create(transform.position + Vector3.up * 1.5f, poisonDmg.ToString(), new Color(0.2f, 0.9f, 0.2f));

                if (_currentHp <= 0)
                {
                    Die();
                    yield break;
                }
                nextTick += tick;
            }

            yield return null;
        }

        // Lv5 전염: 주변 적에게 독 전파
        if (isSpreading)
        {
            Collider[] nearby = Physics.OverlapSphere(transform.position, 2f);
            foreach (Collider col in nearby)
            {
                Enemy nearEnemy = col.GetComponent<Enemy>();
                if (nearEnemy != null && nearEnemy != this)
                    nearEnemy.ApplyPoison(damagePerSecond, duration * 0.5f, false);
            }
        }
    }
    #endregion

    #region Hit Flash
    Renderer[] _cachedRenderers;
    MaterialPropertyBlock _propBlock;

    IEnumerator HitFlashRoutine()
    {
        // 캐싱 (최초 1회)
        if (_cachedRenderers == null)
        {
            _cachedRenderers = GetComponentsInChildren<Renderer>();
            _propBlock = new MaterialPropertyBlock();
        }

        // 빨간색 플래시 (MaterialPropertyBlock으로 머티리얼 인스턴스 안 만듦)
        _propBlock.SetColor("_BaseColor", new Color(1f, 0.3f, 0.3f));
        foreach (Renderer r in _cachedRenderers)
            r.SetPropertyBlock(_propBlock);

        yield return new WaitForSeconds(0.1f);

        // 원래 색상 복원
        _propBlock.Clear();
        foreach (Renderer r in _cachedRenderers)
            r.SetPropertyBlock(_propBlock);
    }
    #endregion

    #region HP Bar
    void UpdateHPBar()
    {
        if (_hpBarFill == null) return;
        float ratio = (float)_currentHp / _maxHp;
        _hpBarFill.fillAmount = ratio;

        // 보스 HP바 UI 연동
        _onHpChanged?.Invoke(ratio);
    }
    #endregion

    #region Death
    void Die()
    {
        _isDead = true;

        // 모든 콜라이더 즉시 비활성화 (자식 포함)
        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;
        if (_agent != null) _agent.enabled = false;
        if (_hpBarFill != null) _hpBarFill.transform.parent.gameObject.SetActive(false);

        // 사망 애니메이션
        if (_animator != null)
        {
            _animator.SetInteger("TriggerNumber", 7);
            _animator.SetTrigger("Trigger");
        }

        // 경험치 구슬 스폰
        if (_expOrbPrefab != null)
            Instantiate(_expOrbPrefab, transform.position, Quaternion.identity);

        // HP 회복 아이템 드롭 (10% 확률)
        if (_healOrbPrefab != null && Random.value < _healDropChance)
            Instantiate(_healOrbPrefab, transform.position + Vector3.up * 0.1f, Quaternion.identity);

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxEnemyDie();

        // 골드 지급 (적 위치에서 UI로 날아감)
        if (GoldManager.Instance != null)
            GoldManager.Instance.AddGold(_goldDrop, transform.position);

        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
            waveManager.OnEnemyDied();

        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        // 애니메이션 재생 대기
        yield return new WaitForSeconds(1f);

        // 페이드 아웃
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        float elapsed = 0f;
        float fadeDuration = 1f;

        // 머티리얼 인스턴스화 및 알파 설정
        foreach (Renderer r in renderers)
            foreach (Material m in r.materials)
                m.SetFloat("_Surface", 1); // URP Transparent 모드

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeDuration);

            foreach (Renderer r in renderers)
                foreach (Material m in r.materials)
                    m.color = new Color(m.color.r, m.color.g, m.color.b, alpha);

            yield return null;
        }

        Destroy(gameObject);
    }
    #endregion
}
