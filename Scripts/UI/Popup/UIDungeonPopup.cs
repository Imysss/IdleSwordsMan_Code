using System;
using System.Numerics;
using UnityEngine;
using static Define;

public class UIDungeonPopup : UIPopup
{
    #region UI 기능 리스트
    //BossDungeonLevelText
    //BossDungeonKeyCountValueText
    //BossDungeonRewardValueText
    //BossDungeonEnterButton
    //GoldDungeonLevelText
    //GoldDungeonKeyCountValueText
    //GoldDungeonRewardValueText
    //GoldDungeonEnterButton
    #endregion

    #region Enum
    enum Buttons
    {
        BossDungeonEnterButton,
        GoldDungeonEnterButton,
        BossDungeonAdButton,
        GoldDungeonAdButton
    }

    enum Texts
    {
        BossDungeonLevelText,
        BossDungeonKeyCountValueText,
        BossDungeonRewardValueText,
        GoldDungeonLevelText,
        GoldDungeonKeyCountValueText,
        GoldDungeonRewardValueText
    }
    #endregion

    private int bossLevel;
    private int goldLevel;

    private void OnEnable()
    {
        EventBus.Subscribe<DungeonTicketChangedEvent>(DungeonTicketChangedEventHandler);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<DungeonTicketChangedEvent>(DungeonTicketChangedEventHandler);
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        
        GetButton((int)Buttons.BossDungeonEnterButton).gameObject.BindEvent(OnClickBossDungeonEnterButton);
        GetButton((int)Buttons.GoldDungeonEnterButton).gameObject.BindEvent(OnClickGoldDungeonEnterButton);
        
        GetButton((int)Buttons.BossDungeonAdButton).gameObject.BindEvent(OnClickBossDungeonAdButton);
        GetButton((int)Buttons.GoldDungeonAdButton).gameObject.BindEvent(OnClickGoldDungeonAdButton);
        
        //튜토리얼 버튼
        Managers.Tutorial.AddUIButton(UIButtonType.BossDungeon,GetButton((int)Buttons.BossDungeonEnterButton).gameObject);
        Managers.Tutorial.AddUIButton(UIButtonType.GoldDungeon,GetButton((int)Buttons.GoldDungeonEnterButton).gameObject);

        return true;
    }

    public void SetInfo()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        bossLevel = Managers.Dungeon.maxBossDungeonStage;
        goldLevel = Managers.Dungeon.maxGoldDungeonStage;
        int bossTicket = Managers.Game.GetDungeonTicketCount(DungeonType.Boss);
        int goldTicket = Managers.Game.GetDungeonTicketCount(DungeonType.Gold);

        GetText((int)Texts.BossDungeonLevelText).text = $"LEVEL " + bossLevel.ToString("D2");
        GetText((int)Texts.BossDungeonKeyCountValueText).text = $"{bossTicket}/{DUNGEON_TICKET_MAX_VALUE}";
        GetText((int)Texts.BossDungeonRewardValueText).text = Managers.Data.BossDungeonDataDic[bossLevel].reward.ToString();

        GetText((int)Texts.GoldDungeonLevelText).text = $"LEVEL " + goldLevel.ToString("D2");
        GetText((int)Texts.GoldDungeonKeyCountValueText).text = $"{goldTicket}/{DUNGEON_TICKET_MAX_VALUE}";
        GetText((int)Texts.GoldDungeonRewardValueText).text = Managers.Data.GoldDungeonDataDic[goldLevel].reward;

        // 던전 입장 버튼 활성화 여부
        GetButton((int)Buttons.BossDungeonEnterButton).interactable = bossTicket >= 1;
        GetButton((int)Buttons.GoldDungeonEnterButton).interactable = goldTicket >= 1;

        // 광고 버튼 표시 조건: 열쇠가 0개이고, 오늘 광고 보상을 받지 않았을 경우
        bool canShowBossAd =
            bossTicket == 0 &&
            Managers.SaveLoad.SaveData.timeData.isBossDungeonAdClaimedToday == false;

        bool canShowGoldAd =
            goldTicket == 0 &&
            Managers.SaveLoad.SaveData.timeData.isGoldDungeonAdClaimedToday == false;

        GetButton((int)Buttons.BossDungeonAdButton).gameObject.SetActive(canShowBossAd);
        GetButton((int)Buttons.GoldDungeonAdButton).gameObject.SetActive(canShowGoldAd);
        
        //Debug.Log($"[BossAd] Active: {canShowBossAd}, Tickets: {bossTicket}, ClaimedToday: {Managers.SaveLoad.SaveData.timeData.isBossDungeonAdClaimedToday}");
        //Debug.Log($"[GoldAd] Active: {canShowGoldAd}, Tickets: {goldTicket}, ClaimedToday: {Managers.SaveLoad.SaveData.timeData.isGoldDungeonAdClaimedToday}");

    }

    private void OnClickBossDungeonEnterButton()
    {
        Managers.Sound.PlayButtonClick();
        
        //난이도 선택창 띄우기
        //level: max 클리어 스테이지 +1
        Managers.UI.ShowPopupUI<UIDungeonLevelSelectPopup>().SetInfo(DungeonType.Boss, bossLevel);
    }

    private void OnClickGoldDungeonEnterButton()
    {
        Managers.Sound.PlayButtonClick();
        
        //난이도 선택창 띄우기
        //level: max 클리어 스테이지 +1
        Managers.UI.ShowPopupUI<UIDungeonLevelSelectPopup>().SetInfo(DungeonType.Gold, goldLevel);
    }
    
    private void OnClickBossDungeonAdButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.Ad.ShowRewardedAd(Define.RewardType.BossDungeonTicket);
    }

    private void OnClickGoldDungeonAdButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.Ad.ShowRewardedAd(Define.RewardType.GoldDungeonTicket);
    }

    private void DungeonTicketChangedEventHandler(DungeonTicketChangedEvent evnt)
    {
        RefreshUI();
    }
}
