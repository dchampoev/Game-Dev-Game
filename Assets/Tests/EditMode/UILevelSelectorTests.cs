using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILevelSelectorTests
{
    EventSystem CreateEventSystem()
    {
        EventSystem eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
        typeof(EventSystem)
            .GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(eventSystem, null);

        EventSystem.current = eventSystem;
        return eventSystem;
    }

    Toggle CreateToggle(string name, Transform parent)
    {
        GameObject toggleObject = new GameObject(name);
        toggleObject.transform.SetParent(parent, false);

        Image image = toggleObject.AddComponent<Image>();
        Toggle toggle = toggleObject.AddComponent<Toggle>();
        toggle.targetGraphic = image;
        return toggle;
    }

    Button CreateButton(string name, string labelText)
    {
        GameObject buttonObject = new GameObject(name);
        Image image = buttonObject.AddComponent<Image>();
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        GameObject labelObject = new GameObject("Text");
        labelObject.transform.SetParent(buttonObject.transform, false);
        labelObject.AddComponent<TextMeshProUGUI>().text = labelText;

        return button;
    }

    UISceneDataDisplay CreateSceneDataDisplay(UILevelSelector selector)
    {
        GameObject displayObject = new GameObject("Scene Data Display");
        UISceneDataDisplay display = displayObject.AddComponent<UISceneDataDisplay>();
        display.levelSelector = selector;

        new GameObject("Names").transform.SetParent(displayObject.transform, false);
        displayObject.transform.GetChild(0).gameObject.AddComponent<TextMeshProUGUI>();

        new GameObject("Values").transform.SetParent(displayObject.transform, false);
        displayObject.transform.GetChild(1).gameObject.AddComponent<TextMeshProUGUI>();

        new GameObject("Extra").transform.SetParent(displayObject.transform, false);
        displayObject.transform.GetChild(2).gameObject.AddComponent<TextMeshProUGUI>();

        return display;
    }

    UILevelSelector CreateSelector(int levelCount = 2)
    {
        GameObject selectorObject = new GameObject("Level Selector");
        UILevelSelector selector = selectorObject.AddComponent<UILevelSelector>();

        GameObject gridObject = new GameObject("Grid");
        gridObject.transform.SetParent(selectorObject.transform, false);
        GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        Toggle template = CreateToggle("Level 1", gridObject.transform);
        selector.toggleTemplate = template;
        selector.selectableToggles.Add(template);

        for (int i = 1; i < levelCount; i++)
        {
            selector.selectableToggles.Add(CreateToggle("Level " + (i + 1), gridObject.transform));
        }

        selector.selectLevelButton = CreateButton("Level Select Button", "Select Level");
        selector.backButton = CreateButton("Back Button", "Back to Character Select");

        for (int i = 0; i < levelCount; i++)
        {
            selector.levels.Add(new UILevelSelector.SceneData
            {
                sceneName = "Level " + (i + 1),
                displayName = "Level " + (i + 1),
                label = "Stage " + (i + 1),
                description = "Description " + (i + 1),
                timeLimit = 60f * (i + 1),
                clockSpeed = 1f + i,
                playerModifier = new CharacterData.Stats
                {
                    moveSpeed = i,
                    greed = i + 1,
                    luck = 0,
                    growth = 0
                },
                enemyModifier = new EnemyStats.Stats
                {
                    maxHealth = i + 2
                },
                extraNotes = "Notes " + (i + 1)
            });
        }

        selector.statsUI = CreateSceneDataDisplay(selector);
        return selector;
    }

    void CallPrivateMethod(object target, string methodName)
    {
        target.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(target, null);
    }

    [SetUp]
    public void SetUp()
    {
        UILevelSelector.selectedLevel = -1;
        UILevelSelector.currentLevel = null;
        UILevelSelector.globalBuff = null;
        UILevelSelector.globalBuffAffectsPlayer = false;
        UILevelSelector.globalBuffAffectsEnemies = false;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        TestScriptableObjectCleanup.DestroyRuntimeObjects<BuffData>();

        UILevelSelector.selectedLevel = -1;
        UILevelSelector.currentLevel = null;
        UILevelSelector.globalBuff = null;
        UILevelSelector.globalBuffAffectsPlayer = false;
        UILevelSelector.globalBuffAffectsEnemies = false;
    }

    [Test]
    public void Start_WhenLevelsExist_ShouldSelectFirstLevelAndFocusToggle()
    {
        CreateEventSystem();
        UILevelSelector selector = CreateSelector();

        CallPrivateMethod(selector, "Start");

        Assert.AreEqual(0, UILevelSelector.selectedLevel);
        Assert.IsTrue(selector.selectableToggles[0].isOn);
        Assert.AreSame(selector.selectableToggles[0].gameObject, EventSystem.current.currentSelectedGameObject);
    }

    [Test]
    public void Start_WhenLevelsExist_ShouldCreateGoldenFocusOutline()
    {
        CreateEventSystem();
        UILevelSelector selector = CreateSelector();

        CallPrivateMethod(selector, "Start");

        Outline outline = selector.selectableToggles[0].targetGraphic.GetComponent<Outline>();
        Assert.NotNull(outline);
        Assert.IsTrue(outline.enabled);
        Assert.AreEqual(selector.keyboardFocusColor, outline.effectColor);
        Assert.AreEqual(selector.keyboardFocusDistance, outline.effectDistance);
    }

    [Test]
    public void Navigation_WhenAtLastLevel_ShouldMoveDownToSelectLevelButton()
    {
        CreateEventSystem();
        UILevelSelector selector = CreateSelector();

        CallPrivateMethod(selector, "Start");

        Navigation navigation = selector.selectableToggles[1].navigation;
        Assert.AreSame(selector.selectLevelButton, navigation.selectOnDown);
    }

    [Test]
    public void Navigation_WhenOnBottomButtons_ShouldMoveBetweenButtonsAndBackToSelectedLevel()
    {
        CreateEventSystem();
        UILevelSelector selector = CreateSelector();

        CallPrivateMethod(selector, "Start");

        Navigation selectNavigation = selector.selectLevelButton.navigation;
        Navigation backNavigation = selector.backButton.navigation;

        Assert.AreSame(selector.backButton, selectNavigation.selectOnLeft);
        Assert.AreSame(selector.selectableToggles[0], selectNavigation.selectOnUp);
        Assert.AreSame(selector.selectLevelButton, backNavigation.selectOnRight);
        Assert.AreSame(selector.selectableToggles[0], backNavigation.selectOnUp);
    }

    [Test]
    public void Select_WhenIndexIsValid_ShouldCreateGlobalBuffFromSelectedLevel()
    {
        UILevelSelector selector = CreateSelector();

        selector.Select(1);

        Assert.AreEqual(1, UILevelSelector.selectedLevel);
        Assert.NotNull(UILevelSelector.globalBuff);
        Assert.AreEqual("Global Level Buff", UILevelSelector.globalBuff.name);
        Assert.AreEqual(1f, UILevelSelector.globalBuff.variations[0].playerModifier.moveSpeed);
        Assert.AreEqual(3f, UILevelSelector.globalBuff.variations[0].enemyModifier.maxHealth);
    }

    [Test]
    public void Select_WhenIndexIsInvalid_ShouldLeaveSelectionUnchanged()
    {
        UILevelSelector selector = CreateSelector();

        selector.Select(99);

        Assert.AreEqual(-1, UILevelSelector.selectedLevel);
        Assert.IsNull(UILevelSelector.globalBuff);
    }

    [Test]
    public void UISceneDataDisplay_UpdateFields_ShouldShowSelectedLevelStats()
    {
        UILevelSelector selector = CreateSelector();
        selector.Select(1);

        TextMeshProUGUI names = selector.statsUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI values = selector.statsUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        selector.statsUI.UpdateFields();

        StringAssert.Contains("Time Limit", names.text);
        StringAssert.Contains("Clock Speed", names.text);
        StringAssert.Contains("Move Speed", names.text);
        StringAssert.Contains("Enemy Health", names.text);
        StringAssert.Contains("2:00", values.text);
        StringAssert.Contains("2x", values.text);
        StringAssert.Contains("+100%", values.text);
        StringAssert.Contains("+300%", values.text);
    }
}
