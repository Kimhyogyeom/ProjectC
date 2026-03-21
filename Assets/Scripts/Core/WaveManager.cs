using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

    [Header("Boss Settings")]
    [SerializeField] int _bossWaveInterval = 5;       // N웨이브마다 보스 등장
    [SerializeField] string _bossName = "Dungeon Guardian";
    [SerializeField] bool _debugBossFirstWave = false; // 테스트용: 1웨이브에 보스 즉시 등장

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
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBgmGame();
        StartNextWave();
    }
    #endregion

    #region Wave Control
    void StartNextWave()
    {
        _currentWave++;
        bool isBossWave = _currentWave % _bossWaveInterval == 0 || (_debugBossFirstWave && _currentWave == 1);

        if (isBossWave)
        {
            Debug.Log($"[Wave] 웨이브 {_currentWave} - 보스 등장!");
            OnWaveStarted?.Invoke(_currentWave);

            _aliveEnemyCount = 1;
            _isWaveActive = true;

            SpawnBoss();
        }
        else
        {
            int spawnCount = _startEnemyCount + (_currentWave - 1) * _enemyIncreasePerWave;

            Debug.Log($"[Wave] 웨이브 {_currentWave} 시작 - 적 {spawnCount}마리");
            OnWaveStarted?.Invoke(_currentWave);

            _aliveEnemyCount = spawnCount;
            _isWaveActive = true;

            for (int i = 0; i < spawnCount; i++)
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
            OnWaveCleared?.Invoke();
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxWaveClear();
            StartCoroutine(NextWaveRoutine());
        }
    }
    #endregion

    #region Spawn
    void SpawnEnemy()
    {
        Vector3 spawnPosition = GetValidSpawnPosition();
        GameObject enemyObj = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);

        // 웨이브 난이도 적용
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
            enemy.ApplyWaveDifficulty(_currentWave);
    }

    void SpawnBoss()
    {
        Vector3 spawnPosition = GetValidSpawnPosition();
        GameObject bossObj = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);

        Enemy enemy = bossObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            // 웨이브 난이도 먼저 적용, 그 위에 보스 배율
            enemy.ApplyWaveDifficulty(_currentWave);

            // 보스 HP바 연동
            BossHPBar.Show(_bossName);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxBoss();
            enemy.SetAsBoss(_currentWave, (ratio) =>
            {
                BossHPBar.UpdateHP(ratio);
                if (ratio <= 0f)
                    BossHPBar.Hide();
            });

            // 보스 패턴 추가
            BossBehavior bossBehavior = bossObj.AddComponent<BossBehavior>();
            bossBehavior.ApplyWaveDifficulty(_currentWave);
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        // 최대 10번 시도, NavMesh 위 유효한 위치 찾기
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * _spawnRadius;
            Vector3 candidate = _player.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                return hit.position;
        }

        // 10번 다 실패하면 플레이어 위치 근처에서 NavMesh 탐색
        if (NavMesh.SamplePosition(_player.position, out NavMeshHit fallback, _spawnRadius, NavMesh.AllAreas))
            return fallback.position;

        return _player.position;
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

    /// <summary>해당 웨이브가 보스 웨이브인지 확인</summary>
    public bool IsBossWave(int wave) => wave % _bossWaveInterval == 0 || (_debugBossFirstWave && wave == 1);

    /// <summary>웨이브 시작 시 호출 (wave 번호 전달)</summary>
    public event Action<int> OnWaveStarted;

    /// <summary>웨이브 클리어 시 호출</summary>
    public event Action OnWaveCleared;
    #endregion
}
