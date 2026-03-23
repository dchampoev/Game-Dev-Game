using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public Item item;
        public Image image;

        public void Assign(Item assignedItem)
        {
            item = assignedItem;
            if (item is Weapon)
            {
                Weapon weapon = item as Weapon;
                image.enabled = true;
                image.sprite = weapon.data.icon;
            }
            else
            {
                Passive passive = item as Passive;
                image.enabled = true;
                image.sprite = passive.data.icon;
            }
            Debug.Log(string.Format("Assigned {0} to inventory slot.", item.name));
        }

        public void Clear()
        {
            item = null;
            image.enabled = false;
            image.sprite = null;
        }

        public bool IsEmpty() { return item == null; }
    }
    public List<Slot> weaponSlots = new List<Slot>(6);
    public List<Slot> passiveSlots = new List<Slot>(6);

    [System.Serializable]
    public class UpgradeUI
    {
        public TMP_Text upgradeNameDisplay;
        public TMP_Text upgradeDescriptionDisplay;
        public Image upgradeIcon;
        public Button upgradeButton;
    }

    [Header("UI Elements")]
    public List<WeaponData> availableWeapons = new List<WeaponData>();
    public List<PassiveData> availablePassives = new List<PassiveData>();
    public List<UpgradeUI> upgradeUIOptions = new List<UpgradeUI>();

    PlayerStats player;

    private enum UpgradeCategory
    {
        Weapon = 1,
        Passive = 2
    }

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
            spawnedWeapon.Initialize(data);
            spawnedWeapon.transform.SetParent(transform);
            spawnedWeapon.transform.localPosition = Vector2.zero;
            spawnedWeapon.OnEquip();

            weaponSlots[slotIndex].Assign(spawnedWeapon);

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
        spawnedPassive.Initialize(data);
        spawnedPassive.transform.SetParent(transform);
        spawnedPassive.transform.localPosition = Vector2.zero;

        passiveSlots[slotIndex].Assign(spawnedPassive);

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

    public void LevelUpWeapon(int slotIndex)
    {
        if (weaponSlots.Count > slotIndex)
        {
            Weapon weapon = weaponSlots[slotIndex].item as Weapon;

            if (!weapon.DoLevelUp())
            {
                Debug.LogWarning(string.Format("Failed to level up {0} ", weapon.name));
                return;
            }
        }

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade) GameManager.instance.EndLevelUp();
    }

    public void LevelUpPassiveItem(int slotIndex)
    {
        if (passiveSlots.Count > slotIndex)
        {
            Passive passive = passiveSlots[slotIndex].item as Passive;

            if (!passive.DoLevelUp())
            {
                Debug.LogWarning(string.Format("Failed to level up {0} ", passive.name));
                return;
            }
        }

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade) GameManager.instance.EndLevelUp();
        player.RecalculateStats();
    }

    void RemoveUpgradeOptions()
    {
        foreach (UpgradeUI upgradeOption in upgradeUIOptions)
        {
            upgradeOption.upgradeButton.onClick.RemoveAllListeners();
            DisableUpgradeUI(upgradeOption);
        }
    }

    public void RemoveAndApplyUpgrades()
    {
        RemoveUpgradeOptions();
        ApplyUpgradeOptions();
    }

    void DisableUpgradeUI(UpgradeUI ui)
    {
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(false);
    }

    void EnableUpgradeUI(UpgradeUI ui)
    {
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);
    }

    void ApplyUpgradeOptions()
    {
        List<WeaponData> weaponPool = new List<WeaponData>(availableWeapons);
        List<PassiveData> passivePool = new List<PassiveData>(availablePassives);

        foreach (UpgradeUI upgradeOption in upgradeUIOptions)
        {
            if (NoUpgradesLeft(weaponPool, passivePool))
                return;

            UpgradeCategory category = ChooseUpgradeCategory(weaponPool, passivePool);

            if (category == UpgradeCategory.Weapon)
            {
                ConfigureWeaponUpgradeOption(upgradeOption, weaponPool);
            }
            else
            {
                ConfigurePassiveUpgradeOption(upgradeOption, passivePool);
            }
        }
    }

    bool NoUpgradesLeft(List<WeaponData> weaponPool, List<PassiveData> passivePool)
    {
        return weaponPool.Count == 0 && passivePool.Count == 0;
    }

    UpgradeCategory ChooseUpgradeCategory(List<WeaponData> weaponPool, List<PassiveData> passivePool)
    {
        if (weaponPool.Count == 0)
            return UpgradeCategory.Passive;

        if (passivePool.Count == 0)
            return UpgradeCategory.Weapon;

        return UnityEngine.Random.Range(0, 2) == 0
            ? UpgradeCategory.Weapon
            : UpgradeCategory.Passive;
    }
    void ConfigureWeaponUpgradeOption(UpgradeUI ui, List<WeaponData> weaponPool)
    {
        WeaponData selectedWeapon = TakeRandomWeapon(weaponPool);
        if (selectedWeapon == null)
        {
            DisableUpgradeUI(ui);
            return;
        }

        EnableUpgradeUI(ui);

        if (TryConfigureExistingWeaponLevelUp(ui, selectedWeapon))
            return;

        ConfigureNewWeaponPickup(ui, selectedWeapon);
    }

    WeaponData TakeRandomWeapon(List<WeaponData> weaponPool)
    {
        if (weaponPool.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, weaponPool.Count);
        WeaponData selectedWeapon = weaponPool[randomIndex];
        weaponPool.RemoveAt(randomIndex);
        return selectedWeapon;
    }

    bool TryConfigureExistingWeaponLevelUp(UpgradeUI ui, WeaponData weaponData)
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            Weapon weapon = weaponSlots[i].item as Weapon;

            if (weapon == null || weapon.data != weaponData)
                continue;

            if (weapon.currentLevel >= weaponData.maxLevel)
                return false;

            int slotIndex = i;
            Weapon.Stats nextLevel = weaponData.GetLevelData(weapon.currentLevel + 1);

            BindWeaponLevelUp(ui, slotIndex);
            SetUpgradeUIContent(ui, nextLevel.name, nextLevel.description, weaponData.icon);

            return true;
        }

        return false;
    }

    void BindWeaponLevelUp(UpgradeUI ui, int slotIndex)
    {
        ui.upgradeButton.onClick.AddListener(() => LevelUpWeapon(slotIndex));
    }

    void ConfigureNewWeaponPickup(UpgradeUI ui, WeaponData weaponData)
    {
        ui.upgradeButton.onClick.AddListener(() => Add(weaponData));
        SetUpgradeUIContent(ui, weaponData.baseStats.name, weaponData.baseStats.description, weaponData.icon);
    }

    void ConfigurePassiveUpgradeOption(UpgradeUI ui, List<PassiveData> passivePool)
    {
        PassiveData selectedPassive = TakeRandomPassive(passivePool);
        if (selectedPassive == null)
            return;

        EnableUpgradeUI(ui);

        if (TryConfigureExistingPassiveLevelUp(ui, selectedPassive))
            return;

        ConfigureNewPassivePickup(ui, selectedPassive);
    }

    PassiveData TakeRandomPassive(List<PassiveData> passivePool)
    {
        if (passivePool.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, passivePool.Count);
        PassiveData selectedPassive = passivePool[randomIndex];
        passivePool.RemoveAt(randomIndex);
        return selectedPassive;
    }
    bool TryConfigureExistingPassiveLevelUp(UpgradeUI ui, PassiveData passiveData)
    {
        for (int i = 0; i < passiveSlots.Count; i++)
        {
            Passive passive = passiveSlots[i].item as Passive;

            if (passive == null || passive.data != passiveData)
                continue;

            if (passive.currentLevel >= passiveData.maxLevel)
                return false;

            int slotIndex = i;
            Passive.Modifier nextLevel = passiveData.GetLevelData(passive.currentLevel + 1);

            BindPassiveLevelUp(ui, slotIndex);
            SetUpgradeUIContent(ui, nextLevel.name, nextLevel.description, passiveData.icon);

            return true;
        }

        return false;
    }

    void BindPassiveLevelUp(UpgradeUI ui, int slotIndex)
    {
        ui.upgradeButton.onClick.AddListener(() => LevelUpPassiveItem(slotIndex));
    }

    void ConfigureNewPassivePickup(UpgradeUI ui, PassiveData passiveData)
    {
        ui.upgradeButton.onClick.AddListener(() => Add(passiveData));
        SetUpgradeUIContent(ui, passiveData.baseStats.name, passiveData.baseStats.description, passiveData.icon);
    }

    void SetUpgradeUIContent(UpgradeUI ui, string upgradeName, string description, Sprite icon)
    {
        ui.upgradeNameDisplay.text = upgradeName;
        ui.upgradeDescriptionDisplay.text = description;
        ui.upgradeIcon.sprite = icon;
    }
}