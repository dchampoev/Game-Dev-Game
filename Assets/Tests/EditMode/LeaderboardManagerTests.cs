using NUnit.Framework;
using UnityEngine;

public class LeaderboardManagerTests
{
    private const string Key = "LocalLeaderboard";

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }

    SaveManager CreateSaveManager()
    {
        SaveManager saveManager = new GameObject("SaveManager").AddComponent<SaveManager>();
        saveManager.saveID = "TestSaveManager";
        saveManager.Load(new SaveManager.SaveData());
        return saveManager;
    }

    [Test]
    public void Load_WhenNoSavedScores_ShouldReturnEmptyList()
    {
        LeaderboardManager.EntryList result = LeaderboardManager.Load();

        Assert.NotNull(result);
        Assert.NotNull(result.entries);
        Assert.AreEqual(0, result.entries.Count);
    }

    [Test]
    public void SaveScore_ShouldSaveEntry()
    {
        LeaderboardManager.SaveScore("Hero", 100, 45f);

        LeaderboardManager.EntryList result = LeaderboardManager.Load();

        Assert.AreEqual(1, result.entries.Count);
        Assert.AreEqual("Hero", result.entries[0].characterName);
        Assert.AreEqual(100, result.entries[0].score);
        Assert.AreEqual(45f, result.entries[0].survivedTime);
    }

    [Test]
    public void SaveScore_ShouldSortEntriesByScoreDescending()
    {
        LeaderboardManager.SaveScore("Low", 100, 10f);
        LeaderboardManager.SaveScore("High", 300, 30f);
        LeaderboardManager.SaveScore("Mid", 200, 20f);

        LeaderboardManager.EntryList result = LeaderboardManager.Load();

        Assert.AreEqual("High", result.entries[0].characterName);
        Assert.AreEqual("Mid", result.entries[1].characterName);
        Assert.AreEqual("Low", result.entries[2].characterName);
    }

    [Test]
    public void SaveScore_WhenMoreThanTenEntries_ShouldKeepOnlyTopTen()
    {
        for (int i = 0; i < 12; i++)
        {
            LeaderboardManager.SaveScore("Hero" + i, i * 10, i);
        }

        LeaderboardManager.EntryList result = LeaderboardManager.Load();

        Assert.AreEqual(10, result.entries.Count);
        Assert.AreEqual(110, result.entries[0].score);
        Assert.AreEqual(20, result.entries[9].score);
    }

    [Test]
    public void SaveScore_WhenNewEntryBeatsTenthPlace_ShouldInsertAndDropOldTenth()
    {
        for (int i = 0; i < 10; i++)
        {
            LeaderboardManager.SaveScore("Hero" + i, 100 - i, i);
        }

        LeaderboardManager.SaveScore("Better", 101, 99f);

        LeaderboardManager.EntryList result = LeaderboardManager.Load();

        Assert.AreEqual(10, result.entries.Count);
        Assert.AreEqual("Better", result.entries[0].characterName);
        Assert.False(result.entries.Exists(entry => entry.characterName == "Hero9"));
    }

    [Test]
    public void SaveScore_WhenNewEntryDoesNotReachTopTen_ShouldDiscardEntry()
    {
        for (int i = 0; i < 10; i++)
        {
            LeaderboardManager.SaveScore("Hero" + i, 100 - i, i);
        }

        LeaderboardManager.SaveScore("TooLow", 1, 99f);

        LeaderboardManager.EntryList result = LeaderboardManager.Load();

        Assert.AreEqual(10, result.entries.Count);
        Assert.False(result.entries.Exists(entry => entry.characterName == "TooLow"));
    }

    [Test]
    public void SaveScore_WhenSaveManagerExists_ShouldStoreEntryInSaveData()
    {
        SaveManager saveManager = CreateSaveManager();

        LeaderboardManager.SaveScore("Hero", 100, 45f);

        LeaderboardManager.EntryList savedLeaderboard = saveManager.GetLeaderboard();
        Assert.AreEqual(1, savedLeaderboard.entries.Count);
        Assert.AreEqual("Hero", savedLeaderboard.entries[0].characterName);
        Assert.AreEqual(100, savedLeaderboard.entries[0].score);
        Assert.AreEqual(45f, savedLeaderboard.entries[0].survivedTime);
        Assert.IsFalse(PlayerPrefs.HasKey(Key));
    }

    [Test]
    public void Load_WhenSaveManagerHasScores_ShouldPreferSaveDataOverPlayerPrefs()
    {
        SaveManager saveManager = CreateSaveManager();
        saveManager.SetLeaderboard(new LeaderboardManager.EntryList
        {
            entries = new System.Collections.Generic.List<LeaderboardManager.Entry>
            {
                new LeaderboardManager.Entry { characterName = "Saved", score = 200, survivedTime = 20f }
            }
        });

        PlayerPrefs.SetString(Key, JsonUtility.ToJson(new LeaderboardManager.EntryList
        {
            entries = new System.Collections.Generic.List<LeaderboardManager.Entry>
            {
                new LeaderboardManager.Entry { characterName = "Prefs", score = 100, survivedTime = 10f }
            }
        }));

        LeaderboardManager.EntryList result = LeaderboardManager.Load();

        Assert.AreEqual(1, result.entries.Count);
        Assert.AreEqual("Saved", result.entries[0].characterName);
    }
}
