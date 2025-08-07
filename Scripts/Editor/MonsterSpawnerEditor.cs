using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterSpawner))]
public class MonsterSpawnerEditor : Editor
{
    private int _testMonsterId = 2001; // 기본값 설정
    private int _testSpawnCount = 1;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        MonsterSpawner monsterSpawner = (MonsterSpawner)target;
        
        EditorGUILayout.Space(15); 
        EditorGUILayout.LabelField("에디터 테스트 스폰", EditorStyles.boldLabel); 

        //  몬스터 ID와 마릿수를 입력받는 필드
        _testMonsterId = EditorGUILayout.IntField("몬스터 ID", _testMonsterId);
        _testSpawnCount = EditorGUILayout.IntField("스폰 마릿수", _testSpawnCount);

        // 음수 값이 들어가지 않도록 방지
        if (_testSpawnCount < 1)
        {
            _testSpawnCount = 1;
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("테스트 몬스터 생성"))
        {
            if (Application.isPlaying)
            {
                // MonsterSpawner의 테스트 함수 호출
                monsterSpawner.SpawnMonsterForTest(_testMonsterId, _testSpawnCount);
            }
        }
        
        EditorGUILayout.Space(5);

        if (GUILayout.Button("몬스터 초기화"))
        {
            monsterSpawner.ReturnAllMonstersForTest();
        }
    }
}
