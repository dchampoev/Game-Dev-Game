using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Reflection;
using UnityEngine.EventSystems;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExcludeFromCodeCoverage]
public class UILevelSelector : MonoBehaviour
{
    public UISceneDataDisplay statsUI;

    public static int selectedLevel = -1;
    public static SceneData currentLevel;
    public List<SceneData> levels = new List<SceneData>();
    public Button selectLevelButton;
    public Button backButton;
    public bool selectFirstToggleOnStart = true;
    public Color keyboardFocusColor = new Color(1f, 0.86f, 0.25f, 1f);
    public Vector2 keyboardFocusDistance = new Vector2(5f, -5f);

    [Header("Template")]
    public Toggle toggleTemplate;
    public string levelNamePath = "Level Name";
    public string levelNumberPath = "Level Number";
    public string levelDiscriptionPath = "Level Description";
    public string levelImagePath = "Level Image";
    public List<Toggle> selectableToggles = new List<Toggle>();

    public static BuffData globalBuff;

    public static bool globalBuffAffectsPlayer = false, globalBuffAffectsEnemies = false;

    public const string MAP_NAME_FORMAT = "^(Level .*?) ?- ?(.*)$";
    readonly Dictionary<Toggle, Outline> focusOutlines = new Dictionary<Toggle, Outline>();
    Outline selectLevelButtonFocusOutline;
    Outline backButtonFocusOutline;

    void Start()
    {
        if (!selectLevelButton)
            selectLevelButton = FindButton("Select Level");
        if (!backButton)
            backButton = FindButton("Back to Character Select");

        ConfigureToggleCallbacks();
        SetupToggleNavigation();
        UpdateButtonNavigation();
        SetupFocusOutlines();

        if (selectFirstToggleOnStart && selectableToggles.Count > 0)
        {
            SelectToggle(selectableToggles[0]);
            UpdateButtonNavigation();
        }

        RefreshFocusHighlight();
    }

    void Update()
    {
        UpdateButtonNavigation();
        RefreshFocusHighlight();
    }

    [System.Serializable]
    public class SceneData
    {
#if UNITY_EDITOR
        public SceneAsset scene;
#endif
        public string sceneName;

        [Header("UI Display")]
        public string displayName;
        public string label;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Modifiers")]
        public CharacterData.Stats playerModifier;
        public EnemyStats.Stats enemyModifier;
        [Min(-1)] public float timeLimit = 0f, clockSpeed = 1f;
        [TextArea] public string extraNotes = "--";

        public string SceneName
        {
            get
            {
#if UNITY_EDITOR
                if (scene)
                    return scene.name;
#endif
                return sceneName;
            }
        }
    }

#if UNITY_EDITOR
    public static SceneAsset[] GetAllMaps()
    {
        List<SceneAsset> maps = new List<SceneAsset>();

        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string assetPath in allAssetPaths)
        {
            if (assetPath.EndsWith(".unity"))
            {
                SceneAsset map = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
                if (map != null && Regex.IsMatch(map.name, MAP_NAME_FORMAT))
                {
                    maps.Add(map);
                }
            }
        }
        maps.Reverse();
        return maps.ToArray();
    }
#endif

    public void SceneChange(string name)
    {
        SceneManager.LoadScene(name);
        Time.timeScale = 1f;
    }

    public void LoadSelectedLevel()
    {
        if (selectedLevel >= 0 && selectedLevel < levels.Count && !string.IsNullOrWhiteSpace(levels[selectedLevel].SceneName))
        {
            SceneManager.LoadScene(levels[selectedLevel].SceneName);
            currentLevel = levels[selectedLevel];
            selectedLevel = -1;
            Time.timeScale = 1f;
        }
        else
        {
            Debug.LogWarning("No level selected!");
        }
    }

    public void Select(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= levels.Count)
            return;

        selectedLevel = sceneIndex;
        if (statsUI)
            statsUI.UpdateFields();
        globalBuff = GenerateGlobalBuffData();
        globalBuffAffectsPlayer = globalBuff && isModifierEmpty(globalBuff.variations[0].playerModifier);
        globalBuffAffectsEnemies = globalBuff && isModifierEmpty(globalBuff.variations[0].enemyModifier);
        UpdateButtonNavigation();
    }

    public BuffData GenerateGlobalBuffData()
    {
        BuffData buffData = ScriptableObject.CreateInstance<BuffData>();
        buffData.name = "Global Level Buff";
        buffData.variations[0].damagePerSecond = 0;
        buffData.variations[0].duration = 0;
        buffData.variations[0].playerModifier = levels[selectedLevel].playerModifier;
        buffData.variations[0].enemyModifier = levels[selectedLevel].enemyModifier;
        return buffData;
    }

    private static bool isModifierEmpty(object obj)
    {
        Type type = obj.GetType();
        FieldInfo[] fields = type.GetFields();
        float sum = 0;
        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(obj);
            if (value is int)
                sum += (int)value;
            else if (value is float)
                sum += (float)value;
        }

        return Mathf.Approximately(sum, 0);
    }

    void ConfigureToggleCallbacks()
    {
        for (int i = 0; i < selectableToggles.Count; i++)
        {
            Toggle toggle = selectableToggles[i];
            if (!toggle)
                continue;

            int levelIndex = i;
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn)
                    Select(levelIndex);
            });
        }
    }

    void SetupToggleNavigation()
    {
        ToggleGroup group = toggleTemplate ? toggleTemplate.GetComponentInParent<ToggleGroup>() : null;
        GridLayoutGroup grid = toggleTemplate ? toggleTemplate.GetComponentInParent<GridLayoutGroup>() : null;
        int columns = grid && grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount
            ? Mathf.Max(1, grid.constraintCount)
            : Mathf.Max(1, selectableToggles.Count);

        for (int i = 0; i < selectableToggles.Count; i++)
        {
            Toggle toggle = selectableToggles[i];
            if (!toggle)
                continue;
            if (group && !toggle.group)
                toggle.group = group;

            Navigation navigation = toggle.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnLeft = GetToggleAt(i - 1, i / columns);
            navigation.selectOnRight = GetToggleAt(i + 1, i / columns);
            navigation.selectOnUp = GetToggleAt(i - columns);
            navigation.selectOnDown = GetToggleAt(i + columns) ? GetToggleAt(i + columns) : selectLevelButton;
            toggle.navigation = navigation;
        }
    }

    Toggle GetToggleAt(int index, int requiredRow = -1)
    {
        if (index < 0 || index >= selectableToggles.Count)
            return null;

        Toggle toggle = selectableToggles[index];
        if (!toggle || !toggle.gameObject.activeInHierarchy || !toggle.interactable)
            return null;

        if (requiredRow >= 0)
        {
            GridLayoutGroup grid = toggleTemplate ? toggleTemplate.GetComponentInParent<GridLayoutGroup>() : null;
            int columns = grid && grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount
                ? Mathf.Max(1, grid.constraintCount)
                : Mathf.Max(1, selectableToggles.Count);

            if (index / columns != requiredRow)
                return null;
        }

        return toggle;
    }

    void UpdateButtonNavigation()
    {
        Toggle selectedToggle = GetSelectedLevelToggle();
        Toggle fallbackToggle = GetLastSelectableToggle();

        if (selectLevelButton)
        {
            Navigation navigation = selectLevelButton.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnLeft = backButton;
            navigation.selectOnRight = null;
            navigation.selectOnUp = selectedToggle ? selectedToggle : fallbackToggle;
            selectLevelButton.navigation = navigation;
        }

        if (backButton)
        {
            Navigation navigation = backButton.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnLeft = null;
            navigation.selectOnRight = selectLevelButton;
            navigation.selectOnUp = selectedToggle ? selectedToggle : fallbackToggle;
            backButton.navigation = navigation;
        }
    }

    Toggle GetSelectedLevelToggle()
    {
        if (selectedLevel >= 0 && selectedLevel < selectableToggles.Count)
        {
            Toggle toggle = selectableToggles[selectedLevel];
            if (toggle)
                return toggle;
        }

        foreach (Toggle toggle in selectableToggles)
        {
            if (!toggle)
                continue;
            if (toggle.isOn)
                return toggle;
        }

        return null;
    }

    Toggle GetLastSelectableToggle()
    {
        for (int i = selectableToggles.Count - 1; i >= 0; i--)
        {
            if (selectableToggles[i])
                return selectableToggles[i];
        }

        return null;
    }

    void SelectToggle(Toggle toggle)
    {
        if (!toggle)
            return;

        int index = selectableToggles.IndexOf(toggle);
        if (index >= 0)
            Select(index);

        toggle.isOn = true;
        if (EventSystem.current)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(toggle.gameObject);
        }
    }

    void SetupFocusOutlines()
    {
        focusOutlines.Clear();

        foreach (Toggle toggle in selectableToggles)
        {
            if (!toggle || !toggle.targetGraphic)
                continue;

            Outline outline = GetOrCreateFocusOutline(toggle.targetGraphic.gameObject);
            focusOutlines[toggle] = outline;
        }

        selectLevelButtonFocusOutline = selectLevelButton && selectLevelButton.targetGraphic
            ? GetOrCreateFocusOutline(selectLevelButton.targetGraphic.gameObject)
            : null;

        backButtonFocusOutline = backButton && backButton.targetGraphic
            ? GetOrCreateFocusOutline(backButton.targetGraphic.gameObject)
            : null;
    }

    Outline GetOrCreateFocusOutline(GameObject graphicObject)
    {
        Outline outline = graphicObject.GetComponent<Outline>();
        if (!outline)
            outline = graphicObject.AddComponent<Outline>();

        outline.effectColor = keyboardFocusColor;
        outline.effectDistance = keyboardFocusDistance;
        outline.useGraphicAlpha = false;
        outline.enabled = false;
        return outline;
    }

    void RefreshFocusHighlight()
    {
        GameObject focusedObject = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;

        foreach (KeyValuePair<Toggle, Outline> pair in focusOutlines)
        {
            Toggle toggle = pair.Key;
            Outline outline = pair.Value;

            if (!toggle || !outline)
                continue;
            outline.enabled = toggle.gameObject == focusedObject;
        }

        if (selectLevelButtonFocusOutline && selectLevelButton)
        {
            selectLevelButtonFocusOutline.enabled = selectLevelButton.gameObject == focusedObject;
        }

        if (backButtonFocusOutline && backButton)
        {
            backButtonFocusOutline.enabled = backButton.gameObject == focusedObject;
        }
    }

    Button FindButton(string buttonName)
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            if (!button || !button.gameObject.scene.IsValid())
                continue;
            if (button.name.Contains(buttonName))
                return button;

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label && label.text.Contains(buttonName))
                return button;
        }

        return null;
    }
}
