using UnityEngine;
using UnityEngine.UI;

public class InteractableBarHandler : MonoBehaviour
{
    [SerializeField] Image bg;
    [SerializeField] Image filler;
    [SerializeField, Range(0,10)] float rotationSpeed = 1f;
    public Color CurrentColor { get; private set; }
    public float CurrentValue { get; private set; }

    private Color targetBgColor;
    private Color targetFillerColor;
    private float targetFillerValue;
    private float fillSpeed;

    private void Start()
    {
        CurrentColor = filler.color;
        CurrentValue = filler.fillAmount;
    }
    public void ResetBar()
    {
        targetBgColor = Color.white;
        targetFillerColor = Color.blue;
        targetFillerValue = 0;
        bg.color = Color.white;
        filler.color = Color.blue;
        filler.fillAmount = 0;
        CurrentColor = filler.color;
        CurrentValue = filler.fillAmount;
        fillSpeed = 5f;
    }
    public void SetBar(Color _bgColor, Color _fillerColor, float _fillerValue, float _fillSpeed)
    {
        targetBgColor = _bgColor;
        targetFillerColor = _fillerColor;
        targetFillerValue = Mathf.Clamp01(_fillerValue);
        fillSpeed = _fillSpeed;
    }

    private void Update()
    {
        bg.color = Color.Lerp(bg.color, targetBgColor, Time.deltaTime * fillSpeed);
        filler.color = Color.Lerp(filler.color, targetFillerColor, Time.deltaTime * fillSpeed);
        filler.fillAmount = Mathf.Lerp(filler.fillAmount, targetFillerValue, Time.deltaTime * fillSpeed);

        CurrentColor = filler.color;
        CurrentValue = filler.fillAmount;
    }

    private void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;
    }
}
