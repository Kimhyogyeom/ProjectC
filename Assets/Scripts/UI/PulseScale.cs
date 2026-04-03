using UnityEngine;

/// <summary>
/// 오브젝트를 두웅두웅 맥박처럼 스케일 애니메이션
/// </summary>
public class PulseScale : MonoBehaviour
{
    [SerializeField] float _maxScale = 1.1f;
    [SerializeField] float _speed = 2f;

    Vector3 _originalScale;

    void Awake()
    {
        _originalScale = transform.localScale;
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * _speed, 1f);
        float ease = Mathf.Sin(t * Mathf.PI * 0.5f);
        float scale = Mathf.Lerp(1f, _maxScale, ease);
        transform.localScale = _originalScale * scale;
    }
}
