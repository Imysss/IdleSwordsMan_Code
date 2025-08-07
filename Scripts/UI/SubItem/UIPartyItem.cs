using Data;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIPartyItem : UIBase
{
    #region UI 기능 리스트
    //UIPartyItem: 버튼, 클릭 -> 동료 정보 팝업
    //PartyGradeBackgroundImage: 등급에 따라 배경 이미지 변화
    //PartyImage: 동료 이미지
    //PartyLevelValueText: 동료 레벨 텍스트
    //PartyExpSlider: 동료 경험치 슬라이더
    //PartyExpValueText: 동료 경험치 진행도 (0/13)
    //PartyRedDotImage: 레드닷 이미지
    //EquippedObject: 장착 시 활성화
    //LockedObject: 잠금되어 있을 때 활성화
    #endregion

    #region Enum
    enum GameObjects
    {
        PartyExpSlider,
        RedDotObject,
        EquippedObject,
        LockedObject,
        BestItemRedDotObject,
    }

    enum Texts
    {
        PartyLevelValueText,
        PartyExpValueText,
    }

    enum Images
    {
        PartyGradeBackgroundImage,
        Fill,
        PartyImage,
    }
    #endregion

    private PartyState partyData;
    private bool _isEquipped;
    private bool _isBestItem;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        
        gameObject.BindEvent(OnClickPartyItemButton);
        gameObject.transform.localScale = Vector3.one;
        
        return true;
    }

    public void SetInfo(PartyState data, bool isBestItem = false)
    {
        //동료 데이터 받아오기
        partyData = data;

        if (data.dataId == 6001)
        {
            Managers.Tutorial.AddUIButton(Define.UIButtonType.PartyItem, gameObject);
        }

        _isEquipped = Managers.Equipment.IsEquipped(partyData.dataId);
        _isBestItem = isBestItem;
        
        RefreshUI();
    }

    private void RefreshUI()
    {
        //색상 변경
        switch (partyData.data.rarity)
        {
            case RarityType.Normal:
                GetImage((int)Images.PartyGradeBackgroundImage).color = UIColors.Normal;
                GetImage((int)Images.Fill).color = UIColors.Normal;
                break;
            case RarityType.Rare:
                GetImage((int)Images.PartyGradeBackgroundImage).color = UIColors.Rare;
                GetImage((int)Images.Fill).color = UIColors.Rare;
                break;
            case RarityType.Epic:
                GetImage((int)Images.PartyGradeBackgroundImage).color = UIColors.Epic;
                GetImage((int)Images.Fill).color = UIColors.Epic;
                break;
            case RarityType.Unique:
                GetImage((int)Images.PartyGradeBackgroundImage).color = UIColors.Unique;
                GetImage((int)Images.Fill).color = UIColors.Unique;
                break;
            case RarityType.Legendary:
                GetImage((int)Images.PartyGradeBackgroundImage).color = UIColors.Legendary;
                GetImage((int)Images.Fill).color = UIColors.Legendary;
                break;
        }
        
        //동료 이미지 적용
        Sprite spr = Managers.Resource.Load<Sprite>(partyData.dataId + ".sprite");
        GetImage((int)Images.PartyImage).sprite = spr;
        
        
        //동료 레벨, 경험치 설정
        string level = (Managers.Data.MergeCostDataDic.Count == partyData.level ? "MAX" : partyData.level.ToString());
        GetText((int)Texts.PartyLevelValueText).text = $"LV {level}";
        GetObject((int)GameObjects.PartyExpSlider).GetComponent<Slider>().value = (float)partyData.experience / Managers.Data.MergeCostDataDic[partyData.level].mergeCost;
        GetText((int)Texts.PartyExpValueText).text = $"{partyData.experience}/{Managers.Data.MergeCostDataDic[partyData.level].mergeCost}";
        
        //레드닷, 장착, 잠금
         GetObject((int)GameObjects.RedDotObject).SetActive(partyData.canUpgrade);
         GetObject((int)GameObjects.EquippedObject).SetActive(_isEquipped);
         GetObject((int)GameObjects.LockedObject).SetActive(!partyData.isUnlocked);
         GetObject((int)GameObjects.BestItemRedDotObject).SetActive(_isBestItem && !_isEquipped );
         GetText((int)Texts.PartyLevelValueText).gameObject.SetActive(partyData.isUnlocked);
    }

    private void OnClickPartyItemButton()
    {
        UIPartyInfoPopup partyInfoPopup = Managers.UI.ShowPopupUI<UIPartyInfoPopup>();
        partyInfoPopup.SetInfo(partyData);
    }
}
