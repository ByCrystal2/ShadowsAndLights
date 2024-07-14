using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFade : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float timer = 0.5f;

    private void OnEnable()
    {
        FadeIn();
    }

    public void FadeIn()
    {
        if (gameObject.activeSelf) 
        {
            //Debug.Log("Fade in now");
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroup(0, 1, timer, false));
        }
    }

    public void FadeOut(bool _destroyOnEnd = false)
    {
        if (gameObject.activeSelf)
        {
            //Debug.Log("Fade out now");
            StopAllCoroutines();
            StartCoroutine(FadeCanvasGroup(1, 0, timer, _destroyOnEnd));
        }
    }

    IEnumerator FadeCanvasGroup(float startAlpha, float targetAlpha, float duration, bool _destroyOnEnd)
    {
        float timer = 0f;

        while (timer < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            canvasGroup.alpha = alpha;
            timer += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        if (targetAlpha == 0)
        {
            if (!_destroyOnEnd)
                gameObject.SetActive(false);
            else
                Destroy(gameObject);
        }
    }

    public void SetDelayedFadeOut(float _seconds = 3, bool _destroyOnEnd = false)
    {
        StartCoroutine(DelayedFadeOut(_seconds, _destroyOnEnd));
    }

    IEnumerator DelayedFadeOut(float _seconds, bool _destroyOnEnd)
    {
        yield return new WaitForSeconds(_seconds);
        FadeOut(_destroyOnEnd);
    }
}
