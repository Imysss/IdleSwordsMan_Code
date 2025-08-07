using System;
using System.Collections;
using UnityEngine;

public class UIAttendancePopup : UIPopup
{
    #region UI 기능 리스트

    //AttendanceRewardGroup
    //ExitButton

    #endregion

    #region Enum

    enum GameObjects
    {
        ContentObject,
        AttendanceRewardGroup,
    }

    enum Buttons
    {
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
        
        PopupOpenAnimation(gameObject);

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);

        return true;
    }

    public void SetInfo()
    {
        RefreshUI();
        
        Canvas canvas = Util.GetOrAddComponent<Canvas>(gameObject);
        canvas.sortingOrder = 120;
    }

    private void RefreshUI()
    {
        GameObject container = GetObject((int)GameObjects.AttendanceRewardGroup);
        container.DestroyChilds();
        foreach (var day in Managers.Data.AttendanceDataDic.Values)
        {
            Managers.UI.MakeSubItem<UIDayItem>(container.transform).SetInfo(day);
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
