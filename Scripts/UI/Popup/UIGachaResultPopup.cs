using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using DG.Tweening;
using TMPro; 

public class UIGachaResultPopup : UIPopup
{
    #region UI 기능 리스트
    //ResultContentScrollObject: 가챠 결과 item들 생성되는 위치
    //ExpSlider: 경험치 슬라이더
    //ExpValueText: 경험치 텍스트
    //Summon11Button: 해당 타입 소환 11회
    //Summon35Button: 해당 타입 소환 35회
    //ExitButton: 나가기
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        ResultsContentScrollObject,
        ExpSlider
    }

    enum Buttons
    {
        Background,
        ResultContentObject,
        Summon11Button,
        Summon35Button,
        ExitButton
    }

    enum Texts
    {
        ExpValueText,
        GemValueText,
    }
    #endregion

    private GachaType type;
    private List<(int dataId, bool isNew)> items = new List<(int dataId, bool isNew)>();

    private bool _isSkip = false;
    private Coroutine _showGachaCoroutine;

    private const float GachaSfxDelay = 0.15f;

    private void OnEnable()
    {
        PopupOpenAnimation(GetObject((int)GameObjects.ContentObject));
    }
    
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        
        GetButton((int)Buttons.Summon11Button).gameObject.BindEvent(OnClickSummon11Button);
        GetButton((int)Buttons.Summon35Button).gameObject.BindEvent(OnClickSummon35Button);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);
        
        GetButton((int)Buttons.Background).gameObject.BindEvent(OnClickBackgroundButton);
        GetButton((int)Buttons.ResultContentObject).gameObject.BindEvent(OnClickResultContentObjectButton);
        
        Managers.Tutorial.AddUIButton(UIButtonType.Exit, GetButton((int)Buttons.ExitButton).gameObject);
        
        return true;
    }

    public void SetInfo(GachaType type, List<(int dataId, bool isNew)> result, int prevExp, int prevLevel)
    {
        this.type = type;
        items = result;

        _isSkip = false;
        
        if (_showGachaCoroutine != null)
            StopCoroutine(_showGachaCoroutine);

        // 여기서 이전 경험치 넘겨줌
        RefreshUI(prevExp, prevLevel);
    }

    private void RefreshUI(int? prevExp = null, int? prevLevel = null)
    {
        GetText((int)Texts.GemValueText).text = Managers.Game.Gems.ToString();
        
        Dictionary<GachaType, int> gachaExp = Managers.Gacha.GachaExp;
        Dictionary<GachaType, int> gachaLevel = Managers.Gacha.GachaLevel;

        Slider slider = GetObject((int)GameObjects.ExpSlider).GetComponent<Slider>();
        TextMeshProUGUI expText = GetText((int)Texts.ExpValueText);
        RectTransform sliderWrapper = GetObject((int)GameObjects.ExpSlider).transform.parent.GetComponent<RectTransform>();

        int currentExp = gachaExp[type];
        int currentLevel = gachaLevel[type];

        int levelToUse = prevLevel ?? currentLevel;
        float maxExp = Managers.Data.GachaLevelTableDataDic[levelToUse].experience;
        

        float targetRatio = (float)currentExp / maxExp;
        float prevRatio = prevExp.HasValue ? (float)prevExp.Value / maxExp : slider.value;

        // 레벨업인 경우
        if (targetRatio < prevRatio)
        {
            AnimateSlider(slider, expText, prevRatio, 1f, maxExp, 0.6f).OnComplete(() =>
            {
                sliderWrapper.DOShakeAnchorPos(
                    duration: 1f,
                    strength: new Vector2(40f, 0f),
                    vibrato: 3,
                    randomness: 0f,
                    fadeOut: true
                ).OnComplete(() =>
                {
                    if (currentLevel >= Managers.Data.GachaLevelTableDataDic.Count)
                    {
                        AnimateSlider(slider, expText, 0f, targetRatio, maxExp, 0.6f);
                    }
                    else
                    {
                        float nextMaxExp = Managers.Data.GachaLevelTableDataDic[levelToUse + 1].experience;
                        AnimateSlider(slider, expText, 0f, targetRatio, nextMaxExp, 0.6f);
                    }
                });
            });
        }
        else if (prevRatio != targetRatio)
        {
            // 일반 경험치 증가
            AnimateSlider(slider, expText, prevRatio, targetRatio, maxExp, 0.6f);
        }
        else if (currentLevel == Managers.Data.GachaLevelTableDataDic.Count)
        {
            Debug.Log($"thisthisthis");
            slider.value = 1f;
            expText.text = "MAX";
        }
        else
        {
            AnimateSlider(slider, expText, prevRatio, 1f, maxExp, 0.6f).OnComplete(() =>
            {
                sliderWrapper.DOShakeAnchorPos(
                    duration: 1f,
                    strength: new Vector2(40f, 0f),
                    vibrato: 3,
                    randomness: 0f,
                    fadeOut: true
                ).OnComplete(() =>
                {
                    if (currentLevel >= Managers.Data.GachaLevelTableDataDic.Count)
                    {
                        AnimateSlider(slider, expText, 0f, targetRatio, maxExp, 0.6f);
                    }
                    else
                    {
                        float nextMaxExp = Managers.Data.GachaLevelTableDataDic[levelToUse + 1].experience;
                        AnimateSlider(slider, expText, 0f, targetRatio, nextMaxExp, 0.6f);
                    }
                });
            });
        }
        
        //버튼 잠금
        SetButtonsInteractable(false);

        // 결과 아이템 생성
        GameObject container = GetObject((int)GameObjects.ResultsContentScrollObject);
        container.DestroyChilds();
        _showGachaCoroutine = StartCoroutine(CoShowResultItem(container));
    }

    // 슬라이더 애니메이션을 공통 함수로 추출
    private Tween AnimateSlider(Slider slider, TextMeshProUGUI text, float from, float to, float maxExp, float duration)
    {
        return DOTween.To(() => from, x =>
        {
            slider.value = x;
            if (Managers.Gacha.GachaLevel[type] >= Managers.Data.GachaLevelTableDataDic.Count)
            {
                text.text = "MAX";
            }
            else
            {
                text.text = $"{Mathf.RoundToInt(x * maxExp)}/{(int)maxExp}";
            }
            
        }, to, duration).SetEase(Ease.Linear);
    }
    
    IEnumerator CoShowResultItem(GameObject container)
    {
        float timer = GachaSfxDelay;
        foreach (var (id, isNew) in items)
        {
            UIGachaResultItem gachaResultItem = Managers.UI.MakeSubItem<UIGachaResultItem>();
            gachaResultItem.transform.SetParent(container.transform);
            gachaResultItem.SetInfo(id, isNew);

            if (_isSkip)
                continue;

            if (timer >= GachaSfxDelay)
            {
                Managers.Sound.Play(Sound.Sfx, "GachaResult");
                timer = 0f;
            }
            else
            {
                timer += 0.05f;
            }
            
            yield return new WaitForSecondsRealtime(0.05f);
        }

        SetButtonsInteractable(true);
    }
    
    private void SetButtonsInteractable(bool interactable)
    {
        GetButton((int)Buttons.Summon11Button).interactable = interactable && Managers.Game.Gems >= 500;
        GetButton((int)Buttons.Summon35Button).interactable = interactable && Managers.Game.Gems >= 1500;
        GetButton((int)Buttons.ExitButton).interactable = interactable;
    }
    
    private void OnClickSummon11Button()
    {
        Managers.Sound.PlayButtonClick();
        
        DoGacha(type, 11);
    }

    private void OnClickSummon35Button()
    {
        Managers.Sound.PlayButtonClick();
        
        DoGacha(type, 35);
    }

    private void DoGacha(GachaType type, int count = 1)
    {
        if (_showGachaCoroutine != null)
            StopCoroutine(_showGachaCoroutine);

        int amount = count == 11 ? 500 : count == 35 ? 1500 : 0;

        // 레벨업 전 경험치/레벨 캐싱
        Dictionary<GachaType, int> gachaExp = Managers.Gacha.GachaExp;
        Dictionary<GachaType, int> gachaLevel = Managers.Gacha.GachaLevel;
        int prevExp = gachaExp[type];
        int prevLevel = gachaLevel[type];

        Managers.Game.UseGem(amount);

        _isSkip = false;

        // 실제 가챠 실행
        items = Managers.Gacha.DoGacha(type, count).ToList();

        // 이전 경험치, 레벨 전달
        RefreshUI(prevExp, prevLevel);
    }

    private void OnClickExitButton()
    {
        Managers.Sound.PlayButtonClick();
        
        // 1. 코루틴 강제 중단 (중복 방지)
        if (_showGachaCoroutine != null)
        {
            StopCoroutine(_showGachaCoroutine);
            _showGachaCoroutine = null;
        }
        
        PopupCloseAnimation(GetObject((int)GameObjects.ContentObject), () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }

    private void OnClickBackgroundButton()
    {
        _isSkip = true;
    }

    private void OnClickResultContentObjectButton()
    {
        _isSkip = true;
    }
}
