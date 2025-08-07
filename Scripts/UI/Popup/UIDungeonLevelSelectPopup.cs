using System;
using System.Numerics;
using UnityEngine;
using static Define;

public class UIDungeonLevelSelectPopup : UIPopup
{
    #region UI 기능 리스트
    //DungeonNameTitleText
    //LevelValueText
    //NextLevelButton
    //PreviousLevelButton
    //ExitButton
    //QuickClearButton
    //EnterButton
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject
    }
    
    enum Buttons
    {
        PreviousLevelButton,
        NextLevelButton,
        QuickClearButton,
        EnterButton,
        ExitButton,
    }

    enum Texts
    {
        DungeonNameTitleText,
        LevelValueText,
    }
    #endregion

    private DungeonType type;
    private int level;

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
        
        GetButton((int)Buttons.PreviousLevelButton).gameObject.BindEvent(OnClickPreviousLevelButton);
        GetButton((int)Buttons.NextLevelButton).gameObject.BindEvent(OnClickNextLevelButton);
        GetButton((int)Buttons.EnterButton).gameObject.BindEvent(OnClickEnterButton);
        GetButton((int)Buttons.QuickClearButton).gameObject.BindEvent(OnClickQuickClearButton);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);

        return true;
    }

    public void SetInfo(DungeonType type, int level)
    {
        this.type = type;
        this.level = level;

        RefreshUI();
    }

    private void RefreshUI()
    {
        GetText((int)Texts.DungeonNameTitleText).text = (type == DungeonType.Boss ? "보스 러시" : type == DungeonType.Gold ? "골드 던전" : "");
        GetText((int)Texts.LevelValueText).text = level.ToString("D2");

        GetButton((int)Buttons.PreviousLevelButton).interactable = (level > 1);
        GetButton((int)Buttons.NextLevelButton).interactable = (level < Managers.Data.BossDungeonDataDic.Count);
        GetButton((int)Buttons.QuickClearButton).interactable = Managers.Dungeon.IsDungeonUnlocked(type, level+1);
        GetButton((int)Buttons.EnterButton).interactable = Managers.Dungeon.IsDungeonUnlocked(type, level);
        
        // 튜토리얼 버튼 
        if (type == DungeonType.Boss)
        {
            Managers.Tutorial.AddUIButton(UIButtonType.BossDungeon, GetButton((int)Buttons.EnterButton).gameObject);
        }
        else if (type == DungeonType.Gold)
        {
            Managers.Tutorial.AddUIButton(UIButtonType.GoldDungeon, GetButton((int)Buttons.EnterButton).gameObject);
        }
    }

    private void OnClickPreviousLevelButton()
    {
        Managers.Sound.PlayButtonClick();
        
        level--;
        RefreshUI();
    }

    private void OnClickNextLevelButton()
    {
        Managers.Sound.PlayButtonClick();
        
        level++;
        RefreshUI();
    }

    private void OnClickQuickClearButton()
    {
        Managers.Sound.PlayButtonClick();
        
        if (!Managers.Game.TryUseDungeonTicket(type))
        {
            Managers.UI.ShowToast("티켓이 부족합니다.");
            return;
        }
        

        // 현재 레벨의 보상만 지급 (다음 레벨 해금 X)
        if (type == DungeonType.Gold)
        {
            if (Managers.Data.GoldDungeonDataDic.TryGetValue(level, out var goldData))
            {
                BigInteger rewardInt = NumberFormatter.Parse(goldData.reward);
                Managers.Game.AddGold(rewardInt);
            }
        }
        else if (type == DungeonType.Boss)
        {
            if (Managers.Data.BossDungeonDataDic.TryGetValue(level, out var bossData))
            {
                Managers.Game.AddGem(bossData.reward);
            }
        }

        // 티켓 차감
        Managers.Game.TryUseDungeonTicket(type);

        // 팝업 UI 출력만 (해금 없음)
        Managers.UI.ClosePopupUI(this);
        Managers.UI.ShowPopupUI<UIDungeonClearPopup>().SetInfo(type, level);
    }

    private void OnClickEnterButton()
    {
        //level과 type 넣어서 던전 실행
        Managers.Dungeon.StartDungeon(type, level);
        //현재 팝업 닫고 DungeonScenePopup 호출
        Managers.UI.ClosePopupUI(this);
        Managers.UI.ShowPopupUI<UIDungeonScenePopup>().SetInfo(type, level);
    }

    private void OnClickExitButton()
    {
        PopupCloseAnimation(GetObject((int)GameObjects.ContentObject), () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }
}
