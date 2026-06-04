using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExcludeFromCodeCoverage]
public class SceneController : MonoBehaviour
{
    public void SceneChange(string sceneName)
    {
        if (GameManager.instance)
            GameManager.instance.SaveRunCoins();
        SceneManager.LoadScene(sceneName);
        Time.timeScale = 1f;
    }
}
