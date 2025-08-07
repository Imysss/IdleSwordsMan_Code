using System;
using System.Dynamic;
using TMPro;
using UnityEngine;

public class UIChangeNamePopup : UIPopup
{
    #region UI 기능 리스트
    //GemValueText: 현재 보유 중인 젬 개수
    //NameInputField: 이름 입력 필드
    //ExitButton: 나가기 버튼
    //ClaimButton: 확인 버튼
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        NameInputField
    }
    enum Buttons
    {
        ExitButton,
        ClaimButton,
    }

    enum Texts
    {
        GemValueText,
    }
    #endregion
    
    //조건에 따라 바뀔 색상
    private Color validColor = Util.HexToColor("F9FBE6");
    private Color invalidColor = Util.HexToColor("FF352B");

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
        
        GetButton((int)Buttons.ClaimButton).gameObject.BindEvent(OnClickClaimButton);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);
        
        GetObject((int)GameObjects.NameInputField).GetComponent<TMP_InputField>().onValueChanged.AddListener(OnNameChanged);
        OnNameChanged(GetObject((int)GameObjects.NameInputField).GetComponent<TMP_InputField>().text);

        RefreshUI();
        return true;
    }

    private void RefreshUI()
    {
        GetText((int)Texts.GemValueText).text = Managers.Game.Gems.ToString();
    }

    private void OnNameChanged(string input)
    {
        //조건 검사
        bool isLengthValid = input.Length >= 4 && input.Length <= 14;
        bool hasNoWhitespace = !input.Contains(" ");
        
        bool isValid = isLengthValid && hasNoWhitespace;

        GetObject((int)GameObjects.NameInputField).GetComponent<TMP_InputField>().textComponent.color = isValid ? validColor : invalidColor;
        GetButton((int)Buttons.ClaimButton).interactable = (isValid && Managers.Game.Gems >= 200);
    }

    private void OnClickClaimButton()
    {
        Managers.Sound.PlayButtonClick();
        
        string name = GetObject((int)GameObjects.NameInputField).GetComponent<TMP_InputField>().text;
        Managers.Game.userName = name;
        
        EventBus.Raise(new NameChangedEvent());
        // 유저 이름 저장
        Managers.SaveLoad.SaveData.userName = name;
        
        Managers.Game.UseGem(200);
        Managers.UI.ClosePopupUI(this);
    }

    private void OnClickExitButton()
    {
        Managers.Sound.PlayButtonClick();
        
        PopupCloseAnimation(GetObject((int)GameObjects.ContentObject), () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }
}
