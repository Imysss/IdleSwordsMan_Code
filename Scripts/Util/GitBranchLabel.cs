// using UnityEditor;
// using UnityEngine;
// using System.Diagnostics;
// using System.IO;
//
// // [InitializeOnLoad] 어트리뷰트는 Unity 에디터가 로드될 때(즉, 스크립트가 컴파일되거나 에디터가 시작될 때)
// // 해당 클래스의 정적 생성자(static constructor)를 자동으로 호출하도록 한다.
// //[InitializeOnLoad]
// public static class GitBranchLabel
// {
//     static string branchName = "unknown";
//     static double nextUpdateTime = 0d;       // 다음 업데이트 시간
//     const double updateInterval = 5.0d;      // 업데이트 간격 (초 단위)
//
//     // 정적 생성자: 에디터가 시작될 때 한 번만 호출됩니다.
//     static GitBranchLabel()
//     {
//         UpdateBranchName();
//         SceneView.duringSceneGui += OnSceneGUI;
//         EditorApplication.update += OnEditorUpdate;
//         EditorApplication.projectChanged += UpdateBranchName;
//     }
//
//     // 이 콜백 메소드는 에디터가 업데이트될 때마다 호출
//     static void OnEditorUpdate()
//     {
//         // 주기적으로 브랜치 이름을 업데이트
//         if (EditorApplication.timeSinceStartup > nextUpdateTime)
//         {
//             UpdateBranchName();
//             nextUpdateTime = EditorApplication.timeSinceStartup + updateInterval;
//         }
//     }
//
//     static void UpdateBranchName()
//     {
//         try
//         {
//             // Git 명령어를 쉘에서 실행하여 현재 브랜치 이름 추출
//             ProcessStartInfo startInfo = new ProcessStartInfo("git", "branch --show-current")
//             {
//                 CreateNoWindow = true,                              // 창을 생성하지 않음
//                 UseShellExecute = false,                            // 셸을 사용하지 않음
//                 RedirectStandardOutput = true,                      // 표준 출력 리디렉션
//                 RedirectStandardError = true,                       // 표준 오류 리디렉션
//                 WorkingDirectory = Directory.GetCurrentDirectory()  // 현재 작업 디렉토리 설정
//             };
//
//             using (Process process = new Process())
//             {
//                 process.StartInfo = startInfo;
//                 process.Start();    // 프로세스 실행
//
//                 string output = process.StandardOutput.ReadToEnd().Trim();
//                 process.WaitForExit();
//
//                 if (!string.IsNullOrEmpty(output) && output != branchName)
//                 {
//                     branchName = output;
//                     SceneView.RepaintAll();
//                 }
//                 else if (string.IsNullOrEmpty(output))
//                 {
//                     branchName = "HEAD";
//                 }
//             }
//         }
//         catch 
//         {
//             branchName = "깃 설정이 되지 않은 상태이거나 다른 오류 발생";
//         }
//     }
//
//     // SceneView의 GUI를 업데이트하는 메소드
//     static void OnSceneGUI(SceneView sceneView)
//     {
//         Handles.BeginGUI();
//
//         string text = $"{branchName}";
//
//         // GUI 스타일 설정
//         GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
//         {
//             fontSize = 12,
//             fontStyle = FontStyle.Bold,
//             normal = { textColor = Color.white },
//             alignment = TextAnchor.MiddleCenter,
//             padding = new RectOffset(5, 5, 1, 1)
//         };
//
//         Vector2 size = boxStyle.CalcSize(new GUIContent(text));
//         float centerX = sceneView.position.width / 2f - size.x / 2f;
//         Rect rect = new Rect(centerX, 10, size.x + 4, size.y + 4);
//
//         GUI.Box(rect, text, boxStyle);
//
//         Handles.EndGUI();
//     }
// }