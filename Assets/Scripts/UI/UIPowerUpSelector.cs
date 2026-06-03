using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using Terresquall;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExcludeFromCodeCoverage]
[RequireComponent(typeof(ToggleGroup))]
public class UIPowerUpSelector : MonoBehaviour
{
    public PowerUpData defaultPrefab;
    public static PowerUpData selected;

    [Header("Template")]
    public Toggle toggleTemplate;
    [Tooltip("Name of tick container")]
    public string tickContainerName = "Tick Boxes";
    [Tooltip("Name of the patter for tick images")]
    public string tickImageName = "tick";

    Dictionary<PowerUpData, List<Image>> cachedTicks = new Dictionary<PowerUpData, List<Image>>();
    Dictionary<PowerUpData, Toggle> cachedToggleButtons = new Dictionary<PowerUpData, Toggle>();

    private int purchasedLevel = 0;
    private int previewLevel = 1;

    [Header("Description Box")]
    public TextMeshProUGUI powerUpName;
    public TextMeshProUGUI powerUpDescription;
    public Image selectedPowerUpIcon;
    public TextMeshProUGUI powerUpPrice;
    public Button buyButton;
    public Button backButton;
    public bool selectFirstToggleOnStart = true;
    public Color keyboardFocusColor = new Color(1f, 0.86f, 0.25f, 1f);
    public Vector2 keyboardFocusDistance = new Vector2(5f, -5f);
    public List<Toggle> selectableToggles = new List<Toggle>();

    readonly Dictionary<Toggle, Outline> focusOutlines = new Dictionary<Toggle, Outline>();
    Outline buyButtonFocusOutline;
    Outline backButtonFocusOutline;

    void Start()
    {
        if (!buyButton)
            buyButton = FindButton("Buy");
        if (!backButton)
            backButton = FindButton("Back");

        RebuildSelectableTogglesIfNeeded();
        SetupToggleNavigation();
        UpdateButtonNavigation();
        SetupFocusOutlines();

        SaveManager saveManager = SaveManager.Instance;
        if (saveManager != null)
        {
            saveManager.OnPowerUpsChanged += SyncTicksForAll;
            saveManager.OnPowerUpsChanged += RefreshUIAfterLoad;
        }

        SyncTicksForAll();

        if (defaultPrefab)
            Select(defaultPrefab);

        if (selectFirstToggleOnStart)
            SelectToggle(GetDefaultToggle());

        RefreshFocusHighlight();
    }

    void Update()
    {
        UpdateButtonNavigation();
        RefreshFocusHighlight();
    }

    void OnDestroy()
    {
        SaveManager saveManager = SaveManager.Current;
        if (saveManager != null)
        {
            saveManager.OnPowerUpsChanged -= SyncTicksForAll;
            saveManager.OnPowerUpsChanged -= RefreshUIAfterLoad;
        }
    }

    void RefreshUIAfterLoad()
    {
        if (selected != null)
        {
            purchasedLevel = SaveManager.Instance.GetLevel(selected);
            previewLevel = Mathf.Clamp(purchasedLevel + 1, 1, selected.maxLevel);
            RefreshUI();
        }
    }

    void RebuildSelectableTogglesIfNeeded()
    {
        selectableToggles.RemoveAll(toggle => !toggle);
        if (selectableToggles.Count > 0)
            return;

        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out Toggle toggle))
                selectableToggles.Add(toggle);
        }
    }

    Toggle FindToggle(PowerUpData powerUp)
    {
        if (powerUp == null)
            return null;
        if (cachedToggleButtons.TryGetValue(powerUp, out Toggle cachedToggle))
        {
            return cachedToggle;
        }
        foreach (Transform child in transform)
        {
            if (child.name == powerUp.name)
            {
                if (child.TryGetComponent<Toggle>(out Toggle toggle))
                {
                    cachedToggleButtons[powerUp] = toggle;
                    return toggle;
                }

                return null;
            }
        }
        Debug.LogWarning($"Could not find toggle for power-up {powerUp.name}.");
        return null;
    }

    List<Image> FindTicks(PowerUpData powerUp)
    {
        if (powerUp == null)
            return new List<Image>();

        if (cachedTicks.TryGetValue(powerUp, out List<Image> imageTicks))
            return imageTicks;

        List<Image> ticks = new List<Image>();

        Toggle toggleButton = FindToggle(powerUp);
        if (toggleButton == null)
            return ticks;

        Transform content = toggleButton.transform.Find("Content");

        if (content == null)
        {
            Debug.LogWarning($"Could not find content for power-up {powerUp.name} toggle.");
            return ticks;
        }

        Transform tickBoxesContainer = content.Find(tickContainerName);
        if (tickBoxesContainer == null)
        {
            Debug.LogWarning($"Could not find tick container for power-up {powerUp.name} toggle.");
            return ticks;
        }

        foreach (Transform box in tickBoxesContainer)
        {
            foreach (Transform child in box)
            {
                if (child.name.Contains(tickImageName, StringComparison.OrdinalIgnoreCase))
                {
                    Image image = child.GetComponent<Image>();
                    if (image != null)
                    {
                        ticks.Add(image);
                    }
                }
            }
        }

        cachedTicks[powerUp] = ticks;
        return ticks;
    }

    public void SyncTicksForAll()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("SaveManager instance not found. Cannot sync power-up ticks.");
            return;
        }
#if UNITY_EDITOR
        PowerUpData[] allPowerUps = GetAllPowerUps();
        foreach (PowerUpData powerUp in allPowerUps)
        {
            SyncTicks(powerUp);
        }
#else
        if (selected != null)
        {
            SyncTicks(selected);
        }
#endif
    }

    void SyncTicks(PowerUpData powerUp)
    {
        if (powerUp == null)
            return;

        int currentLevel = SaveManager.Instance.GetLevel(powerUp);
        List<Image> ticks = FindTicks(powerUp);

        for (int i = 0; i < ticks.Count; i++)
        {
            if (ticks[i] != null)
            {
                ticks[i].gameObject.SetActive(i < currentLevel);
            }
        }
    }

    public void Select(PowerUpData powerUp)
    {
        if (!powerUp)
            return;

        selected = powerUp;

        purchasedLevel = SaveManager.Instance.GetLevel(powerUp);
        previewLevel = Mathf.Clamp(purchasedLevel + 1, 1, selected.maxLevel);

        RefreshUI();
    }

    void RefreshUI()
    {
        if (!selected)
            return;

        Item.LevelData levelData;

        if (purchasedLevel >= selected.maxLevel)
        {
            levelData = selected.GetLevelData(selected.maxLevel);
            powerUpPrice.text = "MAX";

            if (buyButton != null)
                buyButton.gameObject.SetActive(false);
        }
        else
        {
            levelData = selected.GetLevelData(previewLevel);

            int totalBoughtLevels = SaveManager.Instance.GetTotalPowerUpLevels();
            float cost = selected.GetCost(previewLevel, totalBoughtLevels);
            powerUpPrice.text = cost.ToString("0");

            if (buyButton != null)
                buyButton.gameObject.SetActive(true);
        }

        powerUpName.text = selected.name;
        powerUpDescription.text = levelData.description;
        selectedPowerUpIcon.sprite = selected.icon;

        UpdateLevelTicks();
        UpdateButtonNavigation();
    }

    void UpdateLevelTicks()
    {
        if (!selected)
            return;

        List<Image> ticks = FindTicks(selected);

        for (int i = 0; i < ticks.Count; i++)
        {
            if (ticks[i] != null)
            {
                ticks[i].gameObject.SetActive(i < purchasedLevel);
            }
        }
    }

    public void Buy()
    {
        if (!selected)
            return;

        if (purchasedLevel >= selected.maxLevel)
        {
            powerUpPrice.text = "MAX";
            if (buyButton != null)
                buyButton.gameObject.SetActive(false);
            return;
        }

        SaveManager saveManager = SaveManager.Instance;
        int totalBoughtLevels = saveManager.GetTotalPowerUpLevels();
        float cost = selected.GetCost(previewLevel, totalBoughtLevels);
        if (!saveManager.TrySpendCoins(cost))
        {
            Debug.Log("Not enough coins to purchase power-up " + selected.name);
            return;
        }

        saveManager.LevelUp(selected);

        purchasedLevel = saveManager.GetLevel(selected);
        previewLevel = Mathf.Clamp(purchasedLevel + 1, 1, selected.maxLevel);

        RefreshUI();

        Bench.SaveGame(Bench.currentSlot, true);
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
            navigation.selectOnDown = GetToggleAt(i + columns) ? GetToggleAt(i + columns) : GetPrimaryBottomButton();
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
        Toggle selectedToggle = GetSelectedPowerUpToggle();
        Toggle fallbackToggle = GetLastSelectableToggle();
        Selectable upperTarget = selectedToggle ? selectedToggle : fallbackToggle;

        if (buyButton)
        {
            Navigation navigation = buyButton.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnLeft = null;
            navigation.selectOnRight = backButton;
            navigation.selectOnUp = upperTarget;
            buyButton.navigation = navigation;
        }

        if (backButton)
        {
            Navigation navigation = backButton.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnLeft = buyButton && buyButton.gameObject.activeInHierarchy ? buyButton : null;
            navigation.selectOnRight = null;
            navigation.selectOnUp = upperTarget;
            backButton.navigation = navigation;
        }
    }

    Button GetPrimaryBottomButton()
    {
        if (buyButton && buyButton.gameObject.activeInHierarchy && buyButton.interactable)
            return buyButton;

        return backButton;
    }

    Toggle GetDefaultToggle()
    {
        Toggle selectedToggle = GetSelectedPowerUpToggle();
        if (selectedToggle)
            return selectedToggle;

        foreach (Toggle toggle in selectableToggles)
        {
            if (toggle && toggle.gameObject.activeInHierarchy && toggle.interactable)
                return toggle;
        }

        return null;
    }

    Toggle GetSelectedPowerUpToggle()
    {
        if (selected)
        {
            Toggle selectedToggle = FindToggle(selected);
            if (selectedToggle)
                return selectedToggle;
        }

        foreach (Toggle toggle in selectableToggles)
        {
            if (toggle && toggle.isOn)
                return toggle;
        }

        return null;
    }

    Toggle GetLastSelectableToggle()
    {
        for (int i = selectableToggles.Count - 1; i >= 0; i--)
        {
            Toggle toggle = selectableToggles[i];
            if (toggle && toggle.gameObject.activeInHierarchy && toggle.interactable)
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

    void SetupFocusOutlines()
    {
        focusOutlines.Clear();

        foreach (Toggle toggle in selectableToggles)
        {
            if (!toggle || !toggle.targetGraphic)
                continue;

            focusOutlines[toggle] = GetOrCreateFocusOutline(toggle.targetGraphic.gameObject);
        }

        buyButtonFocusOutline = buyButton && buyButton.targetGraphic
            ? GetOrCreateFocusOutline(buyButton.targetGraphic.gameObject)
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

        if (buyButtonFocusOutline && buyButton)
        {
            buyButtonFocusOutline.enabled = buyButton.gameObject.activeInHierarchy && buyButton.gameObject == focusedObject;
        }

        if (backButtonFocusOutline && backButton)
        {
            backButtonFocusOutline.enabled = backButton.gameObject.activeInHierarchy && backButton.gameObject == focusedObject;
        }
    }

    Button FindButton(string buttonName)
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            if (!button || !button.gameObject.scene.IsValid())
                continue;
            if (button.name.Contains(buttonName, StringComparison.OrdinalIgnoreCase))
                return button;

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label && label.text.Contains(buttonName, StringComparison.OrdinalIgnoreCase))
                return button;
        }

        return null;
    }

    public static PowerUpData GetSelected()
    {
        return selected;
    }

#if UNITY_EDITOR

    public static PowerUpData[] GetAllPowerUps()
    {
        List<PowerUpData> powerUps = new List<PowerUpData>();
        string[] guids = AssetDatabase.FindAssets("t:PowerUpData");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PowerUpData powerUp = AssetDatabase.LoadAssetAtPath<PowerUpData>(path);
            if (powerUp != null)
            {
                powerUps.Add(powerUp);
            }
        }

        return powerUps.ToArray();
    }
#endif
}
