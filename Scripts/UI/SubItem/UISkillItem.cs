using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UISkillItem : UIBase
{
    #region UI 기능 리스트
    //UISkillItem: 버튼, 클릭 -> 스킬 정보 팝업
    //SkillGradeBackgroundImage: 등급에 따라 배경 이미지 변화
    //SkillImage: 스킬 이미지
    //SkillLevelValueText: 스킬 레벨 텍스트
    //SkillExpSlider: 스킬 경험치 슬라이더
    //SkillExpValueText: 스킬 경험치 진행도 (0/13)
    //SkillRedDotImage: 레드닷 이미지
    //EquippedObject: 장착 시 활성화
    //LockedObject: 잠금되어 있을 때 활성화
    #endregion

    #region Enum
    enum GameObjects
    {
        SkillExpSlider,
        RedDotObject,
        RedDotImage,
        EquippedObject,
        LockedObject,
    }

    enum Texts
    {
        SkillLevelValueText,
        SkillExpValueText,
    }

    enum Images
    {
        SkillGradeBackgroundImage,
        Fill,
        SkillImage,
    }
    #endregion

    private SkillState skillData;
    private bool _isBestItem;
    private bool _isEquipped;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        
        gameObject.BindEvent(OnClickSkillItemButton);
        
        return true;
    }

    public void SetInfo(SkillState data)
    {
        //스킬 정보 받아오기
        skillData = data;

        if (data.dataId == 5001)
        {
            Managers.Tutorial.AddUIButton(Define.UIButtonType.SkillItem, gameObject);
        }

        _isEquipped = Managers.Equipment.IsEquipped(skillData.dataId);
        
        RefreshUI();
    }

    private void RefreshUI()
    {
        //색상 설정
        switch (skillData.data.rarity)
        {
            case RarityType.Normal:
                GetImage((int)Images.SkillGradeBackgroundImage).color = UIColors.Normal;
                GetImage((int)Images.Fill).color = UIColors.Normal;
                break;
            case RarityType.Rare:
                GetImage((int)Images.SkillGradeBackgroundImage).color = UIColors.Rare;
                GetImage((int)Images.Fill).color = UIColors.Rare;
                break;
            case RarityType.Epic:
                GetImage((int)Images.SkillGradeBackgroundImage).color = UIColors.Epic;
                GetImage((int)Images.Fill).color = UIColors.Epic;
                break;
            case RarityType.Unique:
                GetImage((int)Images.SkillGradeBackgroundImage).color = UIColors.Unique;
                GetImage((int)Images.Fill).color = UIColors.Unique;
                break;
            case RarityType.Legendary:
                GetImage((int)Images.SkillGradeBackgroundImage).color = UIColors.Legendary;
                GetImage((int)Images.Fill).color = UIColors.Legendary;
                break;
        }
        
        //스킬 이미지 적용
        Sprite spr = Managers.Resource.Load<Sprite>(skillData.dataId + ".sprite");
        GetImage((int)Images.SkillImage).sprite = spr;
        
        //스킬 레벨, 경험치 설정
        string level = (Managers.Data.MergeCostDataDic.Count == skillData.level ? "MAX" : skillData.level.ToString());
        GetText((int)Texts.SkillLevelValueText).text = $"Lv {level}";
        GetObject((int)GameObjects.SkillExpSlider).GetComponent<Slider>().value = (float)skillData.experience / Managers.Data.MergeCostDataDic[skillData.level].mergeCost;
        GetText((int)Texts.SkillExpValueText).text = $"{skillData.experience}/{Managers.Data.MergeCostDataDic[skillData.level].mergeCost}";
        
        //레드닷, 장착, 잠금
        GetObject((int)GameObjects.RedDotObject).SetActive(skillData.canUpgrade);
        GetObject((int)GameObjects.EquippedObject).SetActive(_isEquipped);
        GetObject((int)GameObjects.LockedObject).SetActive(!skillData.isUnlocked);
        GetText((int)Texts.SkillLevelValueText).gameObject.SetActive(skillData.isUnlocked);
    }

    private void OnClickSkillItemButton()
    {
        UISkillInfoPopup skillInfoPopup = Managers.UI.ShowPopupUI<UISkillInfoPopup>();
        skillInfoPopup.SetInfo(skillData);
    }
}
