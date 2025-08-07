using System;
using UnityEngine;

public class UIParticleEffect : UIPopup
{
    #region UI 기능 리스트

    // TransitionEffect : 스테이지 전환 이팩트

    #endregion

    #region Enum

    private Canvas canvas;

    enum GameObjects
    {
        TransitionEffect
    }

    #endregion

    private void OnEnable()
    {
        EventBus.Subscribe<TransitionStartEvent>(OnTransitionStartHandler);
        EventBus.Subscribe<TransitionEndEvent>(OnTransitionEndHandler);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<TransitionStartEvent>(OnTransitionStartHandler);
        EventBus.UnSubscribe<TransitionEndEvent>(OnTransitionEndHandler);
    }

    public override bool Init()
    {
        if(base.Init() == false)
            return false;
        
        canvas = GetComponent<Canvas>();
        
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 100;
        
        BindObject(typeof(GameObjects));
        
        GetObject((int)(GameObjects.TransitionEffect)).SetActive(false);

        return true;
    }

    private void OnTransitionStartHandler(TransitionStartEvent evnt)
    {
        GetObject((int)(GameObjects.TransitionEffect)).SetActive(true);
    }

    private void OnTransitionEndHandler(TransitionEndEvent evnt)
    {
        GetObject((int)(GameObjects.TransitionEffect)).SetActive(false);
    }
}
