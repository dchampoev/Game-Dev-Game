using UnityEngine;

[CreateAssetMenu(fileName = "PassiveItemScriptableObject", menuName = "Scriptable Objects/Passive Item")]
public class PassiveItemScriptableObject : ScriptableObject
{
    [SerializeField]
    float multiplier;
    public float Multiplier { get => multiplier; private set => multiplier = value; }

    [SerializeField]
    int level; // Not meant to be modified in the game [Only in Editor]
    public int Level { get => level; private set => level = value; }

    [SerializeField]
    GameObject nextLevelPrefab; // The prefab for the next level of the passive item, used for upgrading
    public GameObject NextLevelPrefab { get => nextLevelPrefab; private set => nextLevelPrefab = value; }

    [SerializeField]
    Sprite icon; // Not meant to be modified in the game [Only in Editor]
    public Sprite Icon { get => icon; private set => icon = value; }
}
