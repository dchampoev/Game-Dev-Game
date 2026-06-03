using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;

/// <summary>
/// Component that is attached to GameObjects to make it display the player's coins.
/// Either in-game, or the total number of coins the player has, depending on the use.
/// </summary>

[ExcludeFromCodeCoverage]
public class UICoinDisplay : MonoBehaviour
{
    TextMeshProUGUI displayTarget;
    public PlayerCollector collector;

    void Start()
    {
        displayTarget = GetComponentInChildren<TextMeshProUGUI>();
        if (collector != null)
            collector.onCoinCollected += UpdateDisplay;
        else
        {
            SaveManager saveManager = SaveManager.Instance;
            if (saveManager)
                saveManager.OnCoinsChanged += UpdateDisplay;
        }

        UpdateDisplay();
    }

    void OnDestroy()
    {
        if (collector != null)
            collector.onCoinCollected -= UpdateDisplay;

        SaveManager saveManager = SaveManager.Current;
        if (saveManager != null)
            saveManager.OnCoinsChanged -= UpdateDisplay;
    }

    public void UpdateDisplay()
    {
        if (!displayTarget)
            return;

        if (collector != null)
            displayTarget.text = Mathf.RoundToInt(collector.GetCoins()).ToString();
        else
        {
            SaveManager saveManager = SaveManager.Current;
            float coins = saveManager ? saveManager.GetTotalCoins() : 0f;
            displayTarget.text = Mathf.RoundToInt(coins).ToString();
        }
    }
}
