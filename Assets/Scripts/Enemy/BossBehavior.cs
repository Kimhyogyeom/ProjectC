using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 보스 전용 패턴
/// - 돌진: 전조(빨간 깜빡임 + 뒤로 빠짐) → 플레이어 방향 일직선 돌진
/// - 충격파: 전조(바닥 빨간 원) → 넓은 범위 폭발 데미지
/// </summary>
public class BossBehavior : MonoBehaviour
{
    #region Settings
    float _chargeCooldown = 6f;
    float _chargeSpeed = 10f;
    float _chargeDuration = 0.8f;
    int _chargeDamage = 25;
    float _chargeHitRadius = 2.5f;

    float _shockwaveCooldown = 9f;
    float _shockwaveRadius = 7f;
    int _shockwaveDamage = 15;

    float _meteorCooldown = 12f;
    int _meteorCount = 5;
    float _meteorRadius = 1.5f;
    float _meteorSpread = 5f;
    int _meteorDamage = 20;
    #endregion

    #region Private Fields
    Enemy _enemy;
    NavMeshAgent _agent;
    Transform _player;
    float _chargeTimer;
    float _shockwaveTimer;
    float _meteorTimer;
    bool _isActing = false;
    float _originalSpeed;
    Renderer[] _renderers;
    MaterialPropertyBlock _propBlock;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        _enemy = GetComponent<Enemy>();
        _agent = GetComponent<NavMeshAgent>();
        _player = _enemy.Player;
        _renderers = GetComponentsInChildren<Renderer>();
        _propBlock = new MaterialPropertyBlock();

        _chargeTimer = 3f;
        _shockwaveTimer = 6f;
        _meteorTimer = 8f;
    }

    void LateUpdate()
    {
        if (_originalSpeed <= 0f && _agent != null && _agent.speed > 0f)
            _originalSpeed = _agent.speed;
    }

    void Update()
    {
        if (_enemy.IsDead || _player == null || _isActing) return;
        if (_agent == null || !_agent.isOnNavMesh) return;

        _chargeTimer -= Time.deltaTime;
        _shockwaveTimer -= Time.deltaTime;
        _meteorTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, _player.position);

        // 돌진 (거리 10 이내)
        if (_chargeTimer <= 0f && dist < 10f)
        {
            StartCoroutine(ChargeRoutine());
            _chargeTimer = _chargeCooldown;
            return;
        }

        // 충격파 (거리 7 이내)
        if (_shockwaveTimer <= 0f && dist < 7f)
        {
            StartCoroutine(ShockwaveRoutine());
            _shockwaveTimer = _shockwaveCooldown;
            return;
        }

        // 바닥 폭격 (거리 무관, 원거리에서도 사용)
        if (_meteorTimer <= 0f)
        {
            StartCoroutine(MeteorRoutine());
            _meteorTimer = _meteorCooldown;
        }
    }
    #endregion

    #region Charge Pattern
    IEnumerator ChargeRoutine()
    {
        _isActing = true;
        if (!_agent.isOnNavMesh) { _isActing = false; yield break; }

        // --- 전조 1: 멈추고 플레이어 바라보기 ---
        _agent.isStopped = true;
        Vector3 toPlayer = (_player.position - transform.position).normalized;
        toPlayer.y = 0f;
        transform.rotation = Quaternion.LookRotation(toPlayer);

        // --- 전조 2: 빨간 깜빡임 3회 (0.6초) ---
        for (int i = 0; i < 3; i++)
        {
            SetBodyColor(new Color(1f, 0.2f, 0.2f));
            yield return new WaitForSeconds(0.1f);
            ClearBodyColor();
            yield return new WaitForSeconds(0.1f);
        }

        // --- 전조 3: 살짝 뒤로 빠짐 (웅크리기 느낌) ---
        Vector3 backDir = -toPlayer;
        float backElapsed = 0f;
        float backDuration = 0.25f;

        while (backElapsed < backDuration)
        {
            transform.position += backDir * 3f * Time.deltaTime;
            backElapsed += Time.deltaTime;
            yield return null;
        }

        // --- 돌진: 플레이어 방향 일직선 (NavMesh 끄고 직접 이동) ---
        // 돌진 방향 다시 계산 (뒤로 빠진 후)
        Vector3 chargeDir = (_player.position - transform.position).normalized;
        chargeDir.y = 0f;
        transform.rotation = Quaternion.LookRotation(chargeDir);

        _agent.enabled = false;  // NavMesh 끄고 직선 이동
        SetBodyColor(new Color(1f, 0.4f, 0.1f));

        float elapsed = 0f;
        bool hasHit = false;

        while (elapsed < _chargeDuration)
        {
            if (_enemy.IsDead) break;

            // 직선 이동
            transform.position += chargeDir * _chargeSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;

            // 플레이어 충돌 체크
            if (!hasHit)
            {
                float dist = Vector3.Distance(transform.position, _player.position);
                if (dist < _chargeHitRadius)
                {
                    PlayerHealth ph = _player.GetComponent<PlayerHealth>();
                    if (ph != null) ph.TakeDamage(_chargeDamage);
                    hasHit = true;
                }
            }

            yield return null;
        }

        // --- 돌진 종료: NavMesh 복귀 ---
        ClearBodyColor();
        _agent.enabled = true;

        // NavMesh 위로 복귀 시도
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            transform.position = hit.position;

        if (_agent.isOnNavMesh)
        {
            _agent.speed = _originalSpeed;
            _agent.isStopped = false;
        }

        // 돌진 후 잠깐 경직
        yield return new WaitForSeconds(0.5f);
        _isActing = false;
    }
    #endregion

    #region Shockwave Pattern
    IEnumerator ShockwaveRoutine()
    {
        _isActing = true;
        if (!_agent.isOnNavMesh) { _isActing = false; yield break; }

        _agent.isStopped = true;

        // --- 전조 1: 바닥에 빨간 경고 원 표시 ---
        GameObject warningObj = CreateWarningCircle();
        Vector3 warningPos = transform.position + Vector3.up * 0.05f;
        warningObj.transform.position = warningPos;

        // 원이 점점 커지면서 경고 (1.2초)
        float warningElapsed = 0f;
        float warningDuration = 1.2f;
        Renderer warningRenderer = warningObj.GetComponent<Renderer>();

        while (warningElapsed < warningDuration)
        {
            warningElapsed += Time.deltaTime;
            float t = warningElapsed / warningDuration;

            // 원 크기 확장
            float scale = Mathf.Lerp(0f, _shockwaveRadius * 2f, t);
            warningObj.transform.localScale = new Vector3(scale, 0.01f, scale);

            // 깜빡이는 알파 (점점 빨라짐)
            float blinkSpeed = Mathf.Lerp(2f, 12f, t);
            float alpha = 0.15f + 0.15f * Mathf.Sin(warningElapsed * blinkSpeed);
            if (warningRenderer != null)
            {
                MaterialPropertyBlock pb = new MaterialPropertyBlock();
                pb.SetColor("_BaseColor", new Color(1f, 0.1f, 0.1f, alpha));
                warningRenderer.SetPropertyBlock(pb);
            }

            yield return null;
        }

        // --- 폭발: 범위 데미지 ---
        Collider[] hits = Physics.OverlapSphere(transform.position, _shockwaveRadius);
        foreach (Collider col in hits)
        {
            if (!col.CompareTag("Player")) continue;
            PlayerHealth ph = col.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(_shockwaveDamage);
        }

        // --- 폭발 이펙트: 경고 원이 번쩍이고 사라짐 ---
        if (warningRenderer != null)
        {
            MaterialPropertyBlock pb = new MaterialPropertyBlock();
            pb.SetColor("_BaseColor", new Color(1f, 0.3f, 0.1f, 0.6f));
            warningRenderer.SetPropertyBlock(pb);
        }
        warningObj.transform.localScale = new Vector3(_shockwaveRadius * 2f, 0.1f, _shockwaveRadius * 2f);

        yield return new WaitForSeconds(0.15f);

        // 페이드 아웃
        float fadeElapsed = 0f;
        float fadeDuration = 0.3f;
        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            float alpha = 0.6f * (1f - fadeElapsed / fadeDuration);
            if (warningRenderer != null)
            {
                MaterialPropertyBlock pb = new MaterialPropertyBlock();
                pb.SetColor("_BaseColor", new Color(1f, 0.3f, 0.1f, alpha));
                warningRenderer.SetPropertyBlock(pb);
            }
            yield return null;
        }

        Destroy(warningObj);

        if (_agent.isOnNavMesh)
            _agent.isStopped = false;

        // 충격파 후 경직
        yield return new WaitForSeconds(0.3f);
        _isActing = false;
    }

    GameObject CreateWarningCircle()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        obj.name = "BossWarning";
        obj.transform.localScale = Vector3.zero;

        // 콜라이더 제거
        Collider col = obj.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // 반투명 머티리얼 설정
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            mat.SetFloat("_Surface", 1);
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.color = new Color(1f, 0.1f, 0.1f, 0f);
        }

        return obj;
    }
    #endregion

    #region Meteor Pattern
    IEnumerator MeteorRoutine()
    {
        _isActing = true;
        if (_agent.isOnNavMesh) _agent.isStopped = true;

        // 보스 경고 표시
        DamagePopup.Create(transform.position + Vector3.up * 3f, "!!!", new Color(1f, 0f, 0f), 9f);
        yield return new WaitForSeconds(0.3f);

        // 플레이어 주변에 랜덤 위치로 경고 원 N개 생성
        Vector3 playerPos = _player.position;
        GameObject[] warnings = new GameObject[_meteorCount];
        Vector3[] positions = new Vector3[_meteorCount];

        for (int i = 0; i < _meteorCount; i++)
        {
            // 플레이어 주변 랜덤 위치 (첫 번째는 플레이어 바로 밑)
            Vector2 randOffset = Random.insideUnitCircle * _meteorSpread;
            if (i == 0) randOffset = Vector2.zero;  // 첫 발은 플레이어 정확히
            positions[i] = playerPos + new Vector3(randOffset.x, 0.05f, randOffset.y);

            warnings[i] = CreateWarningCircle();
            warnings[i].transform.position = positions[i];
            warnings[i].transform.localScale = new Vector3(_meteorRadius * 2f, 0.01f, _meteorRadius * 2f);
        }

        // 경고 원 깜빡임 (1.5초 — 피할 시간)
        float warningElapsed = 0f;
        float warningDuration = 1.5f;

        while (warningElapsed < warningDuration)
        {
            warningElapsed += Time.deltaTime;
            float t = warningElapsed / warningDuration;
            float blinkSpeed = Mathf.Lerp(3f, 15f, t);
            float alpha = 0.1f + 0.2f * Mathf.Sin(warningElapsed * blinkSpeed);

            // 후반에 더 진하게
            alpha += t * 0.15f;

            for (int i = 0; i < _meteorCount; i++)
            {
                if (warnings[i] == null) continue;
                Renderer r = warnings[i].GetComponent<Renderer>();
                if (r == null) continue;
                MaterialPropertyBlock pb = new MaterialPropertyBlock();
                pb.SetColor("_BaseColor", new Color(1f, 0.1f, 0.1f, alpha));
                r.SetPropertyBlock(pb);
            }

            yield return null;
        }

        // 순차적으로 폭발 (0.15초 간격)
        for (int i = 0; i < _meteorCount; i++)
        {
            // 해당 위치 범위 데미지
            Collider[] hits = Physics.OverlapSphere(positions[i], _meteorRadius);
            foreach (Collider col in hits)
            {
                if (!col.CompareTag("Player")) continue;
                PlayerHealth ph = col.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(_meteorDamage);
            }

            // 폭발 이펙트: 번쩍
            if (warnings[i] != null)
            {
                Renderer r = warnings[i].GetComponent<Renderer>();
                if (r != null)
                {
                    MaterialPropertyBlock pb = new MaterialPropertyBlock();
                    pb.SetColor("_BaseColor", new Color(1f, 0.4f, 0.1f, 0.7f));
                    r.SetPropertyBlock(pb);
                }
                warnings[i].transform.localScale = new Vector3(_meteorRadius * 2.5f, 0.15f, _meteorRadius * 2.5f);
            }

            yield return new WaitForSeconds(0.15f);
        }

        // 전체 페이드 아웃
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < _meteorCount; i++)
            if (warnings[i] != null) Destroy(warnings[i]);

        if (_agent.isOnNavMesh)
            _agent.isStopped = false;

        yield return new WaitForSeconds(0.3f);
        _isActing = false;
    }
    #endregion

    #region Visual Helpers
    void SetBodyColor(Color color)
    {
        _propBlock.SetColor("_BaseColor", color);
        foreach (Renderer r in _renderers)
            r.SetPropertyBlock(_propBlock);
    }

    void ClearBodyColor()
    {
        _propBlock.Clear();
        foreach (Renderer r in _renderers)
            r.SetPropertyBlock(_propBlock);
    }
    #endregion

    #region Public API
    public void ApplyWaveDifficulty(int wave)
    {
        float multiplier = 1f + (wave / 5 - 1) * 0.3f;
        _chargeDamage = Mathf.RoundToInt(_chargeDamage * multiplier);
        _shockwaveDamage = Mathf.RoundToInt(_shockwaveDamage * multiplier);
        _shockwaveRadius += (wave / 5 - 1) * 0.5f;
        _meteorDamage = Mathf.RoundToInt(_meteorDamage * multiplier);
        _meteorCount += (wave / 5 - 1);  // 보스 등장마다 폭격 +1개
    }
    #endregion
}
