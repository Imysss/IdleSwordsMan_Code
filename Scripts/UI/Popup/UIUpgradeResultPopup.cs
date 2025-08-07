using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUpgradeResultPopup : UIPopup
{
    #region UI 기능 리스트
    //UpgradeContentObject: 업그레이드 아이템 들어올 공간
    //ClaimButton: 확인 버튼
    //Background: 배경 버튼
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        UpgradeScroll,
        UpgradeContentObject,
    }

    enum Buttons
    {
        Background,
        ClaimButton
    }
    #endregion

    private bool isSkip = false;
    private Coroutine showRoutine;
    private List<UpgradeResult> results;

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
        
        GetButton((int)Buttons.ClaimButton).gameObject.BindEvent(OnClickClaimButton);
        GetButton((int)Buttons.Background).gameObject.BindEvent(OnClickBackgroundButton);
        GetObject((int)GameObjects.UpgradeScroll).BindEvent(OnClickUpgradeScrollButton);
        
        return true;
    }

    public void SetInfo(List<UpgradeResult> results)
    {
        //업그레이드된 아이템 정보 받아오기
        this.results = results;
        
        //스킵 및 코루틴 초기화
        isSkip = false;
        if (showRoutine != null)
            StopCoroutine(showRoutine);
        
        RefreshUI();
    }

    private void RefreshUI()
    {
        StartCoroutine(CoMakeResultItem());
    }

    private IEnumerator CoMakeResultItem()
    {
        GetButton((int)Buttons.ClaimButton).gameObject.SetActive(false);
        Managers.Sound.Play(Define.Sound.Sfx, "UpgradeComplete");
        
        GameObject container = GetObject((int)GameObjects.UpgradeContentObject);
        container.DestroyChilds();
        
        ScrollRect scroll = GetObject((int)GameObjects.UpgradeScroll).GetComponent<ScrollRect>();
        RectTransform containerRect = container.GetComponent<RectTransform>();
        
        foreach (var result in results)
        {
            //1. 아이템 생성
            Managers.UI.MakeSubItem<UIUpgradeResultItem>(container.transform).SetInfo(result); 
            
            //2. 레이아웃 반영 기다리기
            yield return null;
            
            //3. 강제 레이아웃 갱신
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
            
            //4. 맨 아래로 스크롤
            scroll.verticalNormalizedPosition = 0f;
            
            //5. 스킵 확인
            if (isSkip)
                continue;
            
            //6. 약간의 딜레이 (보여주는 연출용)
            yield return new WaitForSecondsRealtime(0.1f);
        }
        
        scroll.verticalNormalizedPosition = 0f;
        
        GetButton((int)Buttons.ClaimButton).gameObject.SetActive(true);
    }

    private void OnClickClaimButton()
    {
        Managers.Sound.PlayButtonClick();
        
        PopupCloseAnimation(GetObject((int)GameObjects.ContentObject), () =>
        {
            Managers.UI.ClosePopupUI(this);
        });
    }

    private void OnClickBackgroundButton()
    {
        isSkip = true;
    }

    private void OnClickUpgradeScrollButton()
    {
        isSkip = true;
    }
}
