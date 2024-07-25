using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class TargetBehaviour : MonoBehaviour
{
    [Header("Red/Green/Blue Only")]
    public List<LightPuzzleHandler.LightColor> RequiredColors = new();

    [Header("UI/Canvas")]
    public TargetBarHandler TargetBar;

    [Header("Runtime Datas")]
    public List<ActiveSourceOn> ActiveSources = new();

    private float WaitUntil = 3f;
    private int Level;
    private int ObjectID;
    public void AddLightsOn(LightPuzzleHandler.LightColor _hitLight, Transform _lightOwner)
    {
        float timer = Time.time;
#if UNITY_EDITOR
        if(!Application.isPlaying)
            timer = (float)EditorApplication.timeSinceStartup;
#endif
        bool containsOwner = false;
        int length = ActiveSources.Count;
        for (int i = length - 1; i >= 0; i--)
        {
            if (ActiveSources[i].Source == _lightOwner)
            {
                containsOwner = true;
                ActiveSources[i] = new()
                {
                    LifeTime = timer + 0.2f,
                    LightColor = _hitLight,
                    Source = _lightOwner
                };
            }
        }

        if (!containsOwner)
        {
            ActiveSources.Add(new()
            {
                LifeTime = timer + 0.2f,
                LightColor = _hitLight,
                Source = _lightOwner
            });
        }

        LightPuzzleHandler.instance.GetLevelBehaviour(Level).CheckLevelCompleted();
    }

    public void UpdateLightsOn()
    {
        float timer = Time.time;
#if UNITY_EDITOR
        if (!Application.isPlaying)
            timer = (float)EditorApplication.timeSinceStartup;
#endif
        int length = ActiveSources.Count;
        for (int i = length - 1; i >= 0; i--)
        {
            if (ActiveSources[i].LifeTime < timer)
            {
                ActiveSources.RemoveAt(i);
                IsCompleted();
            }
        }
    }

    public void UpdateUI(float _r, float _g, float _b)
    {
        TargetBar.SetColorPercents(_r, _g, _b);
    }

    private void FixedUpdate()
    {
        UpdateLightsOn();
    }

    public bool IsCompleted()
    {
        float redRequired = 0;
        float greenRequired = 0;
        float blueRequired = 0;
        
        float redAmount = 0;
        float greenAmount = 0;
        float blueAmount = 0;

        foreach (var item in RequiredColors)
        {
            if (item == LightPuzzleHandler.LightColor.Red)
                redRequired++;
            else if (item == LightPuzzleHandler.LightColor.Green)
                greenRequired++;
            else if (item == LightPuzzleHandler.LightColor.Blue)
                blueRequired++;
        }

        foreach (var item in ActiveSources)
        {
            if (item.LightColor == LightPuzzleHandler.LightColor.Red)
                redAmount++;
            else if (item.LightColor == LightPuzzleHandler.LightColor.Green)
                greenAmount++;
            else if (item.LightColor == LightPuzzleHandler.LightColor.Blue)
                blueAmount++;
            else if (item.LightColor == LightPuzzleHandler.LightColor.Yellow)
            {
                greenAmount++;
                redAmount++;
            }
            else if (item.LightColor == LightPuzzleHandler.LightColor.Purple)
            {
                blueAmount++;
                redAmount++;
            }
            else if (item.LightColor == LightPuzzleHandler.LightColor.Cyan)
            {
                blueAmount++;
                greenAmount++;
            }
            else if (item.LightColor == LightPuzzleHandler.LightColor.White)
            {
                blueAmount++;
                greenAmount++;
                redAmount++;
            }
        }

        //Debug.Log("redAmount/redRequired: " + redAmount + "/" + redRequired + " - greenAmount/greenRequired: " + greenAmount + "/" + greenRequired + " - blueAmount/blueRequired: " + blueAmount + "/" + blueRequired + " ---- WaitUntil: " + WaitUntil);
        bool puzzleCompleted = redRequired == redAmount && greenRequired == greenAmount && blueRequired == blueAmount;
        if (puzzleCompleted)
        {
            if (WaitUntil > 0)
                WaitUntil -= Time.deltaTime;
            else
                WaitUntil = 0;
        }
        else
            WaitUntil = 3;

        float r = 0;
        if (redRequired == 0)
        {
            r = 1;
            if (redAmount > 0)
                r = 2;
        }
        else
            r = redAmount / redRequired;

        float g = 0;
        if (greenRequired == 0)
        {
            g = 1;
            if (greenAmount > 0)
                g = 2;
        }
        else
            g = greenAmount / greenRequired;

        float b = 0;
        if (blueRequired == 0)
        {
            b = 1;
            if (blueAmount > 0)
                b = 2;
        }
        else
            b = blueAmount / blueRequired;

        UpdateUI(r, g, b);

        return puzzleCompleted && WaitUntil <= 0;
    }

    public void SetLevel(int _level)
    {
        Level = _level;
        ObjectID = transform.GetSiblingIndex();
        int r = 0;
        int g = 0;
        int b = 0;

        foreach (var item in RequiredColors)
        {
            if (item == LightPuzzleHandler.LightColor.Red)
                r++;
            else if (item == LightPuzzleHandler.LightColor.Green)
                g++;
            else if (item == LightPuzzleHandler.LightColor.Blue)
                b++;
        }

        TargetBar.InitRequirements(r,g,b);
    }

    [System.Serializable]
    public struct ActiveSourceOn
    {
        public Transform Source;
        public LightPuzzleHandler.LightColor LightColor;
        public float LifeTime;
    }

    public int GetID()
    {
        return ObjectID;
    }
}

