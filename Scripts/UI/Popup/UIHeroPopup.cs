using System;
using Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UIHeroPopup : UIPopup
{
    #region UI 기능 리스트
    //SkillGroupObject: 장착 중인 스킬 그룹
    //AutoSkillButton: 스킬 온/오프 버튼
    //AutoSkillButtonText: 스킬 온/오프 텍스트
    //AutoSkillOnEffectImage: 스킬 온 -> Z축 회전으로 돌아가기

    //Quest Group
    //QuestGroup: 퀘스트 보상 획득 버튼
    //QuestNumberText: 퀘스트 넘버 텍스트
    //QuestDescriptionText: 퀘스트 설명 텍스트
    //QuestRewardImage: 퀘스트 보상 이미지
    //QuestRewardValueText: 퀘스트 보상 값 텍스트
    
    //Upgrade Group
    //StatUpgradeScrollContentObject: StatUpgradeItem 보유 (캐릭터 스탯 올리는 부분)
    #endregion

    #region Enum
    enum GameObjects
    {
        StatUpgradeScrollContentObject,
    }

    enum Buttons
    {
        QuestGroup,
    }

    enum Texts
    {
        QuestNumberText,
        QuestDescriptionText,
        QuestRewardValueText,
    }

    enum Images
    {
        QuestBackgroundImage,
        QuestRewardImage,
    }
    #endregion

    private Tween _questBgTween;

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
        
        GetButton((int)Buttons.QuestGroup).gameObject.BindEvent(OnClickQuestGroupButton);
        
        //튜토리얼 버튼 연결
        Managers.Tutorial.AddUIButton(Define.UIButtonType.MainQuest, GetButton((int)Buttons.QuestGroup).gameObject);
        
        return true;
    }
    
    public void SetInfo()
    {
        RefreshUI();
        RefreshUpgradeUI();
    }

    private void RefreshUI()
    {
        QuestData currentQuestData = Managers.Quest.CurrentMainQuest;

        if (currentQuestData != null)
        {
            //퀘스트 정보 설정
            GetText((int)Texts.QuestNumberText).text = $"퀘스트 {currentQuestData.key - 3000}";
            string descriptionText;
            if(currentQuestData.type == Define.QuestType.StageClear)
            {
                descriptionText = currentQuestData.name;
            }
            else
            {
                descriptionText =
                    $"{currentQuestData.name} ({Managers.Quest.QuestStates[currentQuestData.key].currentCount}/{currentQuestData.conditionCount})";
            }
            GetText((int)Texts.QuestDescriptionText).text = descriptionText;
            Sprite spr = null;
            switch (Managers.Quest.GetQuestRewardData(currentQuestData.key).rewardType)
            {
                case Define.RewardType.Gem:
                    spr = Managers.Resource.Load<Sprite>(Define.GEM_SPRITE_NAME);
                    break;
                case Define.RewardType.BossDungeonTicket:
                    spr = Managers.Resource.Load<Sprite>(Define.BOSS_DUNGEON_KEY_SPRITE_NAME);
                    break;
                case Define.RewardType.GoldDungeonTicket:
                    spr = Managers.Resource.Load<Sprite>(Define.GOLD_DUNGEON_KEY_SPRITE_NAME);
                    break;
            }
            GetImage((int)Images.QuestRewardImage).sprite = spr;
            RewardData reward = Managers.Quest.GetQuestRewardData(currentQuestData.key);
            GetText((int)Texts.QuestRewardValueText).text = reward.amount.ToString();
        
            //퀘스트 진행 상태 설정
            if (Managers.Quest.QuestStates[currentQuestData.key].status == Define.QuestStatus.Completed)
            {
                if (_questBgTween == null || !_questBgTween.IsActive() || !_questBgTween.IsPlaying())
                {
                    AnimateQuestBackground();   //애니메이션 시작
                }
                GetButton((int)Buttons.QuestGroup).interactable = true;
            }
            else
            {
                _questBgTween?.Kill();
                GetImage((int)Images.QuestBackgroundImage).color = Util.HexToColor("003A48");
                GetButton((int)Buttons.QuestGroup).interactable = false;
            }
        }
        else
        {   // 메인 퀘스트를 전부 완료했을 경우, UI 전부 비활성화
            GetText((int)Texts.QuestNumberText).gameObject.SetActive(false);
            GetText((int)Texts.QuestDescriptionText).gameObject.SetActive(false);
            GetImage((int)Images.QuestRewardImage).gameObject.SetActive(false);
            GetText((int)Texts.QuestRewardValueText).gameObject.SetActive(false);
        }
    }

    private void RefreshUpgradeUI()
    {
        //스탯 업그레이드 버튼 클릭 시 스탯 업그레이드하기 -> 현재 스탯 레벨, 다음 능력치, 골드 변경해 주어야 함
        //매번 destroy -> setinfo 형식으로 진행해야 함
        GetObject((int)GameObjects.StatUpgradeScrollContentObject).DestroyChilds();
        foreach (StatUpgradeData data in Managers.Data.StatUpgradeDataDic.Values)
        {
            UIStatUpgradeItem statUpgradeItem =
                Managers.UI.MakeSubItem<UIStatUpgradeItem>(GetObject((int)GameObjects.StatUpgradeScrollContentObject)
                    .transform);
            statUpgradeItem.SetInfo(data);
        }
    }

    private void AnimateQuestBackground()
    {
        //이미지 컴포넌트 받아오기
        Image questBg = GetImage((int)Images.QuestBackgroundImage);
        
        //초기 색상 설정
        questBg.color = Util.HexToColor("003A48");

        //밝은 색으로 갔다가 다시 어두운 색으로 돌아오기 반복
        _questBgTween = questBg.DOColor(Util.HexToColor("4E95AE"), 0.8f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);   //타임 스케일 영향 X
    }
    

    private void OnClickQuestGroupButton()
    {
        Managers.Sound.PlayRewardButtonClick();
        Managers.Quest.CompleteQuest(Managers.Quest.CurrentMainQuest.key);
        RefreshUI();
    }


    private void QuestStateChangedEventHandler(QuestStateChangedEvent evnt)
    {
        RefreshUI();
    }
    
    
}
