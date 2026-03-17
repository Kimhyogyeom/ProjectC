using UnityEngine;

/// <summary>
/// 수리검 회전 스킬
/// - 플레이어 주변을 도는 수리검 생성
/// - Lv5 유니크: 관통 + 크기 2배
/// </summary>
public class SpinningStarSkill : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] GameObject _starPrefab;    // 수리검 프리팹 (Inspector에서 연결)
    [SerializeField] float _orbitRadius = 1.2f;
    [SerializeField] float _rotationSpeed = 180f;
    [SerializeField] int _damagePerHit = 5;
    #endregion

    #region Private Fields
    GameObject[] _stars;
    float _currentAngle = 0f;
    bool _isPierce = false;
    float _sizeMultiplier = 1f;
    #endregion

    #region Unity Lifecycle
    void Update()
    {
        if (_stars == null) return;

        // 수리검 회전
        _currentAngle += _rotationSpeed * Time.deltaTime;

        for (int i = 0; i < _stars.Length; i++)
        {
            if (_stars[i] == null) continue;

            float angle = _currentAngle + (360f / _stars.Length) * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * _orbitRadius;
            _stars[i].transform.position = transform.position + offset;
            _stars[i].transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
        }
    }
    #endregion

    #region Public API
    /// <summary>수리검 수/옵션 설정</summary>
    public void SetStar(int count, bool isPierce, float sizeMultiplier)
    {
        _isPierce = isPierce;
        _sizeMultiplier = sizeMultiplier;

        // 기존 수리검 제거
        if (_stars != null)
            foreach (GameObject star in _stars)
                if (star != null) Destroy(star);

        if (_starPrefab == null)
        {
            // 프리팹 없으면 기본 큐브로 대체
            CreateDefaultStars(count);
            return;
        }

        _stars = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            _stars[i] = Instantiate(_starPrefab, transform.position, Quaternion.identity);
            _stars[i].transform.localScale = Vector3.one * sizeMultiplier;

            // 충돌 처리
            SpinningStarHitbox hitbox = _stars[i].AddComponent<SpinningStarHitbox>();
            hitbox.Init(_damagePerHit, _isPierce);
        }
    }
    #endregion

    #region Helpers
    void CreateDefaultStars(int count)
    {
        _stars = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            _stars[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _stars[i].transform.localScale = Vector3.one * 0.3f * _sizeMultiplier;

            SpinningStarHitbox hitbox = _stars[i].AddComponent<SpinningStarHitbox>();
            hitbox.Init(_damagePerHit, _isPierce);

            // 충돌체를 트리거로 변경
            Collider col = _stars[i].GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }
    }
    #endregion
}
