using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 슬라임이 발사하는 침 투사체 (오브젝트 풀링)
/// - 발사 시점 플레이어 방향으로 직선 이동 (호밍 아님 → 피할 수 있음)
/// - 플레이어 또는 벽에 닿으면 풀에 반납
/// </summary>
public class SlimeProjectile : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] float _speed = 6f;
    [SerializeField] float _lifetime = 4f;
    #endregion

    #region Pool
    static readonly Queue<SlimeProjectile> _pool = new Queue<SlimeProjectile>();
    static Transform _poolParent;

    static void EnsurePoolParent()
    {
        if (_poolParent == null)
        {
            _poolParent = new GameObject("[SlimeProjectilePool]").transform;
            Object.DontDestroyOnLoad(_poolParent.gameObject);
        }
    }

    public static SlimeProjectile Spawn(GameObject prefab, Vector3 position)
    {
        EnsurePoolParent();

        SlimeProjectile proj;
        if (_pool.Count > 0)
        {
            proj = _pool.Dequeue();
            proj.transform.SetParent(null);
            proj.transform.position = position;
            proj.gameObject.SetActive(true);
        }
        else
        {
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            proj = obj.GetComponent<SlimeProjectile>();
        }
        return proj;
    }

    void ReturnToPool()
    {
        gameObject.SetActive(false);
        EnsurePoolParent();
        transform.SetParent(_poolParent);
        _pool.Enqueue(this);
    }
    #endregion

    #region Private Fields
    Vector3 _direction;
    int _damage;
    float _aliveTimer;
    #endregion

    /// <summary>슬라임이 발사 시 호출 — 방향과 데미지 설정</summary>
    public void Init(Vector3 direction, int damage)
    {
        _direction = direction.normalized;
        _damage = damage;
        _aliveTimer = 0f;
    }

    #region Unity Lifecycle
    void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;

        _aliveTimer += Time.deltaTime;
        if (_aliveTimer >= _lifetime)
            ReturnToPool();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(_damage);
            ReturnToPool();
        }
        else if (!other.isTrigger)
        {
            ReturnToPool();
        }
    }
    #endregion
}
