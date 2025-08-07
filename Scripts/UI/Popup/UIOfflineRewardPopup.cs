using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Data;
using DG.Tweening;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class UIOfflineRewardPopup : UIPopup
{
    #region UI 기능 리스트
    //TotalTimeValueText: 방치 시간 (획득 후 최대 24시간까지 흐른 시간)
    //ResultGoldValueText: 스테이지별로 1시간마다 획득할 수 있는 골드값 텍스트
    //RewardItemScrollContentObject: 보상 아이템 나열하는 스크롤 오브젝트
    //BonusButton: 보너스 획득 버튼
    //ClaimButton: 방치 보상 받기 버튼
    //ClaimButtonText: 받기 버튼 텍스트
    //ExitButton: 나가기 버튼
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        RewardItemScrollContentObject
    }

    enum Buttons
    {
        BonusButton,
        ClaimButton,
        ExitButton,
    }

    enum Texts
    {
        TotalTimeValueText,
        ResultGoldValueText,
        ClaimButtonText,
    }
    #endregion

    private Coroutine _refreshRewardItemRoutine;
    
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
        BindText(typeof(Texts));
        
        GetButton((int)Buttons.BonusButton).gameObject.BindEvent(OnClickBonusButton);
        GetButton((int)Buttons.ClaimButton).gameObject.BindEvent(OnClickClaimButton);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);
        
        RefreshUI();

        StartCoroutine(CoTimeCheck());
        
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
            GetText((int)Texts.ResultGoldValueText).text = $"{offlineReward.rewardGold}/시간";
        }

        if (_refreshRewardItemRoutine != null)
        {
            StopCoroutine(_refreshRewardItemRoutine);
            _refreshRewardItemRoutine = null;
        }

        _refreshRewardItemRoutine = StartCoroutine(CoRefreshRewardItem(offlineReward));
    }

    private IEnumerator CoRefreshRewardItem(OfflineRewardData offlineReward)
    {
        GameObject container = GetObject((int)GameObjects.RewardItemScrollContentObject);
        container.DestroyChilds();

        yield return null;
        
        if (Managers.Time.TimeSinceLastReward.TotalMinutes > Define.OFFLINE_REWARD_MINIMUM_TIME)
        {
            //1. 골드 보상 표시
            BigInteger rewardGold = NumberFormatter.Parse(offlineReward.rewardGold);
            BigInteger goldAmount = Managers.Time.CalculateGoldPerMinute(rewardGold);
            UIMaterialItem goldItem = Managers.UI.MakeSubItem<UIMaterialItem>(container.transform);
            goldItem.SetInfo(Define.GOLD_SPRITE_NAME, NumberFormatter.FormatNumber(goldAmount));
            
            //2. 아이템 보상 계산 및 표시
            Managers.Time.CalculateOfflineRewardItems();
            foreach (var pair in Managers.Time.OfflineRewardItemIds)
            {
                string iconName = pair.Key + ".sprite";
                UIMaterialItem item = Managers.UI.MakeSubItem<UIMaterialItem>(container.transform);
                item.SetInfo(iconName, pair.Value.ToString());
            }
            
            //3. 다음 프레임에 자식 순서대로 애니메이션 적용
            StartCoroutine(CoApplyAppearAnimation(container));
        }

        GetButton((int)Buttons.BonusButton).interactable = Managers.Time.CanReceiveBonusOfflineReward();
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

    IEnumerator CoTimeCheck()
    {
        while (true)
        {
            TimeSpan timeSpan = Managers.Time.TimeSinceLastReward;
            string formattedTime=string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            if (timeSpan == TimeSpan.FromHours(24))
            {
                formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", 24, 0, 0);
            }

            GetText((int)Texts.TotalTimeValueText).text = formattedTime;

            //보상 획득한 지 5분이 지나지 않았다면 버튼 비활성화
            if (timeSpan.TotalMinutes < Define.OFFLINE_REWARD_MINIMUM_TIME)
            {
                TimeSpan remainingTime = TimeSpan.FromMinutes(Define.OFFLINE_REWARD_MINIMUM_TIME) - timeSpan;
                
                //남은 시간 표기
                string remaining = string.Format("{0:D2}분 {1:D2}초", remainingTime.Minutes, remainingTime.Seconds);
                GetText((int)Texts.ClaimButtonText).text = remaining;
                GetButton((int)Buttons.ClaimButton).interactable = false;
            }
            else
            {
                GetText((int)Texts.ClaimButtonText).text = "받기";
                GetButton((int)Buttons.ClaimButton).interactable = true;
            }
            
            yield return new WaitForSecondsRealtime(1);
        }
    }

    private void OnClickBonusButton()
    {
        //보너스 popup ui 생성
        Managers.UI.ShowPopupUI<UIBonusOfflineRewardPopup>().SetInfo();
    }

    private void OnClickClaimButton()
    {
        Managers.Sound.PlayRewardButtonClick();
        
        if (Managers.Data.OfflineRewardDataDic.TryGetValue(Managers.Level.GetCurrentLevel(),
                out OfflineRewardData offlineReward))
        {
            Managers.Time.GiveOfflineReward(offlineReward);
        }
        
        RefreshUI();
    }

    private void OnClickExitButton()
    {
        PopupCloseAnimation(GetObject((int)GameObjects.ContentObject), () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }
}
