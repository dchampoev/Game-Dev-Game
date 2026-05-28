using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.TestTools;
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

    [Header("Stopwatch")]
    public float timeLimit;
    public TextMeshProUGUI stopwatchDisplay;

    public bool isGameOver { get { return currentState == GameState.GameOver; } }
    public bool choosingUpgrade { get { return currentState == GameState.LevelUp; } }

    PlayerStats[] players;
    GameTimer gameTimer;
    ResultsScreenUI resultsScreenUI;
    FloatingTextSpawner floatingTextSpawner;

    public float GetElapsedTime()
    {
        return gameTimer ? gameTimer.ElapsedTime : 0f;
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

        InitializeSplitComponents();
        DisableScreens();
    }

    void InitializeSplitComponents()
    {
        gameTimer = GetOrAddComponent<GameTimer>();
        gameTimer.Initialize(timeLimit, stopwatchDisplay);
        gameTimer.TimeLimitReached -= EndRunByTimeLimit;
        gameTimer.TimeLimitReached += EndRunByTimeLimit;

        resultsScreenUI = GetOrAddComponent<ResultsScreenUI>();
        resultsScreenUI.Initialize(
            resultsScreen,
            chosenCharacterImage,
            chosenCharacterName,
            levelReachedDisplay,
            timeSurvivedDisplay);

        floatingTextSpawner = GetOrAddComponent<FloatingTextSpawner>();
        floatingTextSpawner.Initialize(damageTextCanvas, textFontSize, damageTextFont, referenceCamera);
    }

    T GetOrAddComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        return component ? component : gameObject.AddComponent<T>();
    }

    void Update()
    {
        switch (currentState)
        {
            case GameState.Gameplay:
                CheckForPauseAndResume();
                gameTimer.Tick(Time.deltaTime);
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
        ChangeState(GameState.GameOver);
        resultsScreenUI.Show(gameTimer.FormattedTime);
    }

    public void GameOver(int levelReached)
    {
        Time.timeScale = 0f;
        ChangeState(GameState.GameOver);
        resultsScreenUI.ShowWithLeaderboardPrompt(levelReached, GetElapsedTime(), gameTimer.FormattedTime);
    }

    public void AssignChosenCharacterUI(CharacterData chosenCharacter)
    {
        resultsScreenUI.SetChosenCharacter(chosenCharacter);
    }

    public void AssignLevelReachedUI(int levelReached)
    {
        if (levelReachedDisplay) levelReachedDisplay.text = levelReached.ToString();
    }

    void EndRunByTimeLimit()
    {
        foreach (PlayerStats player in players)
        {
            if (player) player.Kill();
        }
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
                if (!player) continue;

                PlayerInventory inventory = player.GetComponent<PlayerInventory>();
                if (inventory) inventory.RemoveAndApplyUpgrades();
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

    public static void GenerateFloatingText(string text, Transform target, float duration = 1f, float speed = 1f)
    {
        if (instance == null) return;
        if (!instance.floatingTextSpawner) return;

        instance.floatingTextSpawner.Show(text, target, duration, speed);
    }
}
