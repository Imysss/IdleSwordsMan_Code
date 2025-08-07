using System;
using DG.Tweening;
using UnityEngine;

public class UIBossButtonEffect : MonoBehaviour
{
    private RectTransform rectTransform;
    private Tween _pulseTween;
    
    //크기 설정
    private float scaleUpSize = 1.2f;
    private float scaleDuration = 0.3f;
    private float interval = 3f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        //애니메이션 실행
        if (_pulseTween != null && _pulseTween.IsActive() && _pulseTween.IsPlaying())
            return;
        
        StartPulseLoopAnimation();
    }

    private void StartPulseLoopAnimation()
    {
        _pulseTween = DOTween.Sequence()
            .SetUpdate(true)
            .Append(rectTransform.DOScale(scaleUpSize, scaleDuration).SetEase(Ease.OutQuad))
            .Append(rectTransform.DOScale(1.0f, scaleDuration).SetEase(Ease.InQuad))
            .AppendInterval(interval)
            .SetLoops(-1, LoopType.Restart);
    }
}
