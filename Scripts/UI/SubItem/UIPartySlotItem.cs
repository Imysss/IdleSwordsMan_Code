using System;
using System.Collections;
using Data;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class UIPartySlotItem : UIBase
{
    #region Enum
    enum GameObjects
    {
        Indicator,
        UnlockConditionObject,
    }

    enum Images
    {
        PartyImage,
        PartyGradeImage,
    }
    
    enum Texts
    {
        UnlockConditionText,
    }
    #endregion

    private int index;
    private Define.SlotType type;
    private PartyState _state;
    private int _partyId;
    private Action _partySwapAction;
    
    private void OnEnable()
    {
        EventBus.Subscribe<PartySwapStartEvent>(OnPartySwapStartHandler);
        EventBus.Subscribe<PartySwapEndEvent>(OnPartySwapEndHandler);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<PartySwapStartEvent>(OnPartySwapStartHandler);
        EventBus.UnSubscribe<PartySwapEndEvent>(OnPartySwapEndHandler);
    }
    
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        
        GetObject((int)GameObjects.Indicator).SetActive(false);
        GetObject((int)GameObjects.UnlockConditionObject).SetActive(false);
        
        gameObject.BindEvent(OnSlotButtonClick);
        
        return true;
    }

    public void SetInfo(int dataId, int index)
    {
        type = Define.SlotType.None;
        this.index = index;
        _partyId = dataId;
        _state = Managers.Inventory.PartyStates[_partyId];
        
        SetImageSize(120, 120);

        RefreshUI();
    }

    public void SetInfo(Define.SlotType type, int index)
    {
        this.type = type;
        this.index = index;
        _partyId = -1;
        
        string conditionText = "";
        switch (index)
        {
            case 1:
                conditionText = "보통 1-10 클리어 시 해금";
                break;
            case 2:
                conditionText = "보통 2-10 클리어 시 해금";
                break;
            case 3:
                conditionText = "보통 3-10 클리어 시 해금";
                break;
        }
        GetText((int)Texts.UnlockConditionText).text = conditionText;
        
        SetImageSize(64, 81);

        RefreshUI(type);
    }

    private void RefreshUI()
    {
        Sprite spr = Managers.Resource.Load<Sprite>(_partyId + ".sprite");
        GetImage((int)Images.PartyImage).sprite = spr;
        GetImage((int)Images.PartyGradeImage).gameObject.SetActive(true);
        switch (Managers.Data.PartyDataDic[_partyId].rarity)
        {
            case Define.RarityType.Normal:
                GetImage((int)Images.PartyGradeImage).color = Define.UIColors.Normal;
                break;
            case Define.RarityType.Rare:
                GetImage((int)Images.PartyGradeImage).color = Define.UIColors.Rare;
                break;
            case Define.RarityType.Epic:
                GetImage((int)Images.PartyGradeImage).color = Define.UIColors.Epic;
                break;
            case Define.RarityType.Unique:
                GetImage((int)Images.PartyGradeImage).color = Define.UIColors.Unique;
                break;
            case Define.RarityType.Legendary:
                GetImage((int)Images.PartyGradeImage).color = Define.UIColors.Legendary;
                break;
        }
    }

    private void RefreshUI(Define.SlotType type)
    {
        if (type == Define.SlotType.Empty)
        {
            Sprite empty = Managers.Resource.Load<Sprite>("emptyIcon.sprite"); 
            GetImage((int)Images.PartyImage).sprite = empty;
            GetImage((int)Images.PartyGradeImage).gameObject.SetActive(false);
        }
        else
        {
            Sprite locked = Managers.Resource.Load<Sprite>("lockIcon1.sprite");
            GetImage((int)Images.PartyImage).sprite = locked;
            GetImage((int)Images.PartyGradeImage).gameObject.SetActive(false);
        }
    }
    
    private void SetImageSize(float width, float height)
    {
        RectTransform rect = GetImage((int)Images.PartyImage).GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
    }

    private void OnSlotButtonClick()
    {
        if (type == Define.SlotType.Locked)
        {
            StartCoroutine(CoShowUnlockConditionPopupUI(GetObject((int)GameObjects.UnlockConditionObject)));
        }
    }
    
    private IEnumerator CoShowUnlockConditionPopupUI(GameObject go)
    {
        go.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        go.SetActive(false);
    }

    #region Event Handlers

    private void OnPartySwapStartHandler(PartySwapStartEvent evnt)
    {
        if (_partyId == -1) return;
        
        GetObject((int)GameObjects.Indicator).SetActive(true);

        _partySwapAction = () =>
        {
            Managers.Equipment.PartyEquipment.ReplaceEquip(_state, evnt.Data);
            EventBus.Raise(new PartySwapEndEvent(evnt.Data));
            EventBus.Raise(new PartyChangedEvent());
        };
        gameObject.BindEvent(_partySwapAction);
    }

    private void OnPartySwapEndHandler(PartySwapEndEvent evnt)
    {
        GetObject((int)GameObjects.Indicator).SetActive(false);
        gameObject.UnbindEvent(_partySwapAction);

    }

    #endregion
}
