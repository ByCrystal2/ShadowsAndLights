using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class LightSwitchUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform[] elements;
    public RectTransform mask;
    public float radius = 100f; 
    public float offset = 25f; 

    [SerializeField] private Vector2 startDragPosition;
    [SerializeField] private float currentAngle = 0f;

    private Coroutine snapCoroutine;
    private int LastSelectedIndex = -1;
    private int selectedIndex = 0;
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
