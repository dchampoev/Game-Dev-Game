using UnityEngine;

public class HealthPotion : Pickup, ICollectable
{
    public int healthToRestore;

    public void Collect()
    {
        PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.Heal(healthToRestore);
        }
    }
}
