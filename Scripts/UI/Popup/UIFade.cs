using System.Collections;
using UnityEngine;

public class UIFade : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;

    public IEnumerator FadeIn(float duration)
    {
        gameObject.SetActive(true);
        
        canvasGroup.blocksRaycasts = true;
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / duration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    public IEnumerator FadeOut(float duration)
    {
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / duration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        
        gameObject.SetActive(false);
    }
}