using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SaveManagerTests
{
    SaveManager CreateSaveManager(SaveManager.SaveData data = null)
    {
        SaveManager saveManager = new GameObject("SaveManager").AddComponent<SaveManager>();
        saveManager.saveID = "TestSaveManager";
        saveManager.Load(data ?? new SaveManager.SaveData());
        return saveManager;
    }

    PowerUpData CreatePowerUpData(string powerUpName = "Might", int maxLevel = 3)
    {
        PowerUpData data = ScriptableObject.CreateInstance<PowerUpData>();
        data.name = powerUpName;
        data.maxLevel = maxLevel;
        return data;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        TestScriptableObjectCleanup.DestroyRuntimeObjects<PowerUpData>();
    }

    [Test]
    public void Current_WhenSaveManagerExists_ShouldReturnSceneInstance()
    {
        SaveManager saveManager = CreateSaveManager();

        Assert.AreSame(saveManager, SaveManager.Current);
    }

    [Test]
    public void GetTotalCoins_ShouldReturnSavedTotalCoins()
    {
        SaveManager saveManager = CreateSaveManager(new SaveManager.SaveData { totalCoins = 25f });

        Assert.AreEqual(25f, saveManager.GetTotalCoins());
    }

    [Test]
    public void AddCoins_WhenAmountIsPositive_ShouldIncreaseCoinsAndRaiseEvent()
    {
        SaveManager saveManager = CreateSaveManager();
        int eventCount = 0;
        saveManager.OnCoinsChanged += () => eventCount++;

        saveManager.AddCoins(15f);

        Assert.AreEqual(15f, saveManager.GetTotalCoins());
        Assert.AreEqual(1, eventCount);
    }

    [Test]
    public void AddCoins_WhenAmountIsZero_ShouldNotRaiseEvent()
    {
        SaveManager saveManager = CreateSaveManager();
        int eventCount = 0;
        saveManager.OnCoinsChanged += () => eventCount++;

        saveManager.AddCoins(0f);

        Assert.AreEqual(0f, saveManager.GetTotalCoins());
        Assert.AreEqual(0, eventCount);
    }

    [Test]
    public void TrySpendCoins_WhenAmountIsZeroOrNegative_ShouldReturnTrueWithoutChangingCoins()
    {
        SaveManager saveManager = CreateSaveManager(new SaveManager.SaveData { totalCoins = 10f });
        int eventCount = 0;
        saveManager.OnCoinsChanged += () => eventCount++;

        Assert.IsTrue(saveManager.TrySpendCoins(0f));
        Assert.IsTrue(saveManager.TrySpendCoins(-5f));
        Assert.AreEqual(10f, saveManager.GetTotalCoins());
        Assert.AreEqual(0, eventCount);
    }

    [Test]
    public void TrySpendCoins_WhenCoinsAreInsufficient_ShouldReturnFalseWithoutChangingCoins()
    {
        SaveManager saveManager = CreateSaveManager(new SaveManager.SaveData { totalCoins = 10f });
        int eventCount = 0;
        saveManager.OnCoinsChanged += () => eventCount++;

        bool result = saveManager.TrySpendCoins(15f);

        Assert.IsFalse(result);
        Assert.AreEqual(10f, saveManager.GetTotalCoins());
        Assert.AreEqual(0, eventCount);
    }

    [Test]
    public void TrySpendCoins_WhenCoinsAreEnough_ShouldSpendCoinsAndRaiseEvent()
    {
        SaveManager saveManager = CreateSaveManager(new SaveManager.SaveData { totalCoins = 10f });
        int eventCount = 0;
        saveManager.OnCoinsChanged += () => eventCount++;

        bool result = saveManager.TrySpendCoins(4f);

        Assert.IsTrue(result);
        Assert.AreEqual(6f, saveManager.GetTotalCoins());
        Assert.AreEqual(1, eventCount);
    }

    [Test]
    public void SetLevel_WhenPowerUpIsNull_ShouldDoNothing()
    {
        SaveManager saveManager = CreateSaveManager();
        int eventCount = 0;
        saveManager.OnPowerUpsChanged += () => eventCount++;

        saveManager.SetLevel(null, 1);

        Assert.IsEmpty(saveManager.GetAllPowerUps());
        Assert.AreEqual(0, eventCount);
    }

    [Test]
    public void SetLevel_WhenNewLevelIsPositive_ShouldAddPowerUpAndRaiseEvent()
    {
        SaveManager saveManager = CreateSaveManager();
        PowerUpData powerUp = CreatePowerUpData();
        int eventCount = 0;
        saveManager.OnPowerUpsChanged += () => eventCount++;

        saveManager.SetLevel(powerUp, 2);

        Assert.AreEqual(2, saveManager.GetLevel(powerUp));
        Assert.AreEqual(1, saveManager.GetAllPowerUps().Count);
        Assert.AreEqual(1, eventCount);
    }

    [Test]
    public void SetLevel_WhenPowerUpAlreadyExists_ShouldUpdateExistingLevel()
    {
        PowerUpData powerUp = CreatePowerUpData();
        SaveManager saveManager = CreateSaveManager(new SaveManager.SaveData
        {
            powerUps = new List<PowerUp.Data>
            {
                new PowerUp.Data(powerUp.name, 1)
            }
        });

        saveManager.SetLevel(powerUp, 2);

        List<PowerUp.Data> powerUps = saveManager.GetAllPowerUps();
        Assert.AreEqual(1, powerUps.Count);
        Assert.AreEqual(2, powerUps[0].level);
    }

    [Test]
    public void SetLevel_WhenLevelIsAboveMax_ShouldClampToMaxLevel()
    {
        SaveManager saveManager = CreateSaveManager();
        PowerUpData powerUp = CreatePowerUpData(maxLevel: 3);

        saveManager.SetLevel(powerUp, 99);

        Assert.AreEqual(3, saveManager.GetLevel(powerUp));
    }

    [Test]
    public void SetLevel_WhenLevelIsZero_ShouldRemoveExistingPowerUp()
    {
        PowerUpData powerUp = CreatePowerUpData();
        SaveManager saveManager = CreateSaveManager(new SaveManager.SaveData
        {
            powerUps = new List<PowerUp.Data>
            {
                new PowerUp.Data(powerUp.name, 2)
            }
        });

        saveManager.SetLevel(powerUp, 0);

        Assert.AreEqual(0, saveManager.GetLevel(powerUp));
        Assert.IsEmpty(saveManager.GetAllPowerUps());
    }

    [Test]
    public void Find_WhenDataIsNullOrEntryIsInvalid_ShouldReturnNull()
    {
        PowerUpData powerUp = CreatePowerUpData();
        SaveManager saveManager = CreateSaveManager(new SaveManager.SaveData
        {
            powerUps = new List<PowerUp.Data>
            {
                null,
                new PowerUp.Data(powerUp.name, 0)
            }
        });

        Assert.IsNull(saveManager.Find(null));
        Assert.IsNull(saveManager.Find(powerUp));
    }

    [Test]
    public void LevelUp_WhenPowerUpIsInvalid_ShouldReturnFalse()
    {
        SaveManager saveManager = CreateSaveManager();
        PowerUpData powerUp = CreatePowerUpData();

        Assert.IsFalse(saveManager.LevelUp(null));
        Assert.IsFalse(saveManager.LevelUp(powerUp, 0));
        Assert.AreEqual(0, saveManager.GetLevel(powerUp));
    }

    [Test]
    public void LevelUp_WhenBelowMaxLevel_ShouldIncreaseLevel()
    {
        SaveManager saveManager = CreateSaveManager();
        PowerUpData powerUp = CreatePowerUpData(maxLevel: 3);

        bool result = saveManager.LevelUp(powerUp, 2);

        Assert.IsTrue(result);
        Assert.AreEqual(2, saveManager.GetLevel(powerUp));
    }

    [Test]
    public void LevelUp_WhenAlreadyAtMaxLevel_ShouldReturnFalse()
    {
        PowerUpData powerUp = CreatePowerUpData(maxLevel: 2);
        SaveManager saveManager = CreateSaveManager(new SaveManager.SaveData
        {
            powerUps = new List<PowerUp.Data>
            {
                new PowerUp.Data(powerUp.name, 2)
            }
        });

        bool result = saveManager.LevelUp(powerUp);

        Assert.IsFalse(result);
        Assert.AreEqual(2, saveManager.GetLevel(powerUp));
    }

    [Test]
    public void GetAllPowerUps_ShouldReturnCopyOfSavedPowerUps()
    {
        SaveManager saveManager = CreateSaveManager(new SaveManager.SaveData
        {
            powerUps = new List<PowerUp.Data>
            {
                new PowerUp.Data("Might", 1)
            }
        });

        List<PowerUp.Data> copy = saveManager.GetAllPowerUps();
        copy.Clear();

        Assert.AreEqual(1, saveManager.GetAllPowerUps().Count);
    }

    [Test]
    public void GetTotalPowerUpLevels_ShouldIgnoreNullEntriesAndClampNegativeLevels()
    {
        SaveManager saveManager = CreateSaveManager(new SaveManager.SaveData
        {
            powerUps = new List<PowerUp.Data>
            {
                null,
                new PowerUp.Data("Might", 2),
                new PowerUp.Data("Armor", -5)
            }
        });

        Assert.AreEqual(2, saveManager.GetTotalPowerUpLevels());
    }

    [Test]
    public void ClearAllPowerUps_ShouldRemoveAllPowerUpsAndRaiseEvent()
    {
        SaveManager saveManager = CreateSaveManager(new SaveManager.SaveData
        {
            powerUps = new List<PowerUp.Data>
            {
                new PowerUp.Data("Might", 1)
            }
        });
        int eventCount = 0;
        saveManager.OnPowerUpsChanged += () => eventCount++;

        saveManager.ClearAllPowerUps();

        Assert.IsEmpty(saveManager.GetAllPowerUps());
        Assert.AreEqual(1, eventCount);
    }

    [Test]
    public void SetLeaderboard_WhenValueIsNull_ShouldCreateEmptyEntryList()
    {
        SaveManager saveManager = CreateSaveManager();

        saveManager.SetLeaderboard(null);

        Assert.NotNull(saveManager.GetLeaderboard());
        Assert.NotNull(saveManager.GetLeaderboard().entries);
        Assert.IsEmpty(saveManager.GetLeaderboard().entries);
    }

    [Test]
    public void GetLeaderboard_WhenEntriesAreNull_ShouldRepairEntryList()
    {
        SaveManager saveManager = CreateSaveManager(new SaveManager.SaveData
        {
            leaderboard = new LeaderboardManager.EntryList
            {
                entries = null
            }
        });

        LeaderboardManager.EntryList leaderboard = saveManager.GetLeaderboard();

        Assert.NotNull(leaderboard.entries);
    }

    [Test]
    public void Save_WhenSaveIdExists_ShouldReturnSavedData()
    {
        SaveManager saveManager = CreateSaveManager(new SaveManager.SaveData { totalCoins = 7f });

        SaveManager.SaveData savedData = (SaveManager.SaveData)saveManager.Save();

        Assert.NotNull(savedData);
        Assert.AreEqual(7f, savedData.totalCoins);
    }

    [Test]
    public void Save_WhenSaveIdIsMissing_ShouldReturnNull()
    {
        SaveManager saveManager = CreateSaveManager();
        saveManager.saveID = string.Empty;

        LogAssert.Expect(LogType.Warning, "SaveManager cannot save at this time.");
        Assert.IsNull(saveManager.Save());
    }

    [Test]
    public void Load_WhenDataIsNull_ShouldInitializeEmptyDataAndReturnFalse()
    {
        SaveManager saveManager = CreateSaveManager();

        LogAssert.Expect(LogType.Warning, "No save data found. Initializing new save data.");
        bool result = saveManager.Load(null);

        Assert.IsFalse(result);
        Assert.AreEqual(0f, saveManager.GetTotalCoins());
        Assert.IsEmpty(saveManager.GetAllPowerUps());
    }

    [Test]
    public void Load_WhenDataHasWrongType_ShouldReturnFalse()
    {
        SaveManager saveManager = CreateSaveManager();

        LogAssert.Expect(LogType.Error, "Failed to load save data: data is not of type SaveData.");
        bool result = saveManager.Load(new Terresquall.PersistentObject.SaveData());

        Assert.IsFalse(result);
    }

    [Test]
    public void Load_WhenPowerUpsAndLeaderboardAreNull_ShouldRepairCollectionsAndRaiseEvents()
    {
        SaveManager saveManager = CreateSaveManager();
        int coinEventCount = 0;
        int powerUpEventCount = 0;
        saveManager.OnCoinsChanged += () => coinEventCount++;
        saveManager.OnPowerUpsChanged += () => powerUpEventCount++;

        bool result = saveManager.Load(new SaveManager.SaveData
        {
            powerUps = null,
            leaderboard = null
        });

        Assert.IsTrue(result);
        Assert.NotNull(saveManager.GetAllPowerUps());
        Assert.NotNull(saveManager.GetLeaderboard());
        Assert.NotNull(saveManager.GetLeaderboard().entries);
        Assert.AreEqual(1, coinEventCount);
        Assert.AreEqual(1, powerUpEventCount);
    }
}
