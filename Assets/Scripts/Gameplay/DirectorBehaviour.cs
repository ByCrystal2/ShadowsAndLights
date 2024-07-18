using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    void Awake()
    {
        CurrentSources = new();
        CurrentSources.Sources = new();
        rotateAnObject = GetComponent<IRotateAnObject>();
        if (transform.parent.TryGetComponent(out DirectorsHolder d))
            LevelID = d.LevelID;
        else
        {
            Debug.Log("Transform name: " + transform.name + " does not placed into the true parent object is destroyed.");
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        //SetRotateValues();
    }
    private void OnValidate()
    {
        SetRotateValues();
    }
    void SetRotateValues() {if (rotateAnObject != null) { rotateAnObject.RotatableObject.RotateAngel = RotateAngel; rotateAnObject.RotatableObject.RotateSpeed = RotateSpeed; }
    }
#if UNITY_EDITOR
    private EditorCoroutine currentEditorCoroutine;
#endif

    private int LevelID;

    public (Color _color, LightPuzzleHandler.LightColor _lightColor) ActivateReflectLight(LightPuzzleHandler.LightColor _HitColor)
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
        if (!reflect._hasMix)
        {
            //Debug.Log("No mix, original color is: " + LightPuzzleHandler.GetColorByLight(_HitColor).ToString());
            //Debug.Log("No mix, original LightColor is: " + reflect._lightColor.ToString());
            //Debug.Log("No mix, original target is: " + reflect._target.ToString());
        }
        return (reflect._hasMix ? reflect._target : LightPuzzleHandler.GetColorByLight(_HitColor), reflect._lightColor);
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
        if (Application.isPlaying)
        {
            for (int i = length - 1; i >= 0; i--)
                if (ColorsOnReflect[i].LifeInSeconds < Time.time)
                    ColorsOnReflect.RemoveAt(i);
        }
        else
        {
#if UNITY_EDITOR
            for (int i = length - 1; i >= 0; i--)
                if (ColorsOnReflect[i].LifeInSeconds < EditorApplication.timeSinceStartup)
                    ColorsOnReflect.RemoveAt(i);
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
        return LevelID;
    }

    public Vector3 AddColorToTheSource(LightBehaviour _source, LightPuzzleHandler.LightColor _currentColor, int _currentBounce, Vector3 _nextDirection, Vector3 _mirrorNormal)
    {
#if UNITY_EDITOR
        if (CurrentSources.Sources == null)
            CurrentSources.Sources = new();
#endif 
        int existSourceIndex = -1;
        if (Type[0] == DirectorType.Mix_Together)
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
            newLight.LifeInSeconds = Time.time + 1f;
            newLight.CurrentBounce = _currentBounce;
            newLight.Index = -1;
            newLight.nextDirection = _nextDirection;
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
        //Create the combined lights
        List<List<LightSourceHold>> allLights = new();
        List<LightSourceHold> yellowLights = new();
        List<LightSourceHold> cyanLights = new();
        List<LightSourceHold> purpleLights = new();
        List<LightSourceHold> whiteLights = new();
        allLights.Add(yellowLights);
        allLights.Add(cyanLights);
        allLights.Add(purpleLights);
        allLights.Add(whiteLights);

        LightSourceHold masterYellowLight = new();
        LightSourceHold masterCyanLight = new();
        LightSourceHold masterPurpleLight = new();
        LightSourceHold masterWhiteLight = new();
        masterYellowLight.Index = -100;
        masterCyanLight.Index = -100;
        masterPurpleLight.Index = -100;
        masterWhiteLight.Index = -100;

        int length = CurrentSources.Sources.Count;
        for (int i = length - 1; i >= 0; i--)
        {
            if (CurrentSources.Sources[i].LifeInSeconds < Time.time || 
                CurrentSources.Sources[i].ColorOnSurface == LightPuzzleHandler.LightColor.Red || 
                CurrentSources.Sources[i].ColorOnSurface == LightPuzzleHandler.LightColor.Blue || 
                CurrentSources.Sources[i].ColorOnSurface == LightPuzzleHandler.LightColor.Green || 
                CurrentSources.Sources[i].ColorOnSurface == LightPuzzleHandler.LightColor.Close || 
                CurrentSources.Sources[i].ColorOnSurface == LightPuzzleHandler.LightColor.DeadWhite || 
                CurrentSources.Sources[i].ColorOnSurface == LightPuzzleHandler.LightColor.Impostor)
            {
                CurrentSources.Sources.RemoveAt(i);
                continue;
            }

            LightSourceHold Light = new();
            Light.LightSource = CurrentSources.Sources[i].LightSource;
            Light.ColorOnSurface = CurrentSources.Sources[i].ColorOnSurface;
            Light.LifeInSeconds = CurrentSources.Sources[i].LifeInSeconds;
            Light.CurrentBounce = CurrentSources.Sources[i].CurrentBounce;
            Light.nextDirection = CurrentSources.Sources[i].nextDirection;
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
                        masterLight.LifeInSeconds = allLights[u][z].LifeInSeconds;
                        masterLight.CurrentBounce = allLights[u][z].CurrentBounce;
                        masterLight.nextDirection = allLights[u][z].nextDirection;
                        masterLight.Index = z;
                    }
                }

                if (u == 0)
                    masterYellowLight = masterLight;
                else if(u == 1)
                    masterCyanLight = masterLight;
                else if(u == 2)
                    masterPurpleLight = masterLight;
                else
                    masterWhiteLight = masterLight;
            }
        }

        
        if (masterYellowLight.Index >= 0 && _lightColor == LightPuzzleHandler.LightColor.Yellow)
        {
            List<Vector3> incomingDirections = new();
            List<Vector3> mirrorNormals = new();
            foreach (var item2 in allLights[0])
            {
                item2.LightSource.SetBlockedByMixUntil(Time.time + 2f, false, item2.CurrentBounce, Vector3.zero);
                incomingDirections.Add(item2.nextDirection);
                mirrorNormals.Add(item2.mirrorNormal);
            }

            Vector3 averageDirection = GetAverageDirection(incomingDirections, mirrorNormals);
            masterYellowLight.LightSource.SetBlockedByMixUntil(Time.time + 2f, true, masterYellowLight.CurrentBounce, averageDirection);

            return averageDirection;
        }

        if (masterCyanLight.Index >= 0 && _lightColor == LightPuzzleHandler.LightColor.Cyan)
        {
            List<Vector3> incomingDirections = new();
            List<Vector3> mirrorNormals = new();
            foreach (var item2 in allLights[1])
            {
                item2.LightSource.SetBlockedByMixUntil(Time.time + 2f, false, item2.CurrentBounce, Vector3.zero);
                incomingDirections.Add(item2.nextDirection);
                mirrorNormals.Add(item2.mirrorNormal);
            }

            Vector3 averageDirection = GetAverageDirection(incomingDirections, mirrorNormals);
            masterYellowLight.LightSource.SetBlockedByMixUntil(Time.time + 2f, true, masterYellowLight.CurrentBounce, averageDirection);
            Debug.Log("Cyan average direction: " + averageDirection);
            return averageDirection;
        }

        if (masterPurpleLight.Index >= 0 && _lightColor == LightPuzzleHandler.LightColor.Purple)
        {
            List<Vector3> incomingDirections = new();
            List<Vector3> mirrorNormals = new();
            foreach (var item2 in allLights[2])
            {
                item2.LightSource.SetBlockedByMixUntil(Time.time + 2f, false, item2.CurrentBounce, Vector3.zero);
                incomingDirections.Add(item2.nextDirection);
                mirrorNormals.Add(item2.mirrorNormal);
            }

            Vector3 averageDirection = GetAverageDirection(incomingDirections, mirrorNormals);
            masterYellowLight.LightSource.SetBlockedByMixUntil(Time.time + 2f, true, masterYellowLight.CurrentBounce, averageDirection);

            return averageDirection;
        }

        if (masterWhiteLight.Index >= 0 && _lightColor == LightPuzzleHandler.LightColor.White)
        {
            List<Vector3> incomingDirections = new();
            List<Vector3> mirrorNormals = new();
            foreach (var item2 in allLights[3])
            {
                item2.LightSource.SetBlockedByMixUntil(Time.time + 2f, false, item2.CurrentBounce, Vector3.zero);
                incomingDirections.Add(item2.nextDirection);
                mirrorNormals.Add(item2.mirrorNormal);
            }

            Vector3 averageDirection = GetAverageDirection(incomingDirections, mirrorNormals);
            masterYellowLight.LightSource.SetBlockedByMixUntil(Time.time + 2f, true, masterYellowLight.CurrentBounce, averageDirection);

            return averageDirection;
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
        //if (mirroredNormal.Count == 0)
        //    return Vector3.zero;

        //Vector3 combinedDirection = Vector3.zero;

        //foreach (var ray in incomingDirections)
        //{
        //    combinedDirection += ray.normalized;
        //}

        //combinedDirection /= incomingDirections.Count;

        //Vector3 mirrorNormal = mirroredNormal[0];
        //Vector3 reflectionDirection = ReflectRay(combinedDirection, mirrorNormal);

        //return reflectionDirection;
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
        public LightPuzzleHandler.LightColor ColorOnSurface;
        public int CurrentBounce;
        public float LifeInSeconds;
        public Vector3 nextDirection;
        public Vector3 mirrorNormal;
    }

    public enum DirectorType
    {
        Mix_Together,
        Mix_Seperated,
        Mix_Blocked,
    }
}
