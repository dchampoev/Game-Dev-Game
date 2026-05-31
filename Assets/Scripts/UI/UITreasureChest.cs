using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[ExcludeFromCodeCoverage]
public class UITreasureChest : MonoBehaviour
{
    const int RewardContentSortingOrder = 30000;
    const int RewardVfxSortingOrder = -10000;
    const float RewardReadyDelay = 1f;
    const float RewardRevealDelay = 0.3f;

    public static UITreasureChest instance;
    PlayerCollector collector;
    TreasureChest currentChest;
    TreasureChestDropProfile dropProfile;

    [Header("Visual Elements")]
    public GameObject openingVFX;
    public GameObject beamVFX;
    public GameObject fireworks;
    public GameObject doneButton;
    public GameObject curvedBeams;
    public List<ItemDisplays> items;
    Color originalColor = new Color32(0x42, 0x41, 0x87, 255);

    [Header("UI Elements")]
    public GameObject chestCover;
    public GameObject chestButton;

    [Header("UI Components")]
    public Image chestPanel;
    public TextMeshProUGUI coinText;
    private float coins;

    private List<Sprite> icons = new List<Sprite>();
    private bool isAnimating = false;
    private Coroutine chestSequenceCoroutine;

    private AudioSource audioSource;
    public AudioClip pickUpSound;

    [System.Serializable]
    public struct ItemDisplays
    {
        public GameObject beam;
        public Image spriteImage;
        public GameObject sprite;
        public GameObject weaponBeam;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        gameObject.SetActive(false);

        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple instances of UITreasureChest detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public static void Activate(PlayerCollector collector, TreasureChest chest)
    {
        if (!instance)
        {
            Debug.LogWarning("UITreasureChest instance not found in the scene.");
            return;
        }

        if (!chest)
        {
            Debug.LogWarning("Cannot activate treasure chest UI without a chest.");
            return;
        }

        instance.collector = collector;
        instance.currentChest = chest;
        instance.dropProfile = chest.GetCurrentDropProfile();
        if (!instance.dropProfile)
        {
            Debug.LogWarning("Cannot activate treasure chest UI without a drop profile.");
            return;
        }

        if (GameManager.instance)
            GameManager.instance.StartTreasureChest();
        instance.gameObject.SetActive(true);
    }

    public static void NotifyItemReceived(Sprite icon)
    {
        if (instance)
            instance.icons.Add(icon);
        else
            Debug.LogWarning("UITreasureChest instance not found to receive item notification.");
    }

    private IEnumerator FlashWhite(Image image, int times, float flashDuration = 0.2f)
    {
        originalColor = image.color;

        for (int i = 0; i < times; i++)
        {
            image.color = Color.white;
            yield return new WaitForSecondsRealtime(flashDuration);

            image.color = originalColor;
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    IEnumerator ActivateCurvedBeams(float spawnTime)
    {
        yield return new WaitForSecondsRealtime(spawnTime);
        curvedBeams.SetActive(true);
    }

    IEnumerator HandleCoinDisplay(float maxCoins, float countDuration)
    {
        chestButton.SetActive(false);
        coinText.gameObject.SetActive(true);
        float elapsedTime = 0f;
        coins = maxCoins;

        while (elapsedTime < countDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float displayedCoins = Mathf.Lerp(0f, maxCoins, elapsedTime / countDuration);
            coinText.text = string.Format("{0:F2}", displayedCoins);
            yield return null;
        }

        coinText.text = string.Format("{0:F2}", maxCoins);
        yield return new WaitForSecondsRealtime(RewardReadyDelay);
        coinText.gameObject.SetActive(false);
        doneButton.gameObject.SetActive(true);
    }

    private void SetupBeam(int index)
    {
        if (index < 0 || index >= items.Count || index >= icons.Count)
            return;

        items[index].weaponBeam.SetActive(true);
        items[index].beam.SetActive(true);
        items[index].spriteImage.sprite = icons[index];
        SendRewardVfxBehindItem(index);
        BringRewardContentToFront(index);

        Image beamImage = items[index].beam.GetComponent<Image>();
        if (beamImage && dropProfile.beamColors != null && index < dropProfile.beamColors.Length)
        {
            beamImage.color = dropProfile.beamColors[index];
        }
    }

    private void BringRewardContentToFront(int index)
    {
        GameObject rewardRoot = items[index].sprite;
        Image icon = items[index].spriteImage;
        int sortingLayerId = GetRewardSortingLayerId(rewardRoot);

        if (rewardRoot)
        {
            rewardRoot.transform.SetAsLastSibling();
            ApplyCanvasSorting(rewardRoot, sortingLayerId);
        }

        if (icon)
        {
            GameObject background = icon.transform.parent ? icon.transform.parent.gameObject : null;
            if (background)
                ApplyCanvasSorting(background, sortingLayerId);

            icon.transform.SetAsLastSibling();
            ApplyCanvasSorting(icon.gameObject, sortingLayerId);
        }
    }

    private void ApplyCanvasSorting(GameObject target, int sortingLayerId)
    {
        ApplyCanvasSorting(target, sortingLayerId, RewardContentSortingOrder);
    }

    private void ApplyCanvasSorting(GameObject target, int sortingLayerId, int sortingOrder)
    {
        Canvas canvas = target.GetComponent<Canvas>();
        if (!canvas)
            canvas = target.AddComponent<Canvas>();

        canvas.overrideSorting = true;
        canvas.sortingLayerID = sortingLayerId;
        canvas.sortingOrder = sortingOrder;
    }

    private int GetRewardSortingLayerId(GameObject rewardRoot)
    {
        if (!rewardRoot)
            return SortingLayer.NameToID("Default");

        ParticleSystemRenderer renderer = rewardRoot.GetComponentInChildren<ParticleSystemRenderer>(true);
        return renderer ? renderer.sortingLayerID : SortingLayer.NameToID("Default");
    }

    private void SendRewardVfxBehindItem(int index)
    {
        GameObject rewardRoot = items[index].sprite;
        if (!rewardRoot)
            return;

        ParticleSystemRenderer[] renderers = rewardRoot.GetComponentsInChildren<ParticleSystemRenderer>(true);
        foreach (ParticleSystemRenderer renderer in renderers)
        {
            renderer.sortingLayerID = GetRewardSortingLayerId(rewardRoot);
            renderer.sortingOrder = RewardVfxSortingOrder;

            renderer.transform.SetAsFirstSibling();
        }
    }

    private IEnumerator ShowDelayedBeams(int startIndex, int endIndex)
    {
        yield return new WaitForSecondsRealtime(dropProfile.delayTime);

        for (int i = startIndex; i < endIndex; i++)
        {
            SetupBeam(i);
        }
    }

    public void DisplayBeams(float numberOfBeams)
    {
        int beamCount = Mathf.Min((int)numberOfBeams, items.Count, icons.Count);
        int delayedStartIndex = Mathf.Max(0, beamCount - dropProfile.delayedBeams);

        for (int i = 0; i < delayedStartIndex; i++)
        {
            SetupBeam(i);
        }

        if (dropProfile.delayedBeams > 0)
            StartCoroutine(ShowDelayedBeams(delayedStartIndex, beamCount));

        StartCoroutine(DisplayItems(beamCount));
    }

    private IEnumerator DisplayItems(float numberOfBeams)
    {
        yield return new WaitForSecondsRealtime(dropProfile.animationDuration);

        int itemCount = Mathf.Min((int)numberOfBeams, items.Count);

        if (itemCount == 5)
        {
            items[0].weaponBeam.SetActive(false);
            items[0].sprite.SetActive(true);
            yield return new WaitForSecondsRealtime(RewardRevealDelay);

            for (int i = 1; i <= 2; i++)
            {
                items[i].weaponBeam.SetActive(false);
                items[i].sprite.SetActive(true);
            }
            yield return new WaitForSecondsRealtime(RewardRevealDelay);

            for (int i = 3; i <= 4; i++)
            {
                items[i].weaponBeam.SetActive(false);
                items[i].sprite.SetActive(true);
            }
            yield return new WaitForSecondsRealtime(RewardRevealDelay);
        }
        else
        {
            for (int i = 0; i < itemCount; i++)
            {
                items[i].weaponBeam.SetActive(false);
                items[i].sprite.SetActive(true);
                yield return new WaitForSecondsRealtime(RewardRevealDelay);
            }
        }
    }

    private float GetRewardRevealDuration(int itemCount)
    {
        if (itemCount <= 0)
            return 0f;
        return itemCount == 5 ? RewardRevealDelay * 3f : RewardRevealDelay * itemCount;
    }

    public IEnumerator Open()
    {
        if (dropProfile.hasFireworks)
        {
            isAnimating = false;
            StartCoroutine(FlashWhite(chestPanel, 5));
            fireworks.SetActive(true);
            yield return new WaitForSecondsRealtime(dropProfile.fireworksDelay);
        }

        isAnimating = true;

        if (dropProfile.hasCurvedBeams)
            StartCoroutine(ActivateCurvedBeams(dropProfile.curveBeamsSpawnTime));

        int itemCount = Mathf.Min(dropProfile.numberOfItems, items.Count, icons.Count);
        float animationDuration = Mathf.Max(0.01f, dropProfile.animationDuration);
        float totalRewardDuration = animationDuration + GetRewardRevealDuration(itemCount);
        StartCoroutine(HandleCoinDisplay(Random.Range(dropProfile.minCoins, dropProfile.maxCoins), totalRewardDuration));

        DisplayBeams(dropProfile.numberOfItems);
        openingVFX.SetActive(true);
        beamVFX.SetActive(true);

        yield return new WaitForSecondsRealtime(animationDuration);
        openingVFX.SetActive(false);
        yield return new WaitForSecondsRealtime(GetRewardRevealDuration(itemCount));
        isAnimating = false;
    }

    public void Begin()
    {
        chestCover.SetActive(false);
        chestButton.SetActive(false);
        chestSequenceCoroutine = StartCoroutine(Open());

        if (audioSource && dropProfile.openingSound)
        {
            audioSource.clip = dropProfile.openingSound;
            audioSource.Play();
        }
    }

    private void SkipToRewards()
    {
        if (chestSequenceCoroutine != null)
            StopCoroutine(chestSequenceCoroutine);

        StopAllCoroutines();

        for (int i = 0; i < icons.Count; i++)
        {
            SetupBeam(i);
            items[i].weaponBeam.SetActive(false);
            items[i].sprite.SetActive(true);
        }

        chestButton.SetActive(false);
        coinText.gameObject.SetActive(true);
        coinText.text = coins.ToString("F2");
        coinText.gameObject.SetActive(false);
        doneButton.gameObject.SetActive(true);
        openingVFX.SetActive(false);
        isAnimating = false;
        chestPanel.color = originalColor;

        if (audioSource != null && dropProfile.openingSound != null)
        {
            audioSource.clip = dropProfile.openingSound;

            float skipToTime = Mathf.Max(0, audioSource.clip.length - 0.5f);
            audioSource.time = skipToTime;
            audioSource.Play();
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (isAnimating && (keyboard.escapeKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
        {
            SkipToRewards();
        }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            TryPressButton(chestButton);
            TryPressButton(doneButton);
        }
    }

    private void TryPressButton(GameObject buttonObject)
    {
        if (buttonObject && buttonObject.activeInHierarchy)
        {
            Button button = buttonObject.GetComponent<Button>();
            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
            }
        }
    }

    public void CloseUI()
    {
        if (collector)
            collector.AddCoins(coins);

        chestCover.SetActive(true);
        chestButton.SetActive(true);
        icons.Clear();
        beamVFX.SetActive(false);
        gameObject.SetActive(false);
        doneButton.gameObject.SetActive(false);
        fireworks.SetActive(false);
        curvedBeams.SetActive(false);
        ResetDisplay();

        if (pickUpSound)
            AudioSource.PlayClipAtPoint(pickUpSound, transform.position);

        isAnimating = false;

        if (GameManager.instance)
            GameManager.instance.EndTreasureChest();
        if (currentChest)
            currentChest.NotifyComplete();
    }

    private void ResetDisplay()
    {
        foreach (var item in items)
        {
            item.beam.SetActive(false);
            item.sprite.SetActive(false);
            item.spriteImage.sprite = null;
        }
        dropProfile = null;
        icons.Clear();
    }
}
