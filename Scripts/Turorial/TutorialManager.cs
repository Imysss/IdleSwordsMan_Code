using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine.Events;
using UnityEngine.UI;
using static Define;

public class TutorialManager
{
    private Dictionary<int, TutorialStepData> _tutorialStepData;
    private Dictionary<int, TutorialData> _tutorialData;
    private Dictionary<UIButtonType, GameObject> tutorialButtons;
    private UITutorial _uiTutorial;
    private SaveLoadManager _saveLoad;
    
    public bool IsTutorialActive { get; private set; }
    
    public List<int> completedTutorials { get; private set;}

    public void Init(UITutorial uiTutorial)
    {
        _saveLoad = Managers.SaveLoad;
        _tutorialStepData = Managers.Data.TutorialStepDataDic;
        _tutorialData = Managers.Data.TutorialDataDic;
        tutorialButtons = new Dictionary<UIButtonType, GameObject>();
        completedTutorials = new List<int>();
        _uiTutorial = uiTutorial;
    }

    public TutorialStepData GetTutorialStepData(int index) => _tutorialStepData[index];
    public void StartTutorial(int id)
    {
        if (_tutorialData.TryGetValue(id, out TutorialData tutorialData))
        {
            Managers.UI.CloseAllPopupUI();
            _uiTutorial.StartTutorial(tutorialData);
            IsTutorialActive = true;
        }
    }

    public void OnCompleteTutorial(TutorialData tutorial)
    {
        completedTutorials.Add(tutorial.key);
        _saveLoad.SaveData.clearedTutorials = completedTutorials;
        IsTutorialActive = false;
        
        //9901 튜토리얼이 끝난 경우 출석 팝업 띄우기
        if (tutorial.key == 9901)
        {
            Managers.UI.ShowPopupUI<UIAttendancePopup>().SetInfo();
        }
        Managers.Analytics.SendTutorialCompleteEvent(tutorial.key, Managers.Time.TotalPlayTimeSec);
    }

    public void AddUIButton(UIButtonType type, GameObject button)
    {
        if (tutorialButtons.ContainsKey(type))
        {
            tutorialButtons[type] = button;
        }
        else
        {
            tutorialButtons.Add(type, button);
        }
    }

    public GameObject GetTargetButton(UIButtonType type)
    {
        return tutorialButtons.GetValueOrDefault(type);
    }
    
    /// <summary>
    /// 버튼/토글에 '한 번만 실행되고 자동 해제되는' 리스너를 추가
    /// </summary>
    public void AddListener(UIButtonType type, UnityAction action)
    {
        // 1. 딕셔너리에서 GameObject를 가져오기
        if (!tutorialButtons.TryGetValue(type, out GameObject uiObject))
        {
            return;
        }

        // 2. Button 컴포넌트가 있는지 확인하고 리스너를 추가
        if (uiObject.TryGetComponent<Button>(out Button button))
        {
            UnityAction wrapperAction = null;
            wrapperAction = () =>
            {
                action?.Invoke();
                button.onClick.RemoveListener(wrapperAction);
            };
            button.onClick.AddListener(wrapperAction);
            return;
        }

        // 3. Button이 없다면, Toggle 컴포넌트가 있는지 확인하고 리스너를 추가
        if (uiObject.TryGetComponent<Toggle>(out Toggle toggle))
        {
            // Toggle은 onValueChanged 이벤트가 bool 값을 전달하므로, 람다식으로 감싸줌.
            // 여기서는 토글이 켜질 때(isOn == true)만 액션을 실행하도록 처리
            UnityAction<bool> wrapperToggleAction = null;
            wrapperToggleAction = (isOn) =>
            {
                if (isOn)
                {
                    action.Invoke();
                    toggle.onValueChanged.RemoveListener(wrapperToggleAction);
                }
            };
            toggle.onValueChanged.AddListener(wrapperToggleAction);
            return; // 토글을 찾았으므로 함수 종료
        }

        Debug.LogWarning($"[TutorialManager] No Button or Toggle component found on object for type: {type}");
    }
}