using System.Collections.Generic;
using UnityEngine;

public class SkillExecutor : MonoBehaviour
{
    [SerializeField] private float checkInterval = 0.2f;

    private float _checkTimer;
    private List<IDamageable> _enemyList = new();

    //쿨타임 갱신, 자동 발사 시도
    private void Update()
    {
        _checkTimer -= Time.deltaTime;

        if (_checkTimer <= 0f)
        {
            
            // 쿨타임 갱신
            foreach (var skill in Managers.Equipment.SkillEquipment.GetActiveSkills())
                skill?.UpdateCooldown(checkInterval);

            
            // 자동 스킬 발사
            if (Managers.Game.IsAutoSkillOn)
                TryAutoCast();

            _checkTimer = checkInterval;
        }
    }

    //사정거리 내 적이 있으면 사용 가능한 스킬부터 발사
    private void TryAutoCast()
    {
        UpdateEnemyList();

        foreach (var state in Managers.Equipment.SkillEquipment.EquippedSlots)
        {
            if (state == null || !state.isAuto) continue;

            if (Managers.Equipment.SkillEquipment.ActiveSkills.TryGetValue(state.dataId, out Skill skill))
            {
                if (skill != null && skill.IsReady)
                {
                    bool used = skill.TryExecute(transform.position, _enemyList);
                    if (used) break;
                }
            }
        }
    }
    
    //현재 씬 내 살아있는 IDamageable 목록을 갱신
    private void UpdateEnemyList()
    {
        _enemyList.Clear();

        var allObjects = FindObjectsOfType<MonoBehaviour>();
        foreach (var obj in allObjects)
        {
            if (obj is IDamageable d && !d.IsDead)
            {
                GameObject go = (obj as MonoBehaviour).gameObject;

                if (go.CompareTag("Enemy"))
                {
                    _enemyList.Add(d);
                }
            }
        }
    }
    
    public bool HasReadySkill()
    {
        foreach (var state in Managers.Equipment.SkillEquipment.EquippedSlots)
        {
            if (state == null || !state.isAuto) continue;

            if (Managers.Equipment.SkillEquipment.ActiveSkills.TryGetValue(state.dataId, out Skill skill))
            {
                if (skill != null && skill.IsReady)
                    return true;
            }
        }
        return false;
    }
    
    // 지정 위치에서 발사 가능한 스킬이 있는 경우 즉시 발사 시도
    public bool TryExecuteAvailableSkill(Vector3 origin)
    {
        if (!Managers.Game.IsAutoSkillOn) return false;

        UpdateEnemyList();

        foreach (var state in Managers.Equipment.SkillEquipment.EquippedSlots)
        {
            if (state == null || !state.isAuto) continue;

            if (Managers.Equipment.SkillEquipment.ActiveSkills.TryGetValue(state.dataId, out Skill skill))
            {
                if (skill != null && skill.IsReady)
                {
                    if (skill.TryExecute(origin, _enemyList))
                        return true; // 발사 성공 시 true
                }
            }
        }

        return false;
    }

}