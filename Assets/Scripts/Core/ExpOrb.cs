using UnityEngine;

/// <summary>
/// 경험치 구슬
/// - 몬스터 사망 시 스폰
/// - PlayerLevel이 OverlapSphere로 감지 후 Attract() 호출
/// - 플레이어에게 닿으면 경험치 지급 후 소멸
/// </summary>
public class ExpOrb : MonoBehaviour
{
    #region Serialized Fields
    [Header("Settings")]
    [SerializeField] int _expAmount = 3;        // 지급할 경험치량
    [SerializeField] float _moveSpeed = 5f;     // 흡수 시 이동 속도
    [SerializeField] float _lifetime = 10f;     // 자동 소멸 시간
    [SerializeField] float _collectDistance = 0.3f; // 경험치 지급 거리
    #endregion

    #region Private Fields
    Transform _player;
    PlayerLevel _playerLevel;
    bool _isAttracted = false;
    bool _collected = false;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
            _playerLevel = playerObj.GetComponent<PlayerLevel>();
        }

        Destroy(gameObject, _lifetime);
    }

    void Update()
    {
        if (_player == null || _collected) return;

        if (_isAttracted)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                _player.position,
                _moveSpeed * Time.deltaTime
            );

            // 플레이어에 충분히 가까워지면 경험치 지급
            if (Vector3.Distance(transform.position, _player.position) <= _collectDistance)
                Collect();
        }
    }
    #endregion

    #region Public API
    /// <summary>PlayerLevel에서 호출 - 흡수 시작</summary>
    public void Attract()
    {
        _isAttracted = true;
    }
    #endregion

    #region Collection
    void Collect()
    {
        _collected = true;

        if (_playerLevel != null)
            _playerLevel.GainExp(_expAmount);

        Destroy(gameObject);
    }
    #endregion
}
