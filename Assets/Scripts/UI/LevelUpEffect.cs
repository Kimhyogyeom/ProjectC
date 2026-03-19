using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 레벨업 시각 이펙트
/// - 플레이어 주변에 빛 파티클 + "LEVEL UP!" 텍스트
/// </summary>
public class LevelUpEffect : MonoBehaviour
{
    /// <summary>레벨업 이펙트 생성 (static 팩토리)</summary>
    public static void Create(Vector3 position)
    {
        GameObject effectObj = new GameObject("LevelUpEffect");
        effectObj.transform.position = position + Vector3.up * 1.5f;
        LevelUpEffect effect = effectObj.AddComponent<LevelUpEffect>();
        effect.StartCoroutine(effect.PlayEffect());
    }

    IEnumerator PlayEffect()
    {
        Camera mainCamera = Camera.main;

        // "LEVEL UP!" 텍스트
        GameObject textObj = new GameObject("LevelUpText");
        textObj.transform.SetParent(transform, false);
        TMP_Text tmpText = textObj.AddComponent<TextMeshPro>();
        tmpText.text = "LEVEL UP!";
        tmpText.fontSize = 7f;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.color = new Color(1f, 0.9f, 0.2f); // 골드색

        // 빛 파티클 링
        GameObject lightObj = new GameObject("LevelUpLight");
        lightObj.transform.SetParent(transform, false);
        lightObj.transform.localPosition = Vector3.down * 1.5f;

        ParticleSystem ps = lightObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.6f;
        main.startSpeed = 3f;
        main.startSize = 0.15f;
        main.startColor = new Color(1f, 0.9f, 0.3f, 0.8f);
        main.maxParticles = 30;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 30)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;

        // 텍스트 애니메이션
        float elapsed = 0f;
        float duration = 1.2f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // 위로 떠오름
            transform.position += Vector3.up * 1.5f * Time.unscaledDeltaTime;

            // 빌보드
            if (mainCamera != null)
                transform.forward = mainCamera.transform.forward;

            // 스케일: 커졌다 안착
            float scale;
            if (t < 0.15f)
                scale = Mathf.Lerp(0f, 1.3f, t / 0.15f);
            else if (t < 0.3f)
                scale = Mathf.Lerp(1.3f, 1f, (t - 0.15f) / 0.15f);
            else
                scale = 1f;
            transform.localScale = Vector3.one * scale;

            // 페이드 아웃 (후반)
            if (t > 0.6f)
            {
                float alpha = 1f - ((t - 0.6f) / 0.4f);
                tmpText.color = new Color(1f, 0.9f, 0.2f, alpha);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
