using System.Collections.Generic;
using UnityEngine;

public static class SkillUseService
{
    public static void TryUseSkill(int skillId, Vector3 originPosition)
    {
        if (!Managers.Equipment.SkillEquipment.ActiveSkills.TryGetValue(skillId, out var skill))
            return;

        if (!skill.IsReady) return;

        var enemies =  GameObject.FindObjectsOfType<MonoBehaviour>();
        List<IDamageable> enemyList = new();

        foreach (var e in enemies)
        {
            if (e is IDamageable d && !d.IsDead &&
                ((MonoBehaviour)d).transform != Managers.Player.PlayerTransform)
            {
                enemyList.Add(d);
            }
        }

        skill.TryExecute(originPosition, enemyList);
    }
}
