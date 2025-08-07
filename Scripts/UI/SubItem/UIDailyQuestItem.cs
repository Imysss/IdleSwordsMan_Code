using System;
using Assets.HeroEditor.Common.Scripts.Common;
using Data;
using UnityEngine;
using UnityEngine.UI;

public class UIDailyQuestItem : UIBase
{
    #region UI 기능 리스트
    //RewardImage
    //RewardValueText
    //QuestNameText
    //QuestConditionSlider
    //QuestConditionValueText
    //ClaimButton
    //ClaimButtonText
    //ClaimButtonClearObject
    #endregion

    #region Enum
    enum GameObjects
    {
        QuestConditionSlider,
        ClaimButtonText,
        ClaimClearObject,
    }

    enum Buttons
    {
        ClaimButton,
    }

    enum Texts
    {
        RewardValueText,
        QuestNameText,
        QuestConditionValueText,
    }

    enum Images
    {
        RewardImage,
    }
    #endregion

    private QuestData data;

    private void OnEnable()
    {
        EventBus.Subscribe<QuestStateChangedEvent>(QuestStateChangedEventHandler);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<QuestStateChangedEvent>(QuestStateChangedEventHandler);
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetButton((int)Buttons.ClaimButton).gameObject.BindEvent(OnClickClaimButton);
        return true;
    }

    public void SetInfo(QuestData data)
    {
        this.data = data;

        RefreshUI();
    }

    private void RefreshUI()
    {
        //Sprite spr = Managers.Resource.Load<Sprite>($"{data.rewardKey}Icon.sprite");
        //GetImage((int)Images.RewardImage).sprite = spr;
        RewardData reward = Managers.Quest.GetQuestRewardData(data.key);
        GetText((int)Texts.RewardValueText).text = reward.amount.ToString();
        GetText((int)Texts.QuestNameText).text = data.name;
        GetObject((int)GameObjects.QuestConditionSlider).GetComponent<Slider>().value =
            (float)Managers.Quest.QuestStates[data.key].currentCount / data.conditionCount;
        GetText((int)Texts.QuestConditionValueText).text =
            $"{Managers.Quest.QuestStates[data.key].currentCount}/{data.conditionCount}";


        Define.QuestStatus status = Managers.Quest.QuestStates[data.key].status;
        
        GetButton((int)Buttons.ClaimButton).interactable = status == Define.QuestStatus.Completed;
        GetButton((int)Buttons.ClaimButton).SetActive(status != Define.QuestStatus.Rewarded);
        GetObject((int)GameObjects.ClaimClearObject).SetActive(status == Define.QuestStatus.Rewarded);
    }

    private void OnClickClaimButton()
    {
        Managers.Sound.PlayRewardButtonClick();
        
        Managers.Quest.CompleteQuest(data.key);
        RefreshUI();
    }

    private void QuestStateChangedEventHandler(QuestStateChangedEvent evnt)
    {
        RefreshUI();
    }
}
