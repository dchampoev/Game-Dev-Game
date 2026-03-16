using UnityEngine;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
[CreateAssetMenu(fileName = "PassiveItemScriptableObject", menuName = "Scriptable Objects/Passive Item")]
public class PassiveItemScriptableObject : ScriptableObject
{
    [SerializeField]
    float multiplier;
    public float Multiplier { get => multiplier; private set => multiplier = value; }

    [SerializeField]
    private int level;
    public int Level { get => level; private set => level = value; }

    [SerializeField]
    GameObject nextLevelPrefab;
    public GameObject NextLevelPrefab { get => nextLevelPrefab; private set => nextLevelPrefab = value; }

    [SerializeField]
    new string name;
    public string Name { get => name; private set => name = value; }

    [SerializeField]
    string description;
    public string Description { get => description; private set => description = value; }

    [SerializeField]
    private Sprite icon;
    public Sprite Icon { get => icon; private set => icon = value; }
}
