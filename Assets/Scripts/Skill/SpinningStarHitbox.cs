using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 회전 수리검 충돌 처리
/// - 적에게 닿으면 데미지
/// - 적별로 히트 쿨다운 관리 (다른 적은 즉시 히트 가능)
/// </summary>
public class SpinningStarHitbox : MonoBehaviour
{
    #region Private Fields
    int _damage;
    bool _isPierce;
    float _hitCooldown = 0.5f;
    Dictionary<Enemy, float> _lastHitTimes = new Dictionary<Enemy, float>();
    #endregion

    #region Public API
    public void Init(int damage, bool isPierce)
    {
        _damage = damage;
        _isPierce = isPierce;
    }
    #endregion

    #region Collision
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        if (_lastHitTimes.TryGetValue(enemy, out float lastHit) && Time.time - lastHit < _hitCooldown) return;

        enemy.TakeDamage(_damage);
        _lastHitTimes[enemy] = Time.time;
    }
    #endregion
}
