using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerInventory inventory = collision.GetComponent<PlayerInventory>();
        if (inventory)
        {
            bool randomBool = Random.Range(0, 2) == 0;

            OpenTreasureChest(inventory, randomBool);
            Destroy(gameObject);
        }
    }

    public void OpenTreasureChest(PlayerInventory inventory, bool isHigherTier)
    {
        foreach (PlayerInventory.Slot slot in inventory.weaponSlots)
        {
            Weapon weapon = slot.item as Weapon;
            if (weapon.data.evolutionData == null) continue;
            
            foreach(ItemData.Evolution evolution in weapon.data.evolutionData)
            {
                if (evolution.condition == ItemData.Evolution.Condition.treasureChest)
                {
                    bool attempt = weapon.AttemptEvolution(evolution, 0);
                    if (attempt) return;
                }
            }
        }
    }
}
