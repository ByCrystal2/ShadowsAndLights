using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif

[ExecuteAlways]
public class DirectorBehaviour : MonoBehaviour
{
    public Light ReflectLight;
    public List<DirectorType> Type;
    [SerializeField] RotateAngle RotateAngel;
    [SerializeField, Range(0, 10)] float RotateSpeed;
    IRotateAnObject rotateAnObject;
    private bool ReflectionActive;

    public LightSourcesOnDirector CurrentSources;
    public List<ColorOnReflect> ColorsOnReflect = new List<ColorOnReflect>();

    public Transform LightPoolParent;
    public List<LightBehaviour> PooledLights = new();

    public float nextUpdate = 0;

    private int Level;
    void Awake()
    {
        nextUpdate = 0;
        CurrentSources = new();
        CurrentSources.Sources = new();
        rotateAnObject = GetComponent<IRotateAnObject>();
    }

    private void Start()
    {
        SetRotateValues();
    }

    private void OnValidate()
    {
        SetRotateValues();
    }

    void SetRotateValues() {
        if (rotateAnObject != null) if(rotateAnObject.RotatableObject != null) { rotateAnObject.RotatableObject.RotateAngel = RotateAngel; rotateAnObject.RotatableObject.RotateSpeed = RotateSpeed; Debug.Log("rotateAnObject values are changed. => " + rotateAnObject.RotatableObject.RotateAngel); }
    }

#if UNITY_EDITOR
    private EditorCoroutine currentEditorCoroutine;
#endif

    public (Color _color, LightPuzzleHandler.LightColor _lightColor, List<LightPuzzleHandler.LightColor> _coreColors) ActivateReflectLight(LightPuzzleHandler.LightColor _HitColor)
    {
        float LightFadeSeconds = 0;
        if (Application.isPlaying)
        {
            LightFadeSeconds = Time.time + 0.2f;
        }
        else
        {
#if UNITY_EDITOR
            LightFadeSeconds = (float)EditorApplication.timeSinceStartup + 0.2f;
#endif
        }

        OpenLight();
        bool contains = false;
        int length = ColorsOnReflect.Count;
        for (int i = 0; i < length; i++)
        {
            if (ColorsOnReflect[i].ColorOnSurface == _HitColor)
            {
                contains = true;
                ColorOnReflect newC = ColorsOnReflect[i];
                newC.LifeInSeconds = LightFadeSeconds;
                newC.ColorOnSurface = _HitColor;
                ColorsOnReflect[i] = newC;
                break;
            }
        }

        if (!contains)
            ColorsOnReflect.Add(new() { ColorOnSurface = _HitColor, LifeInSeconds = LightFadeSeconds });

        List<LightPuzzleHandler.LightColor> mixes = new();
        foreach (var item in ColorsOnReflect)
            mixes.Add(item.ColorOnSurface);
        
        var reflect = LightPuzzleHandler.GetMixedColor(mixes);
        if (Type[0] == DirectorType.Mix_Seperated)
        {
            reflect._hasMix = false;
            reflect._lightColor = _HitColor;
            reflect._coreColors = new();
        }
        if (!reflect._hasMix)
        {
            //Debug.Log("original color: " + _HitColor.ToString() + " / overridedColor: " + LightPuzzleHandler.GetColorByLight(_HitColor));
        }
        return (reflect._hasMix ? reflect._target : LightPuzzleHandler.GetColorByLight(_HitColor), reflect._lightColor, reflect._coreColors);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
            CheckLight();
    }
#endif


    public void FixedUpdate()
    {
        CheckLight();
    }

    void CheckLight()
    {
        int length = ColorsOnReflect.Count;
        int length2 = CurrentSources.Sources.Count;
        if (Application.isPlaying)
        {
            for (int i = length - 1; i >= 0; i--)
                if (ColorsOnReflect[i].LifeInSeconds < Time.time)
                    ColorsOnReflect.RemoveAt(i);
            if (nextUpdate < Time.time)
            {
                for (int i = length2 - 1; i >= 0; i--)
                    if (CurrentSources.Sources[i].LifeInSeconds < Time.time)
                        CurrentSources.Sources.RemoveAt(i);

                nextUpdate = Time.time + 0.1f;
                UpdateTheSources(LightPuzzleHandler.LightColor.Close);
            }
        }
        else
        {
#if UNITY_EDITOR
            for (int i = length - 1; i >= 0; i--)
                if (ColorsOnReflect[i].LifeInSeconds < EditorApplication.timeSinceStartup)
                    ColorsOnReflect.RemoveAt(i);

            if (nextUpdate < EditorApplication.timeSinceStartup)
            {
                for (int i = length2 - 1; i >= 0; i--)
                    if (CurrentSources.Sources[i].LifeInSeconds < EditorApplication.timeSinceStartup)
                        CurrentSources.Sources.RemoveAt(i);

                nextUpdate = (float)EditorApplication.timeSinceStartup + 0.1f;
                UpdateTheSources(LightPuzzleHandler.LightColor.Close);
            }
#endif
        }

        length = ColorsOnReflect.Count;
        if (length == 0)
            CloseLight();
        else
        {
            List<LightPuzzleHandler.LightColor> mixes = new();
            foreach (var item in ColorsOnReflect)
                mixes.Add(item.ColorOnSurface);
            ReflectLight.color = LightPuzzleHandler.GetMixedColor(mixes)._target;
        }
    }

    void CloseLight()
    {
        if (!ReflectionActive)
            return;

        ReflectionActive = false;
        StopAllCoroutines();

        if (Application.isPlaying)
            StartCoroutine(SetLightBrightness(0));
        else
        {
#if UNITY_EDITOR
            if (currentEditorCoroutine != null)
                EditorCoroutineUtility.StopCoroutine(currentEditorCoroutine);
            currentEditorCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(SetLightBrightness(0));
#endif
        }
    }

    void OpenLight()
    {
        if (ReflectionActive)
            return;

        ReflectionActive = true;
        StopAllCoroutines();

        if (Application.isPlaying)
            StartCoroutine(SetLightBrightness(1));
        else
        {
#if UNITY_EDITOR
            if (currentEditorCoroutine != null)
                EditorCoroutineUtility.StopCoroutine(currentEditorCoroutine);
            currentEditorCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(SetLightBrightness(1));
#endif
        }
    }

    private IEnumerator SetLightBrightness(float _to)
    {
        if(_to == 1)
            ReflectLight.gameObject.SetActive(true);
        float start = ReflectLight.intensity;
        float end = _to;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            float currentAngle = Mathf.Lerp(start, end, elapsedTime / 1f);
            ReflectLight.intensity = currentAngle;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ReflectLight.intensity = _to;
        if(_to == 0)
        {
            ReflectLight.gameObject.SetActive(false);
            ReflectLight.intensity = 0;
        }
    }

    public int GetLevelID()
    {
        return Level;
    }

    public Vector3 AddColorToTheSource(LightBehaviour _source, LightPuzzleHandler.LightColor _currentColor, int _currentBounce, Vector3 _nextDirection, Vector3 _mirrorNormal, LightPuzzleHandler.LightColor _originalColor, Vector3 _hitPoint)
    {
#if UNITY_EDITOR
        if (CurrentSources.Sources == null)
            CurrentSources.Sources = new();
#endif 
        int existSourceIndex = -1;
        if (Type[0] != DirectorType.Mix_Blocked)
        {
            int length = CurrentSources.Sources.Count;
            for (int i = 0; i < length; i++)
            {
                if (CurrentSources.Sources[i].LightSource == _source)
                {
                    existSourceIndex = i;
                    break;
                }
            }

            LightSourceHold newLight = new();
            newLight.LightSource = _source;
            newLight.ColorOnSurface = _currentColor;
            newLight.OriginalColor = _originalColor;
            newLight.LifeInSeconds = Time.time + 0.1f;
#if UNITY_EDITOR
            if(!Application.isPlaying)
                newLight.LifeInSeconds = (float)EditorApplication.timeSinceStartup + 0.1f;
#endif
            newLight.CurrentBounce = _currentBounce;
            newLight.Index = -1;
            newLight.nextDirection = _nextDirection;
            newLight.hitPoint = _hitPoint;
            newLight.mirrorNormal = _mirrorNormal;

            if (existSourceIndex > -1)
                CurrentSources.Sources[existSourceIndex] = newLight;
            else
                CurrentSources.Sources.Add(newLight);
        }
        return existSourceIndex > -1 ? UpdateTheSources(_currentColor) : Vector3.zero;
    }

    Vector3 UpdateTheSources(LightPuzzleHandler.LightColor _lightColor)
    {
        List<List<LightSourceHold>> allLights = new();
        List<LightSourceHold> yellowLights = new();
        List<LightSourceHold> cyanLights = new();
        List<LightSourceHold> purpleLights = new();
        List<LightSourceHold> whiteLights = new();
        allLights.Add(yellowLights);
        allLights.Add(cyanLights);
        allLights.Add(purpleLights);
        allLights.Add(whiteLights);

        List<CompareLights> Masters = new();
        LightSourceHold masterYellowLight = new();
        LightSourceHold masterCyanLight = new();
        LightSourceHold masterPurpleLight = new();
        LightSourceHold masterWhiteLight = new();
        masterYellowLight.Index = -100;
        masterCyanLight.Index = -100;
        masterPurpleLight.Index = -100;
        masterWhiteLight.Index = -100;
        if (Type[0] == DirectorType.Mix_Together)
        {
            int length = CurrentSources.Sources.Count;
            for (int i = length - 1; i >= 0; i--)
            {
                LightSourceHold Light = new();
                Light.LightSource = CurrentSources.Sources[i].LightSource;
                Light.ColorOnSurface = CurrentSources.Sources[i].ColorOnSurface;
                Light.OriginalColor = CurrentSources.Sources[i].OriginalColor;
                Light.LifeInSeconds = CurrentSources.Sources[i].LifeInSeconds;
                Light.CurrentBounce = CurrentSources.Sources[i].CurrentBounce;
                Light.nextDirection = CurrentSources.Sources[i].nextDirection;
                Light.hitPoint = CurrentSources.Sources[i].hitPoint;
                Light.mirrorNormal = CurrentSources.Sources[i].mirrorNormal;
                Light.Index = i;

                if (CurrentSources.Sources[i].OriginalColor == LightPuzzleHandler.LightColor.Red ||
                     CurrentSources.Sources[i].OriginalColor == LightPuzzleHandler.LightColor.Green ||
                     CurrentSources.Sources[i].OriginalColor == LightPuzzleHandler.LightColor.Blue)
                {
                    if (CurrentSources.Sources[i].ColorOnSurface == LightPuzzleHandler.LightColor.Yellow)
                        yellowLights.Add(Light);
                    else if (CurrentSources.Sources[i].ColorOnSurface == LightPuzzleHandler.LightColor.Cyan)
                        cyanLights.Add(Light);
                    else if (CurrentSources.Sources[i].ColorOnSurface == LightPuzzleHandler.LightColor.Purple)
                        purpleLights.Add(Light);
                    else if (CurrentSources.Sources[i].ColorOnSurface == LightPuzzleHandler.LightColor.White)
                        whiteLights.Add(Light);
                }

                int allColorLength = allLights.Count;
                for (int u = 0; u < allColorLength; u++)
                {
                    int highestID = -1;
                    LightSourceHold masterLight = new();
                    for (int z = 0; z < allLights[u].Count; z++)
                    {
                        if (highestID < allLights[u][z].Index)
                        {
                            masterLight.LightSource = allLights[u][z].LightSource;
                            masterLight.ColorOnSurface = allLights[u][z].ColorOnSurface;
                            masterLight.OriginalColor = allLights[u][z].OriginalColor;
                            masterLight.LifeInSeconds = allLights[u][z].LifeInSeconds;
                            masterLight.CurrentBounce = allLights[u][z].CurrentBounce;
                            masterLight.nextDirection = allLights[u][z].nextDirection;
                            masterLight.hitPoint = allLights[u][z].hitPoint;
                            masterLight.Index = z;
                        }
                    }

                    if (u == 0)
                        masterYellowLight = masterLight;
                    else if (u == 1)
                        masterCyanLight = masterLight;
                    else if (u == 2)
                        masterPurpleLight = masterLight;
                    else
                        masterWhiteLight = masterLight;
                }
            }

            Masters.Add(new() { lightSourceHold = masterYellowLight, lightColor = LightPuzzleHandler.LightColor.Yellow });
            Masters.Add(new() { lightSourceHold = masterCyanLight, lightColor = LightPuzzleHandler.LightColor.Cyan });
            Masters.Add(new() { lightSourceHold = masterPurpleLight, lightColor = LightPuzzleHandler.LightColor.Purple });
            Masters.Add(new() { lightSourceHold = masterWhiteLight, lightColor = LightPuzzleHandler.LightColor.White });

            int totalLights = yellowLights.Count + cyanLights.Count + purpleLights.Count + whiteLights.Count;
            if (totalLights <= 1)
                return Vector3.zero;

            int masterLightsLength = Masters.Count;
            for (int i = 0; i < masterLightsLength; i++)
            {
                if (Masters[i].lightSourceHold.Index >= 0 && _lightColor == Masters[i].lightColor)
                {
                    if (Masters[i].lightSourceHold.LightSource == null)
                        return Vector3.zero;

                    float currentTime = Time.time + 0.5f;
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        currentTime = (float)EditorApplication.timeSinceStartup + 0.5f;
#endif
                    List<Vector3> incomingDirections = new();
                    List<Vector3> mirrorNormals = new();
                    foreach (var item2 in allLights[i])
                    {
                        item2.LightSource.SetBlockedByMixUntil(currentTime, false, item2.CurrentBounce, Vector3.zero);
                        incomingDirections.Add(item2.nextDirection);
                        mirrorNormals.Add(item2.mirrorNormal);
                    }

                    Vector3 averageDirection = GetAverageDirection(incomingDirections, mirrorNormals);
                    Masters[i].lightSourceHold.LightSource.SetBlockedByMixUntil(currentTime, true, Masters[i].lightSourceHold.CurrentBounce, averageDirection);

                    return averageDirection;
                }
            }
        }
        else if (Type[0] == DirectorType.Mix_Seperated)
        {
            float currentTime = Time.time;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                currentTime = (float)EditorApplication.timeSinceStartup;
#endif
            int currentUseLightCount = 0;
            int lightsOnPool = LightPoolParent.childCount;
            int length = CurrentSources.Sources.Count;
            for (int i = length - 1; i >= 0; i--)
            {
                if (CurrentSources.Sources[i].LifeInSeconds < currentTime)
                {
                    CurrentSources.Sources.RemoveAt(i);
                    continue;
                }
                LightSourceHold Light = new();
                Light.LightSource = CurrentSources.Sources[i].LightSource;
                Light.ColorOnSurface = CurrentSources.Sources[i].ColorOnSurface;
                Light.OriginalColor = CurrentSources.Sources[i].OriginalColor;
                Light.LifeInSeconds = CurrentSources.Sources[i].LifeInSeconds;
                Light.CurrentBounce = CurrentSources.Sources[i].CurrentBounce;
                Light.nextDirection = CurrentSources.Sources[i].nextDirection;
                Light.hitPoint = CurrentSources.Sources[i].hitPoint;
                Light.mirrorNormal = CurrentSources.Sources[i].mirrorNormal;
                Light.Index = i;

                if (CurrentSources.Sources[i].ColorOnSurface == LightPuzzleHandler.LightColor.Yellow)
                    yellowLights.Add(Light);
                else if (CurrentSources.Sources[i].ColorOnSurface == LightPuzzleHandler.LightColor.Cyan)
                    cyanLights.Add(Light);
                else if (CurrentSources.Sources[i].ColorOnSurface == LightPuzzleHandler.LightColor.Purple)
                    purpleLights.Add(Light);
                else if (CurrentSources.Sources[i].ColorOnSurface == LightPuzzleHandler.LightColor.White)
                    whiteLights.Add(Light);

                int allColorLength = allLights.Count;
                for (int u = 0; u < allColorLength; u++)
                {
                    int highestID = -1;
                    LightSourceHold masterLight = new();
                    for (int z = 0; z < allLights[u].Count; z++)
                    {
                        if (highestID < allLights[u][z].Index)
                        {
                            masterLight.LightSource = allLights[u][z].LightSource;
                            masterLight.ColorOnSurface = allLights[u][z].ColorOnSurface;
                            masterLight.OriginalColor = allLights[u][z].OriginalColor;
                            masterLight.LifeInSeconds = allLights[u][z].LifeInSeconds;
                            masterLight.CurrentBounce = allLights[u][z].CurrentBounce;
                            masterLight.nextDirection = allLights[u][z].nextDirection;
                            masterLight.hitPoint = allLights[u][z].hitPoint;
                            masterLight.Index = z;
                        }
                    }
                }
            }

            int totalLights = cyanLights.Count + purpleLights.Count + yellowLights.Count + whiteLights.Count;
            if (totalLights <= 0)
            {
                for (int t = 0; t < lightsOnPool; t++)
                    LightPoolParent.GetChild(t).gameObject.SetActive(false);
                return Vector3.zero;
            }

            int masterLightsLength = Masters.Count;
            for (int i = 0; i < 4; i++)
            {
                List<Vector3> seperatedDirections = new List<Vector3>();
                foreach (var item2 in allLights[i])
                {
                    float time = Time.time + 0.5f;
#if UNITY_EDITOR
                    if(!Application.isPlaying)
                        time = (float)EditorApplication.timeSinceStartup + 0.5f;
#endif
                    item2.LightSource.SetBlockedBySeperateUntil(time, item2.CurrentBounce, Vector3.zero);
                    var currents = GetAverageSeperatedDirection(item2.nextDirection, item2.mirrorNormal, item2.ColorOnSurface);
                    int index = 0;
                    foreach (var item in currents.directions)
                    {
                        if (lightsOnPool <= currentUseLightCount)
                        {
                            GameObject newLightToPool = Instantiate(Resources.Load<GameObject>("Prefabs/ExtractedLight"), LightPoolParent);
                            newLightToPool.transform.position = item2.hitPoint;
                            lightsOnPool = LightPoolParent.childCount;
                            newLightToPool.GetComponent<LightBehaviour>().SetSeperatedOptions(item, currents._colors[index]);
                        }
                        else
                        {
                            GameObject LightFromPool = LightPoolParent.GetChild(currentUseLightCount).gameObject;
                            LightFromPool.gameObject.SetActive(true);
                            LightFromPool.transform.position = item2.hitPoint;
                            LightFromPool.GetComponent<LightBehaviour>().SetSeperatedOptions(item, currents._colors[index]);
                        }
                        index++;
                        currentUseLightCount++;
                        seperatedDirections.Add(item);
                    }
                }

            }

            lightsOnPool = LightPoolParent.childCount;
            for (int t = currentUseLightCount; t < lightsOnPool; t++)
                LightPoolParent.GetChild(t).gameObject.SetActive(false);
        }

        return Vector3.zero;
    }

    private Vector3 GetAverageDirection(List<Vector3> incomingDirections, List<Vector3> mirroredNormal)
    {
        Vector3 sum = Vector3.zero;
        foreach (Vector3 dir in incomingDirections)
            sum += dir;

        Vector3 averageDirection = sum.normalized;
        return averageDirection;
    }

    private (List<Vector3> directions, List<LightPuzzleHandler.LightColor> _colors) GetAverageSeperatedDirection(Vector3 incomingDirection, Vector3 mirroredNormal, LightPuzzleHandler.LightColor _ownerColor)
    {
        List<Vector3> reflections = new List<Vector3>();
        List<LightPuzzleHandler.LightColor> colors = new List<LightPuzzleHandler.LightColor>();
        //Vector3 reflectedDirection = Vector3.Reflect(incomingDirection, mirroredNormal);
        Vector3 rotationAxis = Vector3.Cross(incomingDirection, mirroredNormal).normalized;
        float mainReflectionAngle = Vector3.Angle(incomingDirection, mirroredNormal);
        float splitAngle = Mathf.Clamp(mainReflectionAngle / 2, 3, 90) / 2; // Adjust the range as needed
        if (_ownerColor == LightPuzzleHandler.LightColor.White)
        {
            // Rotate around the calculated axis
            Vector3 reflectionDir1 = Quaternion.AngleAxis(splitAngle, rotationAxis) * incomingDirection;
            Vector3 reflectionDir2 = incomingDirection;
            Vector3 reflectionDir3 = Quaternion.AngleAxis(-splitAngle, rotationAxis) * incomingDirection;

            reflections.Add(reflectionDir1);
            reflections.Add(reflectionDir2);
            reflections.Add(reflectionDir3);

            colors.Add(LightPuzzleHandler.LightColor.Red);
            colors.Add(LightPuzzleHandler.LightColor.Green);
            colors.Add(LightPuzzleHandler.LightColor.Blue);
        }
        else
        {
            // Rotate around the calculated axis
            Vector3 reflectionDir1 = Quaternion.AngleAxis(splitAngle / 2, rotationAxis) * incomingDirection;
            Vector3 reflectionDir2 = Quaternion.AngleAxis(-splitAngle / 2, rotationAxis) * incomingDirection;

            reflections.Add(reflectionDir1);
            reflections.Add(reflectionDir2);

            // Use a dictionary for color splitting
            Dictionary<LightPuzzleHandler.LightColor, List<LightPuzzleHandler.LightColor>> colorSplits = new Dictionary<LightPuzzleHandler.LightColor, List<LightPuzzleHandler.LightColor>>()
        {
            { LightPuzzleHandler.LightColor.Yellow, new List<LightPuzzleHandler.LightColor> { LightPuzzleHandler.LightColor.Red, LightPuzzleHandler.LightColor.Green } },
            { LightPuzzleHandler.LightColor.Cyan, new List<LightPuzzleHandler.LightColor> { LightPuzzleHandler.LightColor.Blue, LightPuzzleHandler.LightColor.Green } },
            { LightPuzzleHandler.LightColor.Purple, new List<LightPuzzleHandler.LightColor> { LightPuzzleHandler.LightColor.Red, LightPuzzleHandler.LightColor.Blue } }
        };

            if (colorSplits.TryGetValue(_ownerColor, out List<LightPuzzleHandler.LightColor> splitColors))
            {
                colors.AddRange(splitColors);
            }
        }

        return (reflections, colors);
    }

    public void SetLevel(int _level)
    {
        Level = _level;
    }

    public Vector3 ReflectRay(Vector3 incomingDirection, Vector3 normal)
    {
        return Vector3.Reflect(incomingDirection, normal);
    }

    [System.Serializable]
    public struct ColorOnReflect
    {
        public LightPuzzleHandler.LightColor ColorOnSurface;
        public float LifeInSeconds;
    }

    [System.Serializable]
    public class LightSourcesOnDirector
    {
        public List<LightSourceHold> Sources;
    }

    [System.Serializable]
    public struct LightSourceHold
    {
        public int Index;
        public LightBehaviour LightSource;
        public LightPuzzleHandler.LightColor OriginalColor;
        public LightPuzzleHandler.LightColor ColorOnSurface;
        public int CurrentBounce;
        public float LifeInSeconds;
        public Vector3 nextDirection;
        public Vector3 mirrorNormal;
        public Vector3 hitPoint;
    }

    public enum DirectorType
    {
        Mix_Together,
        Mix_Seperated,
        Mix_Blocked,
    }

    [System.Serializable]
    public struct CompareLights
    {
        public LightSourceHold lightSourceHold;
        public LightPuzzleHandler.LightColor lightColor;
    }
}
