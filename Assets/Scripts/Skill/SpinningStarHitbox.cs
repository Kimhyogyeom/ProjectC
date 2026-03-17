using UnityEngine;

/// <summary>
/// 회전 수리검 충돌 처리
/// - 적에게 닿으면 데미지
/// - 관통 여부에 따라 연속 히트 가능
/// </summary>
public class SpinningStarHitbox : MonoBehaviour
{
    #region Private Fields
    int _damage;
    bool _isPierce;
    float _hitCooldown = 0.5f;
    float _lastHitTime;
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
        if (Time.time - _lastHitTime < _hitCooldown) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(_damage);
            _lastHitTime = Time.time;
        }
    }
    #endregion
}
