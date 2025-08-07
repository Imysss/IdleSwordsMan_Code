using System;
using System.Collections;
using System.Collections.Generic;
using Assets.HeroEditor.Common.Scripts.Common;
using Data;
using Excellcube.EasyTutorial.Utils;
using UnityEngine;
using static Define;

public class UITutorial : UIPopup
{
    private int _lastClearIndex;
    private int _currentTutorialIndex;
    
    private UIDialogueTutorialStep _uiDialogueTutorialStep;
    private UIActionTutorialStep _uiActionTutorialStep;

    // 진행할 튜토리얼의 모든 단계 모음
    private TutorialData _tutorialData;
    
    private DialogueTutorialStep _dialogTutorialStep;
    private ActionTutorialStep _actionTutorialStep;
    
    private TutorialStepData _currentTutorialData;
    
    private TouchBlockView _touchBlockView;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Canvas canvas = Util.GetOrAddComponent<Canvas>(gameObject);
        canvas.sortingOrder = 101;
        
        _touchBlockView = GetComponentInChildren<TouchBlockView>(true);
        
        _uiDialogueTutorialStep = GetComponentInChildren<UIDialogueTutorialStep>(true);
        _uiActionTutorialStep = GetComponentInChildren<UIActionTutorialStep>(true);

        _dialogTutorialStep = new DialogueTutorialStep(_uiDialogueTutorialStep);
        _actionTutorialStep = new ActionTutorialStep(_uiActionTutorialStep);

        return true;
    }

    public void StartTutorial(TutorialData tutorialSteps)
    {
        // 절전모드에는 튜토리얼 활성화 X
        //if (Managers.SleepMode.IsSleepMode) return;
        
        _tutorialData = tutorialSteps;
        
        _currentTutorialIndex = 0;
        _lastClearIndex = -1;
        
        StartCoroutine(ShowNextTutorials());
    }

    // 다음 튜토리얼 페이지를 표시하는 코루틴
    private IEnumerator ShowNextTutorials()
    {
        _currentTutorialData = null;
        _touchBlockView.SetActive(true);
        FindNextTutorialData();
        
        // 화면 비활성화 시 화면이 깜빡거리는 현상 방지
        yield return new WaitForEndOfFrame();

        HideAllTutorialStepViews();
        
        // 다음 튜토리얼 데이터가 없을 경우(모든 튜토리얼을 클리어한 경우)
        if (_currentTutorialData == null)
        {
            HideAllTutorialStepViews();
            _touchBlockView.SetActive(false);
            Managers.Tutorial.OnCompleteTutorial(_tutorialData);
            yield break;
        }

        yield return new WaitForSeconds(_currentTutorialData.startDelay);

        TutorialStep tutorialStep = CreateTutorialStep();
        
        tutorialStep.ShowUsingData(_currentTutorialData);
    }

    
    // 데이터를 기반으로 튜토리얼 페이지 구성
    private TutorialStep CreateTutorialStep()
    {
        TutorialStep tutorialStep = null;

        if (_currentTutorialData.type == TutorialStepType.Dialogue)
        {
            tutorialStep = _dialogTutorialStep;
            
            _dialogTutorialStep.CompleteTutorial = Complete;
            _uiDialogueTutorialStep.gameObject.SetActive(true);
        }
        else if(_currentTutorialData.type == TutorialStepType.Action) 
        {
            tutorialStep = _actionTutorialStep;
            _touchBlockView.SetActive(false);
            _actionTutorialStep.CompleteTutorial = Complete;
            _uiActionTutorialStep.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Unsupported type of TutorialStep is detected");
        }
        
        return tutorialStep;
    }


    // 다음 튜토리얼 페이지 데이터를 불러오는 메서드
    private void FindNextTutorialData()
    {
        for (int i = _currentTutorialIndex; i < _tutorialData.steps.Count; i++)
        {
            if(i <= _lastClearIndex) continue;

            _currentTutorialIndex = i;
            _currentTutorialData = Managers.Tutorial.GetTutorialStepData(_tutorialData.steps[i]);

            break;
        }
    }
    
    // 모든 튜토리얼 화면을 비활성화
    private void HideAllTutorialStepViews()
    {
        _uiDialogueTutorialStep.gameObject.SetActive(false);
        _uiActionTutorialStep.gameObject.SetActive(false);
    }
    
    // 튜토리얼 페이지 넘어갈 시 호출
    private void Complete()
    {
        _lastClearIndex = _currentTutorialIndex;
        StartCoroutine( ShowNextTutorials());
    }
}
