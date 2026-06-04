using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExcludeFromCodeCoverage]
public class ResultsScreenUI : MonoBehaviour
{
    const int CharacterLimit = 16;
    const int LevelScoreMultiplier = 100;

    GameObject resultsScreen;
    Image chosenCharacterImage;
    TextMeshProUGUI chosenCharacterName;
    TextMeshProUGUI levelReachedDisplay;
    TextMeshProUGUI timeSurvivedDisplay;

    TMP_InputField leaderboardNameInput;
    GameObject leaderboardNamePrompt;
    int pendingLeaderboardLevel;
    float pendingLeaderboardTime;

    public void Initialize(
        GameObject screen,
        Image characterImage,
        TextMeshProUGUI characterName,
        TextMeshProUGUI levelDisplay,
        TextMeshProUGUI timeDisplay)
    {
        resultsScreen = screen;
        chosenCharacterImage = characterImage;
        chosenCharacterName = characterName;
        levelReachedDisplay = levelDisplay;
        timeSurvivedDisplay = timeDisplay;
    }

    public void SetChosenCharacter(CharacterData chosenCharacter)
    {
        if (!chosenCharacter)
            return;

        if (chosenCharacterImage)
            chosenCharacterImage.sprite = chosenCharacter.Icon;
        if (chosenCharacterName)
            chosenCharacterName.text = chosenCharacter.Name;
    }

    public void Show(string survivedTimeText)
    {
        if (timeSurvivedDisplay)
            timeSurvivedDisplay.text = survivedTimeText;
        if (resultsScreen)
            resultsScreen.SetActive(true);
    }

    public void ShowWithLeaderboardPrompt(int levelReached, float survivedTime, string survivedTimeText)
    {
        pendingLeaderboardLevel = levelReached;
        pendingLeaderboardTime = survivedTime;

        if (levelReachedDisplay)
            levelReachedDisplay.text = levelReached.ToString();
        Show(survivedTimeText);
        ShowLeaderboardNamePrompt();
    }

    void ShowLeaderboardNamePrompt()
    {
        if (!resultsScreen)
            return;

        if (leaderboardNamePrompt == null)
            CreateLeaderboardNamePrompt();

        leaderboardNamePrompt.SetActive(true);
        leaderboardNameInput.text = string.Empty;
        leaderboardNameInput.ActivateInputField();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(leaderboardNameInput.gameObject);
    }

    void CreateLeaderboardNamePrompt()
    {
        leaderboardNamePrompt = CreateUIObject("Leaderboard Name Prompt", resultsScreen.transform);
        RectTransform promptRect = leaderboardNamePrompt.GetComponent<RectTransform>();
        promptRect.anchorMin = Vector2.zero;
        promptRect.anchorMax = Vector2.one;
        promptRect.offsetMin = Vector2.zero;
        promptRect.offsetMax = Vector2.zero;

        Image overlay = leaderboardNamePrompt.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.78f);

        CreatePromptLabel();
        CreateNameInput();
    }

    void CreatePromptLabel()
    {
        GameObject labelObject = CreateUIObject("Leaderboard Name Label", leaderboardNamePrompt.transform);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = new Vector2(0f, 78f);
        labelRect.sizeDelta = new Vector2(720f, 70f);

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = "Enter a name for the leaderboard";
        label.fontSize = 38f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
    }

    void CreateNameInput()
    {
        GameObject inputObject = CreateUIObject("Leaderboard Name Input", leaderboardNamePrompt.transform);
        RectTransform inputRect = inputObject.GetComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.5f);
        inputRect.anchorMax = new Vector2(0.5f, 0.5f);
        inputRect.pivot = new Vector2(0.5f, 0.5f);
        inputRect.anchoredPosition = Vector2.zero;
        inputRect.sizeDelta = new Vector2(520f, 72f);

        Image inputBackground = inputObject.AddComponent<Image>();
        inputBackground.color = new Color(0.08f, 0.08f, 0.1f, 1f);

        leaderboardNameInput = inputObject.AddComponent<TMP_InputField>();
        leaderboardNameInput.targetGraphic = inputBackground;
        leaderboardNameInput.characterLimit = CharacterLimit;
        leaderboardNameInput.lineType = TMP_InputField.LineType.SingleLine;
        leaderboardNameInput.caretWidth = 4;
        leaderboardNameInput.caretBlinkRate = 1.25f;
        leaderboardNameInput.customCaretColor = true;
        leaderboardNameInput.caretColor = Color.white;
        leaderboardNameInput.selectionColor = new Color(1f, 1f, 1f, 0.25f);
        leaderboardNameInput.onSubmit.AddListener(SubmitLeaderboardName);

        GameObject textObject = CreateUIObject("Text", inputObject.transform);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(22f, 8f);
        textRect.offsetMax = new Vector2(-22f, -8f);

        TextMeshProUGUI inputText = textObject.AddComponent<TextMeshProUGUI>();
        inputText.fontSize = 36f;
        inputText.alignment = TextAlignmentOptions.MidlineLeft;
        inputText.color = Color.white;

        leaderboardNameInput.textComponent = inputText;
    }

    void SubmitLeaderboardName(string playerName)
    {
        string trimmedName = playerName.Trim();
        if (trimmedName.Length == 0)
        {
            leaderboardNameInput.ActivateInputField();
            return;
        }

        int score = pendingLeaderboardLevel * LevelScoreMultiplier + Mathf.FloorToInt(pendingLeaderboardTime);
        LeaderboardManager.SaveScore(trimmedName, score, pendingLeaderboardTime);
        leaderboardNamePrompt.SetActive(false);
    }

    GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject uiObject = new GameObject(objectName, typeof(RectTransform));
        uiObject.layer = LayerMask.NameToLayer("UI");
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }
}
