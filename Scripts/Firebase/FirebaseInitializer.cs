// using Firebase;
// using Firebase.Extensions;
// using UnityEngine;
//
// public class FirebaseInitializer : MonoBehaviour
// {
//     private void Start()
//     {
//         Debug.Log(" Firebase 초기화 시도 중...");
//
//         FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
//         {
//             var status = task.Result;
//
//             if (status == DependencyStatus.Available)
//             {
//                 Debug.Log("Firebase 초기화 완료");
//                 FirebaseApp app = FirebaseApp.DefaultInstance;
//                 // 여기서 로그인 시도 가능
//             }
//             else
//             {
//                 Debug.LogError($" Firebase 초기화 실패: {status}");
//             }
//         });
//     }
// }