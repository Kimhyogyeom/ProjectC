using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 강화 UI 패널 — 로비에서 골드를 소비해 영구 스탯 강화
/// </summary>
public class EnhanceUI : MonoBehaviour
{
    #region Serialized Fields
    [Header("Panel")]
    [SerializeField] GameObject _panel;

    [Header("Stats Display")]
    [SerializeField] TMP_Text _damageText;
    [SerializeField] TMP_Text _hpText;
    [SerializeField] TMP_Text _moveSpeedText;
    [SerializeField] TMP_Text _attackSpeedText;

    [Header("Upgrade Buttons")]
    [SerializeField] Button _damageBtnObj;
    [SerializeField] Button _hpBtnObj;
    [SerializeField] Button _moveSpeedBtnObj;
    [SerializeField] Button _attackSpeedBtnObj;

    [Header("Button Labels")]
    [SerializeField] TMP_Text _damageBtnText;
    [SerializeField] TMP_Text _hpBtnText;
    [SerializeField] TMP_Text _moveSpeedBtnText;
    [SerializeField] TMP_Text _attackSpeedBtnText;

    [Header("Gold")]
    [SerializeField] TMP_Text _goldText;

    [Header("Level Up Effect")]
    [SerializeField] Image _flashImage;
    [SerializeField] float _flashDuration = 0.3f;

    [Header("Close")]
    [SerializeField] Button _closeButton;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        _damageBtnObj.onClick.AddListener(() => OnUpgradeClicked(StatType.Damage));
        _hpBtnObj.onClick.AddListener(() => OnUpgradeClicked(StatType.Hp));
        _moveSpeedBtnObj.onClick.AddListener(() => OnUpgradeClicked(StatType.MoveSpeed));
        _attackSpeedBtnObj.onClick.AddListener(() => OnUpgradeClicked(StatType.AttackSpeed));
        _closeButton.onClick.AddListener(Close);
    }
    #endregion

    #region Public API
    public void Open()
    {
        _panel.SetActive(true);
        RefreshUI();
    }

    public void Close()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();
        _panel.SetActive(false);
    }
    #endregion

    #region Upgrade
    void OnUpgradeClicked(StatType type)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySfxButton();

        if (UpgradeManager.TryUpgrade(type))
        {
            RefreshUI();
            if (_flashImage != null) StartCoroutine(FlashRoutine());
        }
    }
    #endregion

    #region Flash Effect
    System.Collections.IEnumerator FlashRoutine()
    {
        _flashImage.gameObject.SetActive(true);
        Color c = _flashImage.color;
        c.a = 1f;
        _flashImage.color = c;

        float elapsed = 0f;
        while (elapsed < _flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / _flashDuration);
            _flashImage.color = c;
            yield return null;
        }

        _flashImage.gameObject.SetActive(false);
    }
    #endregion

    #region UI Refresh
    void RefreshUI()
    {
        // 스탯 표시
        _damageText.text = $"공격력: {UpgradeManager.GetTotalDamage()}";
        _hpText.text = $"체력: {UpgradeManager.GetTotalHp()}";
        _moveSpeedText.text = $"이동속도: {UpgradeManager.GetTotalMoveSpeed():F1}";
        _attackSpeedText.text = $"공격속도: {UpgradeManager.GetTotalAttackSpeed():F2}s";

        // 버튼 텍스트 + 활성화
        RefreshButton(StatType.Damage, _damageBtnObj, _damageBtnText, "공격력");
        RefreshButton(StatType.Hp, _hpBtnObj, _hpBtnText, "체력");
        RefreshButton(StatType.MoveSpeed, _moveSpeedBtnObj, _moveSpeedBtnText, "이동속도");
        RefreshButton(StatType.AttackSpeed, _attackSpeedBtnObj, _attackSpeedBtnText, "공격속도");

        // 골드
        if (_goldText != null)
            _goldText.text = GoldManager.TotalGold.ToString("N0");
    }

    void RefreshButton(StatType type, Button btn, TMP_Text label, string statName)
    {
        int level = UpgradeManager.GetLevel(type);
        int cost = UpgradeManager.GetUpgradeCost(type);
        label.text = $"{statName} Lv.{level}\n{cost:N0} G";
        btn.interactable = GoldManager.TotalGold >= cost;
    }
    #endregion
}
