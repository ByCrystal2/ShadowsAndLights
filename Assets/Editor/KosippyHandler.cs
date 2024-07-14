using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Collections.Generic;

public class KosippyHandler : EditorWindow
{
    [MenuItem("Kosippy/Load Language Data")]
    public static void LoadLanguageDataEditor()
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(LoadLanguageData());
    }

    private static IEnumerator LoadLanguageData()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://script.google.com/macros/s/AKfycbxthzFi4jXktn3FrYfTsRAyFJTKiDugoKjN5o0f2bKjl5WAHyp4cO4XAX35c6fErEI1bw/exec");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            Debug.Log("Language Data: " + json);
            string directoryPath = Application.dataPath + "/Resources/LanguageDatabase";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string filePath = directoryPath + "/LanguageDatabase.json";
            File.WriteAllText(filePath, json);
            Debug.Log("Language data saved to " + filePath);
            AssetDatabase.Refresh();
        }
    }
}
