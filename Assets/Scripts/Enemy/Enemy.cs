using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적 기본 클래스
/// - HP 관리 및 피격 처리
/// - HP 바 UI 연동
/// - 추후 AI 이동, 공격 패턴 등 확장 예정
/// </summary>
public class Enemy : MonoBehaviour
{
    #region Serialized Fields
    [Header("Stats")]
    [SerializeField] int _maxHp = 30;

    [Header("UI")]
    [SerializeField] Image _hpBarFill;
    #endregion

    #region Private Fields
    int _currentHp;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _currentHp = _maxHp;
        UpdateHPBar();
    }
    #endregion

    #region Public API
    /// <summary>데미지를 받아 HP 감소, 0 이하 시 사망 처리</summary>
    public void TakeDamage(int damage)
    {
        _currentHp -= damage;
        UpdateHPBar();

        if (_currentHp <= 0)
        {
            Die();
        }
    }
    #endregion

    #region HP Bar
    void UpdateHPBar()
    {
        if (_hpBarFill == null) return;
        _hpBarFill.fillAmount = (float)_currentHp / _maxHp;
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
