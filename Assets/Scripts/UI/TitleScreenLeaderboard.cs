using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreenLeaderboard : MonoBehaviour
{
    const string TitleSceneName = "Title Screen";
    const string RootName = "Leaderboard Panel";

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
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(1f, 0.5f);
        panel.anchorMax = new Vector2(1f, 0.5f);
        panel.pivot = new Vector2(1f, 0.5f);
        panel.anchoredPosition = new Vector2(-44f, 5f);
        panel.sizeDelta = new Vector2(430f, 520f);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = PanelColor;

        VerticalLayoutGroup layout = panelObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 22, 22);
        layout.spacing = 10f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = panelObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        CreateText("Leaderboard Title", panel, "LEADERBOARD", 34f, HeaderColor, TextAlignmentOptions.Center, 42f);
        CreateHeaderRow(panel);
        PopulateEntries(panel);
    }

    static void CreateHeaderRow(RectTransform parent)
    {
        RectTransform row = CreateRow("Leaderboard Header", parent, 34f);
        CreateText("Rank Header", row, "#", 21f, MutedTextColor, TextAlignmentOptions.Left, 34f, 42f, 0f);
        CreateText("Character Header", row, "Character", 21f, MutedTextColor, TextAlignmentOptions.Left, 34f, -1f, 2f);
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

        int count = Mathf.Min(entries.Count, 10);
        for (int i = 0; i < count; i++)
        {
            LeaderboardManager.Entry entry = entries[i];
            RectTransform row = CreateRow($"Leaderboard Entry {i + 1}", parent, 36f);
            CreateText("Rank", row, (i + 1).ToString(), 22f, TextColor, TextAlignmentOptions.Left, 36f, 42f, 0f);
            CreateText("Character", row, CleanCharacterName(entry.characterName), 22f, TextColor, TextAlignmentOptions.Left, 36f, -1f, 2f);
            CreateText("Score", row, entry.score.ToString(), 22f, TextColor, TextAlignmentOptions.Right, 36f, -1f, 1f);
            CreateText("Time", row, FormatTime(entry.survivedTime), 22f, TextColor, TextAlignmentOptions.Right, 36f, -1f, 1f);
        }
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
