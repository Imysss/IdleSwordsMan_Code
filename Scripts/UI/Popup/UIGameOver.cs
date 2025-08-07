using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIGameOver : UIPopup
{
    #region UI 기능 리스트

    //StatUpgradeButton
    //ShopButton
    //DungeonButton

    #endregion
    
    #region Enum
    enum GameObjects
    {
        ContentObject
    }
    
    enum Buttons
    {
        StatUpgradeButton,
        ShopButton,
        DungeonButton,
        ExitButton,
    }
    #endregion

    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
        PlayAppearSequential();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        
        GetButton((int)Buttons.StatUpgradeButton).gameObject.BindEvent(OnClickStatUpgradeButton);
        GetButton((int)Buttons.ShopButton).gameObject.BindEvent(OnClickShopButton);
        GetButton((int)Buttons.DungeonButton).gameObject.BindEvent(OnClickDungeonButton);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);
        
        return true;
    }

    private void PlayAppearSequential()
    {
        GameObject object1 = GetButton((int)Buttons.StatUpgradeButton).gameObject;
        GameObject object2 = GetButton((int)Buttons.ShopButton).gameObject;
        GameObject object3 = GetButton((int)Buttons.DungeonButton).gameObject;
        
        //초기화
        InitObject(object1);
        InitObject(object2);
        InitObject(object3);
        
        //순차 애니메이션
        DOTween.Sequence()
            .AppendInterval(0.3f)
            .AppendCallback(() => PlaySingleAppear(object1)).AppendInterval(0.2f)
            .AppendCallback(() => PlaySingleAppear(object2)).AppendInterval(0.2f)
            .AppendCallback(() => PlaySingleAppear(object3));
    }

    private void InitObject(GameObject obj)
    {
        obj.transform.localScale = Vector3.zero;

        var cg = obj.GetComponent<CanvasGroup>();
        if (cg != null)
            cg.alpha = 0;
    }

    private void PlaySingleAppear(GameObject obj)
    {
        obj.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);

        var cg = obj.GetComponent<CanvasGroup>();
        if (cg != null)
            cg.DOFade(1f, 0.3f).SetUpdate(true);
    }
    
    #region Button
    private void OnClickStatUpgradeButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.UI.ClosePopupUI(this);
        Managers.UI.GameSceneUI.ShowHeroToggle();
    }
    
    private void OnClickDungeonButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.UI.ClosePopupUI(this);
        Managers.UI.GameSceneUI.ShowDungeonToggle();
    }

    private void OnClickShopButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.UI.ClosePopupUI(this);
        Managers.UI.GameSceneUI.ShowShopToggle();
    }
    
    private void OnClickExitButton()
    {
        Managers.Sound.PlayButtonClick();
        
        PopupCloseAnimation(GetObject((int)GameObjects.ContentObject), () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }
    #endregion
}
