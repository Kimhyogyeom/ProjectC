using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 HP 관리
/// - 적에게 닿으면 데미지
/// - HP 바 UI 연동
/// - HP 0 이하 시 사망 처리
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    #region Serialized Fields
    [Header("Stats")]
    [SerializeField] int _maxHp = 100;

    [Header("Invincibility")]
    [SerializeField] float _invincibleDuration = 1.5f;

    [Header("UI")]
    [SerializeField] Image _hpBarFill;

    [Header("Debug (Read Only)")]
    [SerializeField] int _currentHp;
    [SerializeField] bool _isInvincible;
    #endregion

    #region Private Fields
    float _lastHitTime = -999f;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _currentHp = _maxHp;
        UpdateHPBar();
    }

    void Update()
    {
        _isInvincible = Time.time < _lastHitTime + _invincibleDuration;
    }
    #endregion

    #region Public API
    /// <summary>데미지를 받아 HP 감소, 0 이하 시 사망 처리 (무적 시간 중 무시)</summary>
    public void TakeDamage(int damage)
    {
        if (_isInvincible) return;

        _lastHitTime = Time.time;
        _currentHp -= damage;
        _currentHp = Mathf.Max(_currentHp, 0);
        UpdateHPBar();

        if (_currentHp <= 0)
            Die();
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
        // TODO: 게임 오버 UI, 씬 재시작 등 추가 예정
        Debug.Log("[Player] 사망");
    }
    #endregion
}
