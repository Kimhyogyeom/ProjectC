using System;
using System.Collections;
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
    float _dodgeChance = 0f;        // 연막탄 회피 확률
    bool _dodgeTeleport = false;    // 연막탄 Lv5: 회피 시 순간이동
    Renderer[] _renderers;
    bool[] _rendererDefaultState;
    Coroutine _flashCoroutine;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _currentHp = _maxHp;
        _renderers = GetComponentsInChildren<Renderer>();
        _rendererDefaultState = new bool[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _rendererDefaultState[i] = _renderers[i].enabled;
        UpdateHPBar();
    }

    void Update()
    {
        // 인스펙터 실시간 확인용
        _isInvincible = Time.time < _lastHitTime + _invincibleDuration;
    }
    #endregion

    #region Public API
    /// <summary>데미지를 받아 HP 감소, 0 이하 시 사망 처리 (무적 시간 중 무시)</summary>
    public void TakeDamage(int damage)
    {
        if (Time.time < _lastHitTime + _invincibleDuration) return;

        // 연막탄 회피 판정
        if (_dodgeChance > 0f && UnityEngine.Random.value < _dodgeChance)
        {
            // MISS 팝업 (회색)
            DamagePopup.Create(transform.position + Vector3.up * 2f, "MISS", new Color(0.7f, 0.7f, 0.7f), 6f);
            if (_dodgeTeleport) TeleportDodge();
            return;
        }

        _lastHitTime = Time.time;
        _currentHp -= damage;
        _currentHp = Mathf.Max(_currentHp, 0);
        UpdateHPBar();

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxPlayerHit();

        // 플레이어 피격 데미지 팝업 (빨간색)
        DamagePopup.Create(transform.position + Vector3.up * 2f, damage.ToString(), new Color(1f, 0.3f, 0.3f), 6f);

        // 깜빡임 시작
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashRoutine());

        if (_currentHp <= 0)
            Die();
    }

    /// <summary>최대 HP의 ratio만큼 회복</summary>
    public void Heal(float ratio)
    {
        int amount = Mathf.RoundToInt(_maxHp * ratio);
        _currentHp = Mathf.Min(_currentHp + amount, _maxHp);
        UpdateHPBar();

        DamagePopup.Create(transform.position + Vector3.up * 2f, $"+{amount}", new Color(0.3f, 1f, 0.4f), 6f);
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxHeal();
    }

    /// <summary>연막탄 설정 (SkillManager에서 호출)</summary>
    public void SetDodgeChance(float chance, bool teleport)
    {
        _dodgeChance = chance;
        _dodgeTeleport = teleport;
    }
    #endregion

    #region HP Bar
    void UpdateHPBar()
    {
        if (_hpBarFill == null) return;
        _hpBarFill.fillAmount = (float)_currentHp / _maxHp;
    }
    #endregion

    #region Public API (Events)
    /// <summary>플레이어 사망 시 호출</summary>
    public event Action OnPlayerDied;
    #endregion

    /// <summary>연막탄 Lv5: 회피 시 랜덤 위치로 순간이동 + 무적</summary>
    void TeleportDodge()
    {
        Vector3 randomOffset = new Vector3(
            UnityEngine.Random.Range(-3f, 3f), 0f,
            UnityEngine.Random.Range(-3f, 3f));
        transform.position += randomOffset;
        _lastHitTime = Time.time; // 무적 시간 적용
    }

    #region Flash
    IEnumerator FlashRoutine()
    {
        float elapsed = 0f;
        float interval = 0.1f;  // 깜빡임 간격

        while (elapsed < _invincibleDuration)
        {
            SetRenderersVisible(false);
            yield return new WaitForSeconds(interval);
            SetRenderersVisible(true);
            yield return new WaitForSeconds(interval);
            elapsed += interval * 2f;
        }

        SetRenderersVisible(true);
    }

    void SetRenderersVisible(bool visible)
    {
        for (int i = 0; i < _renderers.Length; i++)
            _renderers[i].enabled = visible && _rendererDefaultState[i];
    }
    #endregion

    #region Death
    void Die()
    {
        gameObject.SetActive(false);
        OnPlayerDied?.Invoke();
    }
    #endregion
}
