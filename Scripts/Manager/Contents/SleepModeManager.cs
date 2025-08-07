using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using static Define;

public class SleepModeManager : MonoBehaviour
{
    // 현재 절전 모드 상태인지 나타내는 프로퍼티
    public bool IsSleepMode { get; private set; } = false;
    public bool isAutoSleepMode;
    
    private float _inactiveTimer = 0f;
    private int _targetFrameRate;

    public void Init()
    {
        isAutoSleepMode = true;
    }
    private void Update()
    {
        if (IsSleepMode || !isAutoSleepMode) return;
        

        // 입력이 감지되면 타이머 리셋
        if (Touchscreen.current != null && Touchscreen.current.press.isPressed)
        {
            _inactiveTimer = 0f;
        }
    
        // 입력이 없다면 타이머 증가
        _inactiveTimer += Time.deltaTime;

        // 타이머가 설정된 시간을 초과하고, 절전 모드가 아니라면 자동 진입
        if (_inactiveTimer >= SLEEP_MODE_DELAY)
        {
            _inactiveTimer = 0f;
            ToggleSleepMode(true);
        }

    }

    // 절전 모드를 켜고 끄는 메서드
    public void ToggleSleepMode(bool isOn)
    {
        // 튜토리얼, 던전 진행중에는 절전모드 X
        if (Managers.Tutorial.IsTutorialActive || Managers.Dungeon.IsDungeonActive || IsSleepMode == isOn ) return;
        
        if (isOn)
        {
            _inactiveTimer = 0f;
            StartSleepMode();
        }
        else
        {
            StopSleepMode();
        }
        
        IsSleepMode = isOn;
    }

    private void StartSleepMode()
    {
        // 절전 모드 UI 활성화
        Managers.UI.CloseAllPopupUI();
        Managers.UI.ShowPopupUI<UISleepMode>();
        
        //최대 프레임 조절
        _targetFrameRate = Application.targetFrameRate;
        Application.targetFrameRate = (int)(_targetFrameRate / 2f);
        
        // 사운드 비활성화
        Managers.Sound.MuteAll(true);
    }

    private void StopSleepMode()
    {
        // 원래 상태로 복구
        
        // 최대 프레임 조절
        Application.targetFrameRate = _targetFrameRate;
        
        // 사운드 활성화
        Managers.Sound.MuteAll(false);
    }
}
