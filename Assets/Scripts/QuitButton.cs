using UnityEngine;

public class QuitButton : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Quit button clicked. Exiting game...");
        Application.Quit();
    }
}
