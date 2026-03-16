using UnityEngine;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
public class CharacterSelector : MonoBehaviour
{
    public static CharacterSelector instance;
    public CharacterScriptableObject selectedCharacter;

    void Awake()
    {
        UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Multiple instances of CharacterSelector detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }
    public static CharacterScriptableObject GetData()
    {
        return instance.selectedCharacter;
    }

    public void SelectCharacter(CharacterScriptableObject character)
    {
        selectedCharacter = character;
    }
    
    public void DestroySingleton()
    {
        instance = null;
        Destroy(gameObject);
    }
}
