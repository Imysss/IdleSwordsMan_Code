using UnityEngine;

public class UIToast : UIBase
{
    enum Texts
    {
        ToastMessageText
    }

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        PopupOpenAnimation(gameObject);
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        Managers.UI.SetCanvas(gameObject, isToast: true);
        
        BindText(typeof(Texts));

        return true;
    }

    public void SetInfo(string msg)
    {
        transform.localScale = Vector3.one;
        GetText((int)Texts.ToastMessageText).text = msg;
    }
}

