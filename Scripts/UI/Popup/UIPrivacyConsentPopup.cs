using UnityEngine;

public class UIPrivacyConsentPopup : UIPopup
{
    #region UI 기능 리스트

    // 개인 정보 수집 동의 버튼
    // 개인 정보 수집 비동의 버톤

    #endregion
    
    #region Enum

    enum Buttons
    {
        AgreeButton,
        DeclineButton,
    }
    
    #endregion


    public override bool Init()
    {
        if(base.Init() == false)
            return false;
        
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.AgreeButton).gameObject.BindEvent(OnClickAgreeButton);
        GetButton((int)Buttons.DeclineButton).gameObject.BindEvent(OnClickDeclineButton);
        
        return true;
    }

    private void OnClickAgreeButton()
    {
        Managers.Analytics.GiveConsent();
        Managers.SaveLoad.SaveData.hasUserConsented = true;
        Managers.UI.ClosePopupUI(this);
    }

    private void OnClickDeclineButton()
    {
        Managers.UI.ClosePopupUI(this);
    }
    
}
