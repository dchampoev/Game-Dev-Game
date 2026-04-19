using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for both the Passive and the Weapon classes. It is primarily intented
/// to handle weapon evoulutions.
/// </summary>
public class Item : MonoBehaviour
{
    public int currentLevel = 1, maxLevel = 1;
    protected ItemData.Evolution[] evolutionData;
    protected PlayerInventory inventory;

    protected PlayerStats owner;
    public PlayerStats Owner => owner;

    public virtual void Initialize(ItemData data)
    {
        maxLevel = data.maxLevel;

        evolutionData = data.evolutionData;

        inventory = FindAnyObjectByType<PlayerInventory>();
        owner = FindAnyObjectByType<PlayerStats>();
    }

    public virtual ItemData.Evolution[] CanEvolve()
    {
        List<ItemData.Evolution> possibleEvolutions = new List<ItemData.Evolution>();

        foreach (ItemData.Evolution evolution in evolutionData)
        {
            if (CanEvolve(evolution)) possibleEvolutions.Add(evolution);
        }
        
        return possibleEvolutions.ToArray();
    }

    public virtual bool CanEvolve(ItemData.Evolution evolution, int levelUpAmount = 1)
    {
        if (evolution.evolutionLevel > currentLevel + levelUpAmount)
        {
            Debug.LogWarning(string.Format("Evolution failed. Current level {0}, evolution level {1}", currentLevel, evolution.evolutionLevel));
            return false;
        }

        foreach (ItemData.Evolution.Config catalyst in evolution.catalysts)
        {
            Item item = inventory.Get(catalyst.itemType);
            if (!item || item.currentLevel < catalyst.level)
            {
                Debug.LogWarning(string.Format("Evolution failed. Missing {0}", catalyst.itemType.name));
                return false;
            }
        }
        return true;
    }

    public virtual bool AttemptEvolution(ItemData.Evolution evolutionData, int levelUpAmount = 1)
    {
        if (!CanEvolve(evolutionData, levelUpAmount)) return false;

        bool consumePassives = (evolutionData.consumes & ItemData.Evolution.Consumption.passives) > 0;
        bool consumeWeapons = (evolutionData.consumes & ItemData.Evolution.Consumption.weapons) > 0;

        foreach (ItemData.Evolution.Config catalyst in evolutionData.catalysts)
        {
            if (catalyst.itemType is PassiveData && consumePassives)
            {
                inventory.Remove(catalyst.itemType, true);
            }
            else if (catalyst.itemType is WeaponData && consumeWeapons)
            {
                inventory.Remove(catalyst.itemType, false);
            }
        }

        if (this is Passive && consumePassives) inventory.Remove((this as Passive).data, true);
        else if (this is Weapon && consumeWeapons) inventory.Remove((this as Weapon).data, true);

        inventory.Add(evolutionData.outcome.itemType);

        return true;
    }

    public virtual bool CanLevelUp()
    {
        return currentLevel < maxLevel;
    }

    public virtual bool DoLevelUp()
    {
        if(evolutionData==null || evolutionData.Length == 0) return true;

        foreach (ItemData.Evolution evolution in evolutionData)
        {
            if (evolution.condition == ItemData.Evolution.Condition.auto)
            {
                AttemptEvolution(evolution);
            }
        }
        return true;
    }

    public virtual void OnEquip() { }

    public virtual void OnUnequip() { }
}
