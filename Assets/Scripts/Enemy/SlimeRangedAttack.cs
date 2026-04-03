using UnityEngine;

/// <summary>
/// 슬라임 원거리 공격 컴포넌트
/// - 일정 거리 안에 플레이어가 들어오면 침 발사
/// - Enemy.cs와 함께 슬라임 프리팹에 추가
/// </summary>
public class SlimeRangedAttack : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] float _attackRange = 7f;
    [SerializeField] float _attackCooldown = 2f;
    #endregion

    #region Private Fields
    float _timer;
    Enemy _enemy;
    Transform _player;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        _enemy = GetComponent<Enemy>();
        _timer = _attackCooldown;
    }

    void Update()
    {
        if (_enemy == null || _enemy.IsDead) return;

        // Player 참조 지연 초기화 (Enemy.Start 이후 설정됨)
        if (_player == null)
        {
            _player = _enemy.Player;
            return;
        }

        _timer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist <= _attackRange)
        {
            // 플레이어 방향으로 회전
            Vector3 lookDir = _player.position - transform.position;
            lookDir.y = 0f;
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);

            if (_timer <= 0f)
            {
                Fire();
                _timer = _attackCooldown;
            }
        }
    }
    #endregion

    #region Attack
    void Fire()
    {
        if (_projectilePrefab == null) return;

        Vector3 dir = _player.position - transform.position;
        dir.y = 0f;

        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        SlimeProjectile sp = SlimeProjectile.Spawn(_projectilePrefab, spawnPos);
        if (sp != null)
            sp.Init(dir, _enemy.Damage);
    }
    #endregion
}
