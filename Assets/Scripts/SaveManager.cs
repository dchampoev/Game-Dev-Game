using System;
using System.Collections.Generic;
using Terresquall;
using UnityEngine;

public class SaveManager : PersistentObject
{
    const string SaveManagerId = "SaveManager";

    static SaveManager _instance;

    public static SaveManager Current
    {
        get
        {
            if (_instance == null)
                _instance = FindAnyObjectByType<SaveManager>();

            return _instance;
        }
    }

    public static SaveManager Instance
    {
        get
        {
            SaveManager saveManager = Current;
            if (saveManager == null && Application.isPlaying)
                saveManager = new GameObject(nameof(SaveManager)).AddComponent<SaveManager>();

            return saveManager;
        }
    }

    public event Action OnCoinsChanged;
    public event Action OnPowerUpsChanged;

    protected SaveData savedData = new SaveData();
    public SaveData SavedData => savedData;

    void Awake()
    {
        if (_instance)
        {
            Debug.LogWarning("Multiple SaveManagers found. Destroying duplicate.");
            Destroy(this);
            return;
        }

        _instance = this;
        EnsureSaveId();
        Time.timeScale = 1f;
        if (Bench.SlotHasSave(Bench.currentSlot))
            Bench.LoadGame(Bench.currentSlot);
        else
            Bench.SaveGame(true);
    }

    void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    void EnsureSaveId()
    {
        if (string.IsNullOrEmpty(saveID))
            saveID = SaveManagerId;
    }

    public float GetTotalCoins() { return savedData.totalCoins; }

    public void AddCoins(float amount)
    {
        if (Mathf.Approximately(amount, 0f))
            return;

        savedData.totalCoins += amount;
        OnCoinsChanged?.Invoke();
    }

    public bool TrySpendCoins(float amount)
    {
        if (amount <= 0)
            return true;
        if (savedData.totalCoins >= amount)
        {
            savedData.totalCoins -= amount;
            OnCoinsChanged?.Invoke();
            Bench.SaveGame(Bench.currentSlot, true);
            return true;
        }
        return false;
    }

    public int GetLevel(PowerUpData data)
    {
        PowerUp.Data powerUp = Find(data);
        return powerUp != null ? powerUp.level : 0;
    }

    public void SetLevel(PowerUpData data, int level)
    {
        if (data == null)
            return;

        int clampedLevel = Mathf.Clamp(level, 0, data.maxLevel);
        PowerUp.Data existing = Find(data);

        if (clampedLevel > 0)
        {
            if (existing != null)
                existing.level = clampedLevel;
            else
                savedData.powerUps.Add(new PowerUp.Data(data.name, clampedLevel));
        }
        else if (existing != null)
        {
            savedData.powerUps.Remove(existing);
        }
        OnPowerUpsChanged?.Invoke();
    }

    public PowerUp.Data Find(PowerUpData data)
    {
        if (data == null)
            return null;

        foreach (PowerUp.Data powerUp in savedData.powerUps)
        {
            if (powerUp != null && powerUp.name == data.name && powerUp.level > 0)
                return powerUp;
        }

        return null;
    }

    public bool LevelUp(PowerUpData powerUp, int amount = 1)
    {
        if (powerUp == null || amount <= 0)
            return false;

        int currentLevel = GetLevel(powerUp);
        if (currentLevel >= powerUp.maxLevel)
            return false;

        SetLevel(powerUp, currentLevel + amount);
        return true;
    }

    public List<PowerUp.Data> GetAllPowerUps()
    {
        return new List<PowerUp.Data>(savedData.powerUps);
    }

    public int GetTotalPowerUpLevels()
    {
        int totalLevels = 0;
        foreach (PowerUp.Data powerUp in savedData.powerUps)
        {
            if (powerUp != null)
                totalLevels += Mathf.Max(0, powerUp.level);
        }

        return totalLevels;
    }

    public void ClearAllPowerUps()
    {
        savedData.powerUps.Clear();
        OnPowerUpsChanged?.Invoke();
    }

    public LeaderboardManager.EntryList GetLeaderboard()
    {
        EnsureLeaderboard();
        return savedData.leaderboard;
    }

    public void SetLeaderboard(LeaderboardManager.EntryList leaderboard)
    {
        savedData.leaderboard = leaderboard ?? new LeaderboardManager.EntryList();
        EnsureLeaderboard();
    }

    public void SaveToDisk()
    {
        EnsureSaveId();
        Bench.SaveGame(Bench.currentSlot, true);
    }

    void EnsureLeaderboard()
    {
        savedData.leaderboard ??= new LeaderboardManager.EntryList();
        savedData.leaderboard.entries ??= new List<LeaderboardManager.Entry>();
    }

    [Serializable]
    public new class SaveData : PersistentObject.SaveData
    {
        public float totalCoins;
        public List<PowerUp.Data> powerUps = new List<PowerUp.Data>();
        public LeaderboardManager.EntryList leaderboard = new LeaderboardManager.EntryList();
    }

    public override bool CanSave()
    {
        return base.CanSave();
    }

    public override PersistentObject.SaveData Save()
    {
        if (!CanSave())
        {
            Debug.LogWarning("SaveManager cannot save at this time.");
            return null;
        }
        return savedData;
    }

    public override bool Load(PersistentObject.SaveData data)
    {
        if (data == null)
        {
            savedData = new SaveData
            {
                totalCoins = 0,
                powerUps = new List<PowerUp.Data>()
            };
            Debug.LogWarning("No save data found. Initializing new save data.");
            return false;
        }

        savedData = data as SaveData;
        if (savedData == null)
        {
            Debug.LogError("Failed to load save data: data is not of type SaveData.");
            return false;
        }

        savedData.powerUps ??= new List<PowerUp.Data>();
        EnsureLeaderboard();
        OnCoinsChanged?.Invoke();
        OnPowerUpsChanged?.Invoke();

        return true;
    }
}
