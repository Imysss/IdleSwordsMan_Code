using UnityEngine;
using Data;
using static Define;

public static class SkillEffectPlayer
{
    // SkillData 기반으로 재생할 수 있는 오버로드
    public static void Play(SkillData data, Vector3 position)
    {
        if (string.IsNullOrEmpty(data.effectName)) return;

        GameObject prefab = Managers.Resource.Load<GameObject>(data.effectName);
        if (prefab == null) return;

        GameObject go = Managers.Pool.Pop(prefab);
        go.transform.position = position;

        // 지속형 스킬이면 duration 사용, 나머지는 1초
        float duration = data.executeType switch
        {
            SkillExecuteType.Buff => data.duration,
            SkillExecuteType.AreaRepeat => data.duration,
            _ => 1f
        };

        if (go.TryGetComponent(out SkillEffectController ctrl))
            ctrl.Setup(duration);
    }

    // 기존 방식도 유지 (직접 duration 지정)
    public static void Play(string effectName, Vector3 position, float duration = 0.5f)
    {
        if (string.IsNullOrEmpty(effectName)) return;

        GameObject prefab = Managers.Resource.Load<GameObject>(effectName);
        if (prefab == null) return;

        GameObject go = Managers.Pool.Pop(prefab);
        go.transform.position = position;

        if (go.TryGetComponent(out SkillEffectController ctrl))
            ctrl.Setup(duration);
    }
}