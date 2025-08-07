// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
//
// public class AuthUIController : MonoBehaviour
// {
//     [Header("UI 입력 필드")]
//     public TMP_InputField emailInput;        // 이메일 입력창
//     public TMP_InputField passwordInput;     // 비밀번호 입력창
//
//     [Header("Firebase 인증 매니저")]
//     public FirebaseAuthManager authManager; // Firebase 인증 로직을 담당하는 매니저 스크립트
//
//     /// <summary>
//     /// 로그인 버튼 클릭 시 호출
//     /// </summary>
//     public void OnLoginClicked()
//     {
//         // 입력값을 가져와서 로그인 실행
//         authManager.Login(emailInput.text, passwordInput.text);
//     }
//
//     /// <summary>
//     /// 회원가입 버튼 클릭 시 호출
//     /// </summary>
//     public void OnRegisterClicked()
//     {
//         // 입력값을 가져와서 회원가입 실행
//         authManager.Register(emailInput.text, passwordInput.text);
//     }
// }