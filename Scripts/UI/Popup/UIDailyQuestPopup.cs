using System;
using System.Linq;
using Data;
using UnityEngine;

public class UIDailyQuestPopup : UIPopup
{
    #region UI 기능 리스트
    //DailyQuestScrollContentObject
    //ExitButton
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        DailyQuestScrollContentObject
    }

    enum Buttons
    {
        ExitButton
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
        
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);
        
        return true;
    }

    public void SetInfo()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        Managers.Quest.UpdatePlayTimeQuests();
        GameObject container = GetObject((int)GameObjects.DailyQuestScrollContentObject);
        container.DestroyChilds();
        
        //Completed: 우선순위 0, InProgress: 우선순위 1, Inactive: 우선순위 2, Rewarded: 우선순위 3
        var sortedQuests = Managers.Data.DailyQuestDataDic.Values
            .Select(data => (data, status: Managers.Quest.QuestStates[data.key].status))
            .OrderBy(q => GetSortOrder(q.status))
            .ToList();
        
        //2. 정렬된 순서대로 UI 생성
        foreach (var (data, _) in sortedQuests)
        {
            UIDailyQuestItem dailyQuestItem = Managers.UI.MakeSubItem<UIDailyQuestItem>(container.transform);
            dailyQuestItem.SetInfo(data);
        }
    }

    private int GetSortOrder(Define.QuestStatus status)
    {
        switch (status)
        {
            case Define.QuestStatus.Completed:
                return 0;
            case Define.QuestStatus.InProgress:
                return 1;
            case Define.QuestStatus.Inactive:
                return 2;
            case Define.QuestStatus.Rewarded:
                return 3;
            default:
                return 4;
        }
    }

    private void OnClickExitButton()
    {
        Managers.Sound.PlayButtonClick();
        
        PopupCloseAnimation(GetObject((int)GameObjects.ContentObject), () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }
}
