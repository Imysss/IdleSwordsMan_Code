using System;
using System.Collections;
using Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UISleepMode : UIPopup
{
    #region UI 기능 리스트

    // Time
    // Stage Info
    // Battery Amount
    // Treasure Chest

    #endregion
    
    #region Enum
    
    enum Texts
    {
        TimeText,
        StageInfoText,
        BatteryAmountText,
    }

    enum Images
    {
        TreasureChestButton,
        GaugeImage,
        BatteryFillAmountImage,
    }
    
    #endregion

    private Sequence _unlockSequence;
    private const float UnlockDuration = 1.5f; // 잠금 해제까지 걸리는 시간
    private Coroutine _autoUpdateRoutine;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetImage((int)Images.TreasureChestButton).sprite = Managers.Resource.Load<Sprite>("TreasureChest_Closed.sprite");
        GetImage((int)Images.GaugeImage).fillAmount = 0f;

        _autoUpdateRoutine = StartCoroutine(AutoUpdateRoutine());
            
        return true;
    }

    private void RefreshUI()
    {
        StageData stageData = Managers.Level.CurrentStageData;
        DateTime currentTime = DateTime.Now;
        float batteryAmount = SystemInfo.batteryLevel;  // 배터리 잔량. 0~1값을 가짐
        
        
        // 스테이지 정보 업데이트
        GetText((int)Texts.StageInfoText).text = 
            $"{Managers.Level.GetCurrentMapName()} {stageData.mapIdx}-{stageData.stageIdx}";
        
        // 시간 정보 업데이트
        GetText((int)Texts.TimeText).text =
            $"{currentTime.Hour:D2} : {currentTime.Minute:D2}";
        
        // 배터리 정보 업데이트
        GetText((int)Texts.BatteryAmountText).text = (batteryAmount*100).ToString("F0") + "%";
        GetImage(((int)Images.BatteryFillAmountImage)).fillAmount = batteryAmount;
    }

    IEnumerator AutoUpdateRoutine()
    {
        while (true)
        {
            RefreshUI();
            yield return new WaitForSeconds(10f);
        }
    }
    
    // 버튼 OnPointerDown에서 호출
    public void StartUnlocking()
    {
        // 기존 시퀀스가 있다면 초기화
        _unlockSequence?.Kill();
    
        // 새로운 DOTween 시퀀스 생성
        _unlockSequence = DOTween.Sequence();
        
        // 게이지 채우기 + 상자 흔들기 애니메이션 연출
        _unlockSequence.Append(GetImage((int)Images.GaugeImage)
            .DOFillAmount(1f, UnlockDuration).SetEase(Ease.Linear));
        _unlockSequence.Join(GetImage((int)Images.TreasureChestButton)
            .transform.DOShakePosition(UnlockDuration, 10, 20));

        // 위 애니메이션이 모두 끝나면 상자 열림 연출 실행
        _unlockSequence.AppendCallback(OpenTreasureChest);
    
        // 시퀀스가 모두 완료되면 절전 모드 해제
        _unlockSequence.OnComplete(OpenTreasureChest);
    }

    // 버튼 OnPointerUp에서 호출
    public void CancelUnlocking()
    {
        // 시퀀스를 즉시 중단하고 게이지를 리셋
        _unlockSequence?.Kill();
        GetImage((int)Images.GaugeImage).fillAmount = 0f;
    }
    
    private void OpenTreasureChest()
    {
        GetImage((int)Images.TreasureChestButton).sprite =
            Managers.Resource.Load<Sprite>("TreasureChest_Opened.sprite");
        // 상자 열리는 효과 
        GetImage((int)Images.TreasureChestButton)
            .transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.3f, 1)
            .OnComplete(() =>
            {
                // 모든 연출이 끝나면 절전 모드 해제 및 UI 닫기
                //Managers.SleepMode.ToggleSleepMode(false);
                StopCoroutine(_autoUpdateRoutine);
                _autoUpdateRoutine = null;
                Managers.UI.ClosePopupUI(this);
            });
    }
}
