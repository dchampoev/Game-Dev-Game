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

        SaveManager saveManager = SaveManager.Instance;
        if (saveManager)
        {
            saveManager.SetLeaderboard(list);
            saveManager.SaveToDisk();
            return;
        }

        PlayerPrefs.SetString(Key, JsonUtility.ToJson(list));
        PlayerPrefs.Save();
    }

    public static EntryList Load()
    {
        SaveManager saveManager = SaveManager.Instance;
        if (saveManager)
        {
            EntryList savedLeaderboard = saveManager.GetLeaderboard();
            if (savedLeaderboard.entries.Count > 0)
                return Copy(savedLeaderboard);
        }

        string json = PlayerPrefs.GetString(Key, "");

        if (string.IsNullOrEmpty(json))
            return new EntryList();

        EntryList playerPrefsLeaderboard = JsonUtility.FromJson<EntryList>(json);
        return playerPrefsLeaderboard ?? new EntryList();
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

    private static EntryList Copy(EntryList source)
    {
        EntryList copy = new EntryList();
        if (source?.entries == null)
            return copy;

        foreach (Entry entry in source.entries)
        {
            if (entry == null)
                continue;

            copy.entries.Add(CreateEntry(entry.characterName, entry.score, entry.survivedTime));
        }

        return copy;
    }
}
