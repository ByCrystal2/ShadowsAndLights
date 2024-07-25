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
    public List<SegmentColour> segmentColors = new();
    private LightPuzzleHandler.LightColor OverridedColor;

    public BlockedData BlockByMix;

    private int Level;
    private int ObjectID;
    private Vector3 OverridedStartDirection = Vector3.zero;
    private float SoundTimer;

    private void Awake()
    {
        BlockByMix.blockedUntil = 0;
        BlockByMix.Seperated = false;
        BlockByMix.OverridedDirection = Vector3.zero;
        BlockByMix.masterMixed = false;
        BlockByMix.bounceAfter = 0;
    }

    void FixedUpdate()
    {
        UpdateLight();
        if(SoundTimer < Time.time)
        {
            SoundTimer = Time.time + 4;
            GameAudioManager.instance.PlayLightSourceSound(transform.position);
        }
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

        Vector3 direction = Vector3.zero;
        if (OverridedStartDirection == Vector3.zero)
            direction = Quaternion.Euler(0, rotationAngle, 0) * Vector3.forward;
        else
            direction = OverridedStartDirection;

        segmentColors.Clear();
        OverridedColor = TypeOfLights[0];
        SendRay(PointLight.transform.position, direction, 0);
    }

    void SendRay(Vector3 origin, Vector3 direction, int _bounces)
    {
        if (BlockByMix.blockedUntil > Time.time)
        {
            //Debug.Log("BlockByMix.bounceAfter: " + BlockByMix.bounceAfter + " / _bounces: " + _bounces);
            if (!BlockByMix.masterMixed && BlockByMix.bounceAfter < _bounces)
            {
                //Debug.Log("Light is blocked by mix. Because other light created combined mix.");
                return;
            }
            if (BlockByMix.masterMixed)
            {
                //direction = BlockByMix.OverridedDirection;
            }
            if (BlockByMix.Seperated && BlockByMix.bounceAfter < _bounces)
            {
                Debug.Log("Color has been seperated.");
                return;
            }
        }
        else
        {
            BlockByMix.masterMixed = false;
        }
        Ray ray = new Ray(origin, direction);
        RaycastHit hit;

        if (_bounces < 6 && Physics.Raycast(ray, out hit, LengthPerLight, LayerMaskHelper.LightLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform.gameObject.CompareTag("Reflect"))
            {
                Vector3 newDirection = Vector3.Reflect(direction, hit.normal);
                DirectorBehaviour director = hit.collider.transform.GetComponentInParent<DirectorBehaviour>();
                var nextColorHolder = director.ActivateReflectLight(OverridedColor);
                Vector3 overridedDir = Vector3.zero;

                overridedDir = director.AddColorToTheSource(this, nextColorHolder._lightColor, _bounces, newDirection, hit.normal, OverridedColor, hit.point);
                if (OverridedColor == LightColor.Cyan || OverridedColor == LightColor.Yellow || OverridedColor == LightColor.Purple)
                    overridedDir = Vector2.zero;

                if (overridedDir != Vector3.zero)
                    newDirection = overridedDir;

                if (_bounces > 0)
                {
                    SegmentColour segmentColourLast = new();
                    segmentColourLast._hitEndPos = ray.origin;
                    segmentColourLast._startPos = segmentColors[_bounces - 1]._startPos;
                    segmentColourLast._color = segmentColors[_bounces - 1]._color;
                    segmentColourLast._direction = segmentColors[_bounces - 1]._direction;
                    segmentColourLast._hitDirector = segmentColors[_bounces - 1]._hitDirector;
                    segmentColors[_bounces - 1] = segmentColourLast;
                }

                bool coreColor = false;
                foreach (var item in nextColorHolder._coreColors)
                {
                    if (OverridedColor == item)
                    {
                        coreColor = true;
                        break;
                    }
                }

                SegmentColour segmentColour = new SegmentColour()
                {
                    _color = coreColor ? nextColorHolder._color : LightPuzzleHandler.GetColorByLight(OverridedColor),
                    _startPos = ray.origin,
                    _hitEndPos = hit.point,
                    _direction = ray.direction,
                    _hitDirector = director,
                };

                segmentColors.Add(segmentColour);
                OverridedColor = coreColor ? nextColorHolder._lightColor : OverridedColor;
                SendRay(hit.point, newDirection, _bounces + 1);
            }
            else if (hit.collider.transform.gameObject.CompareTag("Target"))
            {
                SegmentColour segmentColour = new SegmentColour()
                {
                    _color = LightPuzzleHandler.GetColorByLight(OverridedColor),
                    _startPos = ray.origin,
                    _hitEndPos = hit.point,
                    _direction = ray.direction,
                    _hitDirector = null,
                };
                segmentColors.Add(segmentColour);

                if (!Application.isPlaying)
                {
                    //Debug.Log("Target datalarini degistirmek level dizaynina zarar verebilecegi icin target islemesi yalnizca Playtimeda calismaktadir.");
                    return;
                }
                TargetBehaviour target = hit.collider.transform.GetComponentInParent<TargetBehaviour>();
                target.AddLightsOn(OverridedColor, transform);

            }
            else
            {
                SegmentColour finalSegment = new();
                finalSegment._color = Color.black;
                finalSegment._direction = direction;
                finalSegment._startPos = origin;
                finalSegment._hitEndPos = hit.point;
                finalSegment._hitDirector = null;
                segmentColors.Add(finalSegment);
            }
        }
        else
        {
            SegmentColour finalSegment = new();
            finalSegment._color = Color.black;
            finalSegment._direction = direction;
            finalSegment._startPos = origin;
            finalSegment._hitEndPos = origin + direction * LengthPerLight;
            finalSegment._hitDirector = null;
            segmentColors.Add(finalSegment);
        }
    }

    void UpdateGradient(Vector3 endPoint, Vector3 startPoint)
    {
        lineRenderer.positionCount = 0;
        if (!isActive)
            return;

        int l = segmentColors.Count;
        if (l == 0)
            return;        

        Gradient gradient = new Gradient();
        List<GradientColorKey> colorKeys = new List<GradientColorKey>();
        List<GradientAlphaKey> alphaKeys = new List<GradientAlphaKey>();

        float totalDistance = 0;
        List<float> distances = new List<float>();

        lineRenderer.positionCount++;
        lineRenderer.SetPosition(0, segmentColors[0]._startPos);
        // Calculate the distances and total distance
        for (int i = 1; i <= l; i++)
        {
            float distance = 0;
            Vector3 endPos = (segmentColors[i - 1]._hitEndPos == Vector3.zero) ? segmentColors[i - 1]._startPos : segmentColors[i - 1]._hitEndPos;
            distance = Vector3.Distance(segmentColors[i - 1]._startPos, endPos);
            distances.Add(distance);
            totalDistance += distance;
            if(i <= 6)
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(i, segmentColors[i - 1]._hitEndPos);
            }
        }

        float cumulativeDistance = distances[0];

        Color originalColor = LightPuzzleHandler.GetColorByLight(TypeOfLights[0]);
        colorKeys.Add(new GradientColorKey(originalColor, 0));
        alphaKeys.Add(new GradientAlphaKey(1.0f, 0));
        for (int i = 0; i < l; i++)
        {
            float t = cumulativeDistance / totalDistance;
            if (l == 1)
            {
                float secT = t * 1.1f > 1 ? 0.95f : t * 1.1f;
                colorKeys.Add(new GradientColorKey(segmentColors[i]._color, secT));
                alphaKeys.Add(new GradientAlphaKey(1.0f, secT));
            }
            else if (l == 2)
            {
                if (i == 0)
                {
                    colorKeys.Add(new GradientColorKey(originalColor, t * 0.02f));
                    alphaKeys.Add(new GradientAlphaKey(1.0f, t * 0.02f));

                    float secT = t * 1.5f > 1 ? 0.95f : t * 1.5f;
                    colorKeys.Add(new GradientColorKey(segmentColors[i]._color, secT));
                    alphaKeys.Add(new GradientAlphaKey(1.0f, secT));

                    Vector3 lastPoint = lineRenderer.GetPosition(lineRenderer.positionCount - 1); 
                    lineRenderer.positionCount++; 
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, lastPoint);
                    lineRenderer.SetPosition(lineRenderer.positionCount - 2, segmentColors[i + 1]._startPos + segmentColors[i + 1]._direction * (LengthPerLight / 4));
                }
                else
                {
                    float secT = 1;
                    colorKeys.Add(new GradientColorKey(segmentColors[i]._color, secT));
                    alphaKeys.Add(new GradientAlphaKey(1.0f, secT));
                }
            }
            else
            {
                colorKeys.Add(new GradientColorKey(i > 0 ? segmentColors[i]._color : originalColor, t));
                alphaKeys.Add(new GradientAlphaKey(1.0f, t));
            }
            if (i + 1 < l)
                cumulativeDistance += distances[i + 1];
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

    public void SetBlockedByMixUntil(float _time, bool _isMaster, int _bounceAfter, Vector3 _OverridedDirection)
    {
        BlockByMix = new();
        BlockByMix.masterMixed = _isMaster;
        BlockByMix.blockedUntil = _time;
        BlockByMix.bounceAfter = _bounceAfter;
        BlockByMix.OverridedDirection = _OverridedDirection;
        BlockByMix.Seperated = false;
    }
    
    public void SetBlockedBySeperateUntil(float _time, int _bounceAfter, Vector3 _OverridedDirection)
    {
        BlockByMix = new();
        BlockByMix.masterMixed = false;
        BlockByMix.blockedUntil = _time;
        BlockByMix.bounceAfter = _bounceAfter;
        BlockByMix.OverridedDirection = _OverridedDirection;
        BlockByMix.Seperated = true;
    }

    public void SetSeperatedOptions(Vector3 _direction, LightColor _lightColor)
    {
        OverridedStartDirection = _direction;
        TypeOfLights.Clear();
        TypeOfLights.Add(_lightColor);
    }

    public void SetLevel(int _level)
    {
        Level = _level;
        ObjectID = transform.GetSiblingIndex();
    }

    [System.Serializable]
    public struct SegmentColour
    {
        public Color _color;
        public Vector3 _hitEndPos;
        public Vector3 _startPos;
        public Vector3 _direction;
        public DirectorBehaviour _hitDirector;
    }

    [System.Serializable]
    public struct BlockedData
    {
        public bool masterMixed;
        public float blockedUntil;
        public int bounceAfter;
        public Vector3 OverridedDirection;

        public bool Seperated;
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

    public int GetID()
    {
        return ObjectID;
    }
}


