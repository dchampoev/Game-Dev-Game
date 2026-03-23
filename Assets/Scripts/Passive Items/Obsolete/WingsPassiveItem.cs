using UnityEngine;

[System.Obsolete("This class is no longer used.")]
public class WingsPassiveItem : PassiveItem
{
    protected override void ApplyModifier()
    {
        playerStats.CurrentMoveSpeed *= 1 + passiveItemData.Multiplier / 100f;
    }
}
