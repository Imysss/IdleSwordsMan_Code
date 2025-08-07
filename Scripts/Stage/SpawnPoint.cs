using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Player Spawn Points")]
    [SerializeField] private Transform playerSpawnPoint;
    
    [Header("Party Spawn Points")]
    [SerializeField] private Transform[] partySpawnPoints;
    
    [Header("Monster Spawn Points")]
    [SerializeField] private Transform monsterSpawnPoint;
    
    // 외부에서 쉽게 접근할 수 있도록 프로퍼티나 메서드를 제공합니다.
    public Transform PlayerSpawnPoint => playerSpawnPoint;
    public Transform MonsterSpawnPoint => monsterSpawnPoint;

    public Transform GetPartySpawnPoint(int index)
    {
        if (index >= 0 && index < partySpawnPoints.Length)
        {
            return partySpawnPoints[index];
        }
        return transform; 
    }
}
