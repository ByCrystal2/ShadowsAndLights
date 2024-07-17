using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif

[ExecuteAlways]
public class DirectorBehaviour : MonoBehaviour
{
    public Light ReflectLight;
    [SerializeField] RotateAngle RotateAngel;
    [SerializeField, Range(0, 10)] float RotateSpeed;
    IRotateAnObject rotateAnObject;
    private bool ReflectionActive;
    
    public List<ColorOnReflect> ColorsOnReflect = new List<ColorOnReflect>();
    private void Awake()
    {
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
    void SetRotateValues() {if (rotateAnObject != null) { rotateAnObject.RotatableObject.RotateAngel = RotateAngel; rotateAnObject.RotatableObject.RotateSpeed = RotateSpeed; }
    }
#if UNITY_EDITOR
    private EditorCoroutine currentEditorCoroutine;
#endif

    private int LevelID;

    void Awake()
    {
        LevelID = transform.parent.GetComponent<DirectorsHolder>().LevelID;
    }

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

    [System.Serializable]
    public struct ColorOnReflect
    {
        public LightPuzzleHandler.LightColor ColorOnSurface;
        public float LifeInSeconds;
    }
}
