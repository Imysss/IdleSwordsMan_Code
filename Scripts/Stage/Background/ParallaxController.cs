using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParallaxController : MonoBehaviour
{
    [SerializeField]
    private List<ParallaxBackground> backgroundLayers;

    // 모든 배경에 적용될 기본 속도
    [SerializeField]
    private float speed = 5f;
    

    private void Start()
    {
        // 게임 시작 시 모든 배경의 초기 속도를 설정
        // 평소에는 멈춰 있으므로 기본 속도를 0으로 설정함
        UpdateAllBackgroundsSpeed(speed);
        StopMovement(); // 평소에는 멈춰 있도록 설정
    }

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

    // 모든 배경의 움직임을 시작시키는 함수
    public void StartMovement()
    {
        UpdateAllBackgroundsSpeed(speed);
        foreach (var layer in backgroundLayers)
        {
            layer.SetMovingState(true);
        }
    }

    // 모든 배경의 움직임을 멈추는 함수
    public void StopMovement()
    {
        foreach (var layer in backgroundLayers)
        {
            layer.SetMovingState(false);
        }
    }
    
    // 스테이지 클리어 시 호출될 '전진 연출' 함수
    public void StartStageTransition(float duration)
    {
        StartCoroutine(TransitionRoutine(duration));
    }

    private IEnumerator TransitionRoutine(float duration)
    {
        // 1. 움직이기 시작
        StartMovement();

        // 2. 지정된 시간만큼 대기
        yield return new WaitForSeconds(duration);

        // 3. 다시 원래 상태(멈춤)로 돌아가기
        StopMovement();
    }
    
    // 모든 배경 레이어의 속도를 업데이트하는 함수
    private void UpdateAllBackgroundsSpeed(float speed)
    {
        foreach (var layer in backgroundLayers)
        {
            layer.SetMasterSpeed(speed);
        }
    }

    private void OnTransitionStartHandler(TransitionStartEvent evnt)
    {
        StartMovement();
    }

    private void OnTransitionEndHandler(TransitionEndEvent evnt)
    {
        StopMovement();
    }
}