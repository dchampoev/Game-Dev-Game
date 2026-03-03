using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public List<WeaponController> weaponSlots = new List<WeaponController>(6);
    public int[] weaponLevels = new int[6];
    public List<Image> weaponUISlots = new List<Image>(6); // UI images for weapon slots
    public List<PassiveItem> passiveItemSlots = new List<PassiveItem>(6);
    public int[] passiveItemsLevels = new int[6];
    public List<Image> passiveItemUISlots = new List<Image>(6); // UI images for passive item slots

    public void AddWeapon(int slotIndex, WeaponController weapon)
    {
        weaponSlots[slotIndex] = weapon;
        weaponLevels[slotIndex] = weapon.weaponData.Level;
        weaponUISlots[slotIndex].enabled = true;
        weaponUISlots[slotIndex].sprite = weapon.weaponData.Icon; // Update the UI image for the weapon slot
    }

    public void AddPassiveItem(int slotIndex, PassiveItem passiveItem)
    {
        passiveItemSlots[slotIndex] = passiveItem;
        passiveItemsLevels[slotIndex] = passiveItem.passiveItemData.Level;
        passiveItemUISlots[slotIndex].enabled = true;
        passiveItemUISlots[slotIndex].sprite = passiveItem.passiveItemData.Icon; // Update the UI image for the passive item slot
    }

    public void levelUpWeapon(int slotIndex)
    {
        if (weaponSlots.Count > slotIndex)
        { 
            WeaponController weapon = weaponSlots[slotIndex];
            if(!weapon.weaponData.NextLevelPrefab)
            {
                Debug.Log("Weapon is already at max level!");
                return;
            }
            if (weapon != null)
            {
                GameObject upgradedWeapon = Instantiate(weapon.weaponData.NextLevelPrefab, transform.position, Quaternion.identity);
                upgradedWeapon.transform.SetParent(transform); //Set the player as the parent of the spawned weapon
                AddWeapon(slotIndex, upgradedWeapon.GetComponent<WeaponController>());
                Destroy(weapon.gameObject);
                weaponLevels[slotIndex] = upgradedWeapon.GetComponent<WeaponController>().weaponData.Level;
            }
        }
    }
    
    public void levelUpPassiveItem(int slotIndex)
    {
        if (passiveItemSlots.Count > slotIndex)
        { 
            PassiveItem passiveItem = passiveItemSlots[slotIndex];
            if(!passiveItem.passiveItemData.NextLevelPrefab)
            {
                Debug.Log("Passive item is already at max level!");
                return;
            }
            if (passiveItem != null)
            {
                GameObject upgradedPassiveItem = Instantiate(passiveItem.passiveItemData.NextLevelPrefab, transform.position, Quaternion.identity);
                upgradedPassiveItem.transform.SetParent(transform); //Set the player as the parent of the spawned passive item
                AddPassiveItem(slotIndex, upgradedPassiveItem.GetComponent<PassiveItem>());
                Destroy(passiveItem.gameObject);
                passiveItemsLevels[slotIndex] = upgradedPassiveItem.GetComponent<PassiveItem>().passiveItemData.Level;
            }
        }
    }
}
