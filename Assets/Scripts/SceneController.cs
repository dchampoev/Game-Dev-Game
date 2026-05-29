using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

[System.Obsolete("This is an obsolete class. It will be replaced by UILevelSelect")]
[ExcludeFromCoverage]
public class SceneController : MonoBehaviour
{
    public void SceneChange(string sceneName)
    {
        if (GameManager.instance) GameManager.instance.SaveRunCoins();
        SceneManager.LoadScene(sceneName);
        Time.timeScale = 1f;
    }
}
