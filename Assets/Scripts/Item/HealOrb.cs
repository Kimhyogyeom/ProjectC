using UnityEngine;

/// <summary>
/// HP 회복 아이템
/// - 플레이어가 닿으면 최대 HP의 20% 회복
/// - 30초 후 자동 소멸
/// </summary>
public class HealOrb : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] float _healRatio = 0.2f;
    [SerializeField] float _lifetime = 30f;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        Destroy(gameObject, _lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.Heal(_healRatio);

        Destroy(gameObject);
    }
    #endregion
}
