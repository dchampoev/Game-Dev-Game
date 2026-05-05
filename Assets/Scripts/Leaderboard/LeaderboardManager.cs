using System.Collections.Generic;
using UnityEngine;

public static class LeaderboardManager
{
    const string Key = "LocalLeaderboard";

    [System.Serializable]
    public class Entry
    {
        public string characterName;
        public int score;
        public float survivedTime;
    }

    [System.Serializable]
    public class EntryList
    {
        public List<Entry> entries = new List<Entry>();
    }

    public static void SaveScore(string characterName, int score, float survivedTime)
    {
        EntryList list = Load();

        list.entries.Add(new Entry
        {
            characterName = characterName,
            score = score,
            survivedTime = survivedTime
        });

        list.entries.Sort((a, b) => b.score.CompareTo(a.score));

        if (list.entries.Count > 10)
            list.entries.RemoveRange(10, list.entries.Count - 10);

        PlayerPrefs.SetString(Key, JsonUtility.ToJson(list));
        PlayerPrefs.Save();
    }

    public static EntryList Load()
    {
        string json = PlayerPrefs.GetString(Key, "");

        if (string.IsNullOrEmpty(json))
            return new EntryList();

        return JsonUtility.FromJson<EntryList>(json);
    }
}