using Data;
using UnityEngine;
using static Define;

public class UIUpgradeResultItem : UIBase
{
    #region UI 기능 리스트
    //ItemBackground: 아이템 등급 배경
    //ItemImage: 아이템 이미지
    //ItemLevelText: 아이템 레벨 텍스트
    #endregion

    #region Enum
    enum Texts
    {
        ItemLevelText,
    }

    enum Images
    {
        ItemBackground,
        ItemImage,
    }
    #endregion

    private UpgradeResult data;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        BindImage(typeof(Images));
        
        return true;
    }

    public void SetInfo(UpgradeResult data)
    {
        this.data = data;

        if (data.type == ItemType.Gear)
        {
            switch (Managers.Inventory.GearStates[data.itemId].data.type)
            {
                case GearType.Weapon:
                    GetImage((int)Images.ItemImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 156);
                    SetImageSize(204, 409);
                    break;
                case GearType.Hat:
                    GetImage((int)Images.ItemImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 56);
                    SetImageSize(400, 400);
                    break;
                case GearType.Armor:
                    GetImage((int)Images.ItemImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -8);
                    SetImageSize(180, 180);
                    break;
                default:
                    GetImage((int)Images.ItemImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                    SetImageSize(180, 180);
                    break;
            }
        }
        else
        {
            GetImage((int)Images.ItemImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            SetImageSize(200, 200);
        }
        
        RefreshUI();
    }

    private void RefreshUI()
    {
        RarityType rarity;
        
        switch (data.type)
        {
            case ItemType.Skill:
                SkillState skill = Managers.Inventory.SkillStates[data.itemId];
                rarity = skill.data.rarity;
                break;
            case ItemType.Party:
                PartyState party = Managers.Inventory.PartyStates[data.itemId];
                rarity = party.data.rarity;
                break;
            case ItemType.Gear:
                GearState gear = Managers.Inventory.GearStates[data.itemId];
                rarity = gear.data.rarity;
                break;
            default:
                rarity = RarityType.Normal;
                break;
        }

        switch (rarity)
        {
            case RarityType.Normal:
                GetImage((int)Images.ItemBackground).color = UIColors.Normal;
                break;
            case RarityType.Rare:
                GetImage((int)Images.ItemBackground).color = UIColors.Rare;
                break;
            case RarityType.Epic:
                GetImage((int)Images.ItemBackground).color = UIColors.Epic;
                break;
            case RarityType.Unique:
                GetImage((int)Images.ItemBackground).color = UIColors.Unique;
                break;
            case RarityType.Legendary:
                GetImage((int)Images.ItemBackground).color = UIColors.Legendary;
                break;
        }
        
        Sprite spr = Managers.Resource.Load<Sprite>(data.itemId + ".sprite");
        GetImage((int)Images.ItemImage).sprite = spr;

        GetText((int)Texts.ItemLevelText).text = $"{data.previousLevel} > <color=#00FE29>{data.newLevel}</color>";
    }
    
    private void SetImageSize(float width, float height)
    {
        RectTransform rect = GetImage((int)Images.ItemImage).GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
    }
}
