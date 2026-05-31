using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEngine;

/// <summary>
/// A simple SaveManager designed to save the total number of coins the player has as well as store all the player's save data.
/// </summary>

public class SaveManager
{
    public class GameData
    {
        public float coins;
    }

    const string SAVE_FILE_NAME = "SaveData.json";

    static GameData lastLoadedGameData;
    public static GameData LastLoadedGameData
    {
        get
        {
            if (lastLoadedGameData == null)
                Load();
            return lastLoadedGameData;
        }
    }

    public static string GetSavePath()
    {
        return string.Format("{0}/{1}", Application.persistentDataPath, SAVE_FILE_NAME);
    }

    public static void Save(GameData data = null)
    {
        if (data == null)
        {
            if (lastLoadedGameData == null)
                Load();
            data = lastLoadedGameData;
        }
        File.WriteAllText(GetSavePath(), JsonUtility.ToJson(data));
    }

    public static GameData Load(bool usePreviousLoadIfAvailable = false)
    {
        if (usePreviousLoadIfAvailable && lastLoadedGameData != null)
        {
            return lastLoadedGameData;
        }

        string savePath = GetSavePath();
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            lastLoadedGameData = JsonUtility.FromJson<GameData>(json);
            if (lastLoadedGameData == null)
                lastLoadedGameData = new GameData();
        }
        else
        {
            lastLoadedGameData = new GameData();
        }
        return lastLoadedGameData;
    }
}
