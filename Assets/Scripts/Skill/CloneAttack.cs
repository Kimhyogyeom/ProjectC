using UnityEngine;

/// <summary>
/// 분신 공격 컴포넌트
/// - 플레이어 공격 시 동일 방향으로 함께 발사
/// </summary>
public class CloneAttack : MonoBehaviour
{
    #region Private Fields
    ShadowCloneSkill _shadowClone;
    PlayerAttack _playerAttack;
    #endregion

    #region Public API
    public void Init(ShadowCloneSkill shadowClone)
    {
        _shadowClone = shadowClone;
        _playerAttack = GetComponentInParent<PlayerAttack>();
    }

    /// <summary>플레이어 발사 시 분신도 발사 (PlayerAttack에서 호출)</summary>
    public void FireInDirection(Vector3 direction, GameObject projectilePrefab, float speed)
    {
        if (projectilePrefab == null) return;

        Projectile projectile = Projectile.Spawn(projectilePrefab, transform.position, Quaternion.LookRotation(direction));
        if (projectile != null)
        {
            float damageRatio = _shadowClone != null ? _shadowClone.DamageRatio : 0.5f;
            projectile.Init(direction, speed, damageRatio: damageRatio, isPierce: _shadowClone.IsPierce);
        }
    }
    #endregion
}
