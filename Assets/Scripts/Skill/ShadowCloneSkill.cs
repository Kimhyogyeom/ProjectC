using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 분신술 스킬
/// - 플레이어 모델을 복제한 검은 그림자 분신
/// - 플레이어 뒤를 딜레이로 따라다님
/// - 공격도 딜레이 후 동일 동작 수행
/// </summary>
public class ShadowCloneSkill : MonoBehaviour
{
    #region Serialized Fields
    [Header("Shadow Clone")]
    [SerializeField] float _followDelay = 0.15f;      // 따라가기 딜레이 (초)
    [SerializeField] float _followDistance = 1.2f;     // 분신 간 간격
    [SerializeField] Color _shadowColor = new Color(0.05f, 0.05f, 0.1f, 0.6f);
    #endregion

    #region Private Fields
    List<GameObject> _clones = new List<GameObject>();
    List<Animator> _cloneAnimators = new List<Animator>();

    // 위치/회전 기록 버퍼
    List<PositionRecord> _positionHistory = new List<PositionRecord>();
    float _recordInterval = 0.02f;
    float _lastRecordTime;

    float _damageRatio = 0.5f;
    bool _isPierce = false;
    #endregion

    #region Structs
    struct PositionRecord
    {
        public float time;
        public Vector3 position;
        public Quaternion rotation;
    }
    #endregion

    #region Public API
    public float DamageRatio => _damageRatio;
    public bool IsPierce => _isPierce;

    /// <summary>분신들의 CloneAttack 컴포넌트 반환</summary>
    public CloneAttack[] GetCloneAttacks()
    {
        List<CloneAttack> attacks = new List<CloneAttack>();
        foreach (GameObject clone in _clones)
        {
            if (clone == null) continue;
            CloneAttack ca = clone.GetComponent<CloneAttack>();
            if (ca != null) attacks.Add(ca);
        }
        return attacks.ToArray();
    }

    /// <summary>분신 수/데미지 설정</summary>
    public void SetClone(int count, float damageRatio, bool isPierce)
    {
        _damageRatio = damageRatio;
        _isPierce = isPierce;

        // 기존 분신 제거
        foreach (GameObject clone in _clones)
            if (clone != null) Destroy(clone);
        _clones.Clear();
        _cloneAnimators.Clear();
        _positionHistory.Clear();

        // 플레이어 모델 찾기 (Animator가 있는 자식)
        Animator playerAnimator = GetComponentInChildren<Animator>();
        if (playerAnimator == null) return;
        GameObject playerModel = playerAnimator.gameObject;

        // 분신 생성
        for (int i = 0; i < count; i++)
        {
            GameObject clone = Instantiate(playerModel);
            clone.name = $"ShadowClone_{i}";
            clone.transform.position = transform.position;
            clone.transform.rotation = transform.rotation;

            // 불필요한 컴포넌트 제거 (렌더러와 애니메이터만 남김)
            RemoveUnnecessaryComponents(clone);

            // 검은 그림자 머티리얼 적용
            ApplyShadowMaterial(clone);

            // 애니메이션 이벤트 수신용 더미 추가 (FootL/FootR 경고 방지)
            clone.AddComponent<AnimationEventReceiver>();

            // CloneAttack 추가
            CloneAttack cloneAttack = clone.AddComponent<CloneAttack>();
            cloneAttack.Init(this);

            Animator cloneAnimator = clone.GetComponent<Animator>();
            if (cloneAnimator == null)
                cloneAnimator = clone.GetComponentInChildren<Animator>();

            _clones.Add(clone);
            _cloneAnimators.Add(cloneAnimator);
        }
    }

    /// <summary>분신 공격 애니메이션 재생 (PlayerAttack에서 호출)</summary>
    public void PlayAttackAnimation(int actionIndex)
    {
        foreach (Animator anim in _cloneAnimators)
        {
            if (anim == null) continue;
            anim.SetInteger("Weapon", 0);
            anim.SetInteger("Jumping", 0);
            anim.SetInteger("TriggerNumber", 4);
            anim.SetInteger("Action", actionIndex);
            anim.SetTrigger("Trigger");
        }
    }
    #endregion

    #region Unity Lifecycle
    void Update()
    {
        if (_clones.Count == 0) return;

        // 플레이어 위치/회전 기록
        if (Time.time - _lastRecordTime >= _recordInterval)
        {
            _positionHistory.Add(new PositionRecord
            {
                time = Time.time,
                position = transform.position,
                rotation = transform.rotation
            });
            _lastRecordTime = Time.time;

            // 오래된 기록 정리 (최대 딜레이의 2배까지만 보관)
            float maxHistory = _followDelay * (_clones.Count + 1) * 2f;
            while (_positionHistory.Count > 0 && Time.time - _positionHistory[0].time > maxHistory)
                _positionHistory.RemoveAt(0);
        }

        // 각 분신을 딜레이된 위치로 이동
        for (int i = 0; i < _clones.Count; i++)
        {
            if (_clones[i] == null) continue;

            float targetTime = Time.time - _followDelay * (i + 1);
            PositionRecord record = GetRecordAtTime(targetTime);

            _clones[i].transform.position = record.position;
            _clones[i].transform.rotation = record.rotation;

            // 이동 애니메이션 동기화
            if (_cloneAnimators[i] != null)
            {
                Animator playerAnim = GetComponentInChildren<Animator>();
                if (playerAnim != null)
                {
                    _cloneAnimators[i].SetBool("Moving", playerAnim.GetBool("Moving"));
                    _cloneAnimators[i].SetFloat("Velocity X", playerAnim.GetFloat("Velocity X"));
                    _cloneAnimators[i].SetFloat("Velocity Z", playerAnim.GetFloat("Velocity Z"));
                }
            }
        }
    }
    #endregion

    #region Helpers
    PositionRecord GetRecordAtTime(float targetTime)
    {
        // 기록이 없으면 현재 위치
        if (_positionHistory.Count == 0)
            return new PositionRecord { time = Time.time, position = transform.position, rotation = transform.rotation };

        // 가장 오래된 기록보다 이전이면 가장 오래된 것 반환
        if (targetTime <= _positionHistory[0].time)
            return _positionHistory[0];

        // 보간할 두 기록 찾기
        for (int i = 0; i < _positionHistory.Count - 1; i++)
        {
            if (_positionHistory[i].time <= targetTime && targetTime <= _positionHistory[i + 1].time)
            {
                float t = Mathf.InverseLerp(_positionHistory[i].time, _positionHistory[i + 1].time, targetTime);
                return new PositionRecord
                {
                    time = targetTime,
                    position = Vector3.Lerp(_positionHistory[i].position, _positionHistory[i + 1].position, t),
                    rotation = Quaternion.Slerp(_positionHistory[i].rotation, _positionHistory[i + 1].rotation, t)
                };
            }
        }

        // 가장 최근 기록 반환
        return _positionHistory[_positionHistory.Count - 1];
    }

    void RemoveUnnecessaryComponents(GameObject clone)
    {
        // 물리/충돌 컴포넌트 제거
        foreach (Collider col in clone.GetComponentsInChildren<Collider>())
            Destroy(col);
        foreach (Rigidbody rb in clone.GetComponentsInChildren<Rigidbody>())
            Destroy(rb);
        // CharacterController 제거
        CharacterController cc = clone.GetComponent<CharacterController>();
        if (cc != null) Destroy(cc);
        // 기타 스크립트 제거 (Animator, Transform, Renderer 제외)
        foreach (MonoBehaviour mb in clone.GetComponentsInChildren<MonoBehaviour>())
        {
            if (mb is CloneAttack) continue;
            Destroy(mb);
        }
    }

    void ApplyShadowMaterial(GameObject clone)
    {
        foreach (Renderer renderer in clone.GetComponentsInChildren<Renderer>())
        {
            Material[] shadowMats = new Material[renderer.materials.Length];
            for (int j = 0; j < shadowMats.Length; j++)
            {
                shadowMats[j] = new Material(renderer.materials[j]);
                // 반투명 설정
                shadowMats[j].SetFloat("_Surface", 1); // Transparent
                shadowMats[j].SetFloat("_Blend", 0);
                shadowMats[j].SetOverrideTag("RenderType", "Transparent");
                shadowMats[j].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                shadowMats[j].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                shadowMats[j].SetInt("_ZWrite", 0);
                shadowMats[j].renderQueue = 3000;
                shadowMats[j].EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                shadowMats[j].color = _shadowColor;
            }
            renderer.materials = shadowMats;
        }
    }
    #endregion
}
