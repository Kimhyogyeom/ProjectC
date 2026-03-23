using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 강화 UI 패널
/// - 상단: 플레이어 현재 스탯 표시
/// - 하단: 4개 강화 버튼 (스탯명 + 레벨 + 비용)
///
/// ===== Unity 셋업 가이드 =====
/// 1. LobbyScene → Canvas 아래에 빈 GameObject "EnhancePanel" 생성, 이 스크립트 붙이기
/// 2. EnhancePanel 안에 Panel(배경) 생성 → _panel에 연결
/// 3. 상단 영역: TMP_Text 4개 생성 (공격력/HP/이동속도/공격속도) → _damageText, _hpText, _moveSpeedText, _attackSpeedText에 연결
/// 4. (선택) 상단에 플레이어 3D 모델 표시하고 싶으면 RawImage + RenderTexture 또는 씬에 캐릭터 배치
/// 5. 하단 영역: Button 4개 생성 (각 버튼 안에 TMP_Text 자식)
///    - _damageBtnObj, _hpBtnObj, _moveSpeedBtnObj, _attackSpeedBtnObj → 버튼 연결
///    - _damageBtnText, _hpBtnText, _moveSpeedBtnText, _attackSpeedBtnText → 버튼 안 텍스트 연결
/// 6. 골드 표시: TMP_Text → _goldText에 연결
/// 7. 닫기 버튼: Button → _closeButton에 연결
/// 8. _panel은 기본 비활성화(SetActive false) 상태로 두기
/// 9. LobbyManager Inspector에서 _enhanceUI에 이 오브젝트 드래그 연결
/// =============================
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
            RefreshUI();
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
