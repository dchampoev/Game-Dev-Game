using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine.EventSystems;

[ExcludeFromCoverage]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState
    {
        Gameplay,
        Paused,
        GameOver,
        LevelUp
    }

    public GameState currentState;

    public GameState previousState;

    [Header("Damage Text Settings")]
    public Canvas damageTextCanvas;
    public float textFontSize = 20;
    public TMP_FontAsset damageTextFont;
    public Camera referenceCamera;

    [Header("Screens")]
    public GameObject pauseMenu;
    public GameObject resultsScreen;
    public GameObject levelUpScreen;
    int stackedLevelUps = 0;

    [Header("Current Stat Displays")]
    public TextMeshProUGUI currentHealthDisplay;
    public TextMeshProUGUI currentRecoveryDisplay;
    public TextMeshProUGUI currentMoveSpeedDisplay;
    public TextMeshProUGUI currentMightDisplay;
    public TextMeshProUGUI currentProjectileSpeedDisplay;
    public TextMeshProUGUI currentMagnetDisplay;

    [Header("Result Screen Displays")]
    public Image chosenCharacterImage;
    public TextMeshProUGUI chosenCharacterName;
    public TextMeshProUGUI levelReachedDisplay;
    public TextMeshProUGUI timeSurvivedDisplay;
    TMP_InputField leaderboardNameInput;
    GameObject leaderboardNamePrompt;
    int pendingLeaderboardLevel;
    float pendingLeaderboardTime;

    [Header("Stopwatch")]
    public float timeLimit;
    float stopwatchTime;
    public TextMeshProUGUI stopwatchDisplay;

    public bool isGameOver { get { return currentState == GameState.GameOver; } }
    public bool choosingUpgrade { get { return currentState == GameState.LevelUp; } }

    PlayerStats[] players;

    public float GetElapsedTime()
    {
        return stopwatchTime;
    }

    public static float GetCumulativeCurse()
    {
        if (!instance) return 1;

        float totalCurse = 0;
        foreach (PlayerStats player in instance.players)
        {
            totalCurse += player.Actual.curse;
        }
        return Mathf.Max(1, 1 + totalCurse);
    }

    public static int GetCumulativeLevels()
    {
        if (!instance) return 1;

        int totalLevel = 0;
        foreach (PlayerStats player in instance.players)
        {
            totalLevel += player.level;
        }
        return Mathf.Max(1, totalLevel);
    }

    void Awake()
    {
        players = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DisableScreens();
    }

    void Update()
    {
        switch (currentState)
        {
            case GameState.Gameplay:
                CheckForPauseAndResume();
                UpdateStopwatch();
                break;
            case GameState.Paused:
                CheckForPauseAndResume();
                break;
            case GameState.GameOver:
            case GameState.LevelUp:
                break;
            default:
                Debug.LogWarning("Unhandled game state: " + currentState);
                break;
        }
    }

    public void ChangeState(GameState newState)
    {
        previousState = currentState;
        currentState = newState;
    }
    public void PauseGame()
    {
        if (currentState != GameState.Paused)
        {
            ChangeState(GameState.Paused);
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;

            SelectFirstPauseButton();
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            ChangeState(previousState);
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    void CheckForPauseAndResume()
    {
        if (Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame))
        {
            if (currentState == GameState.Gameplay)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }

    void DisableScreens()
    {
        if (pauseMenu) pauseMenu.SetActive(false);
        if (resultsScreen) resultsScreen.SetActive(false);
        if (levelUpScreen) levelUpScreen.SetActive(false);
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        timeSurvivedDisplay.text = stopwatchDisplay.text;
        ChangeState(GameState.GameOver);
        DisplayResults();
    }

    public void GameOver(int levelReached)
    {
        pendingLeaderboardLevel = levelReached;
        pendingLeaderboardTime = GetElapsedTime();
        AssignLevelReachedUI(levelReached);
        GameOver();
        ShowLeaderboardNamePrompt();
    }

    void DisplayResults()
    {
        resultsScreen.SetActive(true);
    }

    void ShowLeaderboardNamePrompt()
    {
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
        leaderboardNameInput.characterLimit = 16;
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

        int score = pendingLeaderboardLevel * 100 + Mathf.FloorToInt(pendingLeaderboardTime);
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

    public void AssignChosenCharacterUI(CharacterData chosenCharacter)
    {
        if (!chosenCharacter) return;

        chosenCharacterImage.sprite = chosenCharacter.Icon;
        chosenCharacterName.text = chosenCharacter.Name;
    }

    public void AssignLevelReachedUI(int levelReached)
    {
        levelReachedDisplay.text = levelReached.ToString();
    }

    void UpdateStopwatch()
    {
        stopwatchTime += Time.deltaTime;

        UpdateStopwatchDisplay();

        if (stopwatchTime >= timeLimit)
        {
            foreach(PlayerStats player in players)
            {
                player.SendMessage("Die");
            }
        }
    }

    void UpdateStopwatchDisplay()
    {
        int minutes = Mathf.FloorToInt(stopwatchTime / 60f);
        int seconds = Mathf.FloorToInt(stopwatchTime % 60f);
        stopwatchDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void StartLevelUp()
    {
        ChangeState(GameState.LevelUp);

        foreach (PlayerStats player in players)
        {
            if (player == null) continue;

            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if(movement != null)
                movement.StopMovement();
        }

        if (levelUpScreen.activeSelf) stackedLevelUps++;
        else
        {
            levelUpScreen.SetActive(true);
            Time.timeScale = 0f;
            foreach (PlayerStats player in players)
            {
                player.SendMessage("RemoveAndApplyUpgrades");
            }

            SelectFirstLevelUpButton();
        }
    }

    public void EndLevelUp()
    {
        Time.timeScale = 1f;
        levelUpScreen.SetActive(false);
        ChangeState(GameState.Gameplay);

        if (stackedLevelUps > 0)
        {
            stackedLevelUps--;
            StartLevelUp();
        }
    }

    void SelectFirstLevelUpButton()
    {
        SelectFirstButton(levelUpScreen);
    }

    void SelectFirstPauseButton()
    {
        SelectFirstButton(pauseMenu);
    }

    void SelectFirstButton(GameObject root)
    {
        if (!root || !EventSystem.current) return;

        Button firstButton = root.GetComponentInChildren<Button>();
        if (!firstButton) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
    }

    IEnumerator GenerateFloatingTextCoroutine(string text, Transform target, float duration = 1f, float speed = 50f)
    {
        if (instance == null || instance.damageTextCanvas == null)
            yield break;

        GameObject floatingTextObj = new GameObject("Damage Floating Text");
        RectTransform rectTransform = floatingTextObj.AddComponent<RectTransform>();
        TextMeshProUGUI textComponent = floatingTextObj.AddComponent<TextMeshProUGUI>();

        textComponent.text = text;
        textComponent.horizontalAlignment = HorizontalAlignmentOptions.Center;
        textComponent.verticalAlignment = VerticalAlignmentOptions.Middle;
        textComponent.fontSize = textFontSize;

        if (damageTextFont != null)
            textComponent.font = damageTextFont;

        Vector3 worldPos = Vector3.zero;
        if (target != null)
            worldPos = target.position;

        rectTransform.position = referenceCamera.WorldToScreenPoint(worldPos);

        Destroy(floatingTextObj, duration);

        floatingTextObj.transform.SetParent(instance.damageTextCanvas.transform, false);
        floatingTextObj.transform.SetAsFirstSibling();

        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        float elapsedTime = 0f;
        float yOffset = 0f;
        Vector3 lastKnownPosition = worldPos;

        while (elapsedTime < duration)
        {
            if (floatingTextObj == null || rectTransform == null || textComponent == null)
                yield break;

            if (target != null)
                lastKnownPosition = target.position;

            Color c = textComponent.color;
            textComponent.color = new Color(c.r, c.g, c.b, 1f - elapsedTime / duration);

            yOffset += speed * Time.deltaTime;
            rectTransform.position = referenceCamera.WorldToScreenPoint(
                lastKnownPosition + new Vector3(0f, yOffset, 0f)
            );

            yield return wait;
            elapsedTime += Time.deltaTime;
        }

        if (floatingTextObj != null)
            Destroy(floatingTextObj);
    }

    public static void GenerateFloatingText(string text, Transform target, float duration = 1f, float speed = 1f)
    {
        if (instance == null) return;
        if (!instance.damageTextCanvas) return;
        if (!instance.referenceCamera) instance.referenceCamera = Camera.main;

        instance.StartCoroutine(instance.GenerateFloatingTextCoroutine(text, target, duration, speed));
    }
}
