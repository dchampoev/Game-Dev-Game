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
    public void GeneratePosition_WhenCameraIsNotOrthographic_ShouldLogWarning()
    {
        WaveData wave = CreateWaveData();
        SpawnManager manager = CreateManager(wave);
        manager.referenceCamera.orthographic = false;

        LogAssert.Expect(
            LogType.Warning,
            "Spawn Manager's reference camera is not orthographic! Defaulting to (0, 0, 0) for spawn position."
        );

        SpawnManager.GeneratePosition();
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