using System;
using System.Collections;
using System.Numerics;
using Data;
using DG.Tweening;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class UIBonusOfflineRewardPopup : UIPopup
{
    #region UI 기능 리스트
    //ContentObject
    //RewardItemScrollContentObject
    //ClaimButton
    //ExitButton
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        RewardItemScrollContentObject,
    }

    enum Buttons
    {
        ClaimButton,
        ExitButton,
    }
    #endregion

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
        
        GetButton((int)Buttons.ClaimButton).gameObject.BindEvent(OnClickClaimButton);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);
        
        return true;
    }

    public void SetInfo()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (Managers.Data.OfflineRewardDataDic.TryGetValue(Managers.Level.GetCurrentLevel(),
                out OfflineRewardData offlineReward))
        {
            
        }
        
        GameObject container = GetObject((int)GameObjects.RewardItemScrollContentObject);
        container.DestroyChilds();
        //1. 골드 보상 표시
        BigInteger goldAmount = NumberFormatter.Parse(offlineReward.rewardGold) * 24;
        UIMaterialItem goldItem = Managers.UI.MakeSubItem<UIMaterialItem>(container.transform);
        goldItem.SetInfo(Define.GOLD_SPRITE_NAME, NumberFormatter.FormatNumber(goldAmount));
            
        //2. 아이템 보상 계산 및 표시
        Managers.Time.CalculateBonusOfflineRewardItems();
        foreach (var pair in Managers.Time.BonusRewardCache)
        {
            string iconName = pair.Key + ".sprite";
            UIMaterialItem item = Managers.UI.MakeSubItem<UIMaterialItem>(container.transform);
            item.SetInfo(iconName, pair.Value.ToString());
        }
            
        //3. 다음 프레임에 자식 순서대로 애니메이션 적용
        StartCoroutine(CoApplyAppearAnimation(container));
    }
    
    private IEnumerator CoApplyAppearAnimation(GameObject container)
    {
        yield return null;

        float delayPerItem = 0.05f;

        for (int i = 0; i < container.transform.childCount; i++)
        {
            GameObject child = container.transform.GetChild(i).gameObject;
            PlayAppearAnimation(child, i * delayPerItem);
        }
    }

    private void PlayAppearAnimation(GameObject target, float delay)
    {
        RectTransform rect = target.transform as RectTransform;
        rect.localScale = Vector3.zero;
        
        //페이드 인
        CanvasGroup cg = target.GetOrAddComponent<CanvasGroup>();

        DOTween.Sequence().AppendInterval(delay).Append(rect.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true))
            .Join(cg.DOFade(1f, 0.3f).SetUpdate(true)).Play();
    }
    
    private void OnClickClaimButton()
    {
        Managers.Sound.PlayRewardButtonClick();
        
        Managers.Ad.ShowRewardedAd(Define.RewardType.BonusOfflineReward);
    }

    public void OnClickExitButton()
    {
        Managers.Sound.PlayButtonClick();
        
        PopupCloseAnimation(GetObject((int)GameObjects.ContentObject), () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }
}
