using UnityEngine;

/// <summary>
/// 적 기본 클래스
/// - HP 관리 및 피격 처리
/// - 추후 AI 이동, 공격 패턴 등 확장 예정
/// </summary>
public class Enemy : MonoBehaviour
{
    #region Serialized Fields
    [Header("Stats")]
    [SerializeField] int _maxHp = 30;
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
        Debug.Log($"[Enemy] HP: {_currentHp} / {_maxHp}");

        if (_currentHp <= 0)
        {
            Die();
        }
    }
    #endregion

    #region Death
    void Die()
    {
        // TODO: 사망 이펙트, 경험치 드랍 등 추가 예정
        Destroy(gameObject);
    }
    #endregion
}
