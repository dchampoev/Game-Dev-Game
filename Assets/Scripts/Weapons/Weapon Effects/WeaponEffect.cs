using UnityEngine;

/// <summary>
/// A GameObject that is spawned as an effect of a weapon firing, e.g. projectiles, auras, pulses.
/// </summary>
public class WeaponEffect : MonoBehaviour
{
    [HideInInspector] public PlayerStats owner;
    [HideInInspector] public Weapon weapon;

    public PlayerStats Owner => owner;

    public float GetDamage()
    {
        return weapon.GetDamage();
    }
}
