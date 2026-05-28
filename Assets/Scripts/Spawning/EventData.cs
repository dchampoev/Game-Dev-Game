using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Serialization;

[ExcludeFromCodeCoverage]
public abstract class EventData : SpawnData
{
    [Header("Event Data")]
    [FormerlySerializedAs("probality")]
    [Range(0f, 1f)] public float probability = 1f;
    [Range(0f, 1f)] public float luckFactor = 1f;

    [Tooltip("If a value is specified, this event will occur after the level runs for this amount of time (in seconds).")]
    public float activeAfter = 0;

    public abstract bool Active(PlayerStats player = null);

    public bool isActive()
    {
        if (!GameManager.instance) return false;
        if (GameManager.instance.GetElapsedTime() > activeAfter) return true;
        return false;
    }

    public bool CheckIfWillHappen(PlayerStats player)
    {
        if (probability >= 1) return true;

        if (probability / Mathf.Max(1, (player.Stats.luck * luckFactor)) >= Random.Range(0f, 1f))
        {
            return true;
        }

        return false;
    }
}
