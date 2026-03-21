using UnityEngine;

/// <summary>
/// 스테이지 정보 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "StageData", menuName = "ProjectC/StageData")]
public class StageData : ScriptableObject
{
    public string stageName;        // 스테이지 이름 (예: 무한의 탑)
    public string sceneName;        // 로드할 씬 이름 (예: GameScene)
    public Sprite stageImage;       // 스테이지 이미지
    public bool isLocked = true;    // 잠금 여부
}
