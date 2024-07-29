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
    //public bool Test;
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

    IEnumerator SmoothSnap(float targetAngle)
    {
        float duration = 0.5f;
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
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SnapToNearestAngle();
    }

    //private void OnDrawGizmos()
    //{
    //    ArrangeElements();
    //    if (Test)
    //    {
    //        Test = false;
    //        SnapToNearestAngle();
    //    }
    //}
}
