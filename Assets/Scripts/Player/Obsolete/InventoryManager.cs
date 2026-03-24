using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Obsolete("This class is no longer used. It has been replaced by PlayerInventory and related classes.")]
public class InventoryManager : MonoBehaviour
{
    public List<WeaponController> weaponSlots = new List<WeaponController>(6);
    public int[] weaponLevels = new int[6];
    public List<Image> weaponUISlots = new List<Image>(6);
    public List<PassiveItem> passiveItemSlots = new List<PassiveItem>(6);
    public int[] passiveItemsLevels = new int[6];
    public List<Image> passiveItemUISlots = new List<Image>(6);

    [System.Serializable]
    public class WeaponUpgrade
    {
        public int weaponUpgradeIndex;
        public GameObject initialWeapon;
        public WeaponScriptableObject weaponData;
    }

    [System.Serializable]
    public class PassiveItemUpgrade
    {
        public int passiveItemUpgradeIndex;
        public GameObject initialPassiveItem;
        public PassiveItemScriptableObject passiveItemData;
    }

    [System.Serializable]
    public class UpgradeUI
    {
        public TextMeshProUGUI upgradeNameDisplay;
        public TextMeshProUGUI upgradeDescriptionDisplay;
        public Image upgradeIcon;
        public Button upgradeButton;
    }

    public List<WeaponUpgrade> weaponUpgradeOptions = new List<WeaponUpgrade>();
    public List<PassiveItemUpgrade> passiveItemUpgradeOptions = new List<PassiveItemUpgrade>();
    public List<UpgradeUI> upgradeUIOptions = new List<UpgradeUI>();

    public List<WeaponEvolutionBlueprint> weaponEvolutions = new List<WeaponEvolutionBlueprint>();

    PlayerStats player;

    void Start()
    {
        player = GetComponent<PlayerStats>();
    }

    public void AddWeapon(int slotIndex, WeaponController weapon)
    {
        weaponSlots[slotIndex] = weapon;
        weaponLevels[slotIndex] = weapon.weaponData.Level;
        weaponUISlots[slotIndex].enabled = true;
        weaponUISlots[slotIndex].sprite = weapon.weaponData.Icon;

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }
    }

    public void AddPassiveItem(int slotIndex, PassiveItem passiveItem)
    {
        passiveItemSlots[slotIndex] = passiveItem;
        passiveItemsLevels[slotIndex] = passiveItem.passiveItemData.Level;
        passiveItemUISlots[slotIndex].enabled = true;
        passiveItemUISlots[slotIndex].sprite = passiveItem.passiveItemData.Icon;

        if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
        {
            GameManager.instance.EndLevelUp();
        }
    }

    public void LevelUpWeapon(int slotIndex, WeaponUpgrade chosenWeaponUpgrade)
    {
        if (weaponSlots.Count > slotIndex)
        {
            WeaponController weapon = weaponSlots[slotIndex];
            if (!weapon.weaponData.NextLevelPrefab)
            {
                Debug.Log("Weapon is already at max level!");
                return;
            }
            if (weapon != null)
            {
                GameObject upgradedWeapon = Instantiate(weapon.weaponData.NextLevelPrefab, transform.position, Quaternion.identity);
                upgradedWeapon.transform.SetParent(transform);
                AddWeapon(slotIndex, upgradedWeapon.GetComponent<WeaponController>());
                Destroy(weapon.gameObject);
                weaponLevels[slotIndex] = upgradedWeapon.GetComponent<WeaponController>().weaponData.Level;

                chosenWeaponUpgrade.weaponData = upgradedWeapon.GetComponent<WeaponController>().weaponData;

                if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
                {
                    GameManager.instance.EndLevelUp();
                }
            }
        }
    }

    public void LevelUpPassiveItem(int slotIndex, PassiveItemUpgrade chosenPassiveItemUpgrade)
    {
        if (passiveItemSlots.Count > slotIndex)
        {
            PassiveItem passiveItem = passiveItemSlots[slotIndex];
            if (!passiveItem.passiveItemData.NextLevelPrefab)
            {
                Debug.Log("Passive item is already at max level!");
                return;
            }
            if (passiveItem != null)
            {
                GameObject upgradedPassiveItem = Instantiate(passiveItem.passiveItemData.NextLevelPrefab, transform.position, Quaternion.identity);
                upgradedPassiveItem.transform.SetParent(transform);
                AddPassiveItem(slotIndex, upgradedPassiveItem.GetComponent<PassiveItem>());
                Destroy(passiveItem.gameObject);
                passiveItemsLevels[slotIndex] = upgradedPassiveItem.GetComponent<PassiveItem>().passiveItemData.Level;

                chosenPassiveItemUpgrade.passiveItemData = upgradedPassiveItem.GetComponent<PassiveItem>().passiveItemData;

                if (GameManager.instance != null && GameManager.instance.choosingUpgrade)
                {
                    GameManager.instance.EndLevelUp();
                }
            }
        }
    }

    void ApplyUpgradeOptions()
    {
        List<WeaponUpgrade> availableWeaponUpgrades = new List<WeaponUpgrade>(weaponUpgradeOptions);
        List<PassiveItemUpgrade> availablePassiveItemUpgrades = new List<PassiveItemUpgrade>(passiveItemUpgradeOptions);

        foreach (UpgradeUI upgradeOption in upgradeUIOptions)
        {
            int upgradeType = DetermineUpgradeType(availableWeaponUpgrades, availablePassiveItemUpgrades);

            if (upgradeType == 1)
            {
                ApplyWeaponUpgradeOption(upgradeOption, availableWeaponUpgrades);
            }
            else if (upgradeType == 2)
            {
                ApplyPassiveItemUpgradeOption(upgradeOption, availablePassiveItemUpgrades);
            }
        }
    }

    int DetermineUpgradeType(List<WeaponUpgrade> availableWeaponUpgrades, List<PassiveItemUpgrade> availablePassiveItemUpgrades)
    {
        if (availablePassiveItemUpgrades.Count == 0)
        {
            return 1;
        }

        if (availableWeaponUpgrades.Count == 0)
        {
            return 2;
        }

        return Random.Range(1, 3);
    }

    void ApplyWeaponUpgradeOption(UpgradeUI upgradeOption, List<WeaponUpgrade> availableWeaponUpgrades)
    {
        WeaponUpgrade chosenWeaponUpgrade = GetRandomWeaponUpgrade(availableWeaponUpgrades);
        if (chosenWeaponUpgrade == null)
        {
            DisableUpgradeUI(upgradeOption);
            return;
        }

        EnableUpgradeUI(upgradeOption);

        int existingWeaponSlotIndex = FindWeaponSlotIndex(chosenWeaponUpgrade.weaponData);

        if (existingWeaponSlotIndex >= 0)
        {
            ConfigureExistingWeaponUpgrade(upgradeOption, chosenWeaponUpgrade, existingWeaponSlotIndex);
        }
        else
        {
            ConfigureNewWeaponUpgrade(upgradeOption, chosenWeaponUpgrade);
        }

        upgradeOption.upgradeIcon.sprite = chosenWeaponUpgrade.weaponData.Icon;
    }

    WeaponUpgrade GetRandomWeaponUpgrade(List<WeaponUpgrade> availableWeaponUpgrades)
    {
        if (availableWeaponUpgrades.Count == 0)
        {
            return null;
        }

        WeaponUpgrade chosenWeaponUpgrade = availableWeaponUpgrades[Random.Range(0, availableWeaponUpgrades.Count)];
        availableWeaponUpgrades.Remove(chosenWeaponUpgrade);
        return chosenWeaponUpgrade;
    }

    int FindWeaponSlotIndex(WeaponScriptableObject weaponData)
    {
        for (int i = 0; i < weaponSlots.Count; i++)
        {
            if (weaponSlots[i] != null && weaponSlots[i].weaponData == weaponData)
            {
                return i;
            }
        }

        return -1;
    }

    void ConfigureExistingWeaponUpgrade(UpgradeUI upgradeOption, WeaponUpgrade chosenWeaponUpgrade, int slotIndex)
    {
        if (!chosenWeaponUpgrade.weaponData.NextLevelPrefab)
        {
            DisableUpgradeUI(upgradeOption);
            return;
        }

        int capturedSlotIndex = slotIndex;
        WeaponUpgrade capturedUpgrade = chosenWeaponUpgrade;

        upgradeOption.upgradeButton.onClick.AddListener(() => LevelUpWeapon(capturedSlotIndex, capturedUpgrade));
        upgradeOption.upgradeDescriptionDisplay.text =
            chosenWeaponUpgrade.weaponData.NextLevelPrefab.GetComponent<WeaponController>().weaponData.Description;
        upgradeOption.upgradeNameDisplay.text =
            chosenWeaponUpgrade.weaponData.NextLevelPrefab.GetComponent<WeaponController>().weaponData.Name;
    }

    void ConfigureNewWeaponUpgrade(UpgradeUI upgradeOption, WeaponUpgrade chosenWeaponUpgrade)
    {
        GameObject initialWeapon = chosenWeaponUpgrade.initialWeapon;

        upgradeOption.upgradeButton.onClick.AddListener(() => player.SpawnWeapon(initialWeapon));
        upgradeOption.upgradeDescriptionDisplay.text = chosenWeaponUpgrade.weaponData.Description;
        upgradeOption.upgradeNameDisplay.text = chosenWeaponUpgrade.weaponData.Name;
    }

    void ApplyPassiveItemUpgradeOption(UpgradeUI upgradeOption, List<PassiveItemUpgrade> availablePassiveItemUpgrades)
    {
        PassiveItemUpgrade chosenPassiveItemUpgrade = GetRandomPassiveItemUpgrade(availablePassiveItemUpgrades);
        if (chosenPassiveItemUpgrade == null)
        {
            DisableUpgradeUI(upgradeOption);
            return;
        }

        EnableUpgradeUI(upgradeOption);

        int existingPassiveItemSlotIndex = FindPassiveItemSlotIndex(chosenPassiveItemUpgrade.passiveItemData);

        if (existingPassiveItemSlotIndex >= 0)
        {
            ConfigureExistingPassiveItemUpgrade(upgradeOption, chosenPassiveItemUpgrade, existingPassiveItemSlotIndex);
        }
        else
        {
            ConfigureNewPassiveItemUpgrade(upgradeOption, chosenPassiveItemUpgrade);
        }

        upgradeOption.upgradeIcon.sprite = chosenPassiveItemUpgrade.passiveItemData.Icon;
    }

    PassiveItemUpgrade GetRandomPassiveItemUpgrade(List<PassiveItemUpgrade> availablePassiveItemUpgrades)
    {
        if (availablePassiveItemUpgrades.Count == 0)
        {
            return null;
        }

        PassiveItemUpgrade chosenPassiveItemUpgrade =
            availablePassiveItemUpgrades[Random.Range(0, availablePassiveItemUpgrades.Count)];

        availablePassiveItemUpgrades.Remove(chosenPassiveItemUpgrade);
        return chosenPassiveItemUpgrade;
    }

    int FindPassiveItemSlotIndex(PassiveItemScriptableObject passiveItemData)
    {
        for (int i = 0; i < passiveItemSlots.Count; i++)
        {
            if (passiveItemSlots[i] != null && passiveItemSlots[i].passiveItemData == passiveItemData)
            {
                return i;
            }
        }

        return -1;
    }

    void ConfigureExistingPassiveItemUpgrade(UpgradeUI upgradeOption, PassiveItemUpgrade chosenPassiveItemUpgrade, int slotIndex)
    {
        if (!chosenPassiveItemUpgrade.passiveItemData.NextLevelPrefab)
        {
            DisableUpgradeUI(upgradeOption);
            return;
        }

        int capturedSlotIndex = slotIndex;
        PassiveItemUpgrade capturedUpgrade = chosenPassiveItemUpgrade;

        upgradeOption.upgradeButton.onClick.AddListener(() => LevelUpPassiveItem(capturedSlotIndex, capturedUpgrade));
        upgradeOption.upgradeDescriptionDisplay.text =
            chosenPassiveItemUpgrade.passiveItemData.NextLevelPrefab.GetComponent<PassiveItem>().passiveItemData.Description;
        upgradeOption.upgradeNameDisplay.text =
            chosenPassiveItemUpgrade.passiveItemData.NextLevelPrefab.GetComponent<PassiveItem>().passiveItemData.Name;
    }

    void ConfigureNewPassiveItemUpgrade(UpgradeUI upgradeOption, PassiveItemUpgrade chosenPassiveItemUpgrade)
    {
        GameObject initialPassiveItem = chosenPassiveItemUpgrade.initialPassiveItem;

        upgradeOption.upgradeButton.onClick.AddListener(() => player.SpawnPassiveItem(initialPassiveItem));
        upgradeOption.upgradeDescriptionDisplay.text = chosenPassiveItemUpgrade.passiveItemData.Description;
        upgradeOption.upgradeNameDisplay.text = chosenPassiveItemUpgrade.passiveItemData.Name;
    }

    void RemoveUpgradeOptions()
    {
        foreach (UpgradeUI upgradeOption in upgradeUIOptions)
        {
            upgradeOption.upgradeButton.onClick.RemoveAllListeners();
            DisableUpgradeUI(upgradeOption);
        }
    }

    // public void RemoveAndApplyUpgrades()
    // {
    //     RemoveUpgradeOptions();
    //     ApplyUpgradeOptions();
    // }

    void EnableUpgradeUI(UpgradeUI ui)
    {
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(true);
    }

    void DisableUpgradeUI(UpgradeUI ui)
    {
        ui.upgradeNameDisplay.transform.parent.gameObject.SetActive(false);
    }

    public List<WeaponEvolutionBlueprint> GetPossibleEvolutions()
    {
        List<WeaponEvolutionBlueprint> possibleEvolutions = new List<WeaponEvolutionBlueprint>();

        foreach (WeaponController weapon in weaponSlots)
        {
            if (weapon != null)
            {
                foreach (PassiveItem catalyst in passiveItemSlots)
                {
                    if (catalyst != null)
                    {
                        foreach (WeaponEvolutionBlueprint evolution in weaponEvolutions)
                        {
                            if (weapon.weaponData.Level >= evolution.weaponToEvolveData.Level &&
                               catalyst.passiveItemData.Level >= evolution.catalystPassiveItemData.Level)
                            {
                                possibleEvolutions.Add(evolution);
                            }
                        }
                    }
                }
            }
        }

        return possibleEvolutions;
    }

    public void EvolveWeapon(WeaponEvolutionBlueprint evoulution)
    {
        for (int weaponsSlotIndex = 0; weaponsSlotIndex < weaponSlots.Count; weaponsSlotIndex++)
        {
            WeaponController weapon = weaponSlots[weaponsSlotIndex];

            if (!weapon) continue;

            for (int passiveItemSlotIndex = 0; passiveItemSlotIndex < passiveItemSlots.Count; passiveItemSlotIndex++)
            {
                PassiveItem catalyst = passiveItemSlots[passiveItemSlotIndex];

                if (!catalyst) continue;

                if (weapon != null && catalyst != null &&
                   weapon.weaponData.Level >= evoulution.weaponToEvolveData.Level &&
                   catalyst.passiveItemData.Level >= evoulution.catalystPassiveItemData.Level)
                {
                    GameObject evolvedWeapon = Instantiate(evoulution.evolvedWeapon, transform.position, Quaternion.identity);
                    WeaponController evolvedWeaponController = evolvedWeapon.GetComponent<WeaponController>();

                    evolvedWeapon.transform.SetParent(transform);
                    AddWeapon(weaponsSlotIndex, evolvedWeaponController);
                    Destroy(weapon.gameObject);

                    weaponLevels[weaponsSlotIndex] = evolvedWeaponController.weaponData.Level;
                    weaponUISlots[weaponsSlotIndex].sprite = evolvedWeaponController.weaponData.Icon;

                    weaponUpgradeOptions.RemoveAt(evolvedWeaponController.weaponData.EvolvedUpgradeToRemove);

                    return;
                }
            }
        }
    }
}
