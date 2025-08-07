using UnityEngine;
using DG.Tweening;
using System.Collections;

public class UIDungeonFailPopup : UIPopup
{
    #region UI 기능 리스트
    //DungeonLevelValueText
    //ClaimButton
    //Fade
    //Ribbon_Fail
    #endregion

    #region Enum
    enum Buttons
    {
        ClaimButton
    }

    enum Texts
    {
        DungeonLevelValueText
    }
    #endregion

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

        GetButton((int)Buttons.ClaimButton).gameObject.BindEvent(OnClickClaimButton);

        Transform contentObj = transform.Find("ContentObject");
        if (contentObj != null)
            contentTransform = contentObj.GetComponent<RectTransform>();

        Transform ribbonObj = transform.Find("ContentObject/Ribbon_Fail");
        if (ribbonObj != null)
            ribbonTransform = ribbonObj.GetComponent<RectTransform>();

        Transform fadeObj = transform.Find("Fade");
        if (fadeObj != null)
            fade = fadeObj.GetComponent<UIFade>();

        return true;
    }

    public void SetInfo(int level)
    {
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
    }

    private void OnClickClaimButton()
    {
        if (contentTransform != null)
            contentTransform.gameObject.SetActive(false);

        StartCoroutine(CoFadeAndExitImmediately());
    }

    private IEnumerator CoFadeAndExitImmediately()
    {
        Managers.Sound.PlayButtonClick();
        
        if (fade != null)
        {
            yield return fade.FadeIn(0.2f);

            yield return new WaitForSecondsRealtime(0.5f);

            Managers.Player.ResetAfterDungeon();
                
            Managers.UI.ClosePopupUI(this);
            Managers.Dungeon.EndDungeon();

            yield return fade.FadeOut(0.2f);
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
                PlayRibbonAnimation();
            });
    }

    private void PlayRibbonAnimation()
    {
        if (ribbonTransform == null)
            return;

        ribbonTransform.localScale = Vector3.zero;
        Vector2 ribbonOriginalPos = ribbonTransform.anchoredPosition;

        Vector2 jumpPos = ribbonOriginalPos + new Vector2(0, 40);

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Append(ribbonTransform.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack));
        seq.Join(ribbonTransform.DOAnchorPos(jumpPos, 0.25f).SetEase(Ease.OutCubic));

        seq.Append(ribbonTransform.DOScale(1.0f, 0.2f).SetEase(Ease.InOutSine));
        seq.Join(ribbonTransform.DOAnchorPos(ribbonOriginalPos, 0.2f).SetEase(Ease.InOutSine));

        seq.Append(ribbonTransform.DOAnchorPos(jumpPos, 0.15f).SetEase(Ease.OutSine));
        seq.Append(ribbonTransform.DOAnchorPos(ribbonOriginalPos, 0.15f).SetEase(Ease.InSine));
        
        seq.Append(ribbonTransform.DOAnchorPos(jumpPos, 0.1f).SetEase(Ease.OutSine));
        seq.Append(ribbonTransform.DOAnchorPos(ribbonOriginalPos, 0.1f).SetEase(Ease.InSine));
    }
}
