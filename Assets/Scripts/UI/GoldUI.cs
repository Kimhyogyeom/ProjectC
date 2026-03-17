using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 골드 UI 표시
/// - GoldManager.OnGoldChanged 구독
/// - 골드 변경 시 숫자 카운트업 애니메이션
/// </summary>
public class GoldUI : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] TMP_Text _goldText;
    #endregion

    #region Private Fields
    int _displayedGold = 0;
    int _targetGold = 0;
    Coroutine _countCoroutine;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        // GoldManager보다 늦게 실행될 수 있어서 1프레임 뒤에 구독
        StartCoroutine(Subscribe());
    }

    System.Collections.IEnumerator Subscribe()
    {
        yield return null;
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged += OnGoldChanged;
            UpdateText(0);
        }
    }

    void OnDestroy()
    {
        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldChanged -= OnGoldChanged;
    }
    #endregion

    #region Gold Display
    void OnGoldChanged(int newGold)
    {
        _targetGold = newGold;
        if (_countCoroutine != null) StopCoroutine(_countCoroutine);
        _countCoroutine = StartCoroutine(CountUp());
    }

    IEnumerator CountUp()
    {
        float duration = 0.4f;
        float elapsed = 0f;
        int startGold = _displayedGold;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _displayedGold = Mathf.RoundToInt(Mathf.Lerp(startGold, _targetGold, elapsed / duration));
            UpdateText(_displayedGold);
            yield return null;
        }

        _displayedGold = _targetGold;
        UpdateText(_displayedGold);
    }

    void UpdateText(int gold)
    {
        if (_goldText != null)
            _goldText.text = gold.ToString("N0");
    }
    #endregion
}
