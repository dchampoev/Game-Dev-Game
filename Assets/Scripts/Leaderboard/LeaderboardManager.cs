using System.Collections.Generic;
using UnityEngine;

public static class LeaderboardManager
{
    private const string Key = "LocalLeaderboard";
    private const int MaxLeaderboardEntries = 10;

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
        list.entries.Add(CreateEntry(characterName, score, survivedTime));
        SortByScoreDescending(list.entries);
        TrimToMaxLimit(list.entries);

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

    private static Entry CreateEntry(string characterName, int score, float survivedTime)
    {
        return new Entry
        {
            characterName = characterName,
            score = score,
            survivedTime = survivedTime
        };
    }

    private static void SortByScoreDescending(List<Entry> entries)
    {
        entries.Sort((a, b) => b.score.CompareTo(a.score));
    }

    private static void TrimToMaxLimit(List<Entry> entries)
    {
        if (entries.Count <= MaxLeaderboardEntries)
            return;

        entries.RemoveRange(MaxLeaderboardEntries, entries.Count - MaxLeaderboardEntries);
    }
}
