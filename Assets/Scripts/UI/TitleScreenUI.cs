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
    bool wasInstructionsOpen;

    void Start()
    {
        instructionsScreen = GameObject.Find(InstructionsScreenName);
        SelectButton(firstButton);
    }

    void Update()
    {
        if (!instructionsScreen)
            instructionsScreen = GameObject.Find(InstructionsScreenName);

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
        if (!instructionsScreen) return;

        GameObject selectedObject = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
        if (selectedObject && selectedObject.activeInHierarchy && selectedObject.transform.IsChildOf(instructionsScreen.transform))
            return;

        SelectButton(instructionsScreen.GetComponentInChildren<Button>());
    }

    bool HasCurrentSelection()
    {
        GameObject selectedObject = EventSystem.current ? EventSystem.current.currentSelectedGameObject : null;
        return selectedObject && selectedObject.activeInHierarchy;
    }

    void SelectButton(Button button)
    {
        if (!button || !EventSystem.current) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
}
