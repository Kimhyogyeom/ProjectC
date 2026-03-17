using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 골드 흡수 시 UI 골드 아이콘으로 날아가는 코인
/// - 베지어 곡선으로 자연스럽게 날아감
/// - 도착 시 GoldManager에 골드 지급 콜백
/// </summary>
public class GoldCoinFly : MonoBehaviour
{
    #region Private Fields
    RectTransform _target;
    int _goldAmount;
    Action<int> _onArrived;
    #endregion

    #region Public API
    public void Init(RectTransform target, int goldAmount, Action<int> onArrived)
    {
        _target = target;
        _goldAmount = goldAmount;
        _onArrived = onArrived;
        StartCoroutine(FlyRoutine());
    }
    #endregion

    #region Fly
    IEnumerator FlyRoutine()
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 startPos = rect.position;
        float duration = UnityEngine.Random.Range(0.4f, 0.7f);
        float elapsed = 0f;

        // 베지어 중간점: 살짝 위쪽으로 튀어오름
        Vector3 midOffset = new Vector3(
            UnityEngine.Random.Range(-60f, 60f),
            UnityEngine.Random.Range(40f, 120f), 0f);
        Vector3 mid = Vector3.Lerp(startPos, _target.position, 0.5f) + midOffset;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            // 2차 베지어
            Vector3 p0 = Vector3.Lerp(startPos, mid, t);
            Vector3 p1 = Vector3.Lerp(mid, _target.position, t);
            rect.position = Vector3.Lerp(p0, p1, t);

            // 날아가면서 살짝 줄어들기
            float scale = Mathf.Lerp(1f, 0.4f, t);
            rect.localScale = Vector3.one * scale;

            yield return null;
        }

        rect.position = _target.position;
        _onArrived?.Invoke(_goldAmount);
        Destroy(gameObject);
    }
    #endregion
}
