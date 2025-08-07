using System;
using System.Collections;
using System.Collections.Generic;
using Assets.HeroEditor.Common.Scripts.Common;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UISkillSlotItem : UIBase
{
    #region UI 기능 리스트

    //UISkillSlotItem: 선택 시 스킬 사용
    //SkillImage: 스킬 이미지
    //SkillCoolTimeImage: 스킬 쿨타임 이미지

    #endregion

    #region Enum
    enum GameObjects
    {
        Indicator,
        UnlockConditionObject,
    }
    
    enum Images
    {
        SkillImage,
        SkillCoolTimeImage,
        SkillGradeImage,
    }

    enum Texts
    {
        UnlockConditionText,
    }
    #endregion

    private SkillState _skillData;
    private Define.SlotType type;
    private int index;
    private Action _skillSwapAction;
    
    private int sortOrder = 100;

    private void OnEnable()
    {
        EventBus.Subscribe<SkillSwapStartEvent>(SkillSwapStartHandler);
        EventBus.Subscribe<SkillSwapEndEvent>(SkillSwapEndHandler);
        
        _skillSwapAction = null;
        sortOrder = 100;
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<SkillSwapStartEvent>(SkillSwapStartHandler);
        EventBus.UnSubscribe<SkillSwapEndEvent>(SkillSwapEndHandler);
        
        if (_skillSwapAction != null)
        {
            gameObject.UnbindEvent(_skillSwapAction);
            _skillSwapAction = null;
        }
        GetObject((int)GameObjects.Indicator).SetActive(false);
    }

    private void Update()
    {
        if (_skillData == null)
            return;

        if (!Managers.Equipment.SkillEquipment.ActiveSkills.ContainsKey(_skillData.dataId))
            return;

        //쿨타임 적용
        float amount = Managers.Equipment.SkillEquipment.ActiveSkills[_skillData.dataId].GetCooldownRatio();
        GetImage((int)Images.SkillCoolTimeImage).fillAmount = amount;
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        
        gameObject.BindEvent(OnClickSkillSlotButton);
        
        GetObject((int)GameObjects.Indicator).SetActive(false);
        GetObject((int)GameObjects.UnlockConditionObject).SetActive(false);
        
        return true;
    }

    public void SetInfo(SkillState data, int index)
    {
        _skillData = data;
        type = Define.SlotType.None;
        this.index = index;
        
        SetImageSize(140, 140);
        RefreshUI();
    }

    public void SetInfo(Define.SlotType type, int index)
    {
        _skillData = null;
        this.type = type;
        this.index = index;

        string conditionText = "";
        switch (index)
        {
            case 1:
                conditionText = "보통 1-7 클리어 시 해금";
                break;
            case 2:
                conditionText = "보통 2-3 클리어 시 해금";
                break;
            case 3:
                conditionText = "보통 2-6 클리어 시 해금";
                break;
            case 4:
                conditionText = "보통 2-9 클리어 시 해금";
                break;
            case 5:
                conditionText = "보통 3-4 클리어 시 해금";
                break;
        }
        
        GetText((int)Texts.UnlockConditionText).text = conditionText;
        
        GetObject((int)GameObjects.Indicator).SetActive(false);
        SetImageSize(140, 140);
        RefreshUI(type);
    }

    private void RefreshUI()
    {
        GetImage((int)Images.SkillImage).gameObject.SetActive(true);
        GetImage((int)Images.SkillCoolTimeImage).gameObject.SetActive(true);
        //스킬 이미지 적용
        Sprite spr = Managers.Resource.Load<Sprite>(_skillData.dataId + ".sprite");
        GetImage((int)Images.SkillImage).sprite = spr;
        
        GetImage((int)Images.SkillGradeImage).gameObject.SetActive(true);
        switch (_skillData.data.rarity)
        {
            case Define.RarityType.Normal:
                GetImage((int)Images.SkillGradeImage).color = Define.UIColors.Normal;
                break;
            case Define.RarityType.Rare:
                GetImage((int)Images.SkillGradeImage).color = Define.UIColors.Rare;
                break;
            case Define.RarityType.Epic:
                GetImage((int)Images.SkillGradeImage).color = Define.UIColors.Epic;
                break;
            case Define.RarityType.Unique:
                GetImage((int)Images.SkillGradeImage).color = Define.UIColors.Unique;
                break;
            case Define.RarityType.Legendary:
                GetImage((int)Images.SkillGradeImage).color = Define.UIColors.Legendary;
                break;
        }
    }

    private void RefreshUI(Define.SlotType type)
    {
        GetImage((int)Images.SkillImage).gameObject.SetActive(true);
        GetImage((int)Images.SkillCoolTimeImage).gameObject.SetActive(false);
        switch (type)
        {
            case Define.SlotType.Empty:
                Sprite empty = Managers.Resource.Load<Sprite>("emptyIcon.sprite");
                GetImage((int)Images.SkillImage).sprite = empty;
                break;
            case Define.SlotType.Locked:
                Sprite locked = Managers.Resource.Load<Sprite>("lockIcon1.sprite");
                GetImage((int)Images.SkillImage).sprite = locked;
                SetImageSize(60, 75);
                break;
        }
        GetImage((int)Images.SkillGradeImage).gameObject.SetActive(false);
    }

    private void SetImageSize(float width, float height)
    {
        RectTransform rect = GetImage((int)Images.SkillImage).GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
    }

    public void PlayUnlockEffect(Action onComplete)
    {
        RectTransform lockImage = GetImage((int)Images.SkillImage).GetComponent<RectTransform>();
        Vector3 originalScale = lockImage.localScale;
        Sequence seq = DOTween.Sequence().SetUpdate(true);
        
        //좌우로 쉐이킹 두 번
        for (int i = 0; i < 2; i++)
        {
            seq.Append(lockImage.DOShakePosition(0.2f, new Vector3(20f, 0, 0), 30, 90, false, true));
            seq.AppendInterval(0.05f);
        }
        
        //펑 사라지는 느낌 
        seq.Append(lockImage.DOScale(0.6f, 0.1f).SetEase(Ease.InBack));
        seq.Append(lockImage.DOScale(1.3f, 0.1f).SetEase(Ease.OutBack));
        seq.Append(lockImage.DOScale(0f, 0.2f).SetEase(Ease.InBack));
        
        //완전히 사라지면 비활성화 처리
        seq.AppendCallback(() =>
        {
            lockImage.localScale = originalScale;
            lockImage.SetActive(false);
            onComplete?.Invoke();
        });
    }

    // 수동 모드일 때만 실행
    private void OnClickSkillSlotButton()
    {
        if (type == Define.SlotType.Locked)
        {
            StartCoroutine(CoShowUnlockConditionPopupUI(GetObject((int)GameObjects.UnlockConditionObject)));
        }
        
        if (Managers.Game.IsAutoSkillOn)
            return;

        if (_skillData == null)
            return;
        

        SkillUseService.TryUseSkill(_skillData.dataId, Managers.Player.PlayerPosition);
    }

    private IEnumerator CoShowUnlockConditionPopupUI(GameObject go)
    {
        go.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        go.SetActive(false);
    }

    private void SkillSwapStartHandler(SkillSwapStartEvent evnt)
    {
        if (_skillData == null) return;
        
        GetObject((int)GameObjects.Indicator).SetActive(true);
        SkillState oldSkill = Managers.Inventory.SkillStates[_skillData.dataId];
        if (oldSkill == null) return;

        _skillSwapAction = () =>
        {
            Managers.Equipment.SkillEquipment.ReplaceEquip(oldSkill, evnt.Data);
            EventBus.Raise(new SkillSwapEndEvent(evnt.Data));
            EventBus.Raise(new SkillChangedEvent());
        };
        gameObject.BindEvent(_skillSwapAction);
    }


    private void SkillSwapEndHandler(SkillSwapEndEvent evnt)
    {
        GetObject((int)GameObjects.Indicator).SetActive(false);
        gameObject.UnbindEvent(_skillSwapAction);
        _skillSwapAction = null;
    }
}
