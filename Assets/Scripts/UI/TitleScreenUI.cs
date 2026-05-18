using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExcludeFromCodeCoverage]
public class TitleScreenUI : MonoBehaviour
{
    public Button firstButton;

    void Start()
    {
        if (firstButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstButton.gameObject);
        }
    }
}