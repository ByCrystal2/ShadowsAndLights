using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class LanguageDatabase : MonoBehaviour
{
    public MainLanguageData Language = new MainLanguageData();
    public static LanguageDatabase instance { get; private set; }

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        CreateLanguages();
    }

    public void CreateLanguages()
    {
        TextAsset languageData = Resources.Load<TextAsset>("LanguageDatabase/LanguageDatabase");
        if (languageData != null)
        {
            string jsonString = languageData.text;
            MainLanguageDataCore dataList = JsonUtility.FromJson<MainLanguageDataCore>(jsonString);
            Language = new MainLanguageData();
            Language.Texts = ConvertCoreLanguageListToGameList(dataList.MainLanguageData.Texts);
        }
        else
        {
            Debug.LogError("Could not find the JSON file in the specified path.");
        }
        
    }

    List<LanguageData> ConvertCoreLanguageListToGameList(List<OriginalLanguageData> _list)
    {
        List<LanguageData> list = new List<LanguageData>();
        foreach (var item in _list)
        {
            LanguageData l = new LanguageData();
            l.Key = item.Key;
            l.English = "";

            if (CurrentActiveLanguage == Languages.English)
                l.ActiveLanguage = item.English;
            else if (CurrentActiveLanguage == Languages.Turkish)
                l.ActiveLanguage = item.Turkish;
            else if (CurrentActiveLanguage == Languages.Thai)
                l.ActiveLanguage = item.Thai;
            else if (CurrentActiveLanguage == Languages.Sp_Chinese)
                l.ActiveLanguage = item.Sp_Chinese;
            else if (CurrentActiveLanguage == Languages.Russian)
                l.ActiveLanguage = item.Russian;
            else if (CurrentActiveLanguage == Languages.Spanish)
                l.ActiveLanguage = item.Spanish;
            else if (CurrentActiveLanguage == Languages.BR_Portuguese)
                l.ActiveLanguage = item.BR_Portuguese;
            else if (CurrentActiveLanguage == Languages.German)
                l.ActiveLanguage = item.German;
            else if (CurrentActiveLanguage == Languages.Japanese)
                l.ActiveLanguage = item.Japanese;
            else if (CurrentActiveLanguage == Languages.French)
                l.ActiveLanguage = item.French;
            else if (CurrentActiveLanguage == Languages.Polish)
                l.ActiveLanguage = item.Polish;
            else if (CurrentActiveLanguage == Languages.Korean)
                l.ActiveLanguage = item.Korean;
            else if (CurrentActiveLanguage == Languages.Indonesian)
                l.ActiveLanguage = item.Indonesian;
            else if (CurrentActiveLanguage == Languages.Malay)
                l.ActiveLanguage = item.Malay;

            list.Add(l);
        }

        return list;
    }

    public string GetText(string Key)
    {
        bool missingText = true;
        string xOriginal = "Missing Original Key: " + Key;
        string x = "Missing Current Key: " + Key;
        List<LanguageData> targetList = Language.Texts;

        foreach (var item in targetList)
            if (item.Key == Key)
            {
                missingText = false;
                xOriginal = item.English;

                if (item.ActiveLanguage != "")
                {
                    x = item.ActiveLanguage;
                }
                else
                {
                    x = "Missing Language: " + GetLanguageText((int)GetCurrentLanguage()) + " / Key: " + Key;
                    missingText = true;
                }
            }

        if (missingText)
            Debug.LogError("Language Error: " + x + "\nOriginal Error: " + xOriginal);
        return x;
    }

    public enum Languages
    {
        English,
        Turkish,
        Thai,
        Sp_Chinese,
        Russian,
        Spanish,
        BR_Portuguese,
        German,
        Japanese,
        French,
        Polish,
        Korean,
        Indonesian,
        Malay,
    }

    private Languages CurrentActiveLanguage = 0;

    public void SetNewLanguage(int _newLanguage)
    {
        if(_newLanguage == (int)Languages.English)
            CurrentActiveLanguage = Languages.English;
        else if(_newLanguage == (int)Languages.Turkish)
            CurrentActiveLanguage = Languages.Turkish;
        else if(_newLanguage == (int)Languages.Thai)
            CurrentActiveLanguage = Languages.Thai;
        else if (_newLanguage == (int)Languages.Sp_Chinese)
            CurrentActiveLanguage = Languages.Sp_Chinese;
        else if (_newLanguage == (int)Languages.Russian)
            CurrentActiveLanguage = Languages.Russian;
        else if (_newLanguage == (int)Languages.Spanish)
            CurrentActiveLanguage = Languages.Spanish;
        else if (_newLanguage == (int)Languages.BR_Portuguese)
            CurrentActiveLanguage = Languages.BR_Portuguese;
        else if (_newLanguage == (int)Languages.German)
            CurrentActiveLanguage = Languages.German;
        else if (_newLanguage == (int)Languages.Japanese)
            CurrentActiveLanguage = Languages.Japanese;
        else if (_newLanguage == (int)Languages.French)
            CurrentActiveLanguage = Languages.French;
        else if (_newLanguage == (int)Languages.Polish)
            CurrentActiveLanguage = Languages.Polish;
        else if (_newLanguage == (int)Languages.Korean)
            CurrentActiveLanguage = Languages.Korean;
        else if (_newLanguage == (int)Languages.Indonesian)
            CurrentActiveLanguage = Languages.Indonesian;
        else if (_newLanguage == (int)Languages.Malay)
            CurrentActiveLanguage = Languages.Malay;

        CreateLanguages();
    }

    public int GetLanguagesCount()
    {
        return 14;
    }

    public string GetLanguageText(int _language)
    {
        if (_language == (int)Languages.English)
            return "En";
        else if (_language == (int)Languages.Turkish)
            return "Tr";
        else if (_language == (int)Languages.Thai)
            return "Th";
        else if (_language == (int)Languages.Sp_Chinese)
            return "Ch";
        else if (_language == (int)Languages.Russian)
            return "Ru";
        else if (_language == (int)Languages.Spanish)
            return "Es";
        else if (_language == (int)Languages.BR_Portuguese)
            return "Pt";
        else if (_language == (int)Languages.German)
            return "Gr";
        else if (_language == (int)Languages.Japanese)
            return "Ja";
        else if (_language == (int)Languages.French)
            return "Fr";
        else if (_language == (int)Languages.Polish)
            return "Pl";
        else if (_language == (int)Languages.Korean)
            return "Ko";
        else if (_language == (int)Languages.Indonesian)
            return "Id";
        else if (_language == (int)Languages.Malay)
            return "Ms";
        else
            return "Nun";
    }

    public Languages GetCurrentLanguage()
    {
        return CurrentActiveLanguage;
    }

    public enum TargetLanguageList
    {
        Texts,
    }

    [System.Serializable]
    public class MainLanguageDataCore
    {
        public OriginalMainLanguageData MainLanguageData;
    }

    [System.Serializable]
    public class MainLanguageData
    {
        public List<LanguageData> Items = new List<LanguageData>();
        public List<LanguageData> Texts = new List<LanguageData>();
        public List<LanguageData> Bonuses = new List<LanguageData>();
        public List<LanguageData> Skills = new List<LanguageData>();
        public List<LanguageData> Storys = new List<LanguageData>();
        public List<LanguageData> Knowledges = new List<LanguageData>();
        public List<LanguageData> Researches = new List<LanguageData>();
        public List<LanguageData> Settings = new List<LanguageData>();
        public List<LanguageData> Dialogs = new List<LanguageData>();
        public List<LanguageData> Quests = new List<LanguageData>();
    }

    [System.Serializable]
    public class LanguageData
    {
        public string Key;
        public string English;
        public string ActiveLanguage;
    }

    [System.Serializable]
    public class OriginalMainLanguageData
    {
        public List<OriginalLanguageData> Texts = new List<OriginalLanguageData>();
    }             

    [System.Serializable]
    public class OriginalLanguageData
    {
        public string Header;
        public string Key;
        public string English;
        public string Turkish;
        public string Thai;
        public string Sp_Chinese;
        public string Russian;
        public string Spanish;
        public string BR_Portuguese;
        public string German;
        public string Japanese;
        public string French;
        public string Polish;
        public string Korean;
        public string Indonesian;
        public string Malay;
    }
}
