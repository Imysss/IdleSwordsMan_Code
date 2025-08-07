using UnityEngine;

public class UIDungeonContentItem : UIBase
{
    #region UI 기능 리스트
    //DungeonNameText
    //DungeonLevelText
    //DungeonKeyImage
    //DungeonKeyCountValueText
    //RewardText
    //RewardImage
    //RewardValueText
    //DungeonEnterButton
    #endregion

    #region Enum
    enum Buttons
    {
        DungeonEnterButton
    }

    enum Texts
    {
        DungeonNameText,
        DungeonLevelText,
        DungeonKeyCountValueText,
        RewardText,
        RewardValueText
    }

    enum Images
    {
        DungeonKeyImage,
        RewardImage
    }
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        
        GetButton((int)Buttons.DungeonEnterButton).gameObject.BindEvent(OnClickDungeonEnterButton);

        return true;
    }

    public void SetInfo()
    {
        //던전 정보 받기

        RefreshUI();
    }

    private void RefreshUI()
    {
        // GetText((int)Texts.DungeonNameText).text = "";
        // GetText((int)Texts.DungeonLevelText).text = $"LEVEL {01}";
        // Sprite keySpr = Managers.Resource.Load<Sprite>("dungeonkey.sprite");
        // GetImage((int)Images.DungeonKeyImage).sprite = keySpr;
        // GetText((int)Texts.DungeonKeyCountValueText).text = $"{현재 키 개수}/2";
        // GetText((int)Texts.RewardText).text = "골드 or 젬";
        // Sprite rewardSpr = Managers.Resource.Load<Sprite>("reward.sprite");
        // GetImage((int)Images.RewardImage).sprite = rewardSpr;
        // GetText((int)Texts.RewardValueText).text = 5000 ?;
    }

    private void OnClickDungeonEnterButton()
    {
        //던전 시작
    }
}
