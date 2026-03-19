using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 보스 HP바 (화면 상단)
/// - 씬 캔버스에 직접 배치, Inspector에서 연결
/// - 보스 스폰 시 표시, 사망 시 숨김
/// </summary>
public class BossHPBar : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] GameObject _panel;
    [SerializeField] Image _fillImage;
    [SerializeField] TMP_Text _nameText;
    #endregion

    #region Singleton
    static BossHPBar _instance;

    void Awake()
    {
        _instance = this;
        if (_panel != null)
            _panel.SetActive(false);
    }
    #endregion

    #region Public API
    public static void Show(string bossName)
    {
        if (_instance == null) return;
        _instance._nameText.text = bossName;
        _instance._fillImage.fillAmount = 1f;
        _instance._panel.SetActive(true);
    }

    public static void UpdateHP(float ratio)
    {
        if (_instance == null || _instance._fillImage == null) return;
        _instance._fillImage.fillAmount = Mathf.Clamp01(ratio);
    }

    public static void Hide()
    {
        if (_instance != null && _instance._panel != null)
            _instance._panel.SetActive(false);
    }
    #endregion
}
