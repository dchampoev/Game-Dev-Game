using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManagerSplitTests
{
    const string LeaderboardKey = "LocalLeaderboard";

    void CallPrivateMethod(object target, string methodName)
    {
        target.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(target, null);
    }

    void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    TextMeshProUGUI CreateText(string name = "Text")
    {
        return new GameObject(name).AddComponent<TextMeshProUGUI>();
    }

    Button CreateButton(string name, Transform parent = null)
    {
        GameObject buttonObject = new GameObject(name);
        if (parent)
            buttonObject.transform.SetParent(parent, false);

        buttonObject.AddComponent<RectTransform>();
        Image image = buttonObject.AddComponent<Image>();
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    EventSystem CreateEventSystem()
    {
        EventSystem eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
        typeof(EventSystem)
            .GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(eventSystem, null);

        EventSystem.current = eventSystem;
        return eventSystem;
    }

    [SetUp]
    public void SetUp()
    {
        UILevelSelector.currentLevel = null;
        UILevelSelector.selectedLevel = -1;
        PlayerPrefs.DeleteKey(LeaderboardKey);
        PlayerPrefs.Save();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        PlayerPrefs.DeleteKey(LeaderboardKey);
        PlayerPrefs.Save();

        UILevelSelector.currentLevel = null;
        UILevelSelector.selectedLevel = -1;
    }

    [Test]
    public void GameTimer_FormatTime_ShouldUseMinutesAndSeconds()
    {
        Assert.AreEqual("00:00", GameTimer.FormatTime(-5f));
        Assert.AreEqual("00:59", GameTimer.FormatTime(59.9f));
        Assert.AreEqual("01:01", GameTimer.FormatTime(61.2f));
        Assert.AreEqual("10:05", GameTimer.FormatTime(605f));
    }

    [Test]
    public void GameTimer_Tick_ShouldUpdateElapsedTimeAndDisplay()
    {
        GameTimer timer = new GameObject("Timer").AddComponent<GameTimer>();
        TextMeshProUGUI display = CreateText("Stopwatch");

        timer.Initialize(0f, display);
        timer.Tick(62.5f);

        Assert.AreEqual(62.5f, timer.ElapsedTime);
        Assert.AreEqual("01:02", timer.FormattedTime);
        Assert.AreEqual("01:02", display.text);
    }

    [Test]
    public void GameTimer_Tick_WhenClockSpeedIsSet_ShouldScaleElapsedTime()
    {
        GameTimer timer = new GameObject("Timer").AddComponent<GameTimer>();
        TextMeshProUGUI display = CreateText("Stopwatch");

        timer.Initialize(0f, display, 2f);
        timer.Tick(10f);

        Assert.AreEqual(20f, timer.ElapsedTime);
        Assert.AreEqual("00:20", display.text);
    }

    [Test]
    public void GameTimer_WhenTimeLimitReached_ShouldFireEventOnlyOnce()
    {
        GameTimer timer = new GameObject("Timer").AddComponent<GameTimer>();
        int eventCount = 0;
        timer.TimeLimitReached += () => eventCount++;

        timer.Initialize(3f, CreateText("Stopwatch"));
        timer.Tick(1f);
        timer.Tick(2f);
        timer.Tick(10f);

        Assert.AreEqual(1, eventCount);
    }

    [Test]
    public void GameManager_WhenTimeLimitReached_ShouldStopSpawningAndSpawnOneReaper()
    {
        GameManager manager = new GameObject("GameManager").AddComponent<GameManager>();
        manager.reaperPrefab = new GameObject("ReaperPrefab");
        manager.reaperSpawnDistance = 10f;
        SetPrivateField(manager, "players", new PlayerStats[0]);

        GameObject spawnManagerObject = new GameObject("SpawnManager");
        spawnManagerObject.AddComponent<SpawnManager>();

        GameObject eventManagerObject = new GameObject("EventManager");
        eventManagerObject.AddComponent<EventManager>();

        CallPrivateMethod(manager, "EndRunByTimeLimit");
        CallPrivateMethod(manager, "EndRunByTimeLimit");

        int reaperCloneCount = 0;
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj.name == "ReaperPrefab(Clone)")
                reaperCloneCount++;
        }

        Assert.IsFalse(spawnManagerObject.activeSelf);
        Assert.IsFalse(eventManagerObject.activeSelf);
        Assert.AreEqual(1, reaperCloneCount);
    }

    [Test]
    public void SaveRunCoins_WhenCalledMultipleTimes_ShouldAddCoinsToStashOnce()
    {
        GameManager manager = new GameObject("GameManager").AddComponent<GameManager>();

        GameObject playerObject = new GameObject("Player");
        PlayerStats player = playerObject.AddComponent<PlayerStats>();

        GameObject collectorObject = new GameObject("Collector");
        collectorObject.transform.SetParent(playerObject.transform);
        PlayerCollector collector = collectorObject.AddComponent<PlayerCollector>();
        collector.AddCoins(25f);

        SaveManager.LastLoadedGameData.coins = 100f;
        SetPrivateField(manager, "players", new[] { player });

        manager.SaveRunCoins();
        manager.SaveRunCoins();

        Assert.AreEqual(125f, SaveManager.LastLoadedGameData.coins);
        Assert.AreEqual(0f, collector.GetCoins());
    }

    [Test]
    public void SaveRunCoins_WhenPlayersAreNotCached_ShouldFindCollectorsInScene()
    {
        GameManager manager = new GameObject("GameManager").AddComponent<GameManager>();

        GameObject collectorObject = new GameObject("Collector");
        PlayerCollector collector = collectorObject.AddComponent<PlayerCollector>();
        collector.AddCoins(1f);

        SaveManager.LastLoadedGameData.coins = 0f;
        SetPrivateField(manager, "players", new PlayerStats[0]);

        manager.SaveRunCoins();

        Assert.AreEqual(1f, SaveManager.LastLoadedGameData.coins);
        Assert.AreEqual(0f, collector.GetCoins());
    }

    [Test]
    public void ResultsScreen_ShowWithLeaderboardPrompt_ShouldPopulateLevelTimeAndSelectInput()
    {
        CreateEventSystem();

        GameObject screen = new GameObject("Results Screen");
        screen.SetActive(false);

        ResultsScreenUI results = new GameObject("Results UI").AddComponent<ResultsScreenUI>();
        TextMeshProUGUI levelText = CreateText("Level");
        TextMeshProUGUI timeText = CreateText("Time");

        results.Initialize(screen, null, null, levelText, timeText);
        results.ShowWithLeaderboardPrompt(7, 125f, "02:05");

        TMP_InputField input = screen.GetComponentInChildren<TMP_InputField>();

        Assert.IsTrue(screen.activeSelf);
        Assert.AreEqual("7", levelText.text);
        Assert.AreEqual("02:05", timeText.text);
        Assert.NotNull(input);
        Assert.AreSame(input.gameObject, EventSystem.current.currentSelectedGameObject);
    }

    [Test]
    public void ResultsScreen_WhenLeaderboardNameSubmitted_ShouldSaveScoreAndHidePrompt()
    {
        GameObject screen = new GameObject("Results Screen");
        screen.SetActive(false);

        ResultsScreenUI results = new GameObject("Results UI").AddComponent<ResultsScreenUI>();
        results.Initialize(screen, null, null, CreateText("Level"), CreateText("Time"));
        results.ShowWithLeaderboardPrompt(4, 42.8f, "00:42");

        TMP_InputField input = screen.GetComponentInChildren<TMP_InputField>();
        input.onSubmit.Invoke("  Hero  ");

        LeaderboardManager.EntryList entries = LeaderboardManager.Load();

        Assert.AreEqual(1, entries.entries.Count);
        Assert.AreEqual("Hero", entries.entries[0].characterName);
        Assert.AreEqual(442, entries.entries[0].score);
        Assert.AreEqual(42.8f, entries.entries[0].survivedTime);
        Assert.IsFalse(input.transform.parent.gameObject.activeSelf);
    }

    [Test]
    public void TitleScreenUI_WhenInstructionsOpen_ShouldSelectInstructionsButton()
    {
        CreateEventSystem();

        Button firstButton = CreateButton("First Button");
        GameObject instructionsScreen = new GameObject("Instructions Screen");
        Button closeButton = CreateButton("Close Instructions", instructionsScreen.transform);
        instructionsScreen.SetActive(false);

        TitleScreenUI titleScreen = new GameObject("Title Screen UI").AddComponent<TitleScreenUI>();
        titleScreen.firstButton = firstButton;

        CallPrivateMethod(titleScreen, "Start");
        Assert.AreSame(firstButton.gameObject, EventSystem.current.currentSelectedGameObject);

        instructionsScreen.SetActive(true);
        CallPrivateMethod(titleScreen, "Update");

        Assert.AreSame(closeButton.gameObject, EventSystem.current.currentSelectedGameObject);
    }

    [Test]
    public void TitleScreenUI_WhenInstructionsClose_ShouldRestoreFirstButtonSelection()
    {
        CreateEventSystem();

        Button firstButton = CreateButton("First Button");
        GameObject instructionsScreen = new GameObject("Instructions Screen");
        CreateButton("Close Instructions", instructionsScreen.transform);
        instructionsScreen.SetActive(false);

        TitleScreenUI titleScreen = new GameObject("Title Screen UI").AddComponent<TitleScreenUI>();
        titleScreen.firstButton = firstButton;

        CallPrivateMethod(titleScreen, "Start");
        instructionsScreen.SetActive(true);
        CallPrivateMethod(titleScreen, "Update");

        instructionsScreen.SetActive(false);
        CallPrivateMethod(titleScreen, "Update");

        Assert.AreSame(firstButton.gameObject, EventSystem.current.currentSelectedGameObject);
    }
}
