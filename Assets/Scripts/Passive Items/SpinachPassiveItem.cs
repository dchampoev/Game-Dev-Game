using UnityEngine;

public class SpinachPassiveItem : PassiveItem
{
    protected override void ApplyModifier()
    {
        playerStats.currentMight *= 1 + passiveItemData.Multiplier/100f;
    }
}
