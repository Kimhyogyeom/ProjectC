using UnityEngine;

/// <summary>
/// 분신술 스킬
/// - 플레이어 주변에 분신 배치
/// - 분신이 플레이어와 같은 방향으로 공격
/// </summary>
public class ShadowCloneSkill : MonoBehaviour
{
    #region Private Fields
    GameObject[] _clones;
    float _damageRatio = 0.5f;
    bool _isPierce = false;
    #endregion

    #region Public API
    public float DamageRatio => _damageRatio;
    public bool IsPierce => _isPierce;

    /// <summary>분신 수/데미지 설정</summary>
    public void SetClone(int count, float damageRatio, bool isPierce)
    {
        _damageRatio = damageRatio;
        _isPierce = isPierce;

        // 기존 분신 제거
        if (_clones != null)
            foreach (GameObject clone in _clones)
                if (clone != null) Destroy(clone);

        // 새 분신 생성 (플레이어 옆에 배치)
        _clones = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i + 90f;
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.right * 1.5f;

            GameObject clone = new GameObject($"Clone_{i}");
            clone.transform.SetParent(transform);
            clone.transform.localPosition = offset;

            // 분신 공격 컴포넌트 추가
            CloneAttack cloneAttack = clone.AddComponent<CloneAttack>();
            cloneAttack.Init(this);
        }
    }
    #endregion
}
