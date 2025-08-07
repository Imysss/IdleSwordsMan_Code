using System.ComponentModel;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    private int _testMapIndex = 0;
    private int _testStageIndex = 0;
    private int _testWaveIndex = 0;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        LevelManager levelManager = (LevelManager)target;

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("에디터 테스트 컨트롤", EditorStyles.boldLabel);

        // 게임 실행 중에만 표시
        if (Application.isPlaying)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("현재 진행 상태", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                if (levelManager.CurrentStageData != null)
                {
                    EditorGUILayout.LabelField("현재 맵:", $" ({levelManager.CurrentStageData.mapName})");
                }
                else
                {
                    EditorGUILayout.LabelField("현재 맵:", "데이터 로드 중...");
                }

                if (levelManager.CurrentStageData != null)
                {
                    EditorGUILayout.LabelField("현재 스테이지:", (levelManager.CurrentStageData.stageIdx ).ToString());
                }
                else
                {
                    EditorGUILayout.LabelField("현재 스테이지:", "N/A");
                }

                if (levelManager.CurrentStageData != null)
                {
                    EditorGUILayout.LabelField("현재 웨이브:", (levelManager.CurrentStageData.waveIdx).ToString());
                }
                else
                {
                    EditorGUILayout.LabelField("현재 웨이브:", "N/A");
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("다음 웨이브"))
            {
                if (Application.isPlaying)
                {
                    levelManager.StartNextWaveTest();
                }
            }
        
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("스테이지 이동", EditorStyles.boldLabel);
            _testMapIndex = EditorGUILayout.IntField("맵 인덱스", _testMapIndex);
            _testStageIndex = EditorGUILayout.IntField("스테이지 인덱스", _testStageIndex);
        
            if (GUILayout.Button("해당 스테이지로 이동"))
            {
                levelManager.MoveToStageTest(_testMapIndex, _testStageIndex);
            }
            EditorGUILayout.Space(10);
            // === 웨이브 생성 섹션 ===
            EditorGUILayout.LabelField("현재 스테이지 내 웨이브 생성", EditorStyles.boldLabel);
            _testWaveIndex = EditorGUILayout.IntField("웨이브 인덱스", _testWaveIndex);
            if (GUILayout.Button("해당 웨이브 시작"))
            {
                levelManager.StartWaveTest(_testWaveIndex);
            }
        }
        
    }
}
