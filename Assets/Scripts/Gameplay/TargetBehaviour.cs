using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TargetBehaviour : MonoBehaviour
{
    public List<LightPuzzleHandler.LightColor> RequiredColors = new();
    public List<ActiveSourceOn> ActiveSources = new();

    private int Level;
    public void UpdateLightsOn()
    {

    }

    public bool IsCompleted()
    {
        List<int> requirementCount = new List<int>(20);
        foreach (var item in RequiredColors)
            requirementCount[(int)item] += 1;

        List<int> sourcesCount = new List<int>(20);
        foreach (var item in ActiveSources)
            sourcesCount[(int)item.LightColor] += 1;

        int final = requirementCount.Count;
        for (int i = 0; i < final; i++)
            if (requirementCount[i] != sourcesCount[i])
                return false;

        return true;
    }

    public void SetLevel(int _level)
    {
        Level = _level;
    }

    [System.Serializable]
    public struct ActiveSourceOn
    {
        public Transform Source;
        public LightPuzzleHandler.LightColor LightColor;
        public float LifeTime;
    }
}

