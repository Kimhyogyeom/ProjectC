using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어 자동 공격 시스템
/// - 기본: 멈추면 가장 가까운 적에게 자동 발사 (Archero 방식)
/// - 질풍 스킬: 이동 중 공격 가능
/// - 다중 표창: 부채꼴로 N발 발사
/// - 독 묻히기: 투사체에 독 효과 부여
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerAttack : MonoBehaviour
{
    #region Serialized Fields
    [Header("Attack")]
    [SerializeField] float _attackRange = 10f;
    [SerializeField] float _attackCooldown = 0.5f;
    [SerializeField] Transform _firePoint;

    [Header("Projectile")]
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] float _projectileSpeed = 15f;
    #endregion

    #region Private Fields
    PlayerController _playerController;
    Animator _animator;
    float _lastAttackTime;

    // 스킬 상태
    int _multiShotCount = 1;            // 다중 표창 발사 수
    bool _isShotPierce = false;         // 관통 여부 (다중 표창 Lv5)
    bool _canAttackWhileMoving = false; // 질풍 스킬
    bool _galeDoubleSpeed = false;      // 질풍 Lv5
    bool _hasPoison = false;            // 독 묻히기
    float _poisonDamage = 0f;
    float _poisonDuration = 0f;
    bool _isPoisonSpreading = false;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        _animator = GetComponentInChildren<Animator>();
        _attackCooldown = Mathf.Max(0.1f, _attackCooldown - UpgradeManager.GetBonusAttackSpeed());
    }

    void Update()
    {
        bool isMoving = _playerController.IsMoving;

        // 질풍 스킬 없으면 멈춰야 공격, 있으면 항상 공격
        if (!isMoving || _canAttackWhileMoving)
        {
            float cooldown = (_galeDoubleSpeed && isMoving) ? _attackCooldown * 0.5f : _attackCooldown;
            HandleAttack(cooldown);
        }
    }
    #endregion

    #region Attack
    void HandleAttack(float cooldown)
    {
        if (Time.time - _lastAttackTime < cooldown) return;

        Transform target = FindClosestEnemy();
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;
        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = targetRot;

        // 공격 애니메이션 (R1=4, R2=5, R3=6 랜덤)
        int actionIndex = Random.Range(1, 7);
        if (_animator != null)
        {
            _animator.SetInteger("Weapon", 0);
            _animator.SetInteger("Jumping", 0);
            _animator.SetInteger("TriggerNumber", 4);
            _animator.SetInteger("Action", actionIndex);
            _animator.SetTrigger("Trigger");
        }

        // 분신 공격 애니메이션 (약간 딜레이)
        ShadowCloneSkill shadowClone = GetComponent<ShadowCloneSkill>();
        if (shadowClone != null)
            shadowClone.PlayAttackAnimation(actionIndex);

        StartCoroutine(FireWithDelay(direction, 0.2f));
        _lastAttackTime = Time.time;
    }

    IEnumerator FireWithDelay(Vector3 direction, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 이동 중이면 발사 시점에 적 방향 재계산
        if (_canAttackWhileMoving && _playerController.IsMoving)
        {
            Transform target = FindClosestEnemy();
            if (target != null)
            {
                direction = (target.position - transform.position).normalized;
                direction.y = 0f;
            }
        }

        FireProjectiles(direction);
    }

    Transform FindClosestEnemy()
    {
        Transform closest = null;
        float closestDistance = _attackRange;

        var enemies = Enemy.AliveEnemies;
        for (int i = 0; i < enemies.Count; i++)
        {
            Enemy enemy = enemies[i];
            if (enemy == null || enemy.IsDead) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy.transform;
            }
        }

        return closest;
    }

    void FireProjectiles(Vector3 direction)
    {
        if (_projectilePrefab == null || _firePoint == null) return;

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxShoot();

        if (_multiShotCount <= 1)
        {
            SpawnProjectile(_firePoint.position, direction, 1f, _isShotPierce);
        }
        else
        {
            // 부채꼴로 N발 발사
            float spreadAngle = 15f;
            float totalAngle = spreadAngle * (_multiShotCount - 1);
            float startAngle = -totalAngle / 2f;

            for (int i = 0; i < _multiShotCount; i++)
            {
                float angle = startAngle + spreadAngle * i;
                Vector3 spreadDir = Quaternion.Euler(0, angle, 0) * direction;
                SpawnProjectile(_firePoint.position, spreadDir, 1f, _isShotPierce);
            }
        }

        // 분신 발사
        ShadowCloneSkill shadowClone = GetComponent<ShadowCloneSkill>();
        if (shadowClone != null)
        {
            CloneAttack[] clones = shadowClone.GetCloneAttacks();
            foreach (CloneAttack clone in clones)
            {
                if (clone == null) continue;
                Vector3 cloneDir = direction;
                Transform cloneTarget = FindClosestEnemy();
                if (cloneTarget != null)
                {
                    cloneDir = (cloneTarget.position - clone.transform.position).normalized;
                    cloneDir.y = 0f;
                }
                clone.FireInDirection(cloneDir, _projectilePrefab, _projectileSpeed);
            }
        }
    }

    void SpawnProjectile(Vector3 position, Vector3 direction, float damageRatio, bool isPierce)
    {
        GameObject proj = Instantiate(_projectilePrefab, position, Quaternion.LookRotation(direction));
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile == null) return;

        projectile.Init(direction, _projectileSpeed, damageRatio, isPierce);

        if (_hasPoison)
            projectile.SetPoison(_poisonDamage, _poisonDuration, _isPoisonSpreading);
    }
    #endregion

    #region Skill Setters (SkillManager에서 호출)
    /// <summary>다중 표창 설정</summary>
    public void SetMultiShot(int count, bool isPierce)
    {
        _multiShotCount = count;
        _isShotPierce = isPierce;
    }

    /// <summary>독 묻히기 설정</summary>
    public void SetPoison(float damage, float duration, bool isSpreading)
    {
        _hasPoison = true;
        _poisonDamage = damage;
        _poisonDuration = duration;
        _isPoisonSpreading = isSpreading;
    }

    /// <summary>질풍 설정</summary>
    public void SetCanAttackWhileMoving(bool canAttack, bool doubleSpeed)
    {
        _canAttackWhileMoving = canAttack;
        _galeDoubleSpeed = doubleSpeed;
    }
    #endregion
}
