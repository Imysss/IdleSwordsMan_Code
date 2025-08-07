using UnityEngine;
using static Define;

public class UIGachaResultItem : UIBase
{
    #region UI 기능 리스트
    //ItemGradeBackgroundImage: 아이템 등급 배경 이미지
    //ItemImage: 아이템 이미지
    #endregion

    #region Enum
    enum GameObjects
    {
        EffectObject
    }
    
    enum Images
    {
        ItemGradeBackgroundImage,
        ItemImage,
    }
    #endregion

    private int dataId;
    private bool isFirst;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));

        return true;
    }

    public void SetInfo(int dataId, bool isFirst)
    {
        this.dataId = dataId;
        this.isFirst = isFirst;

        transform.localScale = Vector3.one;

        //장비 아이템의 경우
        if (dataId >= 4000 && dataId < 5000)
        {
            switch (Managers.Data.GearDataDic[dataId].type)
            {
                case GearType.Weapon:
                    GetImage((int)Images.ItemImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 16);
                    SetImageSize(153, 306);
                    break;
                case GearType.Hat:
                    GetImage((int)Images.ItemImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -49);
                    SetImageSize(300, 300);
                    break;
                case GearType.Armor:
                    GetImage((int)Images.ItemImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                    SetImageSize(200, 200);
                    break;
                default:
                    GetImage((int)Images.ItemImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                    SetImageSize(230, 230);
                    break;
            }
        }
        else
        {
            GetImage((int)Images.ItemImage).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            SetImageSize(230, 230);
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        var data = Managers.ItemDatabase.GetItemData(dataId);
        
        //배경 색상 변경
        switch (data.rarity)
        {
            case RarityType.Normal:
                GetImage((int)Images.ItemGradeBackgroundImage).color = UIColors.Normal;
                break;
            case RarityType.Rare:
                GetImage((int)Images.ItemGradeBackgroundImage).color = UIColors.Rare;
                break;
            case RarityType.Epic:
                GetImage((int)Images.ItemGradeBackgroundImage).color = UIColors.Epic;
                break;
            case RarityType.Unique:
                GetImage((int)Images.ItemGradeBackgroundImage).color = UIColors.Unique;
                break;
            case RarityType.Legendary:
                GetImage((int)Images.ItemGradeBackgroundImage).color = UIColors.Legendary;
                break;
        }
        
        //이미지 적용
        Sprite spr = Managers.Resource.Load<Sprite>(data.dataId + ".sprite");
        GetImage((int)Images.ItemImage).sprite = spr;
        
        //처음 획득한 경우에만 이펙트 On
        GetObject((int)GameObjects.EffectObject).SetActive(isFirst);
    }
    
    private void SetImageSize(float width, float height)
    {
        RectTransform rect = GetImage((int)Images.ItemImage).GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
    }
}
