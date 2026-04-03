using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HP 회복 아이템 (오브젝트 풀링)
/// - 플레이어가 닿으면 최대 HP의 20% 회복
/// - 30초 후 풀에 반납
/// </summary>
public class HealOrb : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] float _healRatio = 0.2f;
    [SerializeField] float _lifetime = 30f;
    #endregion

    #region Pool
    static readonly Queue<HealOrb> _pool = new Queue<HealOrb>();
    static Transform _poolParent;

    static void EnsurePoolParent()
    {
        if (_poolParent == null)
        {
            _poolParent = new GameObject("[HealOrbPool]").transform;
            Object.DontDestroyOnLoad(_poolParent.gameObject);
        }
    }

    public static HealOrb Spawn(GameObject prefab, Vector3 position)
    {
        EnsurePoolParent();

        HealOrb orb;
        if (_pool.Count > 0)
        {
            orb = _pool.Dequeue();
            orb.transform.SetParent(null);
            orb.transform.position = position;
            orb._aliveTimer = 0f;
            orb.gameObject.SetActive(true);
        }
        else
        {
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            orb = obj.GetComponent<HealOrb>();
        }
        return orb;
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
    float _aliveTimer = 0f;
    #endregion

    #region Unity Lifecycle
    void Update()
    {
        _aliveTimer += Time.deltaTime;
        if (_aliveTimer >= _lifetime)
            ReturnToPool();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.Heal(_healRatio);

        ReturnToPool();
    }
    #endregion
}
