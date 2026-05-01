using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    float currentEventCooldown = 0;

    public EventData[] events;

    [Tooltip("How long to wait before this becomes active")]
    public float firstTriggerDelay = 180f;

    [Tooltip("How long to wait between each event (in seconds)")]
    public float triggerInterval = 30f;

    public static EventManager instance;

    [System.Serializable]
    public class Event
    {
        public EventData data;
        public float duration, cooldown = 0;
    }
    List<Event> runningEvents = new List<Event>();

    PlayerStats[] allPlayers;

    void Start()
    {
        if (instance) Debug.LogWarning("There is more than one Event Manager in the scene! Please remove the extra Event Managers.");
        instance = this;
        currentEventCooldown = firstTriggerDelay > 0 ? firstTriggerDelay : triggerInterval;
        allPlayers = FindObjectsByType<PlayerStats>(FindObjectsSortMode.None);
    }

    void Update()
    {
        currentEventCooldown -= Time.deltaTime;
        if (currentEventCooldown <= 0)
        {
            EventData eventData = GetRandomEvent();
            if (eventData && eventData.CheckIfWillHappen(allPlayers[Random.Range(0, allPlayers.Length)]))
            {
                runningEvents.Add(new Event() { data = eventData, duration = eventData.duration });
            }

            currentEventCooldown = triggerInterval;
        }

        List<Event> toRemove = new List<Event>();

        foreach (Event e in runningEvents)
        {
            e.duration -= Time.deltaTime;
            if (e.duration <= 0)
            {
                toRemove.Add(e);
                continue;
            }

            e.cooldown -= Time.deltaTime;
            if (e.cooldown <= 0)
            {
                e.data.Active(allPlayers[Random.Range(0, allPlayers.Length)]);
                e.cooldown = e.data.GetSpawnInterval();
            }
        }

        foreach (Event e in toRemove)
        {
            runningEvents.Remove(e);
        }
    }

    public EventData GetRandomEvent()
    {
        if (events.Length <= 0) return null;

        List<EventData> possibleEvents = new List<EventData>(events);

        foreach (EventData e in events)
        {
            if (e.isActive()) possibleEvents.Add(e);
        }
        if (possibleEvents.Count > 0)
        {
            EventData result = possibleEvents[Random.Range(0, possibleEvents.Count)];
            return result;
        }
        return null;
    }
}
