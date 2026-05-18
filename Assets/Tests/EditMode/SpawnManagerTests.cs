using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SpawnManagerTests
{
    private WaveData CreateWaveData(
        uint totalSpawns = 5,
        float duration = 10f,
        WaveData.ExitCondition exitConditions = WaveData.ExitCondition.waveDuration,
        bool mustKillAllEnemies = false)
    {
        WaveData wave = ScriptableObject.CreateInstance<WaveData>();
        wave.totalSpawns = totalSpawns;
        wave.duration = duration;
        wave.exitConditions = exitConditions;
        wave.mustKillAllEnemies = mustKillAllEnemies;
        wave.spawnCount = new Vector2Int(1, 1);
        wave.spawnInterval = new Vector2(1f, 1f);
        wave.possibleSpawnablePrefabs = new GameObject[]
        {
            new GameObject("EnemyPrefab")
        };

        return wave;
    }

    private SpawnManager CreateManager(WaveData wave)
    {
        GameObject managerObject = new GameObject("SpawnManager");
        SpawnManager manager = managerObject.AddComponent<SpawnManager>();
        manager.enabled = false;
        manager.data = new WaveData[] { wave };
        manager.maxEnemiesAllowed = 300;

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        cameraObject.tag = "MainCamera";

        manager.referenceCamera = camera;
        SpawnManager.instance = manager;

        return manager;
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    private T GetPrivateField<T>(object target, string fieldName)
    {
        return (T)target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(target);
    }

    private PlayerStats CreatePlayer(float curse)
    {
        GameObject playerObject = new GameObject("Player");
        PlayerStats player = playerObject.AddComponent<PlayerStats>();
        player.enabled = false;

        CharacterData.Stats stats = new CharacterData.Stats
        {
            maxHealth = 20f,
            recovery = 0f,
            armor = 0f,
            moveSpeed = 1f,
            might = 1f,
            area = 1f,
            speed = 1f,
            duration = 1f,
            amount = 0,
            cooldown = 1f,
            luck = 1f,
            growth = 1f,
            greed = 1f,
            curse = curse,
            magnet = 1f,
            revival = 0
        };

        player.Stats = stats;
        player.level = 1;
        return player;
    }

    private void CreateGameManagerWithPlayers(params PlayerStats[] players)
    {
        GameObject gameManagerObject = new GameObject("GameManager");
        gameManagerObject.SetActive(false);
        GameManager manager = gameManagerObject.AddComponent<GameManager>();
        GameManager.instance = manager;
        SetPrivateField(manager, "players", players);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (var wave in Resources.FindObjectsOfTypeAll<WaveData>())
        {
            Object.DestroyImmediate(wave, true);
        }

        SpawnManager.instance = null;
        GameManager.instance = null;
        EnemyStats.count = 0;
    }

    [Test]
    public void CanSpawn_WhenBelowTotalSpawnsAndEnemyLimit_ShouldReturnTrue()
    {
        WaveData wave = CreateWaveData(totalSpawns: 5);
        SpawnManager manager = CreateManager(wave);

        SetPrivateField(manager, "currentWaveSpawnCount", 0);
        EnemyStats.count = 0;

        bool result = manager.CanSpawn();

        Assert.IsTrue(result);
    }

    [Test]
    public void CanSpawn_WhenCurrentWaveReachedTotalSpawns_ShouldReturnFalse()
    {
        WaveData wave = CreateWaveData(totalSpawns: 2);
        SpawnManager manager = CreateManager(wave);

        SetPrivateField(manager, "currentWaveSpawnCount", 2);
        EnemyStats.count = 0;

        bool result = manager.CanSpawn();

        Assert.IsFalse(result);
    }

    [Test]
    public void HasExceededTotalSpawns_WhenEnemyCountIsAboveLimit_ShouldReturnTrue()
    {
        WaveData wave = CreateWaveData();
        SpawnManager manager = CreateManager(wave);
        manager.maxEnemiesAllowed = 3;

        EnemyStats.count = 4;

        bool result = SpawnManager.HasExceededTotalSpawns();

        Assert.IsTrue(result);
    }

    [Test]
    public void HasExceededTotalSpawns_WhenNoInstance_ShouldReturnFalse()
    {
        SpawnManager.instance = null;
        EnemyStats.count = 999;

        bool result = SpawnManager.HasExceededTotalSpawns();

        Assert.IsFalse(result);
    }

    [Test]
    public void ActiveCooldown_WhenBoostedByCurse_ShouldDivideSpawnIntervalByCurse()
    {
        WaveData wave = CreateWaveData();
        wave.spawnInterval = new Vector2(6f, 6f);
        SpawnManager manager = CreateManager(wave);
        manager.boostedByCurse = true;
        CreateGameManagerWithPlayers(CreatePlayer(curse: 2f));

        manager.ActiveCooldown();

        Assert.AreEqual(2f, GetPrivateField<float>(manager, "spawnTimer"), 0.001f);
    }

    [Test]
    public void ActiveCooldown_WhenNotBoostedByCurse_ShouldUseFullSpawnInterval()
    {
        WaveData wave = CreateWaveData();
        wave.spawnInterval = new Vector2(6f, 6f);
        SpawnManager manager = CreateManager(wave);
        manager.boostedByCurse = false;
        CreateGameManagerWithPlayers(CreatePlayer(curse: 2f));

        manager.ActiveCooldown();

        Assert.AreEqual(6f, GetPrivateField<float>(manager, "spawnTimer"), 0.001f);
    }

    [Test]
    public void HasWaveEnded_WhenDurationConditionNotReached_ShouldReturnFalse()
    {
        WaveData wave = CreateWaveData(
            duration: 10f,
            exitConditions: WaveData.ExitCondition.waveDuration
        );

        SpawnManager manager = CreateManager(wave);
        SetPrivateField(manager, "currentWaveDuration", 5f);

        bool result = manager.HasWaveEnded();

        Assert.IsFalse(result);
    }

    [Test]
    public void HasWaveEnded_WhenDurationConditionReached_ShouldReturnTrue()
    {
        WaveData wave = CreateWaveData(
            duration: 10f,
            exitConditions: WaveData.ExitCondition.waveDuration
        );

        SpawnManager manager = CreateManager(wave);
        SetPrivateField(manager, "currentWaveDuration", 10f);

        bool result = manager.HasWaveEnded();

        Assert.IsTrue(result);
    }

    [Test]
    public void HasWaveEnded_WhenReachedTotalSpawnsConditionNotReached_ShouldReturnFalse()
    {
        WaveData wave = CreateWaveData(
            totalSpawns: 3,
            exitConditions: WaveData.ExitCondition.reachedTotalSpawns
        );

        SpawnManager manager = CreateManager(wave);
        SetPrivateField(manager, "currentWaveSpawnCount", 2);

        bool result = manager.HasWaveEnded();

        Assert.IsFalse(result);
    }

    [Test]
    public void HasWaveEnded_WhenReachedTotalSpawnsConditionReached_ShouldReturnTrue()
    {
        WaveData wave = CreateWaveData(
            totalSpawns: 3,
            exitConditions: WaveData.ExitCondition.reachedTotalSpawns
        );

        SpawnManager manager = CreateManager(wave);
        SetPrivateField(manager, "currentWaveSpawnCount", 3);

        bool result = manager.HasWaveEnded();

        Assert.IsTrue(result);
    }

    [Test]
    public void HasWaveEnded_WhenMustKillAllEnemiesAndEnemiesRemain_ShouldReturnFalse()
    {
        WaveData wave = CreateWaveData(
            duration: 1f,
            exitConditions: WaveData.ExitCondition.waveDuration,
            mustKillAllEnemies: true
        );

        SpawnManager manager = CreateManager(wave);
        SetPrivateField(manager, "currentWaveDuration", 1f);
        EnemyStats.count = 1;

        bool result = manager.HasWaveEnded();

        Assert.IsFalse(result);
    }

    [Test]
    public void GeneratePosition_WhenCameraIsOrthographic_ShouldReturnPositionOnViewportEdge()
    {
        WaveData wave = CreateWaveData();
        SpawnManager manager = CreateManager(wave);

        Vector3 result = SpawnManager.GeneratePosition();

        Vector3 viewport = manager.referenceCamera.WorldToViewportPoint(result);

        bool onHorizontalEdge = Mathf.Approximately(viewport.x, 0f) || Mathf.Approximately(viewport.x, 1f);
        bool onVerticalEdge = Mathf.Approximately(viewport.y, 0f) || Mathf.Approximately(viewport.y, 1f);

        Assert.IsTrue(onHorizontalEdge || onVerticalEdge);
    }

    [Test]
    public void GeneratePosition_WhenReferenceCameraIsNull_ShouldUseMainCamera()
    {
        WaveData wave = CreateWaveData();
        SpawnManager manager = CreateManager(wave);
        manager.referenceCamera = null;

        Vector3 result = SpawnManager.GeneratePosition();

        Assert.AreEqual(0f, result.z);
        Assert.IsNotNull(manager.referenceCamera);
    }

    [Test]
    public void IsWithinBoundaries_WhenObjectInsideCameraView_ShouldReturnTrue()
    {
        WaveData wave = CreateWaveData();
        CreateManager(wave);

        GameObject obj = new GameObject("CheckedObject");
        obj.transform.position = Vector3.zero;

        bool result = SpawnManager.IsWithinBoundaries(obj.transform);

        Assert.IsTrue(result);
    }

    [Test]
    public void IsWithinBoundaries_WhenObjectOutsideCameraView_ShouldReturnFalse()
    {
        WaveData wave = CreateWaveData();
        SpawnManager manager = CreateManager(wave);
        manager.referenceCamera.orthographicSize = 5f;

        GameObject obj = new GameObject("CheckedObject");
        obj.transform.position = new Vector3(100f, 100f, 0f);

        bool result = SpawnManager.IsWithinBoundaries(obj.transform);

        Assert.IsFalse(result);
    }
}
