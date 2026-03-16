using UnityEngine;

/// <summary>
/// 플레이어 HP 관리
/// - 적에게 닿으면 데미지
/// - HP 0 이하 시 사망 처리
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    #region Serialized Fields
    [Header("Stats")]
    [SerializeField] int _maxHp = 100;
    #endregion

    #region Private Fields
    int _currentHp;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _currentHp = _maxHp;
    }
    #endregion

    #region Public API
    /// <summary>데미지를 받아 HP 감소, 0 이하 시 사망 처리</summary>
    public void TakeDamage(int damage)
    {
        _currentHp -= damage;
        Debug.Log($"[Player] HP: {_currentHp} / {_maxHp}");

        if (_currentHp <= 0)
        {
            Die();
        }
    }
    #endregion

    #region Death
    void Die()
    {
        // TODO: 게임 오버 UI, 씬 재시작 등 추가 예정
        Debug.Log("[Player] 사망");
    }
    #endregion
}
