using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 강화 버튼 — 클릭 시 EnhanceUI 패널 열기
/// 버튼 오브젝트에 붙이고 Inspector OnClick에서 OnClicked() 연결
/// </summary>
public class EnhanceButton : MonoBehaviour
{
    [SerializeField] EnhanceUI _enhanceUI;

    public void OnClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        if (_enhanceUI != null) _enhanceUI.Open();
    }
}
