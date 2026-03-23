using UnityEngine;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
public class CharacterSelector : MonoBehaviour
{
    public static CharacterSelector instance;
    public CharacterData selectedCharacter;

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
    public static CharacterData GetData()
    {
        if (instance && instance.selectedCharacter)
        {
            return instance.selectedCharacter;
        }
        else
        {
            CharacterData[] characters = Resources.FindObjectsOfTypeAll<CharacterData>();
            if (characters.Length > 0)
            {
                return characters[Random.Range(0, characters.Length)];
            }
        }
        return null;
    }

    public void SelectCharacter(CharacterData character)
    {
        selectedCharacter = character;
    }
    
    public void DestroySingleton()
    {
        instance = null;
        Destroy(gameObject);
    }
}
