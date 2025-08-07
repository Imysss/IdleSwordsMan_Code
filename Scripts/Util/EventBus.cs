using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    // 이벤트 타입을 키로, 해당 이벤트 타입의 핸들러(delegate)를 값으로 저장하는 딕셔너리
    private static readonly Dictionary<Type, Delegate> eventDict
        = new Dictionary<Type, Delegate>();
    //특정 타입의 이벤트를 구독하는 메서드
    public static void Subscribe<TEvent>(Action<TEvent> handler)
    {
        Type eventType = typeof(TEvent);
        if(eventDict.TryGetValue(eventType, out Delegate existingHandler))
        {
            //이미 해당 타입의 핸들러가 존재하면, 델리게이트에 추가
            eventDict[eventType] = Delegate.Combine(existingHandler, handler);
        }
        else
        {
            //새로운 타입의 핸들러라면, 딕셔너리에 새로 추가
            eventDict[eventType] = handler;
        }
    }
    //특정 타입의 이벤트 구독을 해제하는 메서드
    public static void UnSubscribe<TEvent>(Action<TEvent> handler)
    {
        Type eventType = typeof(TEvent);
    
        if(eventDict.TryGetValue(eventType, out Delegate existingHandler))
        {
            Delegate newHandler = Delegate.Remove(existingHandler, handler);
            if(newHandler == null)
            {   //해당 타입의 핸들러가 모두 제거되었을 경우
                eventDict.Remove(eventType);
            }
            else
            {
                eventDict[eventType] = newHandler;
                //Debug.Log($"[EventBus] Unsubscribed from {eventType.Name}");
            }
        }
    }
    
    //특정 타입의 이벤트를 발행하는 메서드
    public static void Raise<TEvent>(TEvent eventArgs)
    {
        Type eventType = typeof(TEvent);
        if(eventDict.TryGetValue(eventType, out Delegate existingHandler))
        {   //저장된 델리게이트를 원래의 Action<TEvent> 타입으로 캐스팅하여 호출
            (existingHandler as Action<TEvent>)?.Invoke(eventArgs);
        }
        else
        {
            //Debug.LogWarning($"[EventBus] No subscribers for event {eventType.Name}");
        }
    }
}