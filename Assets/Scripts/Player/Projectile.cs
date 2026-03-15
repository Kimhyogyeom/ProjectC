using UnityEngine;

/// <summary>
/// 표창 투사체
/// - 직선으로 날아가서 적에게 충돌 시 데미지
/// - 일정 시간 후 자동 제거
/// </summary>
public class Projectile : MonoBehaviour
{
    #region Serialized Fields
    [Header("Projectile Settings")]
    [SerializeField] float _lifeTime = 3f;
    [SerializeField] int _damage = 10;
    #endregion

    #region Private Fields
    Vector3 _direction;
    float _speed;
    #endregion

    #region Unity Lifecycle
    void Update()
    {
        // 직선 이동
        transform.Translate(_direction * _speed * Time.deltaTime, Space.World);
    }
    #endregion

    #region Collision
    void OnTriggerEnter(Collider other)
    {
        // 적과 충돌 시 데미지 처리
        if (!other.CompareTag("Enemy")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(_damage);
        }

        Destroy(gameObject);
    }
    #endregion

    #region Public API
    /// <summary>발사 방향과 속도를 설정 (PlayerAttack에서 호출)</summary>
    public void Init(Vector3 direction, float speed)
    {
        _direction = direction;
        _speed = speed;

        // 수명 끝나면 자동 제거
        Destroy(gameObject, _lifeTime);
    }
    #endregion
}
