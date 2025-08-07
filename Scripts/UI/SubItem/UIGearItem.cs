using Data;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIGearItem : UIBase
{
    #region UI 기능 리스트
    //GearGradeBackgroundImage: 장비 배경 이미지 (등급에 따라 달라짐)
    //GearImage: 장비 이미지
    //GearLevelValueText: 장비 레벨 텍스트
    //GearExpSlider: 장비 경험치 슬라이더
    //GearExpValueText: 장비 경험치 텍스트 (0/13)
    //RedDotObject: 레드닷 이미지
    //EquippedObject: 장착 이미지
    //LockedObject: 잠금 이미지
    #endregion

    #region MyRegion
    enum GameObjects
    {
        GearExpSlider,
        RedDotObject,
        EquippedObject,
        LockedObject,
        BestItemRedDotObject,
    }

    enum Texts
    {
        GearLevelValueText,
        GearExpValueText,
    }

    enum Images
    {
        GearGradeBackgroundImage,
        Fill,
        GearImage,
    }
    #endregion

    private GearState gearData;
    private bool _isEquipped;
    private bool _isBestItem;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        
        gameObject.BindEvent(OnClickGearItemButton);
        
        return true;
    }

    public void SetInfo(GearState data, bool isBestItem = false)
    {
        //장비 데이터 받아오기
        gearData = data;

        if (gearData.data.type == GearType.Weapon)
        {
            GetImage((int)Images.GearImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 28);
            SetImageSize(153, 307);
        }
        else if (gearData.data.type == GearType.Hat)
        {
            GetImage((int)Images.GearImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -62);
            SetImageSize(390, 390);
        }
        else if (gearData.data.type == GearType.Armor)
        {
            GetImage((int)Images.GearImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            SetImageSize(220, 220);
        }
        else
        {
            GetImage((int)Images.GearImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            SetImageSize(260, 260);
        }

        //낡은 무기를 장착하는 것을 해 볼 것
        if (data.dataId == 4001)
        {
            Managers.Tutorial.AddUIButton(Define.UIButtonType.GearItem, gameObject);
        }
        
        _isEquipped = Managers.Equipment.IsEquipped(gearData.dataId);
        _isBestItem = isBestItem;

        RefreshUI();
    }

    private void RefreshUI()
    {
        //색상 변경
        switch (gearData.data.rarity)
        {
            case RarityType.Normal:
                GetImage((int)Images.GearGradeBackgroundImage).color = UIColors.Normal;
                GetImage((int)Images.Fill).color = UIColors.Normal;
                break;
            case RarityType.Rare:
                GetImage((int)Images.GearGradeBackgroundImage).color = UIColors.Rare;
                GetImage((int)Images.Fill).color = UIColors.Rare;
                break;
            case RarityType.Epic:
                GetImage((int)Images.GearGradeBackgroundImage).color = UIColors.Epic;
                GetImage((int)Images.Fill).color = UIColors.Epic;
                break;
            case RarityType.Unique:
                GetImage((int)Images.GearGradeBackgroundImage).color = UIColors.Unique;
                GetImage((int)Images.Fill).color = UIColors.Unique;
                break;
            case RarityType.Legendary:
                GetImage((int)Images.GearGradeBackgroundImage).color = UIColors.Legendary;
                GetImage((int)Images.Fill).color = UIColors.Legendary;
                break;
        }
        
        //장비 이미지 적용
        Sprite spr = Managers.Resource.Load<Sprite>(gearData.dataId + ".sprite");
        GetImage((int)Images.GearImage).sprite = spr;
        
        //장비 레벨, 경험치 설정
        string level = (Managers.Data.MergeCostDataDic.Count == gearData.level ? "MAX" : gearData.level.ToString());
        GetText((int)Texts.GearLevelValueText).text = $"Lv {level}";
        GetObject((int)GameObjects.GearExpSlider).GetComponent<Slider>().value = (float)gearData.experience / Managers.Data.MergeCostDataDic[gearData.level].mergeCost;
        GetText((int)Texts.GearExpValueText).text = $"{gearData.experience}/{Managers.Data.MergeCostDataDic[gearData.level].mergeCost}";
        
        //레드닷, 장착, 잠금
        GetObject((int)GameObjects.RedDotObject).SetActive(gearData.canUpgrade);
        GetObject((int)GameObjects.EquippedObject).SetActive(_isEquipped);
        GetObject((int)GameObjects.LockedObject).SetActive(!gearData.isUnlocked);
        GetObject((int)GameObjects.BestItemRedDotObject).SetActive(_isBestItem && !_isEquipped);
        GetText((int)Texts.GearLevelValueText).gameObject.SetActive(gearData.isUnlocked);
    }
    
    private void SetImageSize(float width, float height)
    {
        RectTransform rect = GetImage((int)Images.GearImage).GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
    }

    private void OnClickGearItemButton()
    {
        Managers.UI.ShowPopupUI<UIGearInfoPopup>().SetInfo(gearData);
        EventBus.Raise(new GearSelectedEvent(gearData));
    }
}
