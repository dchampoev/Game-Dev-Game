using UnityEngine;
using UnityEngine.TestTools;

[ExcludeFromCoverage]
[System.Obsolete("This class is no longer used. Please use the new PassiveItem class instead.")]
public class PassiveItem : MonoBehaviour
{
    protected PlayerStats playerStats;
    public PassiveItemScriptableObject passiveItemData;

    [ExcludeFromCoverage]
    protected virtual void ApplyModifier()
    {
        //Apply the modifier to the appropriate stat in child classes
    }

    void Start()
    {
        playerStats = FindAnyObjectByType<PlayerStats>();
        ApplyModifier();
    }
}
