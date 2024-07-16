using System.Collections;
using System.Linq;
using UnityEngine;
using static LightPuzzleHandler;
using System.Collections.Generic;
using MalbersAnimations;

#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif

[ExecuteAlways]
public class LightBehaviour : MonoBehaviour
{
    public LineRenderer lineRenderer;
    [Range(0, 360)]
    public float rotationAngle;
    [Range(0, 100)]
    public float chargeAmount;

    public Light PointLight;

    public LightSourceType SourceType;
    public List<LightPuzzleHandler.LightColor> TypeOfLights;

    public float OriginalLength = 10;
    public bool PlayOnEditor;
    public bool isActive;
    private float LengthPerLight = 10;
    private bool isEditorUpdateRegistered;
    private float rotateTimer;
    private float rotateInterval = 3;
    private Gradient originalGradient;
    public List<Color> segmentColors = new();
    private LightPuzzleHandler.LightColor OverridedColor;

    void FixedUpdate()
    {
        UpdateLight();
    }

    void UpdateLight()
    {
        UpdateLine();
        if (SourceType == LightSourceType.Chargable)
            UpdateChargableColor();
        else
            SetLengthOriginal();

        if (rotateTimer < Time.time)
        {
            if (SourceType == LightSourceType.Rotate90_Each3)
                StartCoroutine(SmoothRotate());
            else if (SourceType == LightSourceType.ChangeColor_Each3)
                StartCoroutine(SmoothColorTransition());
            else if (SourceType == LightSourceType.Switch_Each3)
            {
                if (isActive)
                    StartCoroutine(SmoothSwitchTransitionClose());
                else
                    StartCoroutine(SmoothSwitchTransitionOpen());
            }
            else
                UpdateColor();
            rotateTimer = Time.time + rotateInterval;
        }
        UpdateGradient(Vector3.zero, Vector3.zero);
    }

    void SetLengthOriginal()
    {
        LengthPerLight = OriginalLength;
    }

    void UpdateChargableColor()
    {
        PointLight.intensity = (chargeAmount * 2 / 100);
        LengthPerLight = (chargeAmount * OriginalLength / 100);

        Gradient newGradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[2];

        float colorRatio = 1f - (chargeAmount / 100f);
        if(originalGradient == null)
            originalGradient = lineRenderer.colorGradient;
        Color startColor = Color.Lerp(originalGradient.colorKeys[0].color, Color.black, colorRatio);
        Color endColor = Color.Lerp(originalGradient.colorKeys[1].color, Color.black, colorRatio);

        colorKeys[0] = new GradientColorKey(startColor, originalGradient.colorKeys[0].time);
        colorKeys[1] = new GradientColorKey(endColor, originalGradient.colorKeys[1].time);

        newGradient.SetKeys(colorKeys, originalGradient.alphaKeys);
    }

    public void UpdateColor()
    {
        PointLight.color = lineRenderer.colorGradient.colorKeys.First().color;
    }

    public void UpdateLine()
    {
        if (!isActive)
            return;

        Vector3 direction = Quaternion.Euler(0, rotationAngle, 0) * Vector3.forward;

        lineRenderer.SetPosition(0, PointLight.transform.position);
        lineRenderer.SetPosition(1, PointLight.transform.position + direction * LengthPerLight);

        segmentColors.Clear();
        segmentColors.Add(LightPuzzleHandler.GetColorByLight(TypeOfLights[0]));
        OverridedColor = TypeOfLights[0];
        SendRay(PointLight.transform.position, direction, 1);
    }

    void SendRay(Vector3 origin, Vector3 direction, int _bounces)
    {
        if (_bounces >= 8)
        {
            segmentColors.Add(LightPuzzleHandler.GetColorByLight(TypeOfLights[0]));
            return;
        }

        Ray ray = new Ray(origin, direction);
        RaycastHit hit;

        lineRenderer.positionCount =_bounces + 1;
        if (Physics.Raycast(ray, out hit, LengthPerLight))
        {
            lineRenderer.SetPosition(_bounces, hit.point);
            if (hit.transform.gameObject.CompareTag("Reflect"))
            {
                Vector3 newDirection = Vector3.Reflect(direction, hit.normal);
                var nextColorHolder = hit.transform.GetComponentInParent<DirectorBehaviour>().ActivateReflectLight(OverridedColor);
                segmentColors.Add(nextColorHolder._color);
                OverridedColor = nextColorHolder._lightColor;
                SendRay(hit.point, newDirection, _bounces + 1);
            }
            else
            {
                //Debug.Log("Hit normal object at " + hit.point + " name: " + hit.transform.name);
            }
        }
        else
        {
            lineRenderer.SetPosition(_bounces, origin + direction * LengthPerLight);
        }
    }

    void UpdateGradient(Vector3 endPoint, Vector3 startPoint)
    {
        if (!isActive)
            return;

        int l = segmentColors.Count;
        if (l == 0)
            return;

        Gradient gradient = new Gradient();
        List<GradientColorKey> colorKeys = new List<GradientColorKey>();
        List<GradientAlphaKey> alphaKeys = new List<GradientAlphaKey>();

        for (int i = 0; i < l; i++)
        {
            float t = 0;
            if(l > 1)
                t = (float)i / (l-1);
            colorKeys.Add(new GradientColorKey(segmentColors[i], t));
            alphaKeys.Add(new GradientAlphaKey(1.0f, t));
        }

        gradient.SetKeys(colorKeys.ToArray(), alphaKeys.ToArray());
        lineRenderer.colorGradient = gradient;
        originalGradient = lineRenderer.colorGradient;
    }

    private IEnumerator SmoothColorTransition()
    {
        LightColor lastElement = TypeOfLights[TypeOfLights.Count - 1];
        TypeOfLights.RemoveAt(TypeOfLights.Count - 1);
        TypeOfLights.Insert(0, lastElement);

        Gradient newGradient = LightPuzzleHandler.GetColorGradient(TypeOfLights[0]);

        GradientColorKey[] startColorKeys = originalGradient.colorKeys;
        GradientColorKey[] endColorKeys = newGradient.colorKeys;

        SynchronizeGradientLengths(ref startColorKeys, ref endColorKeys);

        float elapsedTime = 0f;
        float duration = 0.75f;

        while (elapsedTime < duration)
        {
            GradientColorKey[] interpolatedColorKeys = new GradientColorKey[startColorKeys.Length];
            for (int i = 0; i < startColorKeys.Length; i++)
            {
                Color startColor = startColorKeys[i].color;
                Color endColor = endColorKeys[i].color;
                Color interpolatedColor = Color.Lerp(startColor, endColor, elapsedTime / duration);
                interpolatedColorKeys[i] = new GradientColorKey(interpolatedColor, startColorKeys[i].time);
            }

            Gradient updatedGradient = new Gradient();
            updatedGradient.SetKeys(interpolatedColorKeys, originalGradient.alphaKeys);
            lineRenderer.colorGradient = updatedGradient;

            PointLight.color = interpolatedColorKeys[0].color;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Gradient finalGradient = new Gradient();
        finalGradient.SetKeys(endColorKeys, lineRenderer.colorGradient.alphaKeys);
        lineRenderer.colorGradient = originalGradient;

        PointLight.color = endColorKeys[0].color;
        //UpdateGradient(Vector3.zero, Vector3.zero);
    }

    private IEnumerator SmoothRotate()
    {
        float startAngle = rotationAngle;
        float endAngle = (rotationAngle + 90) % 360;
        float elapsedTime = 0f;

        if (startAngle > endAngle)
            endAngle += 360;

        while (elapsedTime < 0.75f)
        {
            float currentAngle = Mathf.Lerp(startAngle, endAngle, elapsedTime / 0.75f);
            rotationAngle = currentAngle % 360;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rotationAngle = endAngle % 360;
    }

    private IEnumerator SmoothSwitchTransitionOpen()
    {
        Gradient newGradient = LightPuzzleHandler.GetColorGradient(TypeOfLights[0]);

        GradientColorKey[] startColorKeys = lineRenderer.colorGradient.colorKeys;
        GradientColorKey[] endColorKeys = newGradient.colorKeys;

        float elapsedTime = 0f;
        float duration = 0.75f;

        while (elapsedTime < duration)
        {
            if (elapsedTime > 0.3f)
                isActive = true;
            LengthPerLight = Mathf.Lerp(0, OriginalLength, elapsedTime / 0.75f);
            GradientColorKey[] interpolatedColorKeys = new GradientColorKey[startColorKeys.Length];
            for (int i = 0; i < startColorKeys.Length; i++)
            {
                Color startColor = Color.black;
                Color endColor = endColorKeys[i].color;
                Color interpolatedColor = Color.Lerp(startColor, endColor, elapsedTime / duration);
                interpolatedColorKeys[i] = new GradientColorKey(interpolatedColor, startColorKeys[i].time);
            }

            Gradient updatedGradient = new Gradient();
            updatedGradient.SetKeys(interpolatedColorKeys, lineRenderer.colorGradient.alphaKeys);
            lineRenderer.colorGradient = updatedGradient;

            PointLight.color = interpolatedColorKeys[0].color;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Gradient finalGradient = new Gradient();
        finalGradient.SetKeys(endColorKeys, lineRenderer.colorGradient.alphaKeys);
        lineRenderer.colorGradient = finalGradient;

        PointLight.color = endColorKeys[0].color;

        
    }

    private IEnumerator SmoothSwitchTransitionClose()
    {
        Gradient newGradient = LightPuzzleHandler.GetColorGradient(LightColor.Close);

        GradientColorKey[] startColorKeys = lineRenderer.colorGradient.colorKeys;
        GradientColorKey[] endColorKeys = newGradient.colorKeys;

        float elapsedTime = 0f;
        float duration = 0.75f;

        while (elapsedTime < duration)
        {
            LengthPerLight = Mathf.Lerp(OriginalLength, 0, elapsedTime / 0.75f);
            GradientColorKey[] interpolatedColorKeys = new GradientColorKey[startColorKeys.Length];
            for (int i = 0; i < startColorKeys.Length; i++)
            {
                Color startColor = startColorKeys[i].color;
                Color endColor = Color.black;
                Color interpolatedColor = Color.Lerp(startColor, endColor, elapsedTime / duration);
                interpolatedColorKeys[i] = new GradientColorKey(interpolatedColor, startColorKeys[i].time);
            }

            Gradient updatedGradient = new Gradient();
            updatedGradient.SetKeys(interpolatedColorKeys, lineRenderer.colorGradient.alphaKeys);
            lineRenderer.colorGradient = updatedGradient;

            PointLight.color = interpolatedColorKeys[0].color;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Gradient finalGradient = new Gradient();
        finalGradient.SetKeys(endColorKeys, lineRenderer.colorGradient.alphaKeys);
        lineRenderer.colorGradient = finalGradient;

        PointLight.color = endColorKeys[0].color;

        isActive = false;
    }

    void SynchronizeGradientLengths(ref GradientColorKey[] startKeys, ref GradientColorKey[] endKeys)
    {
        int maxLength = Mathf.Max(startKeys.Length, endKeys.Length);

        startKeys = ResizeAndCopyKeys(startKeys, maxLength);
        endKeys = ResizeAndCopyKeys(endKeys, maxLength);
    }

    GradientColorKey[] ResizeAndCopyKeys(GradientColorKey[] keys, int newSize)
    {
        GradientColorKey[] newKeys = new GradientColorKey[newSize];
        for (int i = 0; i < newSize; i++)
        {
            if (i < keys.Length)
            {
                newKeys[i] = keys[i];
            }
            else
            {
                // Set default color key if out of range
                newKeys[i] = new GradientColorKey(Color.white, 1f);
            }
        }
        return newKeys;
    }

#if UNITY_EDITOR

    private float timeAccumulator;
    void OnEnable()
    {
        RegisterEditorUpdate();
    }

    void OnDisable()
    {
        UnregisterEditorUpdate();
    }

    void OnDestroy()
    {
        UnregisterEditorUpdate();
    }

    private void RegisterEditorUpdate()
    {
        if (!isEditorUpdateRegistered)
        {
            EditorApplication.update += EditorUpdate;
            isEditorUpdateRegistered = true;
        }
    }

    private void UnregisterEditorUpdate()
    {
        if (isEditorUpdateRegistered)
        {
            EditorApplication.update -= EditorUpdate;
            isEditorUpdateRegistered = false;
        }
    }

    private void EditorUpdate()
    {
        if (PlayOnEditor)
            if (SourceType != LightSourceType.Switch_Each3)
                isActive = true;
        
        if (!Application.isPlaying && PlayOnEditor)
        {
            UpdateLine();
            if (SourceType == LightSourceType.Chargable)
                UpdateChargableColor();
            else
                SetLengthOriginal();

            if (timeAccumulator < (float)EditorApplication.timeSinceStartup)
            {
                if (SourceType == LightSourceType.Rotate90_Each3)
                    EditorCoroutineUtility.StartCoroutineOwnerless(SmoothRotate());
                else if (SourceType == LightSourceType.ChangeColor_Each3)
                    EditorCoroutineUtility.StartCoroutineOwnerless(SmoothColorTransition());
                else if (SourceType == LightSourceType.Switch_Each3)
                {
                    if (isActive)
                        EditorCoroutineUtility.StartCoroutineOwnerless(SmoothSwitchTransitionClose());
                    else
                        EditorCoroutineUtility.StartCoroutineOwnerless(SmoothSwitchTransitionOpen());
                }
                else
                    UpdateColor();

                timeAccumulator = (float)EditorApplication.timeSinceStartup + rotateInterval;
            }
            UpdateGradient(Vector3.zero, Vector3.zero);
        }
    }
#endif
}


