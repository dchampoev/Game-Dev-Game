using UnityEngine;

public class UnholyVespersWeapon : KingBibleWeapon
{
    public override bool ActivateCooldown(bool strict = false)
    {
        if (strict && currentCooldown > 0)
            return false;

        float actualCooldown = (currentStats.lifespan) * Owner.Stats.cooldown;

        currentCooldown = Mathf.Min(actualCooldown, currentCooldown + actualCooldown);
        return true;
    }
}
