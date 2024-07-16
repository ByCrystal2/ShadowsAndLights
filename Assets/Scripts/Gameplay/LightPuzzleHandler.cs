using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LightPuzzleHandler : MonoBehaviour
{
    public static LightPuzzleHandler instance {  get; private set; }

    public Transform LightsParent;
    public Transform DirectorsParent;
    public Transform TrapsParent;

    public static List<LightData> LightsOfLevel = new() {
        new(){ lightType = LightColor.White ,ColorOfLight = new() { r = 1, b = 1, g = 1, a = 1} },
        new(){ lightType = LightColor.Red ,ColorOfLight = new() { r = 1, b = 0.2f, g = 0.2f, a = 1} },
        new(){ lightType = LightColor.Blue ,ColorOfLight = new() { r = 0, b = 0.95f, g = 0.3f, a = 1} },
        new(){ lightType = LightColor.Yellow ,ColorOfLight = new() { r = 1, b = 0.1f, g = 0.7f, a = 1} },
        new(){ lightType = LightColor.Cyan ,ColorOfLight = new() { r = 0, b = 0.95f, g = 1, a = 1} },
        new(){ lightType = LightColor.Purple ,ColorOfLight = new() { r = 1, b = 0.85f, g = 0.2f, a = 1} },
        new(){ lightType = LightColor.Green ,ColorOfLight = new() { r = 0, b = 0.5f, g = 1, a = 1} },
        new(){ lightType = LightColor.Impostor ,ColorOfLight = new() { r = 1, b = 1, g = 1, a = 1} },
        new(){ lightType = LightColor.DeadWhite ,ColorOfLight = new() { r = 0.3f, b = 0.3f, g = 0.3f, a = 0.3f} },
        new(){ lightType = LightColor.Close ,ColorOfLight = new() { r = 0, b = 0, g = 0, a = 0 } },
    };

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        LightsParent = FindFirstObjectByType<LightsHolder>().transform;
        DirectorsParent = FindFirstObjectByType<DirectorsHolder>().transform;
        TrapsParent = FindFirstObjectByType<TrapsHolder>().transform;
    }

    public Transform GetLightsParent()
    {
        return LightsParent;
    }

    public Transform GetDirectorsParent()
    {
        return DirectorsParent;
    }

    public Transform GetTrapsParent()
    {
        return TrapsParent;
    }

    public static Gradient GetColorGradient(LightColor color)
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[2];
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];

        colorKey[0].color = GetColorByLight(color);
        colorKey[1].color = GetColorByLight(color);

        colorKey[0].time = 0.0f;
        colorKey[1].time = 1.0f;

        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);

        return gradient;
    }

    [System.Serializable]
    public struct LightData
    {
        public LightColor lightType;
        public Color ColorOfLight;
    }

    public static Color GetColorByLight(LightColor _light)
    {
        foreach (var item in LightsOfLevel)
        {
            if (item.lightType == _light)
            {
                return item.ColorOfLight;
            }
        }

        return Color.white;
    }

    public static ( Color _target, bool _hasMix, LightColor _lightColor) GetMixedColor(List<LightColor> _colors)
    {
        bool containsBlue = _colors.Contains(LightColor.Blue);
        bool containsRed = _colors.Contains(LightColor.Red);
        bool containsGreen = _colors.Contains(LightColor.Green);

        if (containsBlue && containsRed && containsGreen)
        {
            return (Color.white, true, LightColor.White);
        }
        else if (containsBlue && containsRed)
        {
            foreach (var item in LightsOfLevel)
                if (item.lightType == LightColor.Purple)
                    return (item.ColorOfLight, true, LightColor.Purple);
        }
        else if (containsBlue && containsGreen)
        {
            foreach (var item in LightsOfLevel)
                if (item.lightType == LightColor.Cyan)
                    return (item.ColorOfLight, true, LightColor.Cyan);
        }
        else if (containsRed && containsGreen)
        {
            foreach (var item in LightsOfLevel)
                if (item.lightType == LightColor.Yellow)
                    return (item.ColorOfLight, true, LightColor.Yellow);
        }
        foreach (var item in LightsOfLevel)
            if (item.lightType == _colors[0])
                return (item.ColorOfLight, false, item.lightType);

        return (Color.white, false, LightColor.White);
    }

    public enum LightColor
    {
        White,
        Red,
        Blue,
        Yellow,
        Cyan,
        Purple,
        Green,
        Impostor,
        DeadWhite,
        Close,
    }

    public enum LightSourceType
    {
        Normal,
        ChangeColor_Each3,
        Rotate90_Each3,
        Switch_Each3,
        Chargable,
    }
}
