using UnityEngine;
using UnityEngine.UI;

public class BatteryUI : MonoBehaviour
{
    [SerializeField] private GameObject Holder;
    [SerializeField] private Image BatteryFiller;
    [SerializeField] private TMPro.TextMeshProUGUI BatterySecondsText;

    public void Activate()
    {
        if (!Holder.activeSelf)
            Holder.SetActive(true);
        else
            Holder.GetComponent<UIFade>().FadeIn();
    }

    public void Deactivate()
    {
        if (Holder.activeSelf)
            Holder.GetComponent<UIFade>().FadeOut();
    }

    public void UpdateUI(float _currentSeconds, float _limitSeconds)
    {
        BatteryFiller.fillAmount = _currentSeconds / _limitSeconds;
        BatterySecondsText.text = string.Format("F2", _limitSeconds - _currentSeconds);
    }
}
