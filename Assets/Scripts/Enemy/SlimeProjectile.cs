using UnityEngine;

/// <summary>
/// 슬라임이 발사하는 침 투사체
/// - 발사 시점 플레이어 방향으로 직선 이동 (호밍 아님 → 피할 수 있음)
/// - 플레이어 또는 벽에 닿으면 소멸
/// </summary>
public class SlimeProjectile : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] float _speed = 6f;
    [SerializeField] float _lifetime = 4f;
    #endregion

    #region Private Fields
    Vector3 _direction;
    int _damage;
    #endregion

    /// <summary>슬라임이 발사 시 호출 — 방향과 데미지 설정</summary>
    public void Init(Vector3 direction, int damage)
    {
        _direction = direction.normalized;
        _damage = damage;
        Destroy(gameObject, _lifetime);
    }

    #region Unity Lifecycle
    void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(_damage);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            // 벽이나 지형에 맞으면 소멸
            Destroy(gameObject);
        }
    }
    #endregion
}
