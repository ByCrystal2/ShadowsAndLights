using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance { get; private set; }

    public Settings ActiveSetting;
    public Settings LastActiveSetting;

    private List<Vector2Int> ScreenResolutions = new List<Vector2Int>
    {
        new Vector2Int(1024,768),
        new Vector2Int(1280,800),
        new Vector2Int(1280,1024),
        new Vector2Int(1366,768),
        new Vector2Int(1440,900),
        new Vector2Int(1600,900),
        new Vector2Int(1920,1080),
        new Vector2Int(2560,1440),
    };

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        LastActiveSetting = new Settings();
        LoadActiveSetting();
    }

    public void LoadActiveSetting()
    {
        string path = "MyAmazingSetting";
        if (File.Exists(Application.persistentDataPath + "/" + path + ".setting"))
        {
            string jsonString = File.ReadAllText(Application.persistentDataPath + "/" + path + ".setting");
            ActiveSetting = JsonUtility.FromJson<Settings>(jsonString);
        }
        else
        {
            ActiveSetting = new Settings();
            //test icin yazilmis kodlardir, istenirse silinebilir. (Ahmet yazdi.)
            AudioSettings audioSettings = new AudioSettings();
            audioSettings.MainVolume = 80f;
            audioSettings.SFXVolume = 80f;
            audioSettings.BGMVolume = 80f;
            ActiveSetting.audioSettings = audioSettings;
            //test icin yazilmis kodlardir, istenirse silinebilir. (Ahmet yazdi.)
            Debug.Log("Settings could not be found. Will be set as default");
        }
        ApplySettingEffects();
    }

    void ApplySettingEffects()
    {
        LanguageDatabase.instance.SetNewLanguage(ActiveSetting.displaySettings.LanguageID);
        QualitySettings.SetQualityLevel((int)ActiveSetting.graphicSettings.GraphicQuality);
        int res = ActiveSetting.graphicSettings.ScreenResolution;
        Screen.SetResolution(ScreenResolutions[res].x, ScreenResolutions[res].y, true);
    }

    void SaveSettings()
    {
        string path = "MyAmazingSetting";
        if (File.Exists(Application.persistentDataPath + "/" + path + ".setting"))
            File.Delete(Application.persistentDataPath + "/" + path + ".setting");

        string jsonString = JsonUtility.ToJson(ActiveSetting);
        File.WriteAllText(Application.persistentDataPath + "/" + path + ".setting", jsonString);
    }

    [System.Serializable]
    public struct Settings
    {
        public AudioSettings audioSettings;
        public GraphicSettings graphicSettings;
        public DisplaySettings displaySettings;
    }

    [System.Serializable]
    public struct AudioSettings
    {
        public float MainVolume;
        public float BGMVolume;
        public float SFXVolume;
    }

    [System.Serializable]
    public struct GraphicSettings
    {
        public GraphicQuality GraphicQuality;
        public int ScreenResolution;
    }

    [System.Serializable]
    public struct DisplaySettings
    {
        public int LanguageID;
    }

    public enum GraphicQuality 
    { 
        Low,
        Medium,
        High,
    }
}
