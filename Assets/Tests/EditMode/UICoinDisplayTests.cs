using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class UICoinDisplayTests
{
    void CallStart(UICoinDisplay display)
    {
        typeof(UICoinDisplay)
            .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(display, null);
    }

    UICoinDisplay CreateDisplay(out TextMeshProUGUI text)
    {
        GameObject displayObject = new GameObject("Coin Display");
        UICoinDisplay display = displayObject.AddComponent<UICoinDisplay>();

        GameObject textObject = new GameObject("Coin Text");
        textObject.transform.SetParent(displayObject.transform);
        text = textObject.AddComponent<TextMeshProUGUI>();

        return display;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }
    }

    [Test]
    public void Start_WhenCollectorIsNull_ShouldDisplaySavedTotalCoins()
    {
        SaveManager.LastLoadedGameData.coins = 42f;
        UICoinDisplay display = CreateDisplay(out TextMeshProUGUI text);

        CallStart(display);

        Assert.AreEqual("42", text.text);
    }

    [Test]
    public void Start_WhenCollectorExists_ShouldDisplayRunCoinsAndRefreshAfterCollection()
    {
        UICoinDisplay display = CreateDisplay(out TextMeshProUGUI text);
        PlayerCollector collector = new GameObject("Collector").AddComponent<PlayerCollector>();
        display.collector = collector;

        CallStart(display);
        collector.AddCoins(3f);

        Assert.AreEqual("3", text.text);
    }
}
