using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIRotatingGlowEffect : MonoBehaviour
{
    private Image image;
    private float duration = 1f;

    private void Awake()
    {
        image = GetComponent<Image>();
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        PlayEffect();
    }

    private void PlayEffect()
    {
        image.color = new Color(1f, 1f, 1f, 1f);    //투명도 초기화
        transform.localScale = Vector3.zero;                      //작게 시작
        transform.localRotation = Quaternion.identity;            //회전 초기화
        
        Sequence seq = DOTween.Sequence();
        
        //커지기 + 회전 + fade out 동시에
        seq.Append(transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
        seq.Join(((RectTransform)transform).DOLocalRotate(new Vector3(0, 0, -180f), duration, RotateMode.FastBeyond360).SetEase(Ease.Linear));
        seq.Join(image.DOFade(0f, duration).SetEase(Ease.InQuad));
        
        //끝나면 제거
        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });

    }
}
