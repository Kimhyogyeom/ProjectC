using TMPro;
using UnityEngine;

/// <summary>
/// DamagePopup 폰트 설정
/// - 씬의 아무 오브젝트에 붙여서 Inspector에서 폰트 지정
/// </summary>
public class DamagePopupSettings : MonoBehaviour
{
    [SerializeField] TMP_FontAsset _font;

    void Awake()
    {
        if (_font != null)
            DamagePopup.SharedFont = _font;
    }
}
