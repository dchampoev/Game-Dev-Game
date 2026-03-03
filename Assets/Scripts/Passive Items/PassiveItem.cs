using UnityEngine;

public class PassiveItem : MonoBehaviour
{
    protected PlayerStats playerStats;
    public PassiveItemScriptableObject passiveItemData;

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
