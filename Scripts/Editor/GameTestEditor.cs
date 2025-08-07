using Data;
using UnityEditor;
using UnityEngine;
using static Define;

[CustomEditor(typeof(GameTest))]
public class GameTestEditor : Editor
{
    private int _testGoldAmount;
    private int _testGemAmount;
    private int _testBossTicketAmount;
    private int _testGoldTicketAmount;
    
    private ItemType _selectedItemType;
    public override void OnInspectorGUI()
    {
        GameTest gameTest = (GameTest)target; 
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("게임 테스트", EditorStyles.boldLabel);
        
        if (Application.isPlaying)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("재화 추가", EditorStyles.boldLabel);

            _testGoldAmount = EditorGUILayout.IntField("추가 골드:", _testGoldAmount);
            if (GUILayout.Button("골드 추가"))
            {
                Managers.Game.AddGold(_testGoldAmount);
            }
            EditorGUILayout.Space(10);
            _testGemAmount = EditorGUILayout.IntField("추가 보석:", _testGemAmount);
            if (GUILayout.Button("보석 추가"))
            {
                Managers.Game.AddGem(_testGemAmount);
            }  
            EditorGUILayout.Space(10);
            _testBossTicketAmount = EditorGUILayout.IntField("보스 던전 티켓:", _testBossTicketAmount);
            if (GUILayout.Button("티켓 추가"))
            {
                Managers.Game.AddDungeonTicket(DungeonType.Boss, _testBossTicketAmount);
            }   
            
            EditorGUILayout.Space(10);
            _testGoldTicketAmount = EditorGUILayout.IntField("골드 던전 티켓:", _testGoldTicketAmount);
            if (GUILayout.Button("티켓 추가"))
            {
                Managers.Game.AddDungeonTicket(DungeonType.Gold, _testGoldTicketAmount);
            }
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("시스템 잠금 해제", EditorStyles.boldLabel);

            if (GUILayout.Button("스킬 해금"))
            {
                gameTest.Unlock(UnlockType.Skill);
            }
            EditorGUILayout.Space(10);
            if (GUILayout.Button("동료 해금"))
            {
                gameTest.Unlock(UnlockType.Party);
            }
            EditorGUILayout.Space(10);
            if (GUILayout.Button("던전 해금"))
            {
                gameTest.Unlock(UnlockType.Dungeon);
            }
            EditorGUILayout.Space(10);
            if (GUILayout.Button("스킬 슬롯 추가"))
            {
                gameTest.Unlock(UnlockType.SkillSlot);
            }
            EditorGUILayout.Space(10);
            if (GUILayout.Button("동료 슬롯 추가"))
            {
                gameTest.Unlock(UnlockType.PartySlot);
            }
            
            EditorGUILayout.Space(20);
            EditorGUILayout.LabelField("광고 제거 테스트", EditorStyles.boldLabel);

            bool currentAdRemoveState = Managers.Purchase.IsAdRemoved;
            EditorGUILayout.LabelField($"현재 광고 제거 상태: {(currentAdRemoveState ? "ON" : "OFF")}");

            if (GUILayout.Button("광고 제거 상태 토글"))
            {
                Managers.Purchase.SetTestRemoveAds(!currentAdRemoveState);
            }
            
            // EditorGUILayout.Space(20);
            // _selectedItemType = (Define.ItemType)EditorGUILayout.EnumPopup("아이템 타입 선택", _selectedItemType);
            //
            // if (GUILayout.Button("가장 강한 아이템 찾기"))
            // {
            //     ItemState bestItem = Managers.Inventory.FindBestEquippableItem(_selectedItemType);
            //
            //     if (bestItem != null)
            //     {
            //         Debug.Log($"찾은 아이템: {bestItem.dataId} / 레벨: {bestItem.level}");
            //     }
            //     else
            //     {
            //         Debug.Log("장착 가능한 아이템을 찾지 못했습니다.");
            //     }
            // }
            
        }
    }
}
