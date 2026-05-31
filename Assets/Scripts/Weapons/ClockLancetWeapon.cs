using UnityEngine;

public class ClockLancetWeapon : ProjectileWeapon
{
    public const int NUMBER_OF_ANGLES = 12;
    protected float currentAngle = 90;

    protected static float turnAngle = -360f / NUMBER_OF_ANGLES;

    protected override bool Attack(int attackCount = 1)
    {
        if (base.Attack(1))
        {
            currentAngle += turnAngle;

            if (Mathf.Abs(currentAngle) > 180f)
                currentAngle = -Mathf.Sign(currentAngle) * (360f - Mathf.Abs(currentAngle));

            return true;
        }
        return false;
    }
    protected override float GetSpawnAngle() { return currentAngle; }
}
