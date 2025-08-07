using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UITitleScene : UIScene
{
    #region UI 기능 리스트

    //Background: 버튼
    //LoadingSlider: 리소스 로딩 진행도
    //LoadingValueText: 리소스 로딩 진행도 텍스트
    //LoadingText: 로딩 중 띄울 텍스트
    //StartText: 로딩이 끝나고 띄울 텍스트

    #endregion

    #region Enum
    enum GameObjects
    {
        TitleImage,
        LoadingSlider,
    }

    enum Buttons
    {
        Background,
    }

    enum Texts
    {
        LoadingValueText,
        TitleText,
        LoadingText,
        StartText,
    }
    #endregion

    [SerializeField] private AudioClip titleBgmClip;
    
    private bool _isPreloadDone = false;
    private bool _isDataInitDone = false;

    private int _currentDataCount;
    private int _maxDataCount;

    private List<string> loadingTextList = new List<string>
    {
        "검사님, 오늘은 몇 마리 잡으실 건가요?", "아직 닭한테 쫓기는 중...", "치킨집 가려다가 닭한테 혼남...", "전설은 닭에서 시작된다...",
        "검사님, 오늘도 닭 눈빛에 쫄지 마세요!", "장비를 장착하면 외형이 바뀐다는 사실!", "프로필과 프레임도 변경할 수 있습니다!",
        "뽑기 레벨을 올리면 더 높은 등급이 나올지도?", "오늘도 닭 쫓던 검사 인생, 시작합니다."
    };

    private void Start()
    {
        //매니저 초기화
        Managers.Init();
        
        //TITLE BGM 실행
        Managers.Sound.Play(Define.Sound.Bgm, titleBgmClip);
#if UNITY_ANDROID
        Managers.SaveLoad.LoadFromServer();
#endif
        GetText((int)Texts.LoadingText).text = GetLoadingText();
        //1. 리소스 비동기 로딩 시작
        Managers.Resource.LoadAllAsync<Object>("preload", (key, count, totalCount) =>
        {
            GetObject((int)GameObjects.LoadingSlider).GetComponent<Slider>().value = (float)count / totalCount;
            GetText((int)Texts.LoadingValueText).text = $"{((float)count / totalCount) * 100f:F0}%";
            if (count == totalCount)
            {
                _isPreloadDone = true;
                
                //서버 데이터 로딩 시작
                _currentDataCount = 0;
                _maxDataCount = Managers.Data.GetServerDataCount();

                GetObject((int)GameObjects.LoadingSlider).GetComponent<Slider>().value = 0f;
                StartCoroutine(Managers.Data.InitFromServer(
                    
                    onEachLoad: () =>
                    {
                        _currentDataCount++;

                        AnimateSlider(
                            slider: GetObject((int)GameObjects.LoadingSlider).GetComponent<Slider>(),
                            //from: GetObject((int)GameObjects.LoadingSlider).GetComponent<Slider>().value,
                            //to: (float)_currentDataCount / _maxDataCount,
                            from: 0f,
                            to: 1f,
                            maxCount: _maxDataCount,
                            duration: 0.2f
                        );
                       
                        GetText((int)Texts.LoadingText).text = $"{GetLoadingText()} ({_currentDataCount}/{_maxDataCount})";
                    },
                    onComplete: () =>
                    {
                        _isDataInitDone = true;
                        TryStartGame();
                    }));
            }
        });
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));

        GetObject((int)GameObjects.LoadingSlider).GetComponent<Slider>().value = 0;
        GetText((int)Texts.LoadingValueText).text = "0%";
        GetButton((int)Buttons.Background).interactable = false;
        GetText((int)Texts.LoadingText).gameObject.SetActive(true);
        GetText((int)Texts.StartText).gameObject.SetActive(false);

        GetButton((int)Buttons.Background).gameObject.BindEvent(OnClickBackgroundButton);

        return true;
    }

    private string GetLoadingText()
    {
        int idx = Random.Range(0, loadingTextList.Count);
        return loadingTextList[idx];
    }
    
    private Tween AnimateSlider(Slider slider, float from, float to, float maxCount, float duration)
    {
        return DOTween.To(() => from, x =>
        {
            slider.value = x;
            GetText((int)Texts.LoadingValueText).text = $"{(x * 100f):F0}%";
        }, to, duration).SetEase(Ease.Linear);
    }
    
    private void StartTitleAnimation()
    {
        Transform titleImage = GetObject((int)GameObjects.TitleImage).transform;
        TextMeshProUGUI titleText = GetText((int)Texts.TitleText);
        
        //초기값 설정
        titleImage.localScale = Vector3.one;
        titleText.transform.localScale = Vector3.zero;
        
        Sequence seq = DOTween.Sequence();
        
        //Title Image 먼저 등장
        seq.Append(titleImage.DOScale(1.9f, 0.4f).SetEase(Ease.OutBack));
        seq.Append(titleImage.DOScale(1.7f, 0.2f).SetEase(Ease.InOutSine));
        
        //Title Text 순차 등장
        seq.Append(titleText.transform.DOScale(1.2f, 0.4f).SetEase(Ease.OutBack));
        seq.Append(titleText.transform.DOScale(1.0f, 0.2f).SetEase(Ease.InOutSine));
    }
    
    private void StartTextAnimation()
    {
        GetText((int)Texts.StartText).DOFade(0, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutCubic).Play();
    }

    private void TryStartGame()
    {
        //두 작업 모두 끝났을 경우에만 실행
        if (_isPreloadDone == false || _isDataInitDone == false)
            return;
        
        //리소스 불러오기 끝
        Managers.Resource.IsPreloadComplete = true;

        // 개인정보 수집 동의서 호출
        // if (!Managers.SaveLoad.hasSaveData)
        // {
        //      Managers.UI.ShowPopupUI<UIPrivacyConsentPopup>();
        // }

        //배경 버튼 클릭 가능, 텍스트 교체
        GetText((int)Texts.LoadingText).gameObject.SetActive(false);
        GetText((int)Texts.StartText).gameObject.SetActive(true);
        GetButton((int)Buttons.Background).interactable = true;
        GetObject((int)GameObjects.LoadingSlider).SetActive(false);

        //시작 애니메이션 수행
        StartTextAnimation();
        StartTitleAnimation();
    }

    private void OnClickBackgroundButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.Scene.LoadScene(Define.Scene.GameScene, transform);
    }
}
