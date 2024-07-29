using System.Collections;
using UnityEngine;

public class FlashlightBehaviour : MonoBehaviour
{
    [Header("--Required Component--")]
    [SerializeField] private Light Flashlight;
    [SerializeField] private BoxCollider HitTrigger;

    [Header("--Runtime Datas (DEBUG)--")]
    [SerializeField] private LightPuzzleHandler.LightColor CurrentLightColor;
    
    public void ChangeLightColorTo(LightPuzzleHandler.LightColor _newLight)
    {
        StopAllCoroutines();
        CurrentLightColor = _newLight;
        StartCoroutine(SmoothSwitch());
    }

    private IEnumerator SmoothSwitch()
    {
        GameAudioManager.instance.PlayButtonFlashlightOffSound();
        float elapsedTime = 0f;
        float duration = 0.25f;

        while (elapsedTime < duration)
        {
            Flashlight.intensity = Mathf.Lerp(25, 0, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.25f);
        GameAudioManager.instance.PlayButtonFlashlightOnSound();

        Flashlight.intensity = 0;

        Flashlight.color = LightPuzzleHandler.GetColorByLight(CurrentLightColor);
        elapsedTime = 0f;
        duration = 0.25f;

        while (elapsedTime < duration)
        {
            Flashlight.intensity = Mathf.Lerp(0, 25, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Flashlight.intensity = 25;
    }
}
