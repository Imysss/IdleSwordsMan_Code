using System;
using Data;
using UnityEngine;

public class UIBossEffect : UIPopup
{
    #region UI 기능 리스트

    //BossName
    //BossImage

    #endregion
    
    #region Enum
    
    enum Texts
    {
        BossNameText,
    }

    enum Images
    {
        BossImage,
    }
    
    #endregion

    public Canvas canvas;
    private CreatureData _creatureData;
    
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
        }
        
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 600;
        canvas.sortingLayerName = "UI";
        BindText(typeof(Texts));
        BindImage(typeof(Images));
        
        return true;
    }

    public void SetInfo(CreatureData creatureData)
    {
        _creatureData = creatureData;
        
        RefreshUI();
    }

    private void RefreshUI()
    {
        GetText((int)Texts.BossNameText).text = _creatureData.name;
        GetImage((int)Images.BossImage).sprite = Managers.Resource.Load<Sprite>(_creatureData.dataId + ".sprite");
    }
}
