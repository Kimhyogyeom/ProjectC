using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 버튼 클릭 시 스케일 애니메이션
/// - 누를 때 살짝 작아지고, 떼면 원래 크기로
/// </summary>
public class ButtonScaleAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float _pressedScale = 0.9f;
    [SerializeField] float _duration = 0.08f;

    Vector3 _originalScale;
    Coroutine _coroutine;

    void Awake()
    {
        _originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PlayScale(_originalScale * _pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PlayScale(_originalScale);
    }

    void PlayScale(Vector3 target)
    {
        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(ScaleRoutine(target));
    }

    IEnumerator ScaleRoutine(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, target, elapsed / _duration);
            yield return null;
        }

        transform.localScale = target;
    }
}
