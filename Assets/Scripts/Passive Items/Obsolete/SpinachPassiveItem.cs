using UnityEngine;

[System.Obsolete("This class is no longer used.")]
public class SpinachPassiveItem : PassiveItem
{
    protected override void ApplyModifier()
    {
        playerStats.CurrentMight *= 1 + passiveItemData.Multiplier / 100f;
    }
}
