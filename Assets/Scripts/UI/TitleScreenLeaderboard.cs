using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExcludeFromCodeCoverage]
public class TitleScreenLeaderboard : MonoBehaviour
{
    const string TitleSceneName = "Title Screen";
    const string RootName = "Leaderboard Panel";
    const int MaxVisibleEntries = 10;
    const float EntryRowHeight = 28f;
    const float EntryRowSpacing = 2f;
    const float EntriesViewportHeight = 340f;

    static readonly Color PanelColor = new Color(0.03f, 0.04f, 0.08f, 0.82f);
    static readonly Color HeaderColor = new Color(1f, 0.87f, 0.42f, 1f);
    static readonly Color TextColor = new Color(0.94f, 0.94f, 0.94f, 1f);
    static readonly Color MutedTextColor = new Color(0.74f, 0.76f, 0.84f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryCreateForActiveScene();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == TitleSceneName)
        {
            TryCreateForActiveScene();
        }
    }

    static void TryCreateForActiveScene()
    {
        if (SceneManager.GetActiveScene().name != TitleSceneName || GameObject.Find(RootName))
        {
            return;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (!canvas)
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        GameObject panelObject = CreateUIObject(RootName, canvas.transform);
        panelObject.AddComponent<TitleScreenLeaderboardVisibility>();
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(1f, 0.5f);
        panel.anchorMax = new Vector2(1f, 0.5f);
        panel.pivot = new Vector2(1f, 0.5f);
        panel.anchoredPosition = new Vector2(-44f, 5f);
        panel.sizeDelta = new Vector2(430f, 520f);

        panelObject.AddComponent<CanvasGroup>();

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = PanelColor;

        VerticalLayoutGroup layout = panelObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 10f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateText("Leaderboard Title", panel, "LEADERBOARD", 34f, HeaderColor, TextAlignmentOptions.Center, 42f);
        CreateHeaderRow(panel);
        PopulateEntries(CreateEntriesScrollView(panel));
    }

    static void CreateHeaderRow(RectTransform parent)
    {
        RectTransform row = CreateRow("Leaderboard Header", parent, 34f);
        CreateText("Rank Header", row, "#", 21f, MutedTextColor, TextAlignmentOptions.Left, 34f, 42f, 0f);
        CreateText("Character Header", row, "Name", 21f, MutedTextColor, TextAlignmentOptions.Left, 34f, -1f, 2f);
        CreateText("Score Header", row, "Score", 21f, MutedTextColor, TextAlignmentOptions.Right, 34f, -1f, 1f);
        CreateText("Time Header", row, "Time", 21f, MutedTextColor, TextAlignmentOptions.Right, 34f, -1f, 1f);
    }

    static void PopulateEntries(RectTransform parent)
    {
        List<LeaderboardManager.Entry> entries = LeaderboardManager.Load().entries;
        if (entries.Count == 0)
        {
            CreateText("Empty Leaderboard", parent, "No runs yet", 26f, TextColor, TextAlignmentOptions.Center, 68f);
            return;
        }

        int count = Mathf.Min(entries.Count, MaxVisibleEntries);
        for (int i = 0; i < count; i++)
        {
            LeaderboardManager.Entry entry = entries[i];
            RectTransform row = CreateRow($"Leaderboard Entry {i + 1}", parent, EntryRowHeight);
            CreateText("Rank", row, (i + 1).ToString(), 20f, TextColor, TextAlignmentOptions.Left, EntryRowHeight, 42f, 0f);
            CreateText("Character", row, CleanCharacterName(entry.characterName), 20f, TextColor, TextAlignmentOptions.Left, EntryRowHeight, -1f, 2f);
            CreateText("Score", row, entry.score.ToString(), 20f, TextColor, TextAlignmentOptions.Right, EntryRowHeight, -1f, 1f);
            CreateText("Time", row, FormatTime(entry.survivedTime), 20f, TextColor, TextAlignmentOptions.Right, EntryRowHeight, -1f, 1f);
        }
    }

    static RectTransform CreateEntriesScrollView(RectTransform parent)
    {
        GameObject viewportObject = CreateUIObject("Leaderboard Entries Viewport", parent);
        RectTransform viewport = viewportObject.GetComponent<RectTransform>();

        Image viewportImage = viewportObject.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0f);
        viewportObject.AddComponent<RectMask2D>();

        LayoutElement viewportLayout = viewportObject.AddComponent<LayoutElement>();
        viewportLayout.flexibleHeight = 1f;
        viewportLayout.minHeight = EntriesViewportHeight;
        viewportLayout.preferredHeight = EntriesViewportHeight;

        GameObject contentObject = CreateUIObject("Leaderboard Entries Content", viewport);
        RectTransform content = contentObject.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        VerticalLayoutGroup contentLayout = contentObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.spacing = EntryRowSpacing;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        ContentSizeFitter contentFitter = contentObject.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = viewportObject.AddComponent<ScrollRect>();
        scrollRect.content = content;
        scrollRect.viewport = viewport;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 18f;

        return content;
    }

    static RectTransform CreateRow(string name, RectTransform parent, float height)
    {
        GameObject rowObject = CreateUIObject(name, parent);
        RectTransform row = rowObject.GetComponent<RectTransform>();

        HorizontalLayoutGroup layout = rowObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = height;
        layoutElement.preferredHeight = height;

        return row;
    }

    static LayoutElement CreateText(string name, RectTransform parent, string value, float fontSize, Color color, TextAlignmentOptions alignment, float height)
    {
        return CreateText(name, parent, value, fontSize, color, alignment, height, -1f, 1f);
    }

    static LayoutElement CreateText(
        string name,
        RectTransform parent,
        string value,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment,
        float height,
        float preferredWidth,
        float flexibleWidth)
    {
        GameObject textObject = CreateUIObject(name, parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.enableAutoSizing = true;
        text.fontSizeMin = 14f;
        text.fontSizeMax = fontSize;
        text.overflowMode = TextOverflowModes.Ellipsis;

        LayoutElement layoutElement = textObject.AddComponent<LayoutElement>();
        layoutElement.minHeight = height;
        layoutElement.preferredHeight = height;
        layoutElement.preferredWidth = preferredWidth;
        layoutElement.flexibleWidth = flexibleWidth;

        return layoutElement;
    }

    static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name, typeof(RectTransform));
        uiObject.layer = LayerMask.NameToLayer("UI");
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    static string CleanCharacterName(string characterName)
    {
        if (string.IsNullOrWhiteSpace(characterName))
        {
            return "Unknown";
        }

        return characterName.Replace("(Clone)", "").Trim();
    }

    static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }
}

[ExcludeFromCodeCoverage]
public class TitleScreenLeaderboardVisibility : MonoBehaviour
{
    const string InstructionsScreenName = "Instructions Screen";

    CanvasGroup canvasGroup;
    GameObject instructionsScreen;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (!canvasGroup)
            canvasGroup = GetComponent<CanvasGroup>();

        if (!canvasGroup)
            return;

        if (!instructionsScreen)
            instructionsScreen = FindSceneObject(InstructionsScreenName);

        bool instructionsOpen = instructionsScreen && instructionsScreen.activeInHierarchy;
        canvasGroup.alpha = instructionsOpen ? 0f : 1f;
        canvasGroup.interactable = !instructionsOpen;
        canvasGroup.blocksRaycasts = !instructionsOpen;
    }

    static GameObject FindSceneObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject sceneObject in objects)
        {
            if (sceneObject.name == objectName && sceneObject.scene.IsValid())
                return sceneObject;
        }

        return null;
    }
}
