using System;
using System.Collections.Generic;
using System.Dynamic;
using Assets.HeroEditor.Common.Scripts.Common;
using Data;
using UnityEngine;
using static Define;

public class UIGachaListPopup : UIPopup
{
    #region UI 기능 리스트
    //NormalGradeRateValueText: 노멀 등급 총 확률
    //NormalGachaRateListObject: 노멀 등급 아이템 리스트 오브젝트
    //RareGradeRateValueText: 레어 등급 총 확률
    //RareGachaRateListObject: 레어 등급 아이템 리스트 오브젝트
    //EpicGradeRateValueText: 에픽 등급 총 확률
    //EpicGachaRateListObject: 에픽 등급 아이템 리스트 오브젝트
    //UniqueGradeRateValueText: 유니크 등급 총 확률
    //UniqueGachaRateListObject: 유니크 등급 아이템 리스트 오브젝트
    //LegendaryGradeRateValueText: 레전더리 등급 총 확률
    //LegendaryGachaRateListObject: 레전더리 등급 아이템 리스트 오브젝트
    
    //ExitButton: 나가기 버튼
    //PreviousLevelButton: 이전 레벨의 확률표 보기 버튼
    //LevelText: 현재 확률표의 레벨 텍스트
    //NextLevelButton: 다음 레벨의 확률표 보기 버튼
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        NormalGachaRateListObject,
        RareGachaRateListObject,
        EpicGachaRateListObject,
        UniqueGachaRateListObject,
        LegendaryGachaRateListObject,
    }

    enum Buttons
    {
        ExitButton,
        PreviousLevelButton,
        NextLevelButton,
    }

    enum Texts
    {
        NormalGradeRateValueText,
        RareGradeRateValueText,
        EpicGradeRateValueText,
        UniqueGradeRateValueText,
        LegendaryGradeRateValueText,
        LevelText,
    }
    #endregion

    private int level;
    private GachaType type;

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
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);

        return true;
    }

    public void SetInfo(GachaType type)
    {
        //현재 타입이랑 레벨 받아오기
        this.type = type;
        level = Managers.Gacha.GachaLevel[type];
        
        RefreshUI();
    }

    private void RefreshUI()
    {
        GetObject((int)GameObjects.NormalGachaRateListObject).DestroyChilds();
        GetObject((int)GameObjects.RareGachaRateListObject).DestroyChilds();
        GetObject((int)GameObjects.EpicGachaRateListObject).DestroyChilds();
        GetObject((int)GameObjects.UniqueGachaRateListObject).DestroyChilds();
        GetObject((int)GameObjects.LegendaryGachaRateListObject).DestroyChilds();
        
        GachaTableData levelTable = Managers.Data.GachaTableDataDic[level];
        
        float rate;
        if (type == GachaType.Gear)
        {
            foreach (var gear in Managers.Data.GearDataDic.Values)
            {
                List<GearData> candidates = Managers.Gacha.gearsByGrade[gear.rarity];
                switch (gear.rarity)
                {
                    case RarityType.Normal:
                        UIGachaRateItem normalItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Normal / candidates.Count;
                        normalItem.transform.SetParent(GetObject((int)GameObjects.NormalGachaRateListObject).transform);
                        normalItem.SetInfo(gear.dataId, rate);
                        break;
                    case RarityType.Rare:
                        UIGachaRateItem rareItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Rare / candidates.Count;
                        rareItem.transform.SetParent(GetObject((int)GameObjects.RareGachaRateListObject).transform);
                        rareItem.SetInfo(gear.dataId, rate);
                        break;
                    case RarityType.Epic:
                        UIGachaRateItem epicItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Epic / candidates.Count;
                        epicItem.transform.SetParent(GetObject((int)GameObjects.EpicGachaRateListObject).transform);
                        epicItem.SetInfo(gear.dataId, rate);
                        break;
                    case RarityType.Unique:
                        UIGachaRateItem uniqueItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Unique / candidates.Count;
                        uniqueItem.transform.SetParent(GetObject((int)GameObjects.UniqueGachaRateListObject).transform);
                        uniqueItem.SetInfo(gear.dataId, rate);
                        break;
                    case RarityType.Legendary:
                        UIGachaRateItem legendaryItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Legendary / candidates.Count;
                        legendaryItem.transform.SetParent(GetObject((int)GameObjects.LegendaryGachaRateListObject).transform);
                        legendaryItem.SetInfo(gear.dataId, rate);
                        break;
                }
            }
        }
        else if (type == GachaType.Skill)
        {
            foreach (var skill in Managers.Data.SkillDataDic.Values)
            {
                List<SkillData> candidates = Managers.Gacha.skillsByGrade[skill.rarity];
                switch (skill.rarity)
                {
                    case RarityType.Normal:
                        UIGachaRateItem normalItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Normal / candidates.Count;
                        normalItem.transform.SetParent(GetObject((int)GameObjects.NormalGachaRateListObject).transform);
                        normalItem.SetInfo(skill.dataId, rate);
                        break;
                    case RarityType.Rare:
                        UIGachaRateItem rareItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Rare / candidates.Count;
                        rareItem.transform.SetParent(GetObject((int)GameObjects.RareGachaRateListObject).transform);
                        rareItem.SetInfo(skill.dataId, rate);
                        break;
                    case RarityType.Epic:
                        UIGachaRateItem epicItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Epic / candidates.Count;
                        epicItem.transform.SetParent(GetObject((int)GameObjects.EpicGachaRateListObject).transform);
                        epicItem.SetInfo(skill.dataId, rate);
                        break;
                    case RarityType.Unique:
                        UIGachaRateItem uniqueItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Unique / candidates.Count;
                        uniqueItem.transform.SetParent(GetObject((int)GameObjects.UniqueGachaRateListObject).transform);
                        uniqueItem.SetInfo(skill.dataId, rate);
                        break;
                    case RarityType.Legendary:
                        UIGachaRateItem legendaryItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Legendary / candidates.Count;
                        legendaryItem.transform.SetParent(GetObject((int)GameObjects.LegendaryGachaRateListObject).transform);
                        legendaryItem.SetInfo(skill.dataId, rate);
                        break;
                }
            }
        }
        else if (type == GachaType.Party)
        {
            foreach (var party in Managers.Data.PartyDataDic.Values)
            {
                List<PartyData> candidates = Managers.Gacha.partiesByGrade[party.rarity];
                switch (party.rarity)
                {
                    case RarityType.Normal:
                        UIGachaRateItem normalItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Normal / candidates.Count;
                        normalItem.transform.SetParent(GetObject((int)GameObjects.NormalGachaRateListObject).transform);
                        normalItem.SetInfo(party.dataId, rate);
                        break;
                    case RarityType.Rare:
                        UIGachaRateItem rareItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Rare / candidates.Count;
                        rareItem.transform.SetParent(GetObject((int)GameObjects.RareGachaRateListObject).transform);
                        rareItem.SetInfo(party.dataId, rate);
                        break;
                    case RarityType.Epic:
                        UIGachaRateItem epicItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Epic / candidates.Count;
                        epicItem.transform.SetParent(GetObject((int)GameObjects.EpicGachaRateListObject).transform);
                        epicItem.SetInfo(party.dataId, rate);
                        break;
                    case RarityType.Unique:
                        UIGachaRateItem uniqueItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Unique / candidates.Count;
                        uniqueItem.transform.SetParent(GetObject((int)GameObjects.UniqueGachaRateListObject).transform);
                        uniqueItem.SetInfo(party.dataId, rate);
                        break;
                    case RarityType.Legendary:
                        UIGachaRateItem legendaryItem = Managers.UI.MakeSubItem<UIGachaRateItem>();
                        rate = levelTable.Legendary / candidates.Count;
                        legendaryItem.transform.SetParent(GetObject((int)GameObjects.LegendaryGachaRateListObject).transform);
                        legendaryItem.SetInfo(party.dataId, rate);
                        break;
                }
            }
        }

        GetText((int)Texts.NormalGradeRateValueText).text = levelTable.Normal.ToString("P2");
        GetText((int)Texts.RareGradeRateValueText).text = levelTable.Rare.ToString("P2");
        GetText((int)Texts.EpicGradeRateValueText).text = levelTable.Epic.ToString("P2");
        GetText((int)Texts.UniqueGradeRateValueText).text = levelTable.Unique.ToString("P2");
        GetText((int)Texts.LegendaryGradeRateValueText).text = levelTable.Legendary.ToString("P2");

        GetText((int)Texts.LevelText).text = $"LEVEL {level}";

        GetButton((int)Buttons.PreviousLevelButton).interactable = (level > 1);
        GetButton((int)Buttons.NextLevelButton).interactable = (level < Managers.Data.GachaLevelTableDataDic.Count);
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

    private void OnClickExitButton()
    {
        Managers.Sound.PlayButtonClick();
        
        PopupCloseAnimation(GetObject((int)GameObjects.ContentObject), () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }
}
