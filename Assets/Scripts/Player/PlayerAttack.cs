using UnityEngine;

/// <summary>
/// 플레이어 자동 공격 시스템
/// - 멈추면 가장 가까운 적을 탐색하여 자동 발사
/// - 이동 중에는 공격하지 않음 (Archero 방식)
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
    float _lastAttackTime;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        // Archero 방식: 멈췄을 때만 공격
        if (!_playerController.IsMoving)
        {
            HandleAttack();
        }
    }
    #endregion

    #region Attack
    void HandleAttack()
    {
        // 쿨다운 체크
        if (Time.time - _lastAttackTime < _attackCooldown) return;

        // 가장 가까운 적 탐색
        Transform target = FindClosestEnemy();
        if (target == null) return;

        // 적 방향으로 회전
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;
        transform.rotation = Quaternion.LookRotation(direction);

        // 표창 발사
        FireProjectile(direction);
        _lastAttackTime = Time.time;
    }

    Transform FindClosestEnemy()
    {
        // "Enemy" 태그가 달린 오브젝트 중 가장 가까운 적 탐색
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Transform closest = null;
        float closestDistance = _attackRange;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy.transform;
            }
        }

        return closest;
    }

    void FireProjectile(Vector3 direction)
    {
        if (_projectilePrefab == null || _firePoint == null) return;

        // 표창 생성 및 방향 설정
        GameObject projectile = Instantiate(_projectilePrefab, _firePoint.position, Quaternion.LookRotation(direction));
        Projectile proj = projectile.GetComponent<Projectile>();

        if (proj != null)
        {
            proj.Init(direction, _projectileSpeed);
        }
    }
    #endregion
}
