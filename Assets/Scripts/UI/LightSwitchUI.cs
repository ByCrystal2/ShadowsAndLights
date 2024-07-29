using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LightSwitchUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Main")]
    public RectTransform[] elements;
    public RectTransform mask;
    public float radius = 100f; 
    public float offset = 25f; 

    [SerializeField] private Vector2 startDragPosition;
    [SerializeField] private float currentAngle = 0f;

    private Coroutine snapCoroutine;
    private int LastSelectedIndex = 0;
    private int selectedIndex = 0;

    [Header("Battery UI")]
    [SerializeField] private Transform BatteryHolder;

    void Start()
    {
        ArrangeElements();
    }

    void ArrangeElements()
    {
        float angleStep = 360f / elements.Length;
        for (int i = 0; i < elements.Length; i++)
        {
            float angle = i * angleStep + currentAngle + offset;
            Vector2 pos = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
            elements[i].anchoredPosition = pos;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        startDragPosition = eventData.position;
        if (snapCoroutine != null)
            StopCoroutine(snapCoroutine);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentDragPosition = eventData.position;
        float deltaX = currentDragPosition.x - startDragPosition.x;
        currentAngle += deltaX * -0.75f;
        ArrangeElements();
        startDragPosition = currentDragPosition;
    }

    void DetermineSelectedElement()
    {
        float angleStep = 360f / elements.Length;
        selectedIndex = Mathf.RoundToInt(currentAngle / angleStep) % elements.Length;
        selectedIndex = elements.Length - selectedIndex;
        if (selectedIndex < 0) selectedIndex += elements.Length;
        if (selectedIndex >= elements.Length) selectedIndex -= elements.Length;

        OnSelectedNewColor();
    }

    public LightPuzzleHandler.LightColor GetCurrentSelectedLight()
    {
        return (LightPuzzleHandler.LightColor)selectedIndex;
    }

    void SnapToNearestAngle()
    {
        float angleStep = 360f / elements.Length;
        float targetAngle = Mathf.Round(currentAngle / angleStep) * angleStep;
        if (snapCoroutine != null)
        {
            StopCoroutine(snapCoroutine);
        }
        snapCoroutine = StartCoroutine(SmoothSnap(targetAngle));
    }

    void OnSelectedNewColor()
    {
        if (LastSelectedIndex == selectedIndex)
            return;
        Debug.Log("Color: " + ((LightPuzzleHandler.LightColor)selectedIndex).ToString());
        MainUIManager.instance.GetFlashlight().ChangeLightColorTo((LightPuzzleHandler.LightColor)selectedIndex);
        LastSelectedIndex = selectedIndex;
    }

    IEnumerator SmoothSnap(float targetAngle)
    {
        float duration = 0.25f;
        float elapsed = 0f;
        float initialAngle = currentAngle;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentAngle = Mathf.Lerp(initialAngle, targetAngle, elapsed / duration);
            ArrangeElements();
            yield return null;
        }

        currentAngle = targetAngle;
        ArrangeElements();

        DetermineSelectedElement();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SnapToNearestAngle();
    }

    public void SetBattery(int _maxBattery, int _batteryAmount)
    {
        Debug.Log("Max battery: " + _maxBattery + " / batteryAmount: " + _batteryAmount);
        int x = BatteryHolder.childCount;
        for (int i = 0; i < x; i++)
        {
            BatteryHolder.GetChild(i).gameObject.SetActive(i < _maxBattery);
            BatteryHolder.GetChild(i).GetChild(0).GetComponent<Image>().fillAmount = i < _batteryAmount ? 0 : 1;
        }
    }

    public void UpdateBatteriesUI(int _batteryAmount,float _currentBatteryLife)
    {
        if(_batteryAmount > 0)
            BatteryHolder.GetChild(_batteryAmount - 1).GetChild(0).GetComponent<Image>().fillAmount = 1 - _currentBatteryLife;
    }

#if UNITY_EDITOR
    public bool Test;
    private void OnDrawGizmos()
    {
        ArrangeElements();
        if (Test)
        {
            Test = false;
            SnapToNearestAngle();
        }
    }
#endif
}
