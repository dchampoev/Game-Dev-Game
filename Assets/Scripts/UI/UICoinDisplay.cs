using TMPro;
using UnityEngine;

/// <summary>
/// Component that is attached to GameObjects to make it display the player's coins.
/// Either in-game, or the total number of coins the player has, depending on the use.
/// </summary>

public class UICoinDisplay : MonoBehaviour
{
    TextMeshProUGUI displayTarget;
    public PlayerCollector collector;

    void Start()
    {
        displayTarget = GetComponentInChildren<TextMeshProUGUI>();
        UpdateDisplay();
        if (collector != null)
            collector.onCoinCollected += UpdateDisplay;
    }

    public void UpdateDisplay()
    {
        if (!displayTarget)
            return;

        if (collector != null)
            displayTarget.text = Mathf.RoundToInt(collector.GetCoins()).ToString();
        else
        {
            float coins = SaveManager.LastLoadedGameData.coins;
            displayTarget.text = Mathf.RoundToInt(coins).ToString();
        }
    }
}
