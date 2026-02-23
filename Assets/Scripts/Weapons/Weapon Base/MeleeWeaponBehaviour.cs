using UnityEngine;

/// <summary>
/// Base script for all melee weapon behaviours[To be placed on a prefab of a weapon that is a melee].
/// </summary>

public class MeleeWeaponBehaviour : MonoBehaviour
{
    public float destroyAfterSeconds;

    protected virtual void Start()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }
}
