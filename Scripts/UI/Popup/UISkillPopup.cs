using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;

public class UISkillPopup : UIPopup
{
    #region UI 기능 리스트
    //SkillScrollContentObject: 스킬 인벤토리 나열
    
    //UpgradeAllButton: 전부 업그레이드 버튼
    #endregion
    
    #region Enum
    enum GameObjects
    {
        RedDotObject,
        SkillScrollContentObject,
    }

    enum Buttons
    {
        UpgradeAllButton,
    }
    #endregion
    
    private void OnEnable()
    {
        EventBus.Subscribe<SkillChangedEvent>(SkillChangedEventHandler);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<SkillChangedEvent>(SkillChangedEventHandler);
    }
    
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
    
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        
        GetButton((int)Buttons.UpgradeAllButton).gameObject.BindEvent(OnClickUpgradeAllButton);

        return true;
    }
    
    public void SetInfo()
    {
        RefreshUI();
    }
    
    private void RefreshUI()
    {
        //스킬 인벤토리 정보 가져오기
        GameObject container = GetObject((int)GameObjects.SkillScrollContentObject);
        container.DestroyChilds();
        foreach (var data in Managers.Inventory.SkillStates.Values)
        {
            UISkillItem skillItem = Managers.UI.MakeSubItem<UISkillItem>(container.transform);
            skillItem.SetInfo(data);
        }

        bool canAllUpgrade = false;
        foreach (var skill in Managers.Inventory.SkillStates.Values)
        {
            if (skill.canUpgrade)
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
        Dictionary<int, int> beforeLevels = Managers.Inventory.SkillStates.ToDictionary(pair => pair.Key, pair => pair.Value.level);
        
        //모두 업그레이드
        while (Managers.Inventory.SkillStates.Values.Any(s => s.canUpgrade))
        {
            foreach (var skill in Managers.Inventory.SkillStates.Values)
            {
                if (skill.canUpgrade)
                {
                    Managers.Inventory.UpgradeItem(skill.dataId);
                }
            }
        }

        //레벨이 달라진 경우에만 결과값 넣기
        List<UpgradeResult> upgradeResults = new();
        foreach (var kvp in Managers.Inventory.SkillStates)
        {
            int itemId = kvp.Key;
            int oldLevel = beforeLevels[itemId];
            int newLevel = kvp.Value.level;

            if (newLevel > oldLevel)
            {
                upgradeResults.Add(new UpgradeResult(itemId, Define.ItemType.Skill, oldLevel, newLevel));
            }
        }

        Managers.UI.ShowPopupUI<UIUpgradeResultPopup>().SetInfo(upgradeResults);
        
        RefreshUI();
    }

    private void SkillChangedEventHandler(SkillChangedEvent evnt)
    {
        RefreshUI();
    }
}
