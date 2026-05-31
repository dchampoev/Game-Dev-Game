using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    const float DefaultEvolutionUnlockTime = 600f;

    [System.Flags]
    public enum DropType
    {
        NewPassive = 1, NewWeapon = 2, UpgradePassive = 4,
        UpgradeWeapon = 8, Evolution = 16
    }
    public DropType possibleDrops = (DropType)~0;

    public enum DropCountType { sequential, random }
    public DropCountType dropCountType = DropCountType.sequential;
    public TreasureChestDropProfile[] dropProfiles;
    public static int totalPickups = 0;
    int currentDropProfileIndex = 0;
    public Sprite defaultDropSprite;
    public float evolutionUnlockTime = DefaultEvolutionUnlockTime;

    PlayerInventory recipient;
    bool evolvedWeaponThisChest;

    public TreasureChestDropProfile GetCurrentDropProfile()
    {
        if (dropProfiles == null || dropProfiles.Length == 0)
        {
            Debug.LogWarning("No drop profiles assigned to the treasure chest.");
            return null;
        }

        return dropProfiles[currentDropProfileIndex];
    }

    public TreasureChestDropProfile GetNextDropProfile()
    {
        if (dropProfiles == null || dropProfiles.Length == 0)
        {
            Debug.LogWarning("No drop profiles assigned to the treasure chest.");
            return null;
        }

        switch (dropCountType)
        {
            case DropCountType.sequential:
                currentDropProfileIndex = Mathf.Clamp(
                    totalPickups, 0, dropProfiles.Length - 1
                );
                break;

            case DropCountType.random:
                PlayerStats playerStats = recipient ? recipient.GetComponentInChildren<PlayerStats>() : null;
                float playerLuck = playerStats ? playerStats.Actual.luck : 1f;

                List<(int index, TreasureChestDropProfile profile, float weight)> weightedProfiles = new List<(int, TreasureChestDropProfile, float)>();
                for (int i = 0; i < dropProfiles.Length; i++)
                {
                    if (!dropProfiles[i])
                        continue;

                    float weight = dropProfiles[i].baseDropChance * (1 + dropProfiles[i].luckScaling * (playerLuck - 1));
                    weightedProfiles.Add((i, dropProfiles[i], weight));
                }

                if (weightedProfiles.Count == 0)
                    break;

                weightedProfiles.Sort((a, b) => a.weight.CompareTo(b.weight));

                float totalWeight = 0f;
                foreach (var entry in weightedProfiles)
                    totalWeight += entry.weight;
                if (totalWeight <= 0f)
                    break;

                float randomValue = Random.Range(0f, totalWeight);
                float cumulativeWeight = 0f;
                foreach (var entry in weightedProfiles)
                {
                    cumulativeWeight += entry.weight;
                    if (randomValue <= cumulativeWeight)
                    {
                        currentDropProfileIndex = entry.index;
                        return entry.profile;
                    }
                }
                break;
        }

        return GetCurrentDropProfile();
    }

    private int GetRewardCount()
    {
        TreasureChestDropProfile profile = GetNextDropProfile();
        if (profile)
            return profile.numberOfItems;
        return 1;
    }

    bool CanAwardEvolution()
    {
        if (evolvedWeaponThisChest)
            return false;
        if (!GameManager.instance)
            return evolutionUnlockTime <= 0f;

        return GameManager.instance.GetElapsedTime() >= evolutionUnlockTime;
    }

    T TryEvolve<T>(PlayerInventory inventory, bool updateUI = true) where T : Item
    {
        if (!CanAwardEvolution())
            return null;

        T[] evolvables = inventory.GetEvolvables<T>();
        foreach (T item in evolvables)
        {
            ItemData.Evolution[] possibleEvolutions = item.CanEvolve(0);
            foreach (ItemData.Evolution evolution in possibleEvolutions)
            {
                if (evolution.condition != ItemData.Evolution.Condition.treasureChest)
                    continue;

                if (item.AttemptEvolution(evolution, 0, updateUI))
                {
                    evolvedWeaponThisChest = true;
                    UITreasureChest.NotifyItemReceived(evolution.outcome.itemType.icon);
                    return item as T;
                }
            }
        }
        return null;
    }

    T TryUpgrade<T>(PlayerInventory inventory, bool updateUI = true) where T : Item
    {
        T[] upgradables = inventory.GetUpgradables<T>();
        if (upgradables.Length == 0)
            return null;

        T t = upgradables[Random.Range(0, upgradables.Length)];
        inventory.LevelUp(t, updateUI);
        UITreasureChest.NotifyItemReceived(t.data.icon);
        return t;
    }

    T TryGive<T>(PlayerInventory inventory, bool updateUI = true) where T : ItemData
    {
        if (inventory.GetSlotsLeftFor<T>() <= 0)
            return null;

        T[] possibilities = inventory.GetUnowned<T>();
        if (possibilities.Length == 0)
            return null;

        T t = possibilities[Random.Range(0, possibilities.Length)];
        inventory.Add(t, updateUI);
        UITreasureChest.NotifyItemReceived(t.icon);
        return t;
    }

    public void NotifyComplete()
    {
        if (!recipient)
            return;

        if (recipient.weaponUI)
            recipient.weaponUI.Refresh();
        if (recipient.passiveUI)
            recipient.passiveUI.Refresh();
    }

    public void OpenTreasureChest(PlayerInventory inventory, bool updateUI = true)
    {
        Open(inventory, updateUI);
    }

    void Open(PlayerInventory inventory, bool updateUI = true)
    {
        if (inventory == null)
            return;

        if (possibleDrops.HasFlag(DropType.Evolution) && TryEvolve<Weapon>(inventory, updateUI))
            return;
        if (possibleDrops.HasFlag(DropType.UpgradeWeapon) && TryUpgrade<Weapon>(inventory, updateUI))
            return;
        if (possibleDrops.HasFlag(DropType.UpgradePassive) && TryUpgrade<Passive>(inventory, updateUI))
            return;
        if (possibleDrops.HasFlag(DropType.NewWeapon) && TryGive<WeaponData>(inventory, updateUI))
            return;
        if (possibleDrops.HasFlag(DropType.NewPassive) && TryGive<PassiveData>(inventory, updateUI))
            return;
        if (defaultDropSprite)
            UITreasureChest.NotifyItemReceived(defaultDropSprite);
        return;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerInventory player))
        {
            recipient = player;
            evolvedWeaponThisChest = false;

            int rewardCount = GetRewardCount();
            for (int i = 0; i < rewardCount; i++)
                Open(player, false);
            gameObject.SetActive(false);

            UITreasureChest.Activate(player.GetComponentInChildren<PlayerCollector>(), this);

            if (dropProfiles != null && dropProfiles.Length > 0)
            {
                totalPickups = (totalPickups + 1) % (dropProfiles.Length + 1);
            }
        }
    }
}
