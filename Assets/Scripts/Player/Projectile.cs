using System.Collections;
using UnityEngine;

/// <summary>
/// 표창 투사체
/// - 직선으로 날아가서 적에게 충돌 시 데미지
/// - 독, 관통, 데미지 배율 지원
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
    float _damageRatio = 1f;    // 분신 데미지 배율
    bool _initialized = false;
    bool _isPierce = false;     // 관통 여부
    bool _hasPoison = false;    // 독 여부
    float _poisonDamage = 0f;
    float _poisonDuration = 0f;
    bool _isPoisonSpreading = false;    // Lv5 전염

    bool _hit = false;
    #endregion

    #region Unity Lifecycle
    void Update()
    {
        transform.Translate(_direction * _speed * Time.deltaTime, Space.World);
    }
    #endregion

    #region Collision
    void OnTriggerEnter(Collider other)
    {
        if (_hit && !_isPierce) return;
        if (!other.CompareTag("Enemy")) return;

        if (!_isPierce) _hit = true;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            int finalDamage = Mathf.RoundToInt(_damage * _damageRatio);
            enemy.TakeDamage(finalDamage);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxHit();

            // 독 적용
            if (_hasPoison)
                enemy.ApplyPoison(_poisonDamage, _poisonDuration, _isPoisonSpreading);
        }

        if (!_isPierce)
            Destroy(gameObject);
    }
    #endregion

    #region Public API
    /// <summary>기본 초기화</summary>
    public void Init(Vector3 direction, float speed, float damageRatio = 1f, bool isPierce = false)
    {
        _direction = direction;
        _speed = speed;
        _damageRatio = damageRatio;
        _isPierce = isPierce;

        if (!_initialized)
        {
            _damage += UpgradeManager.GetBonusDamage();
            _initialized = true;
        }

        Destroy(gameObject, _lifeTime);
    }

    /// <summary>독 설정</summary>
    public void SetPoison(float damage, float duration, bool isSpreading)
    {
        _hasPoison = true;
        _poisonDamage = damage;
        _poisonDuration = duration;
        _isPoisonSpreading = isSpreading;
    }
    #endregion
}
