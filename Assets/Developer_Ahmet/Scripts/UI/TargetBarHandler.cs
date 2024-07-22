using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetBarHandler : MonoBehaviour
{
    public Image RedFiller;
    public Image RedOverloadFiller;
    public Image GreenFiller;
    public Image GreenOverloadFiller;
    public Image BlueFiller;
    public Image BlueOverloadFiller;

    public Image RedLimitSeperator;
    public Image GreenLimitSeperator;
    public Image BlueLimitSeperator;

    public TextMeshProUGUI RedTargetText;
    public TextMeshProUGUI GreenTargetText;
    public TextMeshProUGUI BlueTargetText;

    public Light TargetLight;

    public float RedPercent;
    public float GreenPercent;
    public float BluePercent;

    public bool RedOverloaded;
    public bool GreenOverloaded;
    public bool BlueOverloaded;

    public void InitRequirements(int _redRequired, int _greenRequired, int _blueRequired)
    {
        RedTargetText.text = _redRequired.ToString();
        GreenTargetText.text = _greenRequired.ToString();
        BlueTargetText.text = _blueRequired.ToString();

        RedFiller.transform.parent.gameObject.SetActive(true);
        GreenFiller.transform.parent.gameObject.SetActive(true);
        BlueFiller.transform.parent.gameObject.SetActive(true);
    }

    public void SetColorPercents(float _redPercent, float _greenPercent, float _bluePercent)
    {
        RedOverloaded = _redPercent > 1;
        GreenOverloaded = _greenPercent > 1;
        BlueOverloaded = _bluePercent > 1;

        RedPercent = Mathf.Clamp01(_redPercent);
        GreenPercent = Mathf.Clamp01(_greenPercent);
        BluePercent = Mathf.Clamp01(_bluePercent);

        if (RedPercent < 0.1f)
            RedPercent = 0.1f;

        if (GreenPercent < 0.1f)
            GreenPercent = 0.1f;

        if (BluePercent < 0.1f)
            BluePercent = 0.1f;
    }

    private void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;

        HandleOverload(RedOverloaded, RedOverloadFiller, ref RedPercent, RedFiller, RedLimitSeperator);
        HandleOverload(GreenOverloaded, GreenOverloadFiller, ref GreenPercent, GreenFiller, GreenLimitSeperator);
        HandleOverload(BlueOverloaded, BlueOverloadFiller, ref BluePercent, BlueFiller, BlueLimitSeperator);

        UpdateTargetLight();
    }

    private void HandleOverload(bool overloaded, Image overloadFiller, ref float percent, Image mainFiller, Image limitSeperator)
    {
        if (overloaded)
        {
            if (mainFiller.fillAmount >= 0.95f)
            {
                overloadFiller.fillAmount = Mathf.Lerp(overloadFiller.fillAmount, 1, Time.deltaTime * 1f);
                limitSeperator.color = Color.Lerp(LightPuzzleHandler.GetColorByLight(LightPuzzleHandler.LightColor.Green), LightPuzzleHandler.GetColorByLight(LightPuzzleHandler.LightColor.Red), overloadFiller.fillAmount);
            }
        }
        else
        {
            if (overloadFiller.fillAmount > 0.1f)
            {
                overloadFiller.fillAmount = Mathf.Lerp(overloadFiller.fillAmount, 0, Time.deltaTime * 1f);
                limitSeperator.color = Color.Lerp(LightPuzzleHandler.GetColorByLight(LightPuzzleHandler.LightColor.Green), LightPuzzleHandler.GetColorByLight(LightPuzzleHandler.LightColor.Red), overloadFiller.fillAmount);
            }
            else
            {
                overloadFiller.fillAmount = 0;
                mainFiller.fillAmount = Mathf.Lerp(mainFiller.fillAmount, percent, Time.deltaTime * 1f);
                limitSeperator.color = Color.Lerp(LightPuzzleHandler.GetColorByLight(LightPuzzleHandler.LightColor.Cyan), LightPuzzleHandler.GetColorByLight(LightPuzzleHandler.LightColor.Green), mainFiller.fillAmount);
            }
        }
    }

    private void UpdateTargetLight()
    {
        Color mixedColor = new Color(RedFiller.fillAmount, GreenFiller.fillAmount, BlueFiller.fillAmount);
        float totalBrightness = ((RedFiller.fillAmount + GreenFiller.fillAmount + BlueFiller.fillAmount) / 3f) * 3;

        TargetLight.color = mixedColor;
        TargetLight.intensity = totalBrightness;
    }
}
