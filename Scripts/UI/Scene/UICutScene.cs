using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class UICutScene : UIPopup
{
    #region Enum

    enum GameObjects
    {
        FirstScene,
        SecondScene,
        ThirdScene,
        FourthScene,
    }

    enum Buttons
    {
        //SkipButton,
    }
    
    enum Images
    {
        FirstPlayerImage,
        FirstDialogueImage,
        SecondCaptainEggImage,
        SecondDialogueImage,
        ThirdChickenImage,
        ThirdDialogueImage,
        FourthPlayerImage,
        FourthChickenImage1,
        FourthChickenImage2,
        FourthDialogueImage,
    }

    #endregion

    private CanvasGroup firstSceneGroup;
    private RectTransform firstPlayerImage;
    private CanvasGroup firstDialogueGroup;
    private CanvasGroup secondSceneGroup;
    private RectTransform secondEggImage;
    private CanvasGroup secondDialogueGroup;

    private CanvasGroup thirdSceneGroup;
    private RectTransform thirdChickenImage;
    private RectTransform thirdDialogueImage;
    
    private CanvasGroup fourthSceneGroup;
    private RectTransform fourthPlayerImage;
    private RectTransform fourthChickenImage;
    private Image fourthChickenImage1;
    private Image fourthChickenImage2;
    private CanvasGroup fourthDialogueGroup;

    private Sequence _currentSequence;
    private Action _onComplete;
    
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        
        Canvas canvas = Util.GetOrAddComponent<Canvas>(gameObject);
        canvas.sortingOrder = 120;
        
        //오브젝트 참조
        firstSceneGroup = GetObject((int)GameObjects.FirstScene).GetComponent<CanvasGroup>();
        firstPlayerImage = GetImage((int)Images.FirstPlayerImage).GetComponent<RectTransform>();
        firstDialogueGroup = GetImage((int)Images.FirstDialogueImage).GetComponent<CanvasGroup>();
        
        secondSceneGroup = GetObject((int)GameObjects.SecondScene).GetComponent<CanvasGroup>();
        secondEggImage = GetImage((int)Images.SecondCaptainEggImage).GetComponent<RectTransform>();
        secondDialogueGroup = GetImage((int)Images.SecondDialogueImage).GetComponent<CanvasGroup>();
        
        thirdSceneGroup = GetObject((int)GameObjects.ThirdScene).GetComponent<CanvasGroup>();
        thirdChickenImage = GetImage((int)Images.ThirdChickenImage).GetComponent<RectTransform>();
        thirdDialogueImage = GetImage((int)Images.ThirdDialogueImage).GetComponent<RectTransform>();

        fourthSceneGroup = GetObject((int)GameObjects.FourthScene).GetComponent<CanvasGroup>();
        fourthPlayerImage = GetImage((int)Images.FourthPlayerImage).GetComponent<RectTransform>();
        fourthChickenImage = GetImage((int)Images.FourthChickenImage1).GetComponent<RectTransform>();
        fourthChickenImage1 = GetImage((int)Images.FourthChickenImage1);
        fourthChickenImage2 = GetImage((int)Images.FourthChickenImage2);
        fourthDialogueGroup = GetImage((int)Images.FourthDialogueImage).GetComponent<CanvasGroup>();
        
        //초기 세팅
        firstSceneGroup.alpha = 0;
        firstPlayerImage.anchoredPosition = new Vector2(-590, -208);
        firstDialogueGroup.alpha = 0;
        
        secondSceneGroup.alpha = 0;
        secondEggImage.anchoredPosition = new Vector2(287, -244);
        secondDialogueGroup.alpha = 0;
        
        thirdSceneGroup.alpha = 0;
        thirdChickenImage.anchoredPosition = new Vector2(404, -62);
        thirdDialogueImage.localScale = Vector3.zero;

        fourthSceneGroup.alpha = 0;
        fourthPlayerImage.anchoredPosition = new Vector2(967, 251);
        fourthPlayerImage.localEulerAngles = new Vector3(0, 0, -76);
        fourthChickenImage.anchoredPosition = new Vector2(750, -46);
        fourthChickenImage2.gameObject.SetActive(false);
        fourthDialogueGroup.alpha = 0;
        
        //GetButton((int)Buttons.SkipButton).gameObject.BindEvent(OnClickSkipButton);
        
        return true;
    }

    public void SetInfo()
    {
        PlayFirstScene();
    }

    public void SetOnComplete(Action onComplete)
    {
        _onComplete = onComplete;
    }

    private void PlayFirstScene()
    {
        //컷신 시퀀스 구성
        _currentSequence = DOTween.Sequence().SetUpdate(true);
        
        //첫 번째 컷신 등장 (1초)
        _currentSequence.Append(firstSceneGroup.DOFade(1f, 1f));
        
        //약간 텀 주기
        _currentSequence.AppendInterval(0.3f);
        
        //플레이어 등장
        _currentSequence.Append(firstPlayerImage.DOAnchorPos(new Vector2(-261, -118), 1.5f).SetEase(Ease.InOutSine));
        
        //약간 텀 주기
        _currentSequence.AppendInterval(0.2f);
        
        //말풍선 등장
        _currentSequence.Append(firstDialogueGroup.DOFade(1f, 1f));

        //컷신 끝났을 경우 다음 컷신 호출
        _currentSequence.OnComplete(() => { PlaySecondScene(); });
        
        //실행
        _currentSequence.Play();
    }

    private void PlaySecondScene()
    {
        //컷신 시퀀스 구성
        _currentSequence = DOTween.Sequence().SetUpdate(true);
        
        //컷신 전체 등장
        _currentSequence.Append(secondSceneGroup.DOFade(1f, 1f));
        
        //약간 텀 주기
        _currentSequence.AppendInterval(0.3f);
        
        //에그 대장 걸어오기
        _currentSequence.Append(secondEggImage.DOAnchorPos(new Vector2(73, -162), 1.5f).SetEase(Ease.InOutSine));
        
        //약간 텀 주기
        _currentSequence.AppendInterval(0.2f);
        
        //말풍선 등장
        _currentSequence.Append(secondDialogueGroup.DOFade(1f, 1f));
        
        //컷신 끝났을 경우 다음 컷신 호출
        _currentSequence.OnComplete(() => { PlayThirdScene(); });
        
        //실행
        _currentSequence.Play();
    }

    private void PlayThirdScene()
    {
        //컷신 시퀀스 구성
        _currentSequence = DOTween.Sequence().SetUpdate(true);
        
        //컷신 등장
        _currentSequence.Append(thirdSceneGroup.DOFade(1f, 1f));
        
        //약간 텀 주기
        _currentSequence.AppendInterval(0.3f);
        
        //닭 이동
        _currentSequence.Append(thirdChickenImage.DOAnchorPos(new Vector2(140, -62), 1.2f).SetEase(Ease.InOutSine));
        
        //약간 텀 주기
        _currentSequence.AppendInterval(0.2f);
        
        //말풍선 튀어나오는 애니메이션
        _currentSequence.AppendCallback(() =>
        {
            //기본 크기: 0.55, 최대: 0.7 -> 튀어오르는 느낌으로 반복
            thirdDialogueImage.localScale = Vector3.one * 0.55f;

            //튕기는 시퀀스
            Sequence bounce = DOTween.Sequence().SetUpdate(true);
            bounce.Append(thirdDialogueImage.DOScale(0.7f, 0.15f).SetEase(Ease.OutBack));
            bounce.Append(thirdDialogueImage.DOScale(0.55f, 0.1f).SetEase(Ease.InOutSine));
            bounce.Append(thirdDialogueImage.DOScale(0.65f, 0.1f).SetEase(Ease.OutQuad));
            bounce.Append(thirdDialogueImage.DOScale(0.55f, 0.08f).SetEase(Ease.InOutQuad));
            bounce.Play();
        });
        
        //컷신 끝났을 경우 다음 컷신 호출
        _currentSequence.OnComplete(() => { PlayFourthScene(); });

        //실행
        _currentSequence.Play();
    }

    private void PlayFourthScene()
    {
        //컷신 시퀀스 구성
        _currentSequence = DOTween.Sequence().SetUpdate(true);
        
        //약간 텀 주기
        _currentSequence.AppendInterval(0.5f);
        
        //컷신 등장
        _currentSequence.Append(fourthSceneGroup.DOFade(1f, 1f));
        
        //약간 텀 주기
        _currentSequence.AppendInterval(0.3f);
        
        //플레이어 포물선 + 회전
        _currentSequence.Append(fourthPlayerImage.DOAnchorPos(new Vector2(130, -133), 1.2f).SetEase(Ease.OutQuad)); //포물선 느낌
        _currentSequence.Join(fourthPlayerImage.DOLocalRotate(Vector3.zero, 1.2f)); //회전도 동시에
        
        //약간 텀 주기
        _currentSequence.AppendInterval(0.4f);
        
        //닭 이동
        _currentSequence.Append(fourthChickenImage.DOAnchorPos(new Vector2(387, -46), 0.8f).SetEase(Ease.InOutSine));
        
        //약간 텀 주기
        _currentSequence.AppendInterval(0.2f);
        
        //닭 이미지 변경
        _currentSequence.AppendCallback(() =>
        {
            fourthChickenImage1.gameObject.SetActive(false);
            fourthChickenImage2.gameObject.SetActive(true);
        });
        
        //약간 텀 주기
        _currentSequence.AppendInterval(0.5f);
        
        //말풍선 등장
        _currentSequence.Append(fourthDialogueGroup.DOFade(1f, 1f));

        //약간 텀 주기
        _currentSequence.AppendInterval(1.5f);
        
        //컷신 끝났을 경우 콜백 호출
        _currentSequence.OnComplete(() => { _onComplete?.Invoke(); });
        
        //실행
        _currentSequence.Play();
    }

    private void OnClickSkipButton()
    {
        _currentSequence?.Kill();

        //컷신 완료 콜백 호출
        _onComplete?.Invoke();
    }
}
