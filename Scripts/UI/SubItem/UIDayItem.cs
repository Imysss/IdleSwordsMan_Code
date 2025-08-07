using System.Collections;
using Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class UIDayItem : UIBase
{
    #region UI 기능 리스트
    //DayReward: 배경 이미지
    //RewardImage: 리워드 이미지
    //RewardValueText: 리워드 값 텍스트
    //DayValueText: 날짜 텍스트
    //DayRewarded: 보상 받았는지 확인
    #endregion

    #region Enum
    enum GameObjects
    {
        DayRewarded,
    }
    
    enum Texts
    {
        RewardValueText,
        DayValueText
    }

    enum Images
    {
        DayReward,
        RewardImage,
        RewardedImage,
    }
    #endregion

    private AttendanceData data;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        gameObject.BindEvent(OnClickDayItem);
        
        return true;
    }

    public void SetInfo(AttendanceData data)
    {
        this.data = data;

        RefreshUI();
        
        //오늘 날짜 + 아직 보상 안 받았으면 자동 보상
        if (data.key == Managers.Time.AttendanceDay && !Managers.Time.IsAttendanceRewardClaimed)
        {
            StartCoroutine(CoDelayedAutoClaim());
        }
    }

    private void RefreshUI()
    {
        GetText((int)Texts.DayValueText).text = $"DAY{data.key}";
        string key = "";
        switch (data.rewardType)
        {
            case RewardType.Gem:
                key = GEM_SPRITE_NAME;
                break;
            case RewardType.BossDungeonTicket:
                key = BOSS_DUNGEON_KEY_SPRITE_NAME;
                break;
            case RewardType.GoldDungeonTicket:
                key = GOLD_DUNGEON_KEY_SPRITE_NAME;
                break;
        }

        Sprite spr = Managers.Resource.Load<Sprite>(key);
        GetImage((int)Images.RewardImage).sprite = spr;
        GetText((int)Texts.RewardValueText).text = data.rewardCount.ToString();

        //버튼 활성화
        bool isInteractable = data.key == Managers.Time.AttendanceDay && !Managers.Time.IsAttendanceRewardClaimed;
        gameObject.GetComponent<Button>().interactable = isInteractable;
        GetImage((int)Images.DayReward).color = isInteractable ? Util.HexToColor("37E8CE") : Util.HexToColor("02273D");

        //보상 받았는지 확인
        //출석체크한 날짜보다 key값이 작다면 무조건 받았을 것이기 때문에 받았다고 표시해 줌
        if (data.key < Managers.Time.AttendanceDay % 12)
        {
            GetObject((int)GameObjects.DayRewarded).SetActive(true);
        }
        //같은 날짜라면 처음에 받았다면 true, 받지 않았다면 false로 처리
        else if (data.key == Managers.Time.AttendanceDay)
        {
            GetObject((int)GameObjects.DayRewarded).SetActive(Managers.Time.IsAttendanceRewardClaimed);
        }
        //남은 날들 같은 경우에는 받지 않았기 때문에 false로 설정
        else
        {
            GetObject((int)GameObjects.DayRewarded).SetActive(false);
        }
    }

    private IEnumerator CoDelayedAutoClaim()
    {
        yield return new WaitForSeconds(0.5f);
        AutoClaimReward();
    }

    private void AutoClaimReward()
    {
        switch (data.rewardType)
        {
            case RewardType.Gem:
                Managers.Game.AddGem(data.rewardCount);
                break;
            case RewardType.BossDungeonTicket:
                Managers.Game.AddDungeonTicket(DungeonType.Boss, data.rewardCount);
                break;
            case RewardType.GoldDungeonTicket:
                Managers.Game.AddDungeonTicket(DungeonType.Gold, data.rewardCount);
                break;
        }

        Managers.Time.IsAttendanceRewardClaimed = true; //보상 획득 나타내기

        PlayRewardAnimation(); //애니메이션
        
        RefreshUI();
    }

    private void PlayRewardAnimation()
    {
        GameObject rewardObj = GetObject((int)GameObjects.DayRewarded);
        rewardObj.SetActive(true);

        GameObject rewardedObj = GetImage((int)Images.RewardedImage).gameObject;

        //alpha 설정
        CanvasGroup cg = Util.GetOrAddComponent<CanvasGroup>(rewardObj);
        cg.alpha = 0;
        
        DOTween.To(() => cg.alpha, x => cg.alpha = x, 1f, 0.8f).SetEase(Ease.OutQuad);
        
        //Scale 애니메이션 (팡!)
        rewardedObj.transform.localScale = Vector3.zero;
        rewardedObj.transform.DOScale(Vector3.one * 2.5f, 0.3f).SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                Managers.Sound.PlayRewardButtonClick();
                rewardedObj.transform.DOScale(Vector3.one * 2f, 0.15f).SetEase(Ease.InOutSine);
            });
    }

    private void OnClickDayItem()
    {
        if (Managers.Time.IsAttendanceRewardClaimed)
            return;
        
        AutoClaimReward();
    }
}
