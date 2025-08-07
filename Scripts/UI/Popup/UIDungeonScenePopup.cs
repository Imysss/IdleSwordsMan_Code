using System;
using System.Collections;
using UnityEngine;
using DG.Tweening; // ← DOTween 사용
using static Define;

public class UIDungeonScenePopup : UIPopup
{
    #region UI 기능 리스트
    //ProgressImage
    //DescriptionText
    //EscapeButton
    //Fade (UIFade 컴포넌트)
    #endregion

    #region Enum
    enum GameObjects
    {
        ContentObject,
        BossGroup,
        ProgressImage,
        Fade
    }

    enum Buttons
    {
        EscapeButton
    }

    enum Texts
    {
        MapNameText,
        StageText,
        TimeText,
        DescriptionText,
        BossText,
    }
    #endregion

    private DungeonType type;
    private int level;

    private float rotationSpeed = 180f;
    private Coroutine _timeUpdateRoutine;

    private UIFade fade;

    private RectTransform contentTransform; // ContentObject RectTransform 참조

    private void OnEnable()
    {
        EventBus.Subscribe<DungeonStateChangedEvent>(DungeonFinishedEventHandler);
        EventBus.Subscribe<DungeonClearedEvent>(DungeonClearedEventHandler);
        EventBus.Subscribe<DungeonFailedEvent>(DungeonFailedEventHandler);
        EventBus.Subscribe<DungeonBossWaveStartEvent>(DungeonBossWaveStartEventHandler);
        _timeUpdateRoutine = StartCoroutine(CoUpdateTimeText());
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<DungeonStateChangedEvent>(DungeonFinishedEventHandler);
        EventBus.UnSubscribe<DungeonClearedEvent>(DungeonClearedEventHandler);
        EventBus.UnSubscribe<DungeonFailedEvent>(DungeonFailedEventHandler);
        EventBus.UnSubscribe<DungeonBossWaveStartEvent>(DungeonBossWaveStartEventHandler);
        if (_timeUpdateRoutine != null)
            StopCoroutine(_timeUpdateRoutine);
    }

    private void Update()
    {
        GetObject((int)GameObjects.ProgressImage).transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));

        GetButton((int)Buttons.EscapeButton).gameObject.BindEvent(OnClickEscapeButton);

        // UIFade 캐싱
        fade = GetObject((int)GameObjects.Fade).GetComponent<UIFade>();

        // ContentObject RectTransform 캐싱 (UI 구조에 따라 경로 조정 가능)
        contentTransform = GetObject((int)GameObjects.ContentObject).GetComponent<RectTransform>();

        return true;
    }

    public void SetInfo(DungeonType type, int level)
    {
        this.type = type;
        this.level = level;

        GetText((int)Texts.BossText).text = $"1/{Managers.Data.BossDungeonDataDic[level].bossIds.Count}";
        
        RefreshUI();

        // 연출 순서: Fade → 내려오기 → 던전 시작
        StartCoroutine(PlayFadeThenEntranceThenStart());
    }

    private void RefreshUI()
    {
        GetText((int)Texts.MapNameText).text =
            (type == DungeonType.Boss ? "보스 러시" : type == DungeonType.Gold ? "골드 던전" : "");
        GetText((int)Texts.StageText).text = level.ToString("D2");

        string description = "";
        switch (type)
        {
            case DungeonType.Boss:
                description = "강력한 보스들이 몰려옵니다!\n보스들을 처치하고 푸짐한 보상을 획득하세요!";
                break;
            case DungeonType.Gold:
                description = "금화를 잔뜩 머금은 돼지가 근처를 지나갑니다!\n어서 가서 약탈하세요!";
                break;
        }
        GetText((int)Texts.DescriptionText).text = description;
        
        //보스 던전일 경우
        GetObject((int)GameObjects.BossGroup).SetActive(type == DungeonType.Boss);
    }

    private IEnumerator CoUpdateTimeText()
    {
        while (Managers.Dungeon.IsDungeonActive)
        {
            float remaining = Managers.Dungeon.RemainingTime;
            int seconds = Mathf.FloorToInt(remaining);
            int milliseconds = Mathf.FloorToInt((remaining - seconds) * 100);
            if (remaining <= 0f)
            {
                GetText((int)Texts.TimeText).text = $"00:00";
                yield break;
            }

            GetText((int)Texts.TimeText).text = $"{seconds:D2}:{milliseconds:D2}";
            yield return null;
        }
    }

    private void OnClickEscapeButton()
    {
        Managers.Sound.PlayButtonClick();
        
        Managers.Dungeon.EndDungeon();
    }

    private IEnumerator PlayFadeThenEntranceThenStart()
    {
        if (fade == null)
        {
            Debug.LogWarning("UIFade is null!");
            yield break;
        }

        if (contentTransform == null)
        {
            Debug.LogWarning("ContentTransform is null!");
            yield break;
        }

        Vector2 originalPos = contentTransform.anchoredPosition; // 도착 지점
        Vector2 startPos = originalPos + new Vector2(0, 1080);     // 시작 지점
        contentTransform.anchoredPosition = startPos;

        yield return fade.FadeIn(0f);

        yield return new WaitForSeconds(0.25f);
        yield return fade.FadeOut(0.25f);

        bool finished = false;
        contentTransform.DOAnchorPos(originalPos, 0.5f)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => finished = true);

        yield return new WaitUntil(() => finished);

        Managers.Dungeon.StartDungeon(type, level);
    }
    
    private void DungeonFinishedEventHandler(DungeonStateChangedEvent evnt)
    {
        if (!Managers.Dungeon.IsDungeonActive)
        {
            Managers.UI.ClosePopupUI(this);
        }
    }

    private void DungeonClearedEventHandler(DungeonClearedEvent evnt)
    {
        if (_timeUpdateRoutine != null)
            StopCoroutine(_timeUpdateRoutine);
    }

    private void DungeonFailedEventHandler(DungeonFailedEvent evnt)
    {
        if (_timeUpdateRoutine != null)
            StopCoroutine(_timeUpdateRoutine);
    }

    private void DungeonBossWaveStartEventHandler(DungeonBossWaveStartEvent evnt)
    {
        if (type == DungeonType.Boss) 
        {
            GetText((int)Texts.BossText).text = $"{evnt.currentBossIndex + 1}/{Managers.Data.BossDungeonDataDic[level].bossIds.Count}";
        }
    }
}
