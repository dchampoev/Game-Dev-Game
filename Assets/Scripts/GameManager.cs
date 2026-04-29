using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.TestTools;
using System.Collections;

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
    float stopwatchTime;
    public TextMeshProUGUI stopwatchDisplay;

    public bool isGameOver { get { return currentState == GameState.GameOver; } }
    public bool choosingUpgrade { get { return currentState == GameState.LevelUp; } }

    public GameObject playerObject;

    void Awake()
    {
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
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
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
        pauseMenu.SetActive(false);
        resultsScreen.SetActive(false);
        levelUpScreen.SetActive(false);
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        timeSurvivedDisplay.text = stopwatchDisplay.text;
        ChangeState(GameState.GameOver);
        DisplayResults();
    }

    void DisplayResults()
    {
        resultsScreen.SetActive(true);
    }

    public void AssignChosenCharacterUI(CharacterData chosenCharacter)
    {
        chosenCharacterImage.sprite = chosenCharacter.Icon;
        chosenCharacterName.text = chosenCharacter.name;
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
            playerObject.SendMessage("Die");
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
        if (levelUpScreen.activeSelf) stackedLevelUps++;
        else
        {
            levelUpScreen.SetActive(true);
            Time.timeScale = 0f;
            playerObject.SendMessage("RemoveAndApplyUpgrades");
        }
    }

    public void EndLevelUp()
    {
        Time.timeScale = 1f;
        levelUpScreen.SetActive(false);
        ChangeState(GameState.Gameplay);

        if(stackedLevelUps > 0)
        {
            stackedLevelUps--;
            StartLevelUp();
        }
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
        Vector3 lastKnownPosition = target.position;

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
