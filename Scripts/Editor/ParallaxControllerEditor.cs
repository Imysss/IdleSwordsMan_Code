using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

[CustomEditor(typeof(ParallaxController))]
public class ParallaxControllerEditor : Editor
{
    private float _transitionTestDuration = 1.5f;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        ParallaxController parallaxController = (ParallaxController)target;
        
        EditorGUILayout.Space(10); 
        EditorGUILayout.LabelField("에디터 테스트 컨트롤", EditorStyles.boldLabel);
        
        if (GUILayout.Button("배경 이동 시작"))
        {
            if (Application.isPlaying)
            {
                parallaxController.StartMovement();
            }
        }

        if (GUILayout.Button("배경 정지"))
        {
            if (Application.isPlaying)
            {
                parallaxController.StopMovement();
            }
        }
        
        EditorGUILayout.Space(5); 
        _transitionTestDuration = EditorGUILayout.FloatField("테스트 지속 시간(초)", _transitionTestDuration);
        if (GUILayout.Button("스테이지 전환 연출 테스트"))
        {
            if (Application.isPlaying)
            {
                parallaxController.StartStageTransition(_transitionTestDuration);
            }
        }
    }
}
