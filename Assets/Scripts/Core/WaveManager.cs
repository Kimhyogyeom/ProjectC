using System.Collections;
using UnityEngine;

/// <summary>
/// 웨이브 스폰 매니저
/// - 웨이브마다 적 N마리 스폰
/// - 모두 처치 시 다음 웨이브로 진행
/// - 웨이브마다 적 수 증가
/// </summary>
public class WaveManager : MonoBehaviour
{
    #region Serialized Fields
    [Header("Wave Settings")]
    [SerializeField] int _startEnemyCount = 3;       // 1웨이브 적 수
    [SerializeField] int _enemyIncreasePerWave = 2;  // 웨이브마다 증가하는 적 수
    [SerializeField] float _waveCooldown = 3f;        // 웨이브 사이 대기 시간

    [Header("Spawn Settings")]
    [SerializeField] GameObject _enemyPrefab;         // 적 프리팹
    [SerializeField] float _spawnRadius = 10f;        // 플레이어 기준 스폰 반경
    [SerializeField] Transform _player;               // 플레이어 트랜스폼
    #endregion

    #region Private Fields
    int _currentWave = 0;
    int _aliveEnemyCount = 0;
    bool _isWaveActive = false;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        StartNextWave();
    }
    #endregion

    #region Wave Control
    void StartNextWave()
    {
        _currentWave++;
        int spawnCount = _startEnemyCount + (_currentWave - 1) * _enemyIncreasePerWave;

        Debug.Log($"[Wave] 웨이브 {_currentWave} 시작 - 적 {spawnCount}마리");

        _aliveEnemyCount = spawnCount;
        _isWaveActive = true;

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnEnemy();
        }
    }

    /// <summary>적 처치 시 Enemy에서 호출</summary>
    public void OnEnemyDied()
    {
        _aliveEnemyCount--;

        if (_aliveEnemyCount <= 0 && _isWaveActive)
        {
            _isWaveActive = false;
            StartCoroutine(NextWaveRoutine());
        }
    }
    #endregion

    #region Spawn
    void SpawnEnemy()
    {
        // 플레이어 주변 랜덤 위치에 스폰
        Vector2 randomCircle = Random.insideUnitCircle.normalized * _spawnRadius;
        Vector3 spawnPosition = _player.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
    }
    #endregion

    #region Routines
    IEnumerator NextWaveRoutine()
    {
        Debug.Log($"[Wave] 웨이브 {_currentWave} 클리어! {_waveCooldown}초 후 다음 웨이브");
        yield return new WaitForSeconds(_waveCooldown);
        StartNextWave();
    }
    #endregion

    #region Public API
    public int CurrentWave => _currentWave;
    #endregion
}
