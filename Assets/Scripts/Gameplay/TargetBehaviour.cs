using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetBehaviour : MonoBehaviour
{
    public List<LightPuzzleHandler.LightColor> RequiredColors = new();
    public List<ActiveSourceOn> ActiveSources = new();

    private int Level;
    public void AddLightsOn(LightPuzzleHandler.LightColor _hitLight, Transform _lightOwner)
    {
        bool containsOwner = false;
        int length = ActiveSources.Count;
        for (int i = length - 1; i >= 0; i--)
        {
            if (ActiveSources[i].Source == _lightOwner)
            {
                containsOwner = true;
                ActiveSources[i] = new()
                {
                    LifeTime = Time.time + 0.2f,
                    LightColor = _hitLight,
                    Source = _lightOwner
                };
            }
        }

        if (!containsOwner)
        {
            ActiveSources.Add(new()
            {
                LifeTime = Time.time + 0.2f,
                LightColor = _hitLight,
                Source = _lightOwner
            });
        }

        LightPuzzleHandler.instance.GetLevelBehaviour(Level).CheckLevelCompleted();
    }

    public void UpdateLightsOn()
    {
        int length = ActiveSources.Count;
        for (int i = length - 1; i >= 0; i--)
        {
            if (ActiveSources[i].LifeTime < Time.time)
            {
                ActiveSources.RemoveAt(i);
                UpdateUI();
            }
        }
    }

    public void UpdateUI()
    {

    }

    private void FixedUpdate()
    {
        UpdateLightsOn();
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

