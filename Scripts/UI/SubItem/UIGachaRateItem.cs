using UnityEngine;

public class UIGachaRateItem : UIBase
{
    #region UI 기능 리스트
    //ItemNameText: 아이템 이름 텍스트
    //ItemRateValueText: 아이템 확률 텍스트
    #endregion

    #region Enum
    enum Texts
    {
        ItemNameText,
        ItemRateValueText,
    }
    #endregion

    private int id;
    private float rate;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindText(typeof(Texts));
        
        return true;
    }
    
    public void SetInfo(int id, float rate)
    {
        this.id = id;
        this.rate = rate;

        transform.localScale = Vector3.one;

        RefreshUI();
    }

    private void RefreshUI()
    {
        GetText((int)Texts.ItemNameText).text = Managers.ItemDatabase.GetItemData(id).name;
        GetText((int)Texts.ItemRateValueText).text = rate.ToString("P2");
    }
}
