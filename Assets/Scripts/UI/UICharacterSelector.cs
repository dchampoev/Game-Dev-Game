using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExcludeFromCodeCoverage]
public class UICharacterSelector : MonoBehaviour
{
    public CharacterData defaultCharacter;
    public static CharacterData selected;
    public UIStatsDisplay statsUI;
    public Button startButton;
    public bool selectFirstToggleOnStart = true;
    public Color keyboardFocusColor = new Color(1f, 0.86f, 0.25f, 1f);
    public Vector2 keyboardFocusDistance = new Vector2(5f, -5f);

    [Header("Template")]
    public Toggle toggleTemplate;
    public string characterNamePath = "Character Name";
    public string weaponIconPath = "Weapon Icon";
    public string characterIconPath = "Character Icon";
    public List<Toggle> selectableToggles = new List<Toggle>();

    [Header("DescriptionBox")]
    public TextMeshProUGUI characterFullName;
    public TextMeshProUGUI characterDescription;
    public Image selectedCharacterIcon;
    public Image selectedCharacterWeapon;

    readonly Dictionary<Toggle, Outline> focusOutlines = new Dictionary<Toggle, Outline>();
    Outline startButtonFocusOutline;

    void Start()
    {
        if (!startButton)
            startButton = FindStartButton();

        SetupToggleNavigation();
        SetupFocusOutlines();

        if (selectFirstToggleOnStart)
        {
            Toggle initialToggle = selectableToggles.Count > 0 ? selectableToggles[0] : GetSelectedCharacterToggle();
            SelectToggle(initialToggle);
        }
        else if (defaultCharacter)
        {
            Select(defaultCharacter);
        }

        RefreshFocusHighlight();
    }

    void Update()
    {
        UpdateStartButtonNavigation();
        RefreshFocusHighlight();
    }

    public static CharacterData[] GetAllCharacterDataAssets()
    {
        List<CharacterData> characters = new List<CharacterData>();

#if UNITY_EDITOR
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string assetPath in allAssetPaths)
        {
            if (assetPath.EndsWith(".asset"))
            {
                CharacterData character = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
                if (character)
                    characters.Add(character);
            }
        }
#else
        Debug.LogWarning("This function cannot be called on builds.");
#endif
        return characters.ToArray();
    }

    public static CharacterData GetData()
    {
        if (selected)
            return selected;
        else
        {
            CharacterData[] characters = GetAllCharacterDataAssets();
            if (characters.Length > 0)
                return characters[Random.Range(0, characters.Length)];
        }
        return null;
    }

    public void Select(CharacterData character)
    {
        selected = statsUI.character = character;
        statsUI.UpdateFields();

        characterFullName.text = character.FullName;
        characterDescription.text = character.CharacterDescription;
        selectedCharacterIcon.sprite = character.Icon;
        selectedCharacterWeapon.sprite = character.StartingWeapon.icon;
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
            navigation.selectOnDown = GetToggleAt(i + columns) ? GetToggleAt(i + columns) : startButton;
            toggle.navigation = navigation;
        }

        UpdateStartButtonNavigation();
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

    Button FindStartButton()
    {
        Button fallback = null;
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button button in buttons)
        {
            if (!button || !button.gameObject.scene.IsValid())
                continue;
            if (!fallback)
                fallback = button;
            if (button.name.Contains("Character Select"))
                return button;
        }

        return fallback;
    }

    void UpdateStartButtonNavigation()
    {
        if (!startButton)
            return;

        Toggle selectedToggle = GetSelectedCharacterToggle();
        Toggle fallbackToggle = null;

        for (int i = selectableToggles.Count - 1; i >= 0; i--)
        {
            if (selectableToggles[i])
            {
                fallbackToggle = selectableToggles[i];
                break;
            }
        }

        Navigation navigation = startButton.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        navigation.selectOnUp = selectedToggle ? selectedToggle : fallbackToggle;
        startButton.navigation = navigation;
    }

    void SetupFocusOutlines()
    {
        focusOutlines.Clear();

        foreach (Toggle toggle in selectableToggles)
        {
            if (!toggle || !toggle.targetGraphic)
                continue;

            GameObject graphicObject = toggle.targetGraphic.gameObject;
            Outline outline = graphicObject.GetComponent<Outline>();
            if (!outline)
                outline = graphicObject.AddComponent<Outline>();

            outline.effectColor = keyboardFocusColor;
            outline.effectDistance = keyboardFocusDistance;
            outline.useGraphicAlpha = false;
            outline.enabled = false;

            focusOutlines[toggle] = outline;
        }

        startButtonFocusOutline = null;
        if (startButton && startButton.targetGraphic)
        {
            GameObject graphicObject = startButton.targetGraphic.gameObject;
            startButtonFocusOutline = graphicObject.GetComponent<Outline>();
            if (!startButtonFocusOutline)
                startButtonFocusOutline = graphicObject.AddComponent<Outline>();

            startButtonFocusOutline.effectColor = keyboardFocusColor;
            startButtonFocusOutline.effectDistance = keyboardFocusDistance;
            startButtonFocusOutline.useGraphicAlpha = false;
            startButtonFocusOutline.enabled = false;
        }
    }

    Toggle GetSelectedCharacterToggle()
    {
        foreach (Toggle toggle in selectableToggles)
        {
            if (!toggle)
                continue;
            if (toggle.isOn)
                return toggle;
        }

        if (!selected)
            return null;

        foreach (Toggle toggle in selectableToggles)
        {
            if (!toggle)
                continue;
            if (toggle.onValueChanged.GetPersistentEventCount() == 0)
                continue;
            if (toggle.gameObject.name == selected.Name)
                return toggle;
        }

        return null;
    }

    void SelectToggle(Toggle toggle)
    {
        if (!toggle)
            return;

        toggle.isOn = true;
        if (EventSystem.current)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(toggle.gameObject);
        }
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

        if (startButtonFocusOutline && startButton)
        {
            startButtonFocusOutline.enabled = startButton.gameObject == focusedObject;
        }
    }
}
