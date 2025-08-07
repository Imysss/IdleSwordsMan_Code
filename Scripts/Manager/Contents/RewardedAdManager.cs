using System;
using System.Collections;
using Data;
using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;

// 이 스크립트는 AdMob 보상형 광고를 담당하는 매니저 클래스
// 광고 로딩, 보여주기, 보상 분기, 이벤트 등록까지 하나로 처리

public class RewardedAdManager : MonoBehaviour
{
    private RewardedAd _rewardedAd;                  // 보상형 광고 객체 (일회용이라 보여주고 나면 새로 로딩해야 함)
    private string _adUnitId;                        // 광고 단위 ID (AdMob에서 발급 받은 실제 ID로 나중엔 교체해야 함)
    private Define.RewardType _pendingRewardType;    // 현재 대기 중인 보상 타입 (어떤 광고 버튼을 눌렀는지를 기억해서 보상 분기용으로 사용)
    private bool _rewardGranted;                     // 보상 수령 여부 플래그 (광고 닫힌 후 실제 보상 적용하기 위해 사용)

    private void Awake()
    {
#if UNITY_ANDROID
        _adUnitId = "ca-app-pub-3940256099942544/5224354917"; // 테스트용 ID (Android)
#elif UNITY_IPHONE
        _adUnitId = "ca-app-pub-3940256099942544/1712485313"; // 테스트용 ID (iOS)
#else
        _adUnitId = "unused"; // 사용하지 않는 플랫폼에서는 무시
#endif
    }

    private void Start()
    {
        // 게임 시작 시 AdMob SDK를 먼저 초기화해야 함
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("Google Mobile Ads SDK Initialized");
            LoadRewardedAd();
        });
    }

    public void LoadRewardedAd()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        var adRequest = new AdRequest();

        RewardedAd.Load(_adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError($"Rewarded ad failed to load: {error}");
                return;
            }

            Debug.Log("Rewarded ad loaded successfully");
            _rewardedAd = ad;

            RegisterEventHandlers(_rewardedAd);
            RegisterReloadHandler(_rewardedAd);
        });
    }

    public void ShowRewardedAd(Define.RewardType rewardType)
    {
        _pendingRewardType = rewardType;
        _rewardGranted = false;

        if (Managers.Purchase.IsAdRemoved)
        {
            // 광고 제거 구매자는 광고 없이 보상 즉시 지급
            Debug.Log("광고 제거 유저 - 광고 없이 보상 지급");
            GrantReward(_pendingRewardType);
            return;
        }

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.Show((Reward reward) =>
            {
                _rewardGranted = true;
            });
        }
        else
        {
            Debug.Log("Ad is not ready yet.");
        }
    }

    private void GrantReward(Define.RewardType type)
    {
        switch (type)
        {
            case Define.RewardType.Gem:
                Debug.Log("보상: 젬 지급");
                // TODO: 젬 지급 로직
                break;

            case Define.RewardType.BossDungeonTicket:
                Managers.Game.AddDungeonTicket(Define.DungeonType.Boss, 1);
                Managers.SaveLoad.SaveData.timeData.isBossDungeonAdClaimedToday = true;

                UIDungeonPopup bossPopup = GameObject.FindObjectOfType<UIDungeonPopup>();
                if (bossPopup != null)
                    bossPopup.SetInfo();
                break;

            case Define.RewardType.GoldDungeonTicket:
                Managers.Game.AddDungeonTicket(Define.DungeonType.Gold, 1);
                Managers.SaveLoad.SaveData.timeData.isGoldDungeonAdClaimedToday = true;

                UIDungeonPopup goldPopup = GameObject.FindObjectOfType<UIDungeonPopup>();
                if (goldPopup != null)
                    goldPopup.SetInfo();
                break;

            case Define.RewardType.EquipmentGacha:
                if (Managers.Time.IsAdGachaAvailable(Define.GachaType.Gear))
                {
                    Managers.Time.SetAdGachaUsed(Define.GachaType.Gear);
                    UIShopPopup gearPopup = GameObject.FindObjectOfType<UIShopPopup>();
                    if (gearPopup != null)
                        gearPopup.DoAdGacha(Define.GachaType.Gear, 11);
                }
                break;

            case Define.RewardType.SkillGacha:
                if (Managers.Time.IsAdGachaAvailable(Define.GachaType.Skill))
                {
                    Managers.Time.SetAdGachaUsed(Define.GachaType.Skill);
                    UIShopPopup skillPopup = GameObject.FindObjectOfType<UIShopPopup>();
                    if (skillPopup != null)
                        skillPopup.DoAdGacha(Define.GachaType.Skill, 11);
                }
                break;

            case Define.RewardType.PartyGacha:
                if (Managers.Time.IsAdGachaAvailable(Define.GachaType.Party))
                {
                    Managers.Time.SetAdGachaUsed(Define.GachaType.Party);
                    UIShopPopup partyPopup = GameObject.FindObjectOfType<UIShopPopup>();
                    if (partyPopup != null)
                        partyPopup.DoAdGacha(Define.GachaType.Party, 11);
                }
                break;

            case Define.RewardType.SpeedBoost:
                Managers.Time?.SetDoubleSpeed(Define.DOUBLE_SPEED_TIME);

                // 2배속 UI 갱신 강제 실행
                UIGameScene scene = GameObject.FindObjectOfType<UIGameScene>();
                if (scene != null)
                    scene.StartDoubleSpeedUI();
                break;
            
            case Define.RewardType.BonusOfflineReward:
                if (Managers.Time.CanReceiveBonusOfflineReward())
                {
                    if (Managers.Data.OfflineRewardDataDic.TryGetValue(Managers.Level.GetCurrentLevel(),
                            out OfflineRewardData offlineReward))
                    {
                        Managers.Time.GiveBonusOfflineReward(offlineReward);
                    }
                    UIBonusOfflineRewardPopup bonusOfflineRewardPopup = GameObject.FindObjectOfType<UIBonusOfflineRewardPopup>();
                    if (bonusOfflineRewardPopup != null)
                        bonusOfflineRewardPopup.OnClickExitButton();
                    UIOfflineRewardPopup offlineRewardPopup = GameObject.FindObjectOfType<UIOfflineRewardPopup>();
                    if (offlineRewardPopup != null)
                        offlineRewardPopup.SetInfo();
                }
                break;
        }
        EventBus.Raise(new AdWatchedEvent());
    }

    public void OnClickRewardGemAd() => ShowRewardedAd(Define.RewardType.Gem);
    public void OnClickRewardBossTicketAd() => ShowRewardedAd(Define.RewardType.BossDungeonTicket);
    public void OnClickRewardGoldTicketAd() => ShowRewardedAd(Define.RewardType.GoldDungeonTicket);
    public void OnClickRewardEquipmentGachaAd() => ShowRewardedAd(Define.RewardType.EquipmentGacha);
    public void OnClickRewardSkillGachaAd() => ShowRewardedAd(Define.RewardType.SkillGacha);
    public void OnClickRewardPartyGachaAd() => ShowRewardedAd(Define.RewardType.PartyGacha);
    public void OnClickRewardSpeedBoostAd() => ShowRewardedAd(Define.RewardType.SpeedBoost);

    // 광고 동작 중에 발생하는 다양한 이벤트를 로그로 확인할 수 있도록 등록해주는 함수야
    private void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log($"Ad paid: {adValue.Value} {adValue.CurrencyCode}");
        };

        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Ad impression recorded.");
        };

        ad.OnAdClicked += () =>
        {
            Debug.Log("Ad clicked.");
        };

        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Ad full screen content opened.");
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Ad closed");

            if (_rewardGranted)
            {
                StartCoroutine(DelayedGrantReward());
            }
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"Ad failed to show full screen content: {error}");
        };
    }

    private IEnumerator DelayedGrantReward()
    {
        yield return null;
        GrantReward(_pendingRewardType);
    }

    private void RegisterReloadHandler(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            LoadRewardedAd();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            LoadRewardedAd();
        };
    }
}
