using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 사운드 전체 ON/OFF 토글 버튼
/// - 클릭 시 스피커/음소거 이미지 전환
/// - BGM + SFX 동시 제어
/// </summary>
public class SoundToggleButton : MonoBehaviour
{
    [SerializeField] Image _icon;
    [SerializeField] Sprite _speakerSprite;
    [SerializeField] Sprite _muteSprite;

    bool _isMuted = false;

    void Start()
    {
        if (AudioManager.Instance != null)
            _isMuted = !AudioManager.Instance.IsBgmOn;
        UpdateIcon();
    }

    public void OnClicked()
    {
        _isMuted = !_isMuted;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBgm(!_isMuted);
            AudioManager.Instance.SetSfx(!_isMuted);
        }

        UpdateIcon();
    }

    void UpdateIcon()
    {
        if (_icon == null) return;
        _icon.sprite = _isMuted ? _muteSprite : _speakerSprite;
    }
}
