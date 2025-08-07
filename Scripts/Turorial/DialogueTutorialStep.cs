using Data;
using Excellcube.EasyTutorial.Page;
using UnityEngine;
using UnityEngine.Events;

public class DialogueTutorialStep : TutorialStep
{
    private UIDialogueTutorialStep _ui;
    private UnityAction _completeTutorial;
    public  UnityAction CompleteTutorial
    {
        get => _completeTutorial;
        set => _completeTutorial = value;
    }

    public DialogueTutorialStep(UIDialogueTutorialStep ui)
    {
        _ui = ui;
    }

    protected override void ConfigureView()
    {
        if(Data == null)
        {
            Debug.LogError("[DialogTutorialStep] Fail to configure the view. Data type isn't matched with DialogTutorialStepData");
            return;
        }

        if(Data.leftSpriteName != null)
        {
            _ui.RightImage.gameObject.SetActive(false);
            _ui.RightImage.sprite = null;

            _ui.LeftImage.gameObject.SetActive(true);
            _ui.LeftImage.sprite = Managers.Resource.Load<Sprite>(Data.leftSpriteName + ".sprite");
        }
        else if(Data.rightSpriteName != null)
        {
            _ui.RightImage.gameObject.SetActive(true);
            _ui.RightImage.sprite = Managers.Resource.Load<Sprite>(Data.rightSpriteName + ".sprite");

            _ui.LeftImage.gameObject.SetActive(false);
            _ui.LeftImage.sprite = null;
        }
        else
        {
            _ui.RightImage.gameObject.SetActive(false);
            _ui.RightImage.sprite = null;
            _ui.LeftImage.gameObject.SetActive(false);
            _ui.LeftImage.sprite = null;
        }

        _ui.NameText.text = Data.npcName;

        _ui.AddClickAction(TouchView);

        // 화면 구성이 끝나면 텍스트 타이핑 시작.
        _ui.StartTyping(Data.dialogueText);
    }

    private void TouchView()
    {
        if(_ui.IsTyping())
        {
            _ui.SkipTyping();
        }
        else
        {
            if(_completeTutorial == null)
            {
                Debug.LogError("[DialogTutorialStep] CompleteTutorial UnityAction isn't assigned!");
            }
            
            _completeTutorial?.Invoke();
        }
    }
}
