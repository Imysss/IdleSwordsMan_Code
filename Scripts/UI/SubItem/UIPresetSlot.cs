using Data;
using UnityEngine;

public class UIPresetSlot : UIBase
{
    #region UI 기능 리스트
    //PresetBackground: 배경 색상
    //PresetImage: 아이템 이미지
    #endregion

    #region Enum
    enum Images
    {
        PresetBackground,
        PresetImage
    }
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImage(typeof(Images));

        return true;
    }

    //스킬 프리셋
    public void SetInfo(SkillState skillData)
    {
        SetImageSize(100, 100);
        RefreshUI(skillData);
    }

    //동료 프리셋
    public void SetInfo(PartyState partyData)
    {
        SetImageSize(100, 100);
        RefreshUI(partyData);
    }

    //잠금된 프리셋
    public void SetInfo(string str)
    {
        SetImageSize(43, 54);
        RefreshUI(str);
    }

    //빈 프리셋
    public void SetInfo()
    {
        SetImageSize(100, 100);
        RefreshUI();
    }

    private void RefreshUI(SkillState skillData)
    {
        GetImage((int)Images.PresetBackground).color = Util.GetBackgroundColor(skillData.data.rarity);
        Sprite spr = Managers.Resource.Load<Sprite>(skillData.dataId + ".sprite");
        GetImage((int)Images.PresetImage).sprite = spr;
    }

    private void RefreshUI(PartyState partyData)
    {
        GetImage((int)Images.PresetBackground).color = Util.GetBackgroundColor(partyData.data.rarity);
        Sprite spr = Managers.Resource.Load<Sprite>(partyData.dataId + ".sprite");
        GetImage((int)Images.PresetImage).sprite = spr;
    }

    private void RefreshUI(string str)
    {
        Sprite spr = Managers.Resource.Load<Sprite>(str + ".sprite");
        GetImage((int)Images.PresetImage).sprite = spr;
    }

    private void RefreshUI()
    {
        Color color = GetImage((int)Images.PresetImage).color;
        color.a = 0f;
        GetImage((int)Images.PresetImage).color = color;
    }
    
    private void SetImageSize(float width, float height)
    {
        RectTransform rect = GetImage((int)Images.PresetImage).GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
    }
}
