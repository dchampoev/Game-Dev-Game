using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
public class SceneController : MonoBehaviour
{
    public void SceneChange(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        Time.timeScale = 1f;
    }
}
