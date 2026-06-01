using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExcludeFromCodeCoverage]
public class TitleScreenUI : MonoBehaviour
{
    const string InstructionsScreenName = "Instructions Screen";

    public Button firstButton;

    GameObject instructionsScreen;
    Button instructionsButton;
    bool wasInstructionsOpen;

    void Start()
    {
        ResolveInstructionsScreen();
        SelectButton(firstButton);
    }

    void Update()
    {
        if (!instructionsScreen)
            ResolveInstructionsScreen();

        bool instructionsOpen = instructionsScreen && instructionsScreen.activeInHierarchy;

        if (instructionsOpen)
        {
            SelectInstructionsButtonIfNeeded();
        }
        else if (wasInstructionsOpen || !HasCurrentSelection())
        {
            SelectButton(firstButton);
        }

        wasInstructionsOpen = instructionsOpen;
    }

    void SelectInstructionsButtonIfNeeded()
    {
        if (!instructionsScreen)
            return;

        GameObject selectedObject = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
        if (selectedObject && selectedObject.activeInHierarchy && selectedObject.transform.IsChildOf(instructionsScreen.transform))
            return;

        SelectButton(instructionsButton);
    }

    void ResolveInstructionsScreen()
    {
        instructionsScreen = GameObject.Find(InstructionsScreenName);
        instructionsButton = instructionsScreen ? instructionsScreen.GetComponentInChildren<Button>(true) : null;
    }

    bool HasCurrentSelection()
    {
        GameObject selectedObject = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
        return selectedObject && selectedObject.activeInHierarchy;
    }

    void SelectButton(Button button)
    {
        if (!button || !EventSystem.current)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
}
