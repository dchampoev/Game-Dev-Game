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
    public List<Image> chosenWeaponsUI = new List<Image>(6);
    public List<Image> chosenPassiveItemsUI = new List<Image>(6);

    [Header("Stopwatch")]
    public float timeLimit;
    float stopwatchTime;
    public TextMeshProUGUI stopwatchDisplay;

    public bool isGameOver = false;

    public bool choosingUpgrade;

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
                if (!isGameOver)
                {
                    isGameOver = true;
                    Time.timeScale = 0f;
                    DisplayResults();
                }
                break;
            case GameState.LevelUp:
                if (!choosingUpgrade)
                {
                    choosingUpgrade = true;
                    Time.timeScale = 0f;
                    levelUpScreen.SetActive(true);
                }
                break;
            default:
                Debug.LogWarning("Unhandled game state: " + currentState);
                break;
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
    }
    public void PauseGame()
    {
        if (currentState != GameState.Paused)
        {
            previousState = currentState;
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
        timeSurvivedDisplay.text = stopwatchDisplay.text;
        ChangeState(GameState.GameOver);
    }

    void DisplayResults()
    {
        resultsScreen.SetActive(true);
    }

    public void AssignChosenCharacterUI(CharacterScriptableObject chosenCharacter)
    {
        chosenCharacterImage.sprite = chosenCharacter.Icon;
        chosenCharacterName.text = chosenCharacter.name;
    }

    public void AssignLevelReachedUI(int levelReached)
    {
        levelReachedDisplay.text = levelReached.ToString();
    }

    public void AssignChosenWeaponsAndPassiveItemsUI(List<Image> chosenWeapons, List<Image> chosenPassiveItems)
    {
        if (chosenWeapons.Count != chosenWeaponsUI.Count || chosenPassiveItems.Count != chosenPassiveItemsUI.Count)
        {
            Debug.LogError("Chosen weapons or passive items count does not match the UI slots count.");
            return;
        }

        for (int i = 0; i < chosenWeapons.Count; i++)
        {
            if (chosenWeapons[i].sprite)
            {
                chosenWeaponsUI[i].enabled = true;
                chosenWeaponsUI[i].sprite = chosenWeapons[i].sprite;
            }
            else
            {
                chosenWeaponsUI[i].enabled = false;
            }
        }
        for (int i = 0; i < chosenPassiveItems.Count; i++)
        {
            if (chosenPassiveItems[i].sprite)
            {
                chosenPassiveItemsUI[i].enabled = true;
                chosenPassiveItemsUI[i].sprite = chosenPassiveItems[i].sprite;
            }
            else
            {
                chosenPassiveItemsUI[i].enabled = false;
            }
        }
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
        playerObject.SendMessage("RemoveAndApplyUpgrades");
    }

    public void EndLevelUp()
    {
        choosingUpgrade = false;
        Time.timeScale = 1f;
        levelUpScreen.SetActive(false);
        ChangeState(GameState.Gameplay);
    }

    IEnumerator GenerateFloatingTextCoroutine(string text, Transform target, float duration = 1f, float speed = 50f)
    {
        if (instance == null || instance.damageTextCanvas == null)
            yield break;

        GameObject floatingTextObj = new GameObject("Damage Floating Text");
        RectTransform rectTransform = floatingTextObj.AddComponent<RectTransform>();
        TextMeshProUGUI textComponent = floatingTextObj.AddComponent<TextMeshProUGUI>();

        floatingTextObj.transform.SetParent(instance.damageTextCanvas.transform, false);
        floatingTextObj.transform.SetAsFirstSibling();

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

        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        float elapsedTime = 0f;
        float yOffset = 0f;

        while (elapsedTime < duration)
        {
            yield return wait;

            if (floatingTextObj == null || rectTransform == null || textComponent == null)
                yield break;

            elapsedTime += Time.deltaTime;

            if (target != null)
                worldPos = target.position;

            yOffset += speed * Time.deltaTime;

            rectTransform.position = referenceCamera.WorldToScreenPoint(
                worldPos + new Vector3(0f, yOffset, 0f)
            );

            Color c = textComponent.color;
            textComponent.color = new Color(c.r, c.g, c.b, 1f - elapsedTime / duration);
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
