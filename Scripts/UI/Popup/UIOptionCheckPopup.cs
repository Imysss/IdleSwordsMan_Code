using System;
using UnityEngine;
using static Define;

public class UIOptionCheckPopup : UIPopup
{
    #region UI 기능 리스트
    //MainText: 메인 텍스트
    //CancelButton
    //ClaimButton
    //ClaimButtonText
    //ExitButton
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObjects
    }
    enum Buttons
    {
        CancelButton,
        ClaimButton,
        ExitButton,
    }

    enum Texts
    {
        MainText,
        ClaimButtonText,
    }
    #endregion

    private OptionCheckType type;

    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObjects));
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        
        GetButton((int)Buttons.CancelButton).gameObject.BindEvent(OnClickCancelButton);
        GetButton((int)Buttons.ClaimButton).gameObject.BindEvent(OnClickClaimButton);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);
        
        return true;
    }

    public void SetInfo(OptionCheckType type)
    {
        this.type = type;

        RefreshUI();
    }

    private void RefreshUI()
    {
        switch (type)
        {
            case OptionCheckType.Logout:
                GetText((int)Texts.MainText).text = "정말 로그아웃 하시겠습니까?";
                GetText((int)Texts.ClaimButtonText).text = "로그아웃";
                break;
            case OptionCheckType.Delete:
                GetText((int)Texts.MainText).text = "정말 계정을 삭제하시겠습니까?";
                GetText((int)Texts.ClaimButtonText).text = "삭제";
                break;
            case OptionCheckType.Exit:
                GetText((int)Texts.MainText).text = "정말 게임을 종료하시겠습니까??";
                GetText((int)Texts.ClaimButtonText).text = "종료";
                break;
        }
    }

    private void OnClickCancelButton()
    {
        Managers.Sound.PlayButtonClick();
        
        PopupCloseAnimation(GetObject((int)GameObjects.ContentObjects), () => 
        {
            Managers.UI.ClosePopupUI(this);
        });
    }

    private void OnClickClaimButton()
    {
//         Managers.Sound.PlayButtonClick();
//         
//         switch (type)
//         {
//             case OptionCheckType.Logout:
//                 Managers.GoogleLogin.GoogleLogout();
//                 break;
//             case OptionCheckType.Delete:
//                 Managers.GoogleLogin.DeleteAccount();
//                 break;
//             case OptionCheckType.Exit:
// #if UNITY_EDITOR
//                 UnityEditor.EditorApplication.isPlaying = false;
// #elif UNITY_ANDROID
//         Application.Quit();
// #endif
//                 break;
//         }
//         Managers.UI.ClosePopupUI(this);
    }

    private void OnClickExitButton()
    {
        Managers.Sound.PlayButtonClick();
        
        PopupCloseAnimation(GetObject((int)GameObjects.ContentObjects), () => 
        {
            Managers.UI.ClosePopupUI(this);
        });
    }
}
