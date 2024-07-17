using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class LightPuzzleHandler : MonoBehaviour
{
    public static LightPuzzleHandler instance {  get; private set; }

    [SerializeField] List<TrapEffectHelper> TargetEffectMaterials = new List<TrapEffectHelper>();
    [SerializeField] List<TrapSoundHelper> TargetEffectSounds = new List<TrapSoundHelper>();
    [SerializeField] List<GameObject> Arrows = new List<GameObject>();
    public List<LightsHolder> LightsParents;
    public List<DirectorsHolder> DirectorsParents;
    public List<TrapsHolder> TrapsParents;
    public List<TargetHolder> TargetsParents;

    public static List<LightData> LightsOfLevel = new() {
        new(){ lightType = LightColor.White ,ColorOfLight = new() { r = 1, b = 1, g = 1, a = 1} },
        new(){ lightType = LightColor.Red ,ColorOfLight = new() { r = 1, b = 0.2f, g = 0.2f, a = 1} },
        new(){ lightType = LightColor.Blue ,ColorOfLight = new() { r = 0, b = 0.95f, g = 0.3f, a = 1} },
        new(){ lightType = LightColor.Yellow ,ColorOfLight = new() { r = 1, b = 0f, g = 0.85f, a = 1} },
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

        LightsParents = FindObjectsByType<LightsHolder>(FindObjectsSortMode.InstanceID).ToList();
        DirectorsParents = FindObjectsByType<DirectorsHolder>(FindObjectsSortMode.InstanceID).ToList();
        TrapsParents = FindObjectsByType<TrapsHolder>(FindObjectsSortMode.InstanceID).ToList();
        TargetsParents = FindObjectsByType<TargetHolder>(FindObjectsSortMode.InstanceID).ToList();
    }

    public Transform GetLightsParent(int _level)
    {
        foreach (var item in LightsParents)
            if (item.LevelID == _level)
                return item.transform;

        Debug.LogError("Id mevcut degil! Tekrar check et.");
        return null;
    }

    public Transform GetDirectorsParent(int _level)
    {
        foreach (var item in DirectorsParents)
            if (item.LevelID == _level)
                return item.transform;

        Debug.LogError("Id mevcut degil! Tekrar check et.");
        return null;
    }

    public Transform GetTrapsParent(int _level)
    {
        foreach (var item in TrapsParents)
            if (item.LevelID == _level)
                return item.transform;

        Debug.LogError("Id mevcut degil! Tekrar check et.");
        return null;
    }
    
    public Transform GetTargetsParent(int _level)
    {
        foreach (var item in TargetsParents)
            if (item.LevelID == _level)
                return item.transform;

        Debug.LogError("Id mevcut degil! Tekrar check et.");
        return null;
    }

    //public void SetAllDirectorsOutlinedForSeconds(float _seconds)
    //{
    //    CancelInvoke();
    //    SetDirectorsOutlined(true);
    //    Invoke(nameof(ResetLayerOfDirectors), _seconds);
    //}

    //void ResetLayerOfDirectors()
    //{
    //    SetDirectorsOutlined(false);
    //}

    //void SetDirectorsOutlined(bool _outlineActive)
    //{
    //    List<Transform> All = new();
    //    foreach (Transform t in DirectorsParent) 
    //    { 
    //        All.Add(t);
    //        Transform[] childs = t.GetComponentsInChildren<Transform>();
    //        foreach (var item in childs)
    //        {
    //            All.Add(item);
    //        }
    //    }

    //    int newLayer = _outlineActive ? LayerMask.NameToLayer("PlayerCarry") : LayerMask.NameToLayer("PlayerIgnore");
    //    foreach (var item in All)
    //        item.gameObject.layer = newLayer;
    //}

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
    public Material GetMaterialInEffectOnTarget(TrapEffectType _trapEffectType)
    {
        List<Material> _materials = TargetEffectMaterials.Where(x => x.EffectType == _trapEffectType).Select(x => x.Material).ToList();
        return _materials[Random.Range(0, _materials.Count)];
    }
    public AudioClip GetSoundInEffectOnTarget(TrapEffectType _trapEffectType)
    {
        List<AudioClip> _clips = TargetEffectSounds.Where(x => x.EffectType == _trapEffectType).Select(x => x.Clip).ToList();
        return _clips[Random.Range(0, _clips.Count)];
    }
    public GameObject GetArrow()
    {
        return Arrows[Random.Range(0, Arrows.Count)];
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

