using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SaveLoadManager))]
public class SaveLoadManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        var manager = (SaveLoadManager)target;
        
     
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("세이브 파일 관리", EditorStyles.boldLabel);
        
        string path = Path.Combine(Application.persistentDataPath, "_saveData.json");
        
        //파일 경로 표시
        EditorGUILayout.LabelField("파일 경로");
        EditorGUILayout.SelectableLabel(path, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        
        EditorGUILayout.Space(5);
        
        //"폴더 열기" 버튼
        if (GUILayout.Button("저장 폴더 열기"))
        {
            // 파일이 존재하든 안하든, 해당 경로의 폴더를 열어줌
            EditorUtility.RevealInFinder(path);
        }
        
        //"저장 데이터 삭제" 버튼
        if (GUILayout.Button("저장 데이터 삭제"))
        {
            // 삭제하기 전에 사용자에게 확인을 받음
            if (EditorUtility.DisplayDialog("데이터 삭제 확인", 
                    "정말로 저장 데이터를 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다.", 
                    "삭제", "취소"))
            {
                manager.DeleteSaveData();
            }
        }
    }
}
