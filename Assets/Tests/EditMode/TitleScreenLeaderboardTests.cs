using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreenLeaderboardTests
{
    RectTransform InvokeCreateEntriesScrollView(RectTransform parent)
    {
        return (RectTransform)typeof(TitleScreenLeaderboard)
            .GetMethod("CreateEntriesScrollView", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, new object[] { parent });
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
    public void CreateEntriesScrollView_ShouldUseExpandedViewportHeight()
    {
        RectTransform parent = new GameObject("Leaderboard Panel", typeof(RectTransform)).GetComponent<RectTransform>();

        RectTransform content = InvokeCreateEntriesScrollView(parent);
        RectTransform viewport = content.parent as RectTransform;
        LayoutElement layoutElement = viewport.GetComponent<LayoutElement>();
        ScrollRect scrollRect = viewport.GetComponent<ScrollRect>();

        Assert.NotNull(layoutElement);
        Assert.AreEqual(340f, layoutElement.minHeight);
        Assert.AreEqual(340f, layoutElement.preferredHeight);
        Assert.NotNull(scrollRect);
        Assert.AreSame(content, scrollRect.content);
        Assert.IsFalse(scrollRect.horizontal);
        Assert.IsTrue(scrollRect.vertical);
    }
}
