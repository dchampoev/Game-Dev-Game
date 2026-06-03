using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public Item item;

        public void Assign(Item assignedItem)
        {
            item = assignedItem;
        }

        public void Clear()
        {
            item = null;
        }

        public bool IsEmpty() { return item == null; }
    }
    public List<Slot> weaponSlots = new List<Slot>(6);
    public List<Slot> passiveSlots = new List<Slot>(6);
    public List<PowerUp> powerUps = new List<PowerUp>();
    public UIInventoryIconsDisplay weaponUI, passiveUI;

    [Header("UI Elements")]
    public List<WeaponData> availableWeapons = new List<WeaponData>();
    public List<PassiveData> availablePassives = new List<PassiveData>();
    public List<PowerUpData> availablePowerUps = new List<PowerUpData>();
    public UIUpgradeWindow upgradeWindow;

    PlayerStats player;

    void Start()
    {
        player = GetComponent<PlayerStats>();

        SaveManager saveManager = SaveManager.Instance;
        if (saveManager == null || saveManager.SavedData.powerUps.Count == 0)
            return;

        foreach (PowerUp.Data data in saveManager.SavedData.powerUps)
        {
            PowerUpData powerUpData = FindPowerUpData(data.name);
            if (powerUpData)
                Add(powerUpData, data.level);
        }
    }

    PowerUpData FindPowerUpData(string powerUpName)
    {
        foreach (PowerUpData powerUp in availablePowerUps)
        {
            if (powerUp && powerUp.name == powerUpName)
                return powerUp;
        }

        return null;
    }

    public bool Has(ItemData type) => Get(type) != null;

    public Item Get(ItemData type)
    {
        return type switch
        {
            WeaponData weaponData => Get(weaponData),
            PowerUpData powerUpData => Get(powerUpData),
            PassiveData passiveData => Get(passiveData),
            _ => null
        };
    }

    public PowerUp Get(PowerUpData type)
    {
        foreach (PowerUp p in powerUps)
        {
            if (p && p.data == type)
            {
                return p;
            }
        }
        return null;
    }

    public Passive Get(PassiveData type)
    {
        if (type is PowerUpData powerUpData)
            return Get(powerUpData);

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
        if (removeUpgradeAvailability)
            availableWeapons.Remove(data);

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
        if (data is PowerUpData powerUpData)
            return Remove(powerUpData);

        if (removeUpgradeAvailability)
            availablePassives.Remove(data);

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

    public bool Remove(PowerUpData data)
    {
        for (int i = 0; i < powerUps.Count; i++)
        {
            PowerUp powerUp = powerUps[i];
            if (powerUp && powerUp.data == data)
            {
                powerUps.RemoveAt(i);
                powerUp.OnUnequip();
                Destroy(powerUp.gameObject);
                return true;
            }
        }

        return false;
    }

    public bool Remove(ItemData data, bool removeUpgradeAvailability = false)
    {
        return data switch
        {
            PowerUpData powerUpData => Remove(powerUpData, removeUpgradeAvailability),
            WeaponData weaponData => Remove(weaponData, removeUpgradeAvailability),
            PassiveData passiveData => Remove(passiveData, removeUpgradeAvailability),
            _ => false
        };
    }

    public int Add(WeaponData data, bool updateUI = true)
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

        if (slotIndex < 0)
            return slotIndex;

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
            RefreshInventoryUI(updateUI);

            if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
                GameManager.instance.EndLevelUp();

            return slotIndex;
        }
        else
        {
            Debug.LogWarning(string.Format("Weapon behaviour script {0} not found! Make sure the class name matches the string in WeaponData.", data.behaviour));
        }
        return -1;
    }

    public int Add(PassiveData data, bool updateUI = true)
    {
        if (data is PowerUpData powerUpData)
            return Add(powerUpData, 1);

        int slotIndex = -1;

        for (int i = 0; i < passiveSlots.Count; i++)
        {
            if (passiveSlots[i].IsEmpty())
            {
                slotIndex = i;
                break;
            }
        }

        if (slotIndex < 0)
            return slotIndex;


        GameObject gameObject = new GameObject(data.baseStats.name + " Passive");
        Passive spawnedPassive = gameObject.AddComponent<Passive>();
        spawnedPassive.transform.SetParent(transform);
        spawnedPassive.transform.localPosition = Vector2.zero;
        spawnedPassive.Initialize(data);

        passiveSlots[slotIndex].Assign(spawnedPassive);
        RefreshInventoryUI(updateUI);

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
            GameManager.instance.EndLevelUp();

        if (player)
            player.RecalculateStats();

        return slotIndex;
    }

    public int Add(PowerUpData data, int level = 1)
    {
        if (!data)
            return -1;

        GameObject gameObject = new GameObject(data.baseStats.name + " Power Up");
        PowerUp power = gameObject.AddComponent<PowerUp>();
        power.Initialize(data);
        power.transform.SetParent(transform);
        power.transform.localPosition = Vector2.zero;
        for (int i = 1; i < level; i++)
            power.DoLevelUp(false);
        powerUps.Add(power);

        if (player)
            player.RecalculateStats();

        return powerUps.Count;
    }

    public bool Add(PowerUp.Data saveData)
    {
        if (saveData == null)
            return false;

        foreach (PowerUpData data in availablePowerUps)
        {
            if (data && data.name == saveData.name)
            {
                Add(data, saveData.level);
                return true;
            }
        }
        return false;
    }

    public int Add(ItemData data, bool updateUI = true)
    {
        return data switch
        {
            WeaponData weaponData => Add(weaponData, updateUI),
            PowerUpData powerUpData => Add(powerUpData, 1),
            PassiveData passiveData => Add(passiveData, updateUI),
            _ => -1
        };
    }

    public bool LevelUp(ItemData data, bool updateUI = true)
    {
        Item item = Get(data);
        if (item)
            return LevelUp(item, updateUI);
        return false;
    }

    public bool LevelUp(Item item, bool updateUI = true)
    {
        if (item == null)
            return false;

        if (!item.DoLevelUp(updateUI))
        {
            Debug.LogWarning(string.Format("Failed to level up {0}", item.name));
            return false;
        }

        RefreshInventoryUI(updateUI);

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
            GameManager.instance.EndLevelUp();

        if (item is Passive && player)
            player.RecalculateStats();

        return true;
    }
    public void RemoveAndApplyUpgrades()
    {
        ApplyUpgradeOptions();
    }

    void ApplyUpgradeOptions()
    {
        if (!player || !upgradeWindow)
            return;

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

    void RefreshInventoryUI(bool updateUI)
    {
        if (!updateUI)
            return;

        if (weaponUI)
            weaponUI.Refresh();
        if (passiveUI)
            passiveUI.Refresh();
    }

    public int GetSlotsLeft<T>() where T : Item
    {
        Slot[] slots = GetSlots<T>();
        return slots == null ? 0 : GetSlotsLeft(new List<Slot>(slots));
    }

    public int GetSlotsLeftFor<T>() where T : ItemData
    {
        Slot[] slots = GetSlotsFor<T>();
        return slots == null ? 0 : GetSlotsLeft(new List<Slot>(slots));
    }

    public Slot[] GetSlots<T>() where T : Item
    {
        if (typeof(T) == typeof(Passive))
        {
            return passiveSlots.ToArray();
        }

        if (typeof(T) == typeof(Weapon))
        {
            return weaponSlots.ToArray();
        }

        if (typeof(T) == typeof(PowerUp))
        {
            return null;
        }

        if (typeof(T) == typeof(Item))
        {
            List<Slot> allSlots = new List<Slot>(passiveSlots);
            allSlots.AddRange(weaponSlots);
            return allSlots.ToArray();
        }

        Debug.LogWarning(string.Format("Invalid type parameter {0} in GetSlots<T>()", typeof(T)));
        return null;
    }

    public Slot[] GetSlotsFor<T>() where T : ItemData
    {
        if (typeof(T) == typeof(PowerUpData))
        {
            return null;
        }
        else if (typeof(T) == typeof(PassiveData))
        {
            return passiveSlots.ToArray();
        }
        else if (typeof(T) == typeof(WeaponData))
        {
            return weaponSlots.ToArray();
        }
        else if (typeof(T) == typeof(ItemData))
        {
            List<Slot> allSlots = new List<Slot>(passiveSlots);
            allSlots.AddRange(weaponSlots);
            return allSlots.ToArray();
        }
        Debug.LogWarning(string.Format("Invalid type parameter {0} in GetSlotsFor<T>()", typeof(T)));
        return null;
    }

    public T[] GetAvailable<T>() where T : ItemData
    {
        if (typeof(T) == typeof(PowerUpData))
        {
            return availablePowerUps.ToArray() as T[];
        }
        else if (typeof(T) == typeof(PassiveData))
        {
            return availablePassives.ToArray() as T[];
        }
        else if (typeof(T) == typeof(WeaponData))
        {
            return availableWeapons.ToArray() as T[];
        }
        else if (typeof(T) == typeof(ItemData))
        {
            List<ItemData> allAvailable = new List<ItemData>(availablePassives);
            allAvailable.AddRange(availableWeapons);
            return allAvailable.ToArray() as T[];
        }

        Debug.LogWarning(string.Format("Invalid type parameter {0} in GetAvailable<T>()", typeof(T)));
        return null;
    }

    public T[] GetUnowned<T>() where T : ItemData
    {
        var available = GetAvailable<T>();

        if (available == null || available.Length == 0)
            return new T[0];

        List<T> list = new List<T>(available);

        var slots = GetSlotsFor<T>();
        if (slots != null)
        {
            foreach (Slot s in slots)
            {
                if (s?.item?.data != null && list.Contains(s.item.data as T))
                {
                    list.Remove(s.item.data as T);
                }
            }
        }
        return list.ToArray();
    }

    public T[] GetEvolvables<T>() where T : Item
    {
        List<T> result = new List<T>();
        Slot[] slots = GetSlots<T>();
        if (slots == null)
            return result.ToArray();

        foreach (Slot s in slots)
        {
            if (s?.item is T t && t.CanEvolve(0).Length > 0)
                result.Add(t);
        }
        return result.ToArray();
    }

    public T[] GetUpgradables<T>() where T : Item
    {
        List<T> result = new List<T>();
        Slot[] slots = GetSlots<T>();
        if (slots == null)
            return result.ToArray();

        foreach (Slot s in slots)
        {
            if (s?.item is T t && t.CanLevelUp())
                result.Add(t);
        }
        return result.ToArray();
    }
}
