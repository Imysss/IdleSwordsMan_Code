using Assets.HeroEditor.Common.Scripts.Common;
using Data;
using Excellcube.EasyTutorial.Utils;
using UnityEngine;
using UnityEngine.Events;

public class ActionTutorialStep : TutorialStep
{
    private UIActionTutorialStep _ui;
    private UnityAction _completeTutorial;
    public  UnityAction CompleteTutorial
    {
        get => _completeTutorial;
        set => _completeTutorial = value;
    }
    public ActionTutorialStep(UIActionTutorialStep ui)
    {
        _ui = ui;
    }
    protected override void ConfigureView()
    {
        if(Data == null)
        {
            Debug.LogError("Fail to configure the view. Data type isn't matched with ActionTutorialPageData");
            return;
        }
        // Highlight target 탐색.
        //SearchDynamicHighlightTarget(ref Data);

        
        // ActionLog 텍스트가 없으면 박스 비활성화
        if (string.IsNullOrEmpty(Data.actionLog))
        {
            _ui.ActionImage.SetActive(false);
            _ui.ActionLogText.SetActive(false);
        }
        else
        {
            _ui.ActionImage.SetActive(true);
            _ui.ActionLogText.SetActive(true);
            _ui.ActionLogText.text = Data.actionLog;
        }
        
        if(Data.targetButtonType != Define.UIButtonType.None)
        {
            GameObject targetButton = Managers.Tutorial.GetTargetButton(Data.targetButtonType);
            if (targetButton == null) return;

            if (!targetButton.TryGetComponent<RectTransform>(out RectTransform target))
            {
                return;
            }
            _ui.UnmaskPanel.transform.parent.gameObject.SetActive(true);
            _ui.UnmaskPanel.fitTarget = target;
            _ui.Indicator.Place(target, Data.indicatorPosition == 1);
            _ui.Indicator.Show(target);
        }
        else
        {
            _ui.UnmaskPanel.transform.parent.gameObject.SetActive(false);
            _ui.Indicator.gameObject.SetActive(false);
        }
        
        if(_completeTutorial == null)
        {
            Debug.LogError("[ActionTutorialPage] CompleteTutorial UnityAction isn't assigned!");
        }
        
        Managers.Tutorial.AddListener(Data.targetButtonType, _completeTutorial);
        //_completeTutorial?.Invoke();
        
    }
    
    // 튜토리얼 클리어 전용 타겟을 찾는 함수
    private void SearchDynamicHighlightTarget(ref TutorialStepData Data) 
    {
        // if(Data.HighlightTarget == null)
        // {
        //     if(Data.DynamicTargetRoot != null)
        //     {
        //         var targets = Data.DynamicTargetRoot.GetComponentsInChildren<TutorialSelectionTarget>();
        //         if(targets.Length == 0)
        //         {
        //             Debug.LogWarning("[ActionTutorialPage] 할당한 DyamicTargetRoot의 children 중에서 TutorialSelectiondTarget을 만족하는 오브젝트를 찾을 수 없음");
        //         }
        //         else
        //         {
        //             var key = Data.DynamicTargetKey;
        //             var targetList = new List<TutorialSelectionTarget>(targets);
        //             var target = targetList.Find(e => e.Key == key);
        //             if(target != null)
        //             {
        //                 Data.HighlightTarget = target.GetComponent<RectTransform>();
        //             }
        //             else
        //             {
        //                 Debug.LogWarning($"[ActionTutorialPage] 탐색된 TutorialSelectionTarget의 리스트 내에서 DynamicTargetKey({Data.DynamicTargetKey})에 해당하는 TutorialSelectionTarget을 찾을 수 없음");
        //             }
        //         }
        //     }
        // }
    }
}
