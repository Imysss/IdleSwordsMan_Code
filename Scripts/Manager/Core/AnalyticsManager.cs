using System;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Analytics;
using UnityEngine;

public class AnalyticsManager : MonoBehaviour
{
    public bool HasUserConsented { get; private set; } = false;
    async void Start()
    {
        try
        {
            // Unity Gaming Services(UGS) 초기화
            await UnityServices.InitializeAsync();
        }
        catch (ServicesInitializationException e)
        {
            Debug.LogError(e.ToString());
        }
        
        GiveConsent();
    }

    /// <summary>
    /// 플레이어의 데이터 수집 동의를 처리하는 메서드.
    /// </summary>
    public void GiveConsent()
    {
        AnalyticsService.Instance.StartDataCollection();
        HasUserConsented = true;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!HasUserConsented) return;
        
        if (pauseStatus)
        {
            //SendSessionEndEvent(Managers.Level.CurrentStageData.key, Managers.Time.DailyPlayTimeSec);
            AnalyticsService.Instance.StopDataCollection();
        }
        else
        {
            AnalyticsService.Instance.StartDataCollection();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!HasUserConsented) return;
        
        if (hasFocus)
        {
            AnalyticsService.Instance.StartDataCollection();
        }
        else
        {
            //SendSessionEndEvent(Managers.Level.CurrentStageData.key, Managers.Time.DailyPlayTimeSec);
            AnalyticsService.Instance.StopDataCollection();
        }
    }

    # region Send Events

    /// <summary>
    /// 레벨 클리어 시 호출할 커스텀 이벤트
    /// </summary>
    public void SendStageClearEvent(int stageId, int timeTaken)
    {
        if (!HasUserConsented) return;
        
        CustomEvent stageClearEvent = new CustomEvent("StageClear")
        {
            {"stage_id", stageId }, 
            {"time_to_clear", timeTaken }
        };

        // 커스텀 이벤트 전송
        AnalyticsService.Instance.RecordEvent(stageClearEvent);
    }

    /// <summary>
    /// 게임 종료 시 호출할 커스텀 이벤트
    /// </summary>
    public void SendSessionEndEvent(int stageId, int playTime)
    {
        if (!HasUserConsented) return;
        
        CustomEvent sessionEndEvent = new CustomEvent("SessionEnd")
        {
            { "stage_id", stageId }, 
            { "play_time", playTime }
        };
        
        AnalyticsService.Instance.RecordEvent(sessionEndEvent);
    }

    /// <summary>
    /// 퀘스트 클리어 시 호출할 커스텀 이벤트
    /// </summary>
    public void SendQuestCompleteEvent(int questId, int timeTaken)
    {
        if (!HasUserConsented) return;
        
        CustomEvent questCompleteEvent = new CustomEvent("QuestComplete")
        {
            { "quest_id", questId },
            { "time_to_clear", timeTaken }
        };
        
        AnalyticsService.Instance.RecordEvent(questCompleteEvent);
    }

    /// <summary>
    /// 튜토리얼 클리어 시 호출될 커스텀 이벤트
    /// </summary>
    public void SendTutorialCompleteEvent(int tutorialId, int timeTaken)
    {
        if (!HasUserConsented) return;
        
        CustomEvent tutorialCompleteEvent = new CustomEvent("TutorialComplete")
        {
            { "tutorial_id", tutorialId }, { "time_to_clear", timeTaken }
        };
        
        AnalyticsService.Instance.RecordEvent(tutorialCompleteEvent);
    }
    
    /// <summary>
    /// 상점에서 아이템 구매 시 호출할 커스텀 이벤트 
    /// </summary>
    public void SendItemPurchaseEvent(int itemId, int itemPrice)
    {
        if (!HasUserConsented) return;
        
        CustomEvent itemPurchaseEvent = new CustomEvent("ItemPurchase")
        {
            { "item_id", itemId }, 
            { "price", itemPrice }
        };

        AnalyticsService.Instance.RecordEvent(itemPurchaseEvent);
    }
    #endregion
}