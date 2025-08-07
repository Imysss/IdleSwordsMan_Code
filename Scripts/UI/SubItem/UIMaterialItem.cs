using System;
using UnityEngine;

public class UIMaterialItem : UIBase
{
    #region UI 기능 리스트
    //ItemImage: 아이템 이미지
    //ItemCountValueText: 아이템 개수 값 텍스트
    //InfoButton: 해당 아이템 정보 확인용 버튼
    #endregion

    #region Enum
    enum Buttons
    {
        InfoButton
    }

    enum Texts
    {
        ItemCountValueText,
    }

    enum Images
    {
        ItemImage,
    }
    #endregion

    private string spriteName;
    private string count;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        
        GetButton((int)Buttons.InfoButton).gameObject.BindEvent(OnClickInfoButton);
        
        return true;
    }

    public void SetInfo(string spriteName, string count)
    {
        //정보 받아오기
        this.spriteName = spriteName;
        this.count = count;
        
        RefreshUI();
    }

    private void RefreshUI()
    {
        Sprite spr = Managers.Resource.Load<Sprite>(spriteName);
        GetImage((int)Images.ItemImage).sprite = spr;
        GetText((int)Texts.ItemCountValueText).text = $"x{count}";
    }

    private void OnClickInfoButton()
    {
        
    }
}