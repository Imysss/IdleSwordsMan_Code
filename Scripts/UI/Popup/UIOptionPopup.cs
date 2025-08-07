using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIOptionPopup : UIPopup
{
    #region UI 기능 리스트

    //UserIdGroup: UserId copy Button
    //UserIdText: UserId
    //SoundSlider
    //BgmSlider
    //AlarmCheckButton
    //AlarmCheckImage

    //TermOfServiceButton
    //PrivacyPolicyButton
    //LogoutButton
    //DeleteAccountButton
    //SaveAndExitButton

    //ExitButton

    #endregion

    #region Enum

    enum GameObjects
    {
        ContentObject,
        SoundSlider,
        BgmSlider,
        //AlarmCheckImage,
        //SleepModeCheckImage,
    }

    enum Buttons
    {
        UserIdGroup,
        //SleepModeCheckButton,
        //AlarmCheckButton,
        TermOfServiceButton,
        PrivacyPolicyButton,
        LogoutButton,
        DeleteAccountButton,
        SaveAndExitButton,
        ExitButton,
    }

    enum Texts
    {
        UserIdText,
        LogoutText
    }

    #endregion

    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        
        GetButton((int)Buttons.UserIdGroup).gameObject.BindEvent(OnClickUserIdCopyButton);

        //GetButton((int)Buttons.SleepModeCheckButton).gameObject.BindEvent(OnClickSleepModeCheckButton);
        //GetButton((int)Buttons.AlarmCheckButton).gameObject.BindEvent(OnClickAlarmCheckButton);

        GetButton((int)Buttons.TermOfServiceButton).gameObject.BindEvent(OnClickTermOfServiceButton);
        GetButton((int)Buttons.PrivacyPolicyButton).gameObject.BindEvent(OnClickPrivacyPolicyButton);
        GetButton((int)Buttons.LogoutButton).gameObject.BindEvent(OnClickLogoutButton);
        GetButton((int)Buttons.DeleteAccountButton).gameObject.BindEvent(OnClickDeleteAccountButton);
        GetButton((int)Buttons.SaveAndExitButton).gameObject.BindEvent(OnClickSaveAndExitButton);

        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);
        
        GetObject((int)GameObjects.SoundSlider).GetComponent<Slider>().onValueChanged.AddListener(OnSoundSliderValueChanged);
        GetObject((int)GameObjects.BgmSlider).GetComponent<Slider>().onValueChanged.AddListener(OnBgmSliderValueChanged);
        
        RefreshUI();

        return true;
    }

    public void SetInfo()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        GetText((int)Texts.UserIdText).text = Managers.Guest.GetCurrentUserId();
        GetObject((int)GameObjects.BgmSlider).GetComponent<Slider>().value = Managers.Game.BgmVolume;
        GetObject((int)GameObjects.SoundSlider).GetComponent<Slider>().value = Managers.Game.SfxVolume;
        
        //GetObject((int)GameObjects.AlarmCheckImage).SetActive(Managers.Game.AlarmCheck);
        //GetObject((int)GameObjects.SleepModeCheckImage).SetActive(Managers.SleepMode.isAutoSleepMode);
        
        //로그인 로그아웃 버튼
        GetText((int)Texts.LogoutText).text =  "계정 연동";
    }

    private void OnClickUserIdCopyButton()
    {
        Managers.Sound.PlayButtonClick();
        
        GUIUtility.systemCopyBuffer = GetText((int)Texts.UserIdText).text;
        Managers.UI.ShowToast("클립보드에 복사되었습니다.");
    }

    // private void OnClickSleepModeCheckButton()
    // {
    //     Managers.Sound.PlayButtonClick();
    //
    //     Managers.SleepMode.isAutoSleepMode = !Managers.SleepMode.isAutoSleepMode;
    //     RefreshUI();
    // }

    private void OnClickAlarmCheckButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.Game.AlarmCheck = !Managers.Game.AlarmCheck;
        RefreshUI();
    }

    private void OnClickTermOfServiceButton()
    {
        Managers.Sound.PlayButtonClick();
        //사이트로 이동
        Application.OpenURL("https://sites.google.com/view/teamfriday13-privacy/%ED%99%88");
    }

    private void OnClickPrivacyPolicyButton()
    {
        Managers.Sound.PlayButtonClick();
        //사이트로 이동
        Application.OpenURL("https://sites.google.com/view/teamfriday13-privacy/%ED%99%88");
    }

    private void OnClickLogoutButton()
    {
        Managers.Sound.PlayButtonClick();
        
        // //로그아웃
        // if (Managers.GoogleLogin.IsLogin)
        // {
        //     Managers.UI.ShowPopupUI<UIOptionCheckPopup>().SetInfo(Define.OptionCheckType.Logout);
        // }
        // //로그인
        // else
        // {
        //     Managers.GoogleLogin.GoogleLogin();
        // }
        
        RefreshUI();
    }

    private void OnClickSaveAndExitButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.UI.ShowPopupUI<UIOptionCheckPopup>().SetInfo(Define.OptionCheckType.Exit);
    }

    private void OnClickDeleteAccountButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.UI.ShowPopupUI<UIOptionCheckPopup>().SetInfo(Define.OptionCheckType.Delete);
    }

    private void OnClickExitButton()
    {
        Managers.Sound.PlayButtonClick();
        
        PopupCloseAnimation(GetObject((int)GameObjects.ContentObject), () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }

    private void OnSoundSliderValueChanged(float value)
    {
        Managers.Game.SfxVolume = value;
    }

    private void OnBgmSliderValueChanged(float value)
    {
        Managers.Game.BgmVolume = value;
    }
}
