using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.UI;

public class UIPartyPopup : UIPopup
{
    #region UI 기능 리스트
    //PartyScrollContentObject: 동료 인벤토리 나열
    
    //UpgradeAllButton: 전부 업그레이드 버튼
    #endregion

    #region Enum
    enum GameObjects
    {
        PartySlotGroup,
        RedDotObject,
        PartyScrollContentObject,
        UnmaskedPanel,
    }
    
    enum Buttons
    {
        UpgradeAllButton,
        MaskedScreen,
    }
    #endregion

    private UIPartySlotItem _nextUnlockSlot = null;
    private Action _partySwapEventAction;
    
    private void OnEnable()
    {
        EventBus.Subscribe<PartyChangedEvent>(PartyChangedEventHandler);
        EventBus.Subscribe<UnlockSystemEvent>(UnlockSystemEventHandler);
        EventBus.Subscribe<PartySwapStartEvent>(PartySwapStartHandler);
        EventBus.Subscribe<PartySwapEndEvent>(PartySwapEndHandler);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<PartyChangedEvent>(PartyChangedEventHandler);
        EventBus.UnSubscribe<UnlockSystemEvent>(UnlockSystemEventHandler);
        EventBus.UnSubscribe<PartySwapStartEvent>(PartySwapStartHandler);
        EventBus.UnSubscribe<PartySwapEndEvent>(PartySwapEndHandler);
    }
    
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
    
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        
        GetObject((int)GameObjects.UnmaskedPanel).SetActive(false);
        GetButton((int)Buttons.UpgradeAllButton).gameObject.BindEvent(OnClickUpgradeAllButton);
        
        return true;
    }
    
    public void SetInfo()
    {
        RefreshUI();
    }
    
    private void RefreshUI()
    {
        //동료 장착 슬롯 정보 가져오기
        GameObject slotContainer = GetObject((int)GameObjects.PartySlotGroup);
        slotContainer.DestroyChilds();

        int index = 0;
        int currentSlots = Managers.Equipment.PartyEquipment.currentSlots;
        int maxSlots = Managers.Equipment.PartyEquipment.maxSlots;
        
        foreach (PartyState data in Managers.Equipment.PartyEquipment.EquippedSlots)
        {
            if (index >= currentSlots)
                break;
            
            if (data == null)
            {
                UIPartySlotItem emptySlotItem = Managers.UI.MakeSubItem<UIPartySlotItem>(slotContainer.transform);
                emptySlotItem.SetInfo(Define.SlotType.Empty, index);
            }
            else
            {
                UIPartySlotItem partySlotItem = Managers.UI.MakeSubItem<UIPartySlotItem>(slotContainer.transform);
                partySlotItem.SetInfo(data.dataId, index);
            }
            index++;
        }

        // while (index < currentSlots)
        // {
        //     UIPartySlotItem emptySlotItem = Managers.UI.MakeSubItem<UIPartySlotItem>(slotContainer.transform);
        //     emptySlotItem.SetInfo(Define.SlotType.Empty);
        //     index++;
        // }

        while (index < maxSlots)
        {
            UIPartySlotItem lockedSlotItem = Managers.UI.MakeSubItem<UIPartySlotItem>(slotContainer.transform);
            lockedSlotItem.SetInfo(Define.SlotType.Locked, index);
            
            //null이면 저장
            if (_nextUnlockSlot == null && index == currentSlots)
                _nextUnlockSlot = lockedSlotItem;

            index++;
        }
        
        
        //동료 인벤토리 정보 가져오기
        GameObject container = GetObject((int)GameObjects.PartyScrollContentObject);
        container.DestroyChilds();
        List<ItemState> currentBestItems = Managers.Inventory
            .FindBestEquippableItem(Define.ItemType.Party, Managers.Equipment.PartyEquipment.currentSlots);
        foreach (var data in Managers.Inventory.PartyStates.Values)
        {
            UIPartyItem partyItem = Managers.UI.MakeSubItem<UIPartyItem>(container.transform);
            partyItem.SetInfo(data, currentBestItems.Contains(data));
        }
        
        //버튼 활성화
        bool canAllUpgrade = false;
        foreach (var party in Managers.Inventory.PartyStates.Values)
        {
            if (party.canUpgrade)
            {
                canAllUpgrade = true;
            }
        }
        GetButton((int)Buttons.UpgradeAllButton).interactable = canAllUpgrade;
        GetObject((int)GameObjects.RedDotObject).SetActive(canAllUpgrade);
    }

    private void OnClickUpgradeAllButton()
    {
        Managers.Sound.PlayButtonClick();
        
        //업그레이드 전 아이템 레벨 기억
        Dictionary<int, int> beforeLevels = Managers.Inventory.PartyStates.ToDictionary(pair => pair.Key, pair => pair.Value.level);
        
        //모두 업그레이드
        while (Managers.Inventory.PartyStates.Values.Any(p => p.canUpgrade))
        {
            foreach (var party in Managers.Inventory.PartyStates.Values)
            {
                if (party.canUpgrade)
                {
                    Managers.Inventory.UpgradeItem(party.dataId);
                }
            }
        }
        
        //레벨이 달라진 경우에만 결과값 넣기
        List<UpgradeResult> upgradeResults = new();
        foreach (var kvp in Managers.Inventory.PartyStates)
        {
            int itemId = kvp.Key;
            int oldLevel = beforeLevels[itemId];
            int newLevel = kvp.Value.level;

            if (newLevel > oldLevel)
            {
                upgradeResults.Add(new UpgradeResult(itemId, Define.ItemType.Party, oldLevel, newLevel));
            }
        }

        Managers.UI.ShowPopupUI<UIUpgradeResultPopup>().SetInfo(upgradeResults);

        RefreshUI();
    }
    

    private void PartyChangedEventHandler(PartyChangedEvent evnt)
    {
        RefreshUI();
    }

    private void UnlockSystemEventHandler(UnlockSystemEvent evnt)
    {
        RefreshUI();
    }

    private void PartySwapStartHandler(PartySwapStartEvent evnt)
    {
        GetObject((int)GameObjects.UnmaskedPanel).SetActive(true);
        _partySwapEventAction = () =>
        {
            EventBus.Raise(new PartySwapEndEvent(evnt.Data));
        };
        GetButton((int)Buttons.MaskedScreen).gameObject.BindEvent(_partySwapEventAction);
    }

    private void PartySwapEndHandler(PartySwapEndEvent evnt)
    {
        GetButton((int)Buttons.MaskedScreen).gameObject.UnbindEvent(_partySwapEventAction);
        GetObject((int)GameObjects.UnmaskedPanel).SetActive(false);
    }
}
