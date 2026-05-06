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
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
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
}