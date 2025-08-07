// using Firebase.Auth;            // Firebase 인증 기능 사용
// using Firebase.Extensions;      // Unity에서 Firebase 비동기 처리를 위한 확장 (ContinueWithOnMainThread)
// using UnityEngine;
//
// public class FirebaseAuthManager : MonoBehaviour
// {
//     // Firebase 인증 객체
//     private FirebaseAuth auth;
//
//     // 현재 로그인한 사용자 정보를 담을 객체
//     private FirebaseUser user;
//
//     // Unity 실행 시 가장 먼저 호출되는 초기화 메서드 (MonoBehaviour)
//     private void Awake()
//     {
//         // Firebase 인증 인스턴스 초기화
//         auth = FirebaseAuth.DefaultInstance;
//     }
//
//     /// <summary>
//     /// 이메일/비밀번호로 회원가입 처리
//     /// </summary>
//     /// <param name="email">이메일 주소</param>
//     /// <param name="password">비밀번호</param>
//     public void Register(string email, string password)
//     {
//         // Firebase에 사용자 생성 요청
//         auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
//
//             // 에러나 취소되었을 경우
//             if (task.IsFaulted || task.IsCanceled)
//             {
//                 Debug.LogError(" 회원가입 실패: " + task.Exception?.Message);
//                 return;
//             }
//
//             // 회원가입 성공 시 FirebaseUser 객체 획득
//             user = task.Result.User;
//
//             // 콘솔에 성공 메시지 출력
//             Debug.Log($" 회원가입 성공: {user.Email}");
//         });
//     }
//
//     /// <summary>
//     /// 이메일/비밀번호로 로그인 처리
//     /// </summary>
//     /// <param name="email">이메일 주소</param>
//     /// <param name="password">비밀번호</param>
//     public void Login(string email, string password)
//     {
//         // Firebase에 로그인 요청
//         auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
//
//             // 에러나 취소되었을 경우
//             if (task.IsFaulted || task.IsCanceled)
//             {
//                 Debug.LogError(" 로그인 실패: " + task.Exception?.Message);
//                 return;
//             }
//
//             // 로그인 성공 시 FirebaseUser 객체 획득
//             user = task.Result.User;
//
//             // 콘솔에 성공 메시지 출력
//             Debug.Log($"로그인 성공: {user.Email}");
//
//             // Firebase UID 확인
//             Debug.Log($"Firebase UID: {user.UserId}");
//
//             // 서버 저장 시도
//             Managers.SaveLoad.Save();
//         });
//     }
// }