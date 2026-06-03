using UnityEngine;

/// <summary>
/// A specialised PassiveData class called PowerUpData, for power-ups that are purchased
/// from the Power Ups screen in between games.
/// </summary>

[CreateAssetMenu(fileName = "Power Up Data", menuName = "2D Rogue-like/Power Up Data")]
public class PowerUpData : PassiveData
{
    public float baseCost = 200f, baseFee = 20f, feeFactor = 1.1f;

    public virtual float GetCost(int level)
    {
        return GetCost(level, 0);
    }

    public virtual float GetCost(int level, int totalBoughtLevels)
    {
        int boughtLevels = Mathf.Max(0, level - 1);
        return GetBaseCost(boughtLevels) + GetFeeCost(totalBoughtLevels);
    }

    public virtual float GetBaseCost(int boughtLevels)
    {
        if (boughtLevels < 0)
            return 0;
        return Mathf.Floor(baseCost * (1 + boughtLevels));
    }

    public virtual float GetFeeCost(int totalBoughtLevels)
    {
        if (totalBoughtLevels <= 0)
            return 0;
        return Mathf.Floor(baseFee * Mathf.Pow(feeFactor, totalBoughtLevels - 1));
    }

    public override Item.LevelData GetLevelData(int level)
    {
        if (level <= 1)
            return baseStats;

        if (level - 2 < growth.Length)
            return growth[level - 2];

        Debug.LogWarning($"Power Up {name} has no level data for level {level}.");
        return new Passive.Modifier();
    }
}
