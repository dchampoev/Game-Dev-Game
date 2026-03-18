using UnityEngine;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
public class QuitButton : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Quit button clicked. Exiting game...");
        Application.Quit();
    }
}
