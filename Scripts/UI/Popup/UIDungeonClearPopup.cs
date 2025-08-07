using System.Numerics;
using UnityEngine;
using DG.Tweening;
using static Define;
using Vector2 = UnityEngine.Vector2;
using System.Collections;
using Vector3 = UnityEngine.Vector3;

public class UIDungeonClearPopup : UIPopup
{
    #region UI 기능 리스트
    //DungeonLevelValueText
    //RewardImage
    //RewardValueText
    //ClaimButton
    //Ribbon_Success
    #endregion

    #region Enum
    enum Buttons
    {
        ClaimButton
    }

    enum Texts
    {
        DungeonLevelValueText,
        RewardValueText,
    }

    enum Images
    {
        RewardImage
    }
    #endregion

    private DungeonType type;
    private int level;

    private RectTransform contentTransform;
    private RectTransform ribbonTransform;
    private UIFade fade;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        GetButton((int)Buttons.ClaimButton).gameObject.BindEvent(OnClickClaimButton);

        Transform contentObj = transform.Find("ContentObject");
        if (contentObj != null)
            contentTransform = contentObj.GetComponent<RectTransform>();

        Transform ribbonObj = transform.Find("ContentObject/Ribbon_Success");
        if (ribbonObj != null)
            ribbonTransform = ribbonObj.GetComponent<RectTransform>();

        Transform fadeObj = transform.Find("Fade");
        if (fadeObj != null)
            fade = fadeObj.GetComponent<UIFade>();

        return true;
    }

    public void SetInfo(DungeonType type, int level)
    {
        this.type = type;
        this.level = level;

        if (contentTransform != null)
            contentTransform.gameObject.SetActive(true);

        RefreshUI();

        if (ribbonTransform != null)
            ribbonTransform.localScale = Vector3.zero;

        PlayAppearAnimation();
    }

    private void RefreshUI()
    {
        GetText((int)Texts.DungeonLevelValueText).text = "LEVEL " + level.ToString("D2");
        
        if (type == DungeonType.Gold)
        {
            if (Managers.Data.GoldDungeonDataDic.TryGetValue(level, out var goldData))
            {
                GetText((int)Texts.RewardValueText).text = $"x{goldData.reward}";
                GetText((int)Texts.RewardValueText).color = UIColors.GoldTextColor;
            }
        }
        else if (type == DungeonType.Boss)
        {
            if (Managers.Data.BossDungeonDataDic.TryGetValue(level, out var bossData))
            {
                GetText((int)Texts.RewardValueText).text = $"x{bossData.reward}";
                GetText((int)Texts.RewardValueText).color = UIColors.GemTextColor;
            }
        }
        
        string iconKey = type == DungeonType.Boss ? GEM_SPRITE_NAME : GOLD_SPRITE_NAME;
        Sprite spr = Managers.Resource.Load<Sprite>(iconKey);
        GetImage((int)Images.RewardImage).sprite = spr;
    }

    private void OnClickClaimButton()
    {
        Managers.Sound.PlayButtonClick();
        
        if (contentTransform != null)
            contentTransform.gameObject.SetActive(false);

        StartCoroutine(CoFadeAndClose());
    }

    private IEnumerator CoFadeAndClose()
    {
        if (fade != null)
        {
            // 어두워지기
            yield return fade.FadeIn(0.2f);
            yield return new WaitForSecondsRealtime(0.1f);

            // 실제 종료 로직
            Managers.UI.ClosePopupUI(this);
            Managers.Dungeon.EndDungeon();

            // 다시 밝아지기
            yield return fade.FadeOut(0.2f);
        }
        else
        {
            Managers.UI.ClosePopupUI(this);
            Managers.Dungeon.EndDungeon();
        }
    }

    private void PlayAppearAnimation()
    {
        if (contentTransform == null)
        {
            Debug.LogWarning("ContentTransform is null!");
            return;
        }

        Vector2 originalPos = contentTransform.anchoredPosition;
        Vector2 startPos = originalPos + new Vector2(0, -2250);

        contentTransform.anchoredPosition = startPos;

        contentTransform.DOAnchorPos(originalPos, 0.5f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (ribbonTransform != null)
                {
                    ribbonTransform.localScale = Vector3.zero;
                    Vector2 ribbonOriginalPos = ribbonTransform.anchoredPosition;
                    Vector2 jumpPos = ribbonOriginalPos + new Vector2(0, 50);

                    Sequence ribbonSeq = DOTween.Sequence();
                    ribbonSeq.SetUpdate(true);
                    ribbonSeq.Append(ribbonTransform.DOScale(1.3f, 0.25f).SetEase(Ease.OutBack));
                    ribbonSeq.Join(ribbonTransform.DOAnchorPos(jumpPos, 0.25f).SetEase(Ease.OutCubic));
                    ribbonSeq.Append(ribbonTransform.DOScale(1.0f, 0.2f).SetEase(Ease.InOutSine));
                    ribbonSeq.Join(ribbonTransform.DOAnchorPos(ribbonOriginalPos, 0.2f).SetEase(Ease.InOutSine));
                }
            });
    }
}
