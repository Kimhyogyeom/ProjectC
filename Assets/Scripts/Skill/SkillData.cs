using UnityEngine;

/// <summary>
/// 스킬 데이터 ScriptableObject
/// - 각 스킬의 이름, 설명, 레벨별 효과 수치 정의
/// - Assets/ScriptableObjects/Skills 폴더에 생성
/// </summary>
[CreateAssetMenu(fileName = "SkillData", menuName = "ProjectC/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    public SkillType skillType;
    public string skillName;
    public Sprite icon;

    [Header("레벨별 설명 (인덱스 0 = Lv.1)")]
    [TextArea] public string[] levelDescriptions = new string[5];

    [Header("레벨별 수치 (인덱스 0 = Lv.1)")]
    public float[] levelValues = new float[5];  // 스킬별 핵심 수치 (배열로 관리)

    public int MaxLevel => levelDescriptions.Length;

    /// <summary>해당 레벨의 설명 반환 (1-based)</summary>
    public string GetDescription(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, levelDescriptions.Length - 1);
        return levelDescriptions[index];
    }

    /// <summary>해당 레벨의 수치 반환 (1-based)</summary>
    public float GetValue(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, levelValues.Length - 1);
        return levelValues[index];
    }
}
