using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyboardNavigationUITests
{
    EventSystem CreateEventSystem()
    {
        EventSystem eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
        typeof(EventSystem)
            .GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(eventSystem, null);

        EventSystem.current = eventSystem;
        return eventSystem;
    }

    Toggle CreateToggle(string name, Transform parent)
    {
        GameObject toggleObject = new GameObject(name);
        toggleObject.transform.SetParent(parent, false);

        Image image = toggleObject.AddComponent<Image>();
        Toggle toggle = toggleObject.AddComponent<Toggle>();
        toggle.targetGraphic = image;
        return toggle;
    }

    Button CreateButton(string name)
    {
        GameObject buttonObject = new GameObject(name);
        Image image = buttonObject.AddComponent<Image>();
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    UIPowerUpSelector CreatePowerUpSelector()
    {
        GameObject selectorObject = new GameObject("Power Up Selector");
        UIPowerUpSelector selector = selectorObject.AddComponent<UIPowerUpSelector>();

        GridLayoutGroup grid = selectorObject.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        Toggle first = CreateToggle("Power Up 1", selectorObject.transform);
        Toggle second = CreateToggle("Power Up 2", selectorObject.transform);

        selector.toggleTemplate = first;
        selector.selectableToggles.Add(first);
        selector.selectableToggles.Add(second);
        selector.buyButton = CreateButton("Buy Button");
        selector.backButton = CreateButton("Back Button");

        return selector;
    }

    UICharacterSelector CreateCharacterSelector()
    {
        GameObject selectorObject = new GameObject("Character Selector");
        UICharacterSelector selector = selectorObject.AddComponent<UICharacterSelector>();

        GridLayoutGroup grid = selectorObject.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        Toggle first = CreateToggle("Antonio", selectorObject.transform);
        Toggle second = CreateToggle("Imelda", selectorObject.transform);

        selector.toggleTemplate = first;
        selector.selectableToggles.Add(first);
        selector.selectableToggles.Add(second);
        selector.startButton = CreateButton("Character Select Button");
        selector.backButton = CreateButton("Back Button");

        return selector;
    }

    void CallPrivateMethod(object target, string methodName)
    {
        target.GetType()
            .GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(target, null);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        UIPowerUpSelector.selected = null;
        UICharacterSelector.selected = null;
    }

    [Test]
    public void PowerUpNavigation_WhenAtLastPowerUp_ShouldMoveDownToBuyButton()
    {
        UIPowerUpSelector selector = CreatePowerUpSelector();

        CallPrivateMethod(selector, "SetupToggleNavigation");

        Navigation navigation = selector.selectableToggles[1].navigation;
        Assert.AreSame(selector.buyButton, navigation.selectOnDown);
    }

    [Test]
    public void PowerUpNavigation_WhenOnBottomButtons_ShouldMoveBetweenButtonsAndBackToSelectedPowerUp()
    {
        UIPowerUpSelector selector = CreatePowerUpSelector();
        selector.selectableToggles[0].isOn = true;

        CallPrivateMethod(selector, "UpdateButtonNavigation");

        Navigation buyNavigation = selector.buyButton.navigation;
        Navigation backNavigation = selector.backButton.navigation;

        Assert.AreSame(selector.backButton, buyNavigation.selectOnRight);
        Assert.AreSame(selector.selectableToggles[0], buyNavigation.selectOnUp);
        Assert.AreSame(selector.buyButton, backNavigation.selectOnLeft);
        Assert.AreSame(selector.selectableToggles[0], backNavigation.selectOnUp);
    }

    [Test]
    public void PowerUpFocus_WhenBuyButtonSelected_ShouldShowGoldenOutline()
    {
        CreateEventSystem();
        UIPowerUpSelector selector = CreatePowerUpSelector();

        CallPrivateMethod(selector, "SetupFocusOutlines");
        EventSystem.current.SetSelectedGameObject(selector.buyButton.gameObject);
        CallPrivateMethod(selector, "RefreshFocusHighlight");

        Outline outline = selector.buyButton.targetGraphic.GetComponent<Outline>();
        Assert.NotNull(outline);
        Assert.IsTrue(outline.enabled);
        Assert.AreEqual(selector.keyboardFocusColor, outline.effectColor);
        Assert.AreEqual(selector.keyboardFocusDistance, outline.effectDistance);
    }

    [Test]
    public void CharacterNavigation_WhenOnBottomButtons_ShouldMoveBetweenSelectAndBackButtons()
    {
        UICharacterSelector selector = CreateCharacterSelector();
        selector.selectableToggles[0].isOn = true;

        CallPrivateMethod(selector, "UpdateStartButtonNavigation");

        Navigation startNavigation = selector.startButton.navigation;
        Navigation backNavigation = selector.backButton.navigation;

        Assert.AreSame(selector.backButton, startNavigation.selectOnLeft);
        Assert.AreSame(selector.selectableToggles[0], startNavigation.selectOnUp);
        Assert.AreSame(selector.startButton, backNavigation.selectOnLeft);
        Assert.AreSame(selector.startButton, backNavigation.selectOnRight);
        Assert.AreSame(selector.selectableToggles[0], backNavigation.selectOnUp);
    }

    [Test]
    public void CharacterFocus_WhenBackButtonSelected_ShouldShowGoldenOutline()
    {
        CreateEventSystem();
        UICharacterSelector selector = CreateCharacterSelector();

        CallPrivateMethod(selector, "SetupFocusOutlines");
        EventSystem.current.SetSelectedGameObject(selector.backButton.gameObject);
        CallPrivateMethod(selector, "RefreshFocusHighlight");

        Outline outline = selector.backButton.targetGraphic.GetComponent<Outline>();
        Assert.NotNull(outline);
        Assert.IsTrue(outline.enabled);
        Assert.AreEqual(selector.keyboardFocusColor, outline.effectColor);
        Assert.AreEqual(selector.keyboardFocusDistance, outline.effectDistance);
    }
}
