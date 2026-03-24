using System.Collections.Generic;
using UnityEditor;
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
#if UNITY_EDITOR
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            List<CharacterData> characters = new List<CharacterData>();
            foreach (string assetPath in allAssetPaths)
            {
                if (assetPath.EndsWith(".asset"))
                {
                    CharacterData character = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
                    if (character) characters.Add(character);
                }
            }

            if (characters.Count > 0) return characters[Random.Range(0, characters.Count)];
#endif
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
