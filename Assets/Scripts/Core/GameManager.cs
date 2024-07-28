using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    public PlayerSaveData currentActiveSaveData;
    public bool isLoadedGame;    
    private const string encryptionKey = "ShadowsAndLightsKosippy";
    private const string fileExtension = ".shadow";
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

    public void SaveGame()
    {
        if (currentActiveSaveData.SaveName == "")
        {
            Debug.LogError("Could not save the game, save data lost.");
            return;
        }

        if (!LoadCompleted)
        {
            Debug.LogError("Loading is not completed yet. Can not save.");
            return;
        }

        string FileName = currentActiveSaveData.UniqueSaveFolderName + "/" + currentActiveSaveData.SaveName;
        if (File.Exists(Application.persistentDataPath + "/" + FileName + fileExtension)) //Check the save name is exist
            File.Delete(Application.persistentDataPath + "/" + FileName + fileExtension);


        long unixTimestamp = ((System.DateTimeOffset)System.DateTime.Now).ToUnixTimeSeconds();

        PlayerSaveData savedata = new PlayerSaveData();
        savedata.SaveName = currentActiveSaveData.SaveName;
        savedata.UniqueSaveFolderName = currentActiveSaveData.UniqueSaveFolderName;
        savedata.SaveImageLocation = currentActiveSaveData.SaveImageLocation;
        savedata.CreatedUID = currentActiveSaveData.CreatedUID != 0 ? currentActiveSaveData.CreatedUID : unixTimestamp;
        savedata.LastSave = unixTimestamp;

        //Currencies
        savedata.Gold = currentActiveSaveData.Gold;
        savedata.Gem = currentActiveSaveData.Gem;

        //Level
        savedata.LevelDatas = currentActiveSaveData.LevelDatas;

        //Battery
        savedata.BatteryLevel = currentActiveSaveData.BatteryLevel;
        currentActiveSaveData = savedata;

        string jsonString = JsonUtility.ToJson(savedata);
        byte[] encryptedData = EncryptStringToBytes(jsonString);
        File.WriteAllBytes(Application.persistentDataPath + "/" + savedata.SaveName + fileExtension, encryptedData);
        Debug.Log("Save Game: " + currentActiveSaveData.SaveName + " / Saved successfully.");
    }

    public void LoadGame()
    {
        string path = currentActiveSaveData.SaveName;
        Debug.Log("Load save file location: " + Application.persistentDataPath + "/" + path + fileExtension);
        if (File.Exists(Application.persistentDataPath + "/" + path + fileExtension))
        {
            byte[] loadBytes = File.ReadAllBytes(Application.persistentDataPath + "/" + path + fileExtension);
            string decryptedData = DecryptStringFromBytes(loadBytes);
            currentActiveSaveData = JsonUtility.FromJson<PlayerSaveData>(decryptedData);
            StartCoroutine(LateLoad());
        }
        else
        {
            Debug.Log("Save could not be found.");
        }
    }

    public bool LoadCompleted = false;
    IEnumerator LateLoad()
    {
        LoadCompleted = false;
        yield return null;
        LoadCompleted = true;
    }

    IEnumerator LateStart()
    {
        yield return null;
        Debug.Log("Whole game for beginning loaded successfully.");
        LoadCompleted = true;
    }

    public void InstallALoadGame(PlayerSaveData _load)
    {
        byte[] loadBytes = File.ReadAllBytes(Application.persistentDataPath + "/" + _load.SaveName + fileExtension);
        string decryptedData = DecryptStringFromBytes(loadBytes);
        currentActiveSaveData = JsonUtility.FromJson<PlayerSaveData>(decryptedData);
    }

    public void CreateNewSave(int _daysInMonth, int _timeSpeed)
    {
        currentActiveSaveData = new();
        long unixTimestamp = ((System.DateTimeOffset)System.DateTime.Now).ToUnixTimeSeconds();
        currentActiveSaveData.CreatedUID = unixTimestamp;
        currentActiveSaveData.SaveName = "New Save";
        currentActiveSaveData.UniqueSaveFolderName = currentActiveSaveData.SaveName + " Folder";
        currentActiveSaveData.SaveImageLocation = currentActiveSaveData.UniqueSaveFolderName + "/SaveImage";
        currentActiveSaveData.LastSave = unixTimestamp;
    }

    public void OnPlayerEnteredGameWorld()
    {
        if(isLoadedGame)
            LoadGame();
        else
            Invoke(nameof(SaveGame), 10);
    }

    private byte[] EncryptStringToBytes(string plainText)
    {
        byte[] encrypted;

        using (Aes aesAlg = Aes.Create())
        {
            // Ensure the key is valid for AES
            aesAlg.Key = GetValidKey(encryptionKey, aesAlg);
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }
                encrypted = msEncrypt.ToArray();
            }
        }

        return encrypted;
    }

    // Decrypt bytes to string
    public string DecryptStringFromBytes(byte[] cipherText)
    {
        string plaintext = "";

        using (Aes aesAlg = Aes.Create())
        {
            byte[] iv = new byte[16];
            System.Array.Copy(cipherText, iv, iv.Length);

            // Ensure the key is valid for AES
            aesAlg.Key = GetValidKey(encryptionKey, aesAlg);
            aesAlg.IV = iv;
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(cipherText, iv.Length, cipherText.Length - iv.Length))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
    }

    private byte[] GetValidKey(string key, Aes aesAlg)
    {
        // Convert the key string to bytes
        byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);

        // AES key sizes: 128-bit (16 bytes), 192-bit (24 bytes), or 256-bit (32 bytes)
        int validKeySize = aesAlg.KeySize / 8;

        // If the key is too large, truncate it. If it's too small, pad it with zeros.
        byte[] validKey = new byte[validKeySize];
        if (keyBytes.Length >= validKeySize)
        {
            Array.Copy(keyBytes, validKey, validKeySize);
        }
        else
        {
            Array.Copy(keyBytes, validKey, keyBytes.Length);
        }

        return validKey;
    }

    public int GetTotalTipCount()
    {
        return 10;
    }

    public void UpdateALevel(LevelSaveData _levelData, bool _endLevel)
    {
        if (_levelData == null || _levelData.LevelID <= 0)
        {
            Debug.LogError("Kaydedilmeye calisan leveldata verileri yanlis. Kaydetme basarisiz.");
            return;
        }

        bool contains = false;
        int length = currentActiveSaveData.LevelDatas.Count;
        for (int i = 0; i < length; i++)
        {
            if (currentActiveSaveData.LevelDatas[i].LevelID == _levelData.LevelID)
            {
                contains = true; //Eger zaten listede var ise update et, yok ise ekle.
                currentActiveSaveData.LevelDatas[i] = new(_levelData);
            }
        }

        if (!contains) //Eger listede mevcut degil ise yeni save datalari ekle.
            currentActiveSaveData.LevelDatas.Add(_levelData); 

        if(_endLevel)
            SaveGame();
    }
}

[System.Serializable]
public class PlayerSaveData
{
    [Header("Game Core Save Data")]
    public string SaveName;
    public string UniqueSaveFolderName;
    public string SaveImageLocation;
    public long CreatedUID;
    public long LastSave;

    [Header("Currencies")]
    public float Gold;
    public float Gem;

    [Header("Levels")]
    public List<LevelSaveData> LevelDatas;

    [Header("Battery")]
    public int BatteryLevel;

    public void ResetSave()
    {
        CreatedUID = 0;
        LastSave = 0;
        SaveName = "";
        SaveImageLocation = "";
        UniqueSaveFolderName = "";
        Gold = 0;
        Gem = 0;
        BatteryLevel = 1;
        LevelDatas = new();
    }
}

[System.Serializable]
public class LevelSaveData
{
    public int LevelID;
    public bool isCompleted; //Kapi acik,
    public bool isChestOpened; //Sandik toplandi eger varsa
    public bool ObscurityPlaced; //Aku objesi bu levele yerlestirildi. // eger hic bir levelde degil ise envanterdedir seklinde denetlenebilir.

    public List<LevelObject> levelObjects;

    public LevelSaveData()
    {

    }

    public LevelSaveData(LevelSaveData _levelSaveData)
    {
        LevelID = _levelSaveData.LevelID;
        isCompleted = _levelSaveData.isCompleted;
        isChestOpened = _levelSaveData.isChestOpened;
        ObscurityPlaced = _levelSaveData.ObscurityPlaced;

        levelObjects = new();
        if(_levelSaveData.levelObjects != null)
            foreach (var item in _levelSaveData.levelObjects)
                levelObjects.Add(new LevelObject(item));
    }
}

[System.Serializable]
public class LevelObject
{
    public int LevelID;
    public int ObjectID;
    public int TypeID; //0 Isik kaynagi /-/ 1 Yonlendirici /-/ 2 Tuzaklar /-/ 3 Hedefler
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale;

    public LevelObject()
    {

    }

    public LevelObject(LevelObject _levelObject)
    {
        LevelID = _levelObject.LevelID;
        TypeID = _levelObject.TypeID;
        Position = _levelObject.Position;
        Rotation = _levelObject.Rotation;
        Scale = _levelObject.Scale;
    }
}
