using UnityEngine;


[CreateAssetMenu(fileName = "WeaponEvolutionBlueprint", menuName = "Scriptable Objects/Weapon Evolution Blueprint")]
public class WeaponEvolutionBlueprint : ScriptableObject
{
    public WeaponScriptableObject weaponToEvolveData;
    public PassiveItemScriptableObject catalystPassiveItemData;
    public WeaponScriptableObject evolvedWeaponData;
    public GameObject evolvedWeapon;
}
