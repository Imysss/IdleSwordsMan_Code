using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerSnapshot
{
    // private float _curHp;
    // private Vector3 _position;
    // private Dictionary<StatType, float> _stats = new();
    //
    // public void Capture(PlayerController player)
    // {
    //     var statManager = player.StatManager;
    //     _curHp = player.GetCurrentHp();
    //     _position = player.transform.position;
    //
    //     //스탯 저장
    //     foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
    //         _stats[statType] = statManager.GetValue(statType);
    // }
    //
    // public void Restore(PlayerController player)
    // {
    //     var statManager = player.StatManager;
    //     player.transform.position = _position;
    //     player.SetCurrentHp(_curHp);
    //     //스탯 복원
    //     foreach (var kvp in _stats)
    //         statManager.SetBase(kvp.Key, kvp.Value);
    // }
}