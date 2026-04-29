using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public Item item;

        public void Assign(Item assignedItem)
        {
            item = assignedItem;
            if (item is Weapon)
            {
                Weapon weapon = item as Weapon;
            }
            else
            {
                Passive passive = item as Passive;
            }
            Debug.Log(string.Format("Assigned {0} to inventory slot.", item.name));
        }

        public void Clear()
        {
            item = null;
        }

        public bool IsEmpty() { return item == null; }
    }
    public List<Slot> weaponSlots = new List<Slot>(6);
    public List<Slot> passiveSlots = new List<Slot>(6);
    public UIInvetoryIconsDisplay weaponUI, passiveUI;

    [Header("UI Elements")]
    public List<WeaponData> availableWeapons = new List<WeaponData>();
    public List<PassiveData> availablePassives = new List<PassiveData>();
    public UIUpgradeWindow upgradeWindow;

    PlayerStats player;

    void Start()
    {
        player = GetComponent<PlayerStats>();
    }

    public bool Has(ItemData type) { return Get(type) != null; }

    public Item Get(ItemData type)
    {
        if (type is WeaponData) return Get(type as WeaponData);
        else if (type is PassiveData) return Get(type as PassiveData);
        return null;
    }

    public Passive Get(PassiveData type)
    {
        foreach (Slot s in passiveSlots)
        {
            Passive p = s.item as Passive;
            if (p != null && p.data == type)
            {
                return p;
            }
        }
        return null;
    }

    public Weapon Get(WeaponData type)
    {
        foreach (Slot s in weaponSlots)
        {
            Weapon w = s.item as Weapon;
            if (w != null && w.data == type)
            {
                return w;
            }
        }
        return null;
    }

    public bool Remove(WeaponData data, bool removeUpgradeAvailability = false)
    {
        if (removeUpgradeAvailability) availableWeapons.Remove(data);

        for (int i = 0; i < weaponSlots.Count; i++)
        {
            Weapon w = weaponSlots[i].item as Weapon;
            if (w != null && w.data == data)
            {
                weaponSlots[i].Clear();
                w.OnUnequip();
                Destroy(w.gameObject);
                return true;
            }
        }
        return false;
    }

    public bool Remove(PassiveData data, bool removeUpgradeAvailability = false)
    {
        if (removeUpgradeAvailability) availablePassives.Remove(data);

        for (int i = 0; i < passiveSlots.Count; i++)
        {
            Passive p = passiveSlots[i].item as Passive;
            if (p != null && p.data == data)
            {
                passiveSlots[i].Clear();
                p.OnUnequip();
                Destroy(p.gameObject);
                return true;
            }
        }
        return false;
    }

    public bool Remove(ItemData data, bool removeUpgradeAvailability = false)
    {
        if (data is WeaponData) return Remove(data as WeaponData, removeUpgradeAvailability);
        else if (data is PassiveData) return Remove(data as PassiveData, removeUpgradeAvailability);
        return false;
    }

    public int Add(WeaponData data)
    {
        int slotIndex = -1;

        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i].IsEmpty())
            {
                slotIndex = i;
                break;
            }
        }

        if (slotIndex < 0) return slotIndex;

        Type weaponType = Type.GetType(data.behaviour);

        if (weaponType != null)
        {
            GameObject gameObject = new GameObject(data.baseStats.name + " Controller");
            Weapon spawnedWeapon = (Weapon)gameObject.AddComponent(weaponType);
            spawnedWeapon.transform.SetParent(transform);
            spawnedWeapon.transform.localPosition = Vector2.zero;
            spawnedWeapon.Initialize(data);
            spawnedWeapon.OnEquip();

            weaponSlots[slotIndex].Assign(spawnedWeapon);
            if(weaponUI != null) weaponUI.Refresh();

            if (GameManager.instance != null && GameManager.instance.choosingUpgrade) GameManager.instance.EndLevelUp();

            return slotIndex;
        }
        else
        {
            Debug.LogWarning(string.Format("Weapon behaviour script {0} not found! Make sure the class name matches the string in WeaponData.", data.behaviour));
        }
        return -1;
    }

    public int Add(PassiveData data)
    {
        int slotIndex = -1;

        for (int i = 0; i < passiveSlots.Count; i++)
        {
            if (passiveSlots[i].IsEmpty())
            {
                slotIndex = i;
                break;
            }
        }

        if (slotIndex < 0) return slotIndex;


        GameObject gameObject = new GameObject(data.baseStats.name + " Passive");
        Passive spawnedPassive = gameObject.AddComponent<Passive>();
        spawnedPassive.transform.SetParent(transform);
        spawnedPassive.transform.localPosition = Vector2.zero;
        spawnedPassive.Initialize(data);

        passiveSlots[slotIndex].Assign(spawnedPassive);
        if(passiveUI != null) passiveUI.Refresh();

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade) GameManager.instance.EndLevelUp();

        player.RecalculateStats();

        return slotIndex;
    }

    public int Add(ItemData data)
    {
        if (data is WeaponData) return Add(data as WeaponData);
        else if (data is PassiveData) return Add(data as PassiveData);
        return -1;
    }

    public bool LevelUp(Item item)
    {
        if (item == null)
            return false;

        if (!item.DoLevelUp())
        {
            Debug.LogWarning(string.Format("Failed to level up {0}", item.name));
            return false;
        }

        if(weaponUI!=null) weaponUI.Refresh();
        if(passiveUI!=null) passiveUI.Refresh();

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
            GameManager.instance.EndLevelUp();

        if (item is Passive)
            player.RecalculateStats();

        return true;
    }
    public void RemoveAndApplyUpgrades()
    {
        ApplyUpgradeOptions();
    }

    void ApplyUpgradeOptions()
    {
        List<ItemData> availableUpgrades = GetAvailableUpgrades();
        int availableUpgradeCount = availableUpgrades.Count;

        if (availableUpgradeCount > 0)
        {
            bool getExtraItem = 1f - 1f / player.Stats.luck > UnityEngine.Random.value;

            if (getExtraItem || availableUpgradeCount < 4)
            {
                upgradeWindow.SetUpgrades(this, availableUpgrades, 4);
            }
            else
            {
                upgradeWindow.SetUpgrades(
                    this,
                    availableUpgrades,
                    3,
                    "Increase your Luck stat for a chance to get 4 items!"
                );
            }
        }
        else if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }
    }

    List<ItemData> GetAvailableUpgrades()
    {
        List<ItemData> availableUpgrades = new List<ItemData>();
        List<ItemData> allUpgrades = new List<ItemData>();

        allUpgrades.AddRange(availableWeapons);
        allUpgrades.AddRange(availablePassives);

        int weaponSlotsLeft = GetSlotsLeft(weaponSlots);
        int passiveSlotsLeft = GetSlotsLeft(passiveSlots);

        foreach (ItemData data in allUpgrades)
        {
            if (CanBeOfferedAsUpgrade(data, weaponSlotsLeft, passiveSlotsLeft))
                availableUpgrades.Add(data);
        }

        return availableUpgrades;
    }

    bool CanBeOfferedAsUpgrade(ItemData data, int weaponSlotsLeft, int passiveSlotsLeft)
    {
        Item existingItem = Get(data);

        if (existingItem != null)
            return existingItem.currentLevel < data.maxLevel;

        if (data is WeaponData)
            return weaponSlotsLeft > 0;

        if (data is PassiveData)
            return passiveSlotsLeft > 0;

        return false;
    }

    int GetSlotsLeft(List<Slot> slots)
    {
        int count = 0;

        foreach (Slot slot in slots)
        {
            if (slot.IsEmpty())
                count++;
        }

        return count;
    }
}