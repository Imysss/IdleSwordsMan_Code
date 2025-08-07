// using UnityEngine;
// using Firebase.Auth;
// using Google;
// using System.Threading.Tasks;
// using Firebase.Extensions;
//
// public class GoogleLoginManager
// {
//     private FirebaseAuth auth;
//     private GoogleSignInConfiguration configuration;
//
//     private bool isProcessing = false; // 중복 로그인 방지용
//     private bool isLogin = false;
//     public bool IsLogin { get => isLogin; }
//
//     public void Init()
//     {
//         auth = FirebaseAuth.DefaultInstance;
//
//         configuration = new GoogleSignInConfiguration
//         {
//             WebClientId = "118805042715-hfr9mn73dns5lbk7mc0ag4rnvr5ij6ch.apps.googleusercontent.com",
//             RequestIdToken = true
//         };
//
//         GoogleSignIn.Configuration = configuration;
//
//         Debug.Log(" FirebaseAuth + Google 설정 완료");
//         
//         //로그인되어있는 계정의 경우? 해당 계정에서 SaveData 받아와서 연결하는 작업 필요함
//         //일단 들어올 때마다 false로 설정
//         isLogin = false;
//     }
//
//     public void GoogleLogin()
//     {
// #if UNITY_ANDROID
//         if (isProcessing)
//         {
//             Debug.LogWarning(" 로그인 처리 중입니다...");
//             return;
//         }
//
//         Debug.Log(" Google 로그인 시작");
//         isProcessing = true;
//
//         GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task =>
//         {
//             if (task.IsCanceled)
//             {
//                 Debug.LogWarning(" Google 로그인 취소됨");
//                 isProcessing = false;
//                 return;
//             }
//
//             if (task.IsFaulted)
//             {
//                 Debug.LogError(" Google 로그인 실패: " + task.Exception);
//                 isProcessing = false;
//                 return;
//             }
//
//             GoogleSignInUser googleUser = task.Result;
//             string idToken = googleUser?.IdToken;
//
//             if (string.IsNullOrEmpty(idToken))
//             {
//                 Debug.LogError(" ID Token 누락 - Firebase 인증 불가");
//                 isProcessing = false;
//                 return;
//             }
//
//             Debug.Log($" Google 로그인 성공 - {googleUser.DisplayName} / {googleUser.Email}");
//
//             Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
//             auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
//             {
//                 isProcessing = false;
//
//                 if (authTask.IsCanceled || authTask.IsFaulted)
//                 {
//                     Debug.LogError(" Firebase 인증 실패: " + authTask.Exception);
//                     return;
//                 }
//
//                 FirebaseUser user = authTask.Result;
//                 Debug.Log($" Firebase 로그인 완료 - UID: {user.UserId}, 이메일: {user.Email}");
//
//                 // 로그인 성공 후 처리
//                 OnLoginSuccess(user);
//             });
//         });
// #else
//         Debug.LogWarning("Google 로그인은 Android 환경에서만 동작합니다.");
// #endif
//     }
//
//     public void GoogleLogout()
//     {
// #if UNITY_ANDROID
//         Debug.Log(" 로그아웃 시작");
//
//         auth.SignOut();
//         GoogleSignIn.DefaultInstance.SignOut();
//
//         Debug.Log(" Firebase + Google 로그아웃 완료");
// #else
//         Debug.LogWarning("Google 로그아웃은 Android 환경에서만 동작합니다.");
// #endif
//     }
//
//     public void DeleteAccount()
//     {
// #if UNITY_ANDROID
//         FirebaseUser user = auth.CurrentUser;
//
//         if (user != null)
//         {
//             Debug.Log(" 회원탈퇴 시도 중...");
//
//             user.DeleteAsync().ContinueWithOnMainThread(task =>
//             {
//                 if (task.IsCanceled || task.IsFaulted)
//                 {
//                     Debug.LogError(" Firebase 계정 삭제 실패: " + task.Exception);
//                     return;
//                 }
//
//                 Debug.Log(" Firebase 계정 삭제 완료");
//
//                 // 로그아웃도 같이 처리
//                 GoogleSignIn.DefaultInstance.SignOut();
//                 auth.SignOut();
//
//                 Debug.Log(" Google 세션 & Firebase 로그아웃 완료");
//
//                 // TODO: 탈퇴 이후 타이틀씬 이동 등 추가 처리
//                 // SceneManager.LoadScene("TitleScene");
//             });
//         }
//         else
//         {
//             Debug.LogWarning(" 현재 로그인된 사용자가 없습니다.");
//         }
// #else
//         Debug.LogWarning("회원탈퇴는 Android 환경에서만 동작합니다.");
// #endif
//     }
//
//     // 로그인 성공 후 처리할 내용은 여기서 정의
//     private void OnLoginSuccess(FirebaseUser user)
//     {
//         //  서버에 UID 보내기 / 씬 이동 / 세이브포인트 불러오기 등
//         Debug.Log($" OnLoginSuccess → UID: {user.UserId}");
//         isLogin = true;
//
//         // 게스트 → UID 마이그레이션 실행
//         Managers.Guest.OnLoginSuccess(user.UserId);
//
//         // 로그인 직후 UID를 강제로 GuestManager에 반영 + 서버 데이터 불러오기
//         string firebaseUid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
//         if (!string.IsNullOrEmpty(firebaseUid))
//         {
//             Managers.Guest.SetCurrentUserId(firebaseUid);  // 현재 UID를 갱신
//             Managers.SaveLoad.LoadFromServer();            // 서버에서 최신 세이브 데이터 불러오기
//         }
//
//         // 이후 UID로 저장된 세이브 데이터 불러오기
//         
//         Managers.UserDatabase.StartCoroutine(
//             Managers.UserDatabase.LoadDataFromServer(user.UserId, saveData =>
//             {
//                 Debug.Log("[Login] 세이브 데이터 불러오기 완료");
//                 // SaveData 반영 처리
//                 Managers.SaveLoad.ApplyServerSave(saveData); // 또는 직접 세이브 적용
//             })
//         );
//
//         // 예: 게임 씬으로 이동
//         // SceneManager.LoadScene("MainScene");
//
//         // 예: 서버에 UID 전송
//         // StartCoroutine(Api.SendLoginUID(user.UserId));
//     }
// }