using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class LightPuzzleHandler : MonoBehaviour
{
    public static LightPuzzleHandler instance {  get; private set; }

    [SerializeField] List<TrapEffectHelper> TargetEffectMaterials = new List<TrapEffectHelper>();
    [SerializeField] List<TrapSoundHelper> TargetEffectSounds = new List<TrapSoundHelper>();
    [SerializeField] List<ArrowHelper> Arrows = new List<ArrowHelper>();
    public List<LevelBehaviour> Levels;
    public int PuzzleRoomCount {get=>Levels.Count;}
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
    public LevelBehaviour CurrentLevelHandler { get; set; }
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        Levels = FindObjectsByType<LevelBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID).ToList();
    }

    public void SetCurrentLevelThis(LevelBehaviour _level)
    {
        CurrentLevelHandler = _level;
    }

    public LightsHolder GetLightsParent(int _level)
    {
        foreach (var item in Levels)
            if (item.LevelID == _level)
                return item.GetLightsParent();

        Debug.LogError("Id mevcut degil! Tekrar check et.");
        return null;
    }

    public DirectorsHolder GetDirectorsParent(int _level)
    {
        foreach (var item in Levels)
            if (item.LevelID == _level)
                return item.GetDirectorsParent();

        Debug.LogError("Id mevcut degil! Tekrar check et.");
        return null;
    }

    public TrapsHolder GetTrapsParent(int _level)
    {
        foreach (var item in Levels)
            if (item.LevelID == _level)
                return item.GetTrapsHolder();

        Debug.LogError("Id mevcut degil! Tekrar check et.");
        return null;
    }
    
    public TargetHolder GetTargetsParent(int _level)
    {
        foreach (var item in Levels)
            if (item.LevelID == _level)
                return item.GetTargetsParent();

        Debug.LogError("Id mevcut degil! Tekrar check et.");
        return null;
    }

    public LevelBehaviour GetLevelBehaviour(int _level)
    {
        foreach (var item in Levels)
        {
            if (_level == item.LevelID)
            {
                return item;
            }
        }

        return null;
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

    public static ( Color _target, bool _hasMix, LightColor _lightColor, List<LightColor> _coreColors) GetMixedColor(List<LightColor> _colors)
    {
        //Improve here
        bool containsBlue = _colors.Contains(LightColor.Blue);
        bool containsRed = _colors.Contains(LightColor.Red);
        bool containsGreen = _colors.Contains(LightColor.Green);

        if (containsBlue && containsRed && containsGreen)
        {
            return (Color.white, true, LightColor.White, new() { LightColor.Blue , LightColor.Red, LightColor.Green});
        }
        else if (containsBlue && containsRed)
        {
            foreach (var item in LightsOfLevel)
                if (item.lightType == LightColor.Purple)
                    return (item.ColorOfLight, true, LightColor.Purple, new() { LightColor.Blue, LightColor.Red });
        }
        else if (containsBlue && containsGreen)
        {
            foreach (var item in LightsOfLevel)
                if (item.lightType == LightColor.Cyan)
                    return (item.ColorOfLight, true, LightColor.Cyan, new() { LightColor.Blue, LightColor.Green });
        }
        else if (containsRed && containsGreen)
        {
            foreach (var item in LightsOfLevel)
                if (item.lightType == LightColor.Yellow)
                    return (item.ColorOfLight, true, LightColor.Yellow, new() { LightColor.Red, LightColor.Green });
        }
        foreach (var item in LightsOfLevel)
            if (item.lightType == _colors[0])
                return (item.ColorOfLight, false, item.lightType, new());

        return (Color.white, false, LightColor.White, new());
    }
    public Material GetMaterialInEffectOnTarget(TrapType _trapType, EffectType _effectType)
    {
        List<Material> _materials = TargetEffectMaterials.Where(x => x.TrapType == _trapType && x.EffectType == _effectType).Select(x => x.Material).ToList();
        return _materials[Random.Range(0, _materials.Count)];
    }
    public AudioClip GetSoundInEffectOnTarget(TrapType _trapType)
    {
        List<AudioClip> _clips = TargetEffectSounds.Where(x => x.TrapType == _trapType).Select(x => x.Clip).ToList();
        return _clips[Random.Range(0, _clips.Count)];
    }
    public GameObject GetArrow(ArrowType _arrowType)
    {
        List<ArrowHelper> randomArrows = Arrows.Where(x=> x.ArrowType == _arrowType).ToList();
        return randomArrows[Random.Range(0, randomArrows.Count)].Arrow;
    }


    public static class LayerMaskHelper
    {
        private const string PlayerLayer = "Animal";
        private const string DirectorLayer = "Director";
        private const string BatteryLayer = "Battery";
        private const string PlayerIgnoreLayer = "PlayerIgnore";
        private const string PlayerCarryLayer = "PlayerCarry";

        public static LayerMask LightLayer { get; private set; }
        public static LayerMask CarryLayer { get; private set; }
        public static LayerMask DirectorFloor { get; private set; }

        static LayerMaskHelper()
        {
            int excludeLayer = LayerMask.NameToLayer(DirectorLayer);
            int playerLayer = LayerMask.NameToLayer(PlayerLayer);
            int excludeLayerMask = (1 << excludeLayer) | (1 << playerLayer);
            LightLayer = ~excludeLayerMask;

            int directorLayer = LayerMask.NameToLayer(DirectorLayer);
            int batteryLayer = LayerMask.NameToLayer(BatteryLayer); // Yeni eklendi
            CarryLayer = (1 << directorLayer) | (1 << batteryLayer); // Güncellendi

            int playerIgnoreLayer = LayerMask.NameToLayer(PlayerIgnoreLayer);
            int playerCarryLayer = LayerMask.NameToLayer(PlayerCarryLayer);

            // Calculate the mask for layers to exclude
            int excludeLayersMask = (1 << directorLayer) | (1 << playerIgnoreLayer) | (1 << playerCarryLayer);
            DirectorFloor = ~excludeLayersMask;
        }
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

