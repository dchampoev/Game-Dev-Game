using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class EventManagerTests
{
    private class TestEventData : EventData
    {
        public bool activeResult = true;
        public int activeCallCount = 0;
        public PlayerStats lastPlayer;

        public override bool Active(PlayerStats player = null)
        {
            activeCallCount++;
            lastPlayer = player;
            return activeResult;
        }

        public override float GetSpawnInterval()
        {
            return 1f;
        }
    }

    private void CallStart(EventManager manager)
    {
        typeof(EventManager)
            .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(manager, null);
    }

    private void CallUpdate(EventManager manager)
    {
        typeof(EventManager)
            .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(manager, null);
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    private T GetPrivateField<T>(object target, string fieldName)
    {
        return (T)target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(target);
    }

    private PlayerStats CreatePlayer()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.SetActive(false);

        PlayerStats player = playerObject.AddComponent<PlayerStats>();
        player.enabled = false;

        player.Stats = new CharacterData.Stats
        {
            luck = 1f
        };

        playerObject.SetActive(true);

        return player;
    }

    private TestEventData CreateEvent(float duration = 5f)
    {
        TestEventData data = ScriptableObject.CreateInstance<TestEventData>();
        data.duration = duration;
        data.probality = 1f;
        data.spawnInterval = new Vector2(1f, 1f);
        return data;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(obj);
        }

        foreach (var data in Resources.FindObjectsOfTypeAll<TestEventData>())
        {
            Object.DestroyImmediate(data, true);
        }

        EventManager.instance = null;
    }

    [Test]
    public void Start_ShouldSetInstanceAndInitialCooldownToFirstTriggerDelay()
    {
        CreatePlayer();

        GameObject managerObject = new GameObject("EventManager");
        EventManager manager = managerObject.AddComponent<EventManager>();
        manager.firstTriggerDelay = 10f;
        manager.triggerInterval = 3f;
        manager.events = new EventData[0];

        CallStart(manager);

        float cooldown = GetPrivateField<float>(manager, "currentEventCooldown");

        Assert.AreSame(manager, EventManager.instance);
        Assert.AreEqual(10f, cooldown);
    }

    [Test]
    public void Start_WhenFirstTriggerDelayIsZero_ShouldUseTriggerInterval()
    {
        CreatePlayer();

        GameObject managerObject = new GameObject("EventManager");
        EventManager manager = managerObject.AddComponent<EventManager>();
        manager.firstTriggerDelay = 0f;
        manager.triggerInterval = 3f;
        manager.events = new EventData[0];

        CallStart(manager);

        float cooldown = GetPrivateField<float>(manager, "currentEventCooldown");

        Assert.AreEqual(3f, cooldown);
    }

    [Test]
    public void GetRandomEvent_WhenEventsArrayIsEmpty_ShouldReturnNull()
    {
        GameObject managerObject = new GameObject("EventManager");
        EventManager manager = managerObject.AddComponent<EventManager>();
        manager.events = new EventData[0];

        EventData result = manager.GetRandomEvent();

        Assert.IsNull(result);
    }

    [Test]
    public void GetRandomEvent_WhenEventsExist_ShouldReturnOneOfEvents()
    {
        GameObject managerObject = new GameObject("EventManager");
        EventManager manager = managerObject.AddComponent<EventManager>();

        TestEventData eventA = CreateEvent();
        TestEventData eventB = CreateEvent();

        manager.events = new EventData[]
        {
            eventA,
            eventB
        };

        EventData result = manager.GetRandomEvent();

        Assert.IsTrue(result == eventA || result == eventB);
    }

    [Test]
    public void Update_WhenCooldownExpires_ShouldAddRunningEvent()
    {
        CreatePlayer();

        GameObject managerObject = new GameObject("EventManager");
        EventManager manager = managerObject.AddComponent<EventManager>();

        TestEventData eventData = CreateEvent(duration: 5f);
        manager.events = new EventData[] { eventData };
        manager.triggerInterval = 10f;

        CallStart(manager);
        SetPrivateField(manager, "currentEventCooldown", 0f);

        CallUpdate(manager);

        List<EventManager.Event> runningEvents =
            GetPrivateField<List<EventManager.Event>>(manager, "runningEvents");

        Assert.AreEqual(1, runningEvents.Count);
        Assert.AreSame(eventData, runningEvents[0].data);
    }

    [Test]
    public void Update_WhenRunningEventCooldownExpires_ShouldCallActive()
    {
        PlayerStats player = CreatePlayer();

        GameObject managerObject = new GameObject("EventManager");
        EventManager manager = managerObject.AddComponent<EventManager>();
        manager.events = new EventData[0];

        TestEventData eventData = CreateEvent(duration: 5f);

        CallStart(manager);

        List<EventManager.Event> runningEvents =
            GetPrivateField<List<EventManager.Event>>(manager, "runningEvents");

        runningEvents.Add(new EventManager.Event
        {
            data = eventData,
            duration = 5f,
            cooldown = 0f
        });

        CallUpdate(manager);

        Assert.AreEqual(1, eventData.activeCallCount);
        Assert.AreSame(player, eventData.lastPlayer);
    }

    [Test]
    public void Update_WhenRunningEventDurationExpires_ShouldRemoveEvent()
    {
        CreatePlayer();

        GameObject managerObject = new GameObject("EventManager");
        EventManager manager = managerObject.AddComponent<EventManager>();
        manager.events = new EventData[0];

        TestEventData eventData = CreateEvent(duration: 1f);

        CallStart(manager);

        List<EventManager.Event> runningEvents =
            GetPrivateField<List<EventManager.Event>>(manager, "runningEvents");

        runningEvents.Add(new EventManager.Event
        {
            data = eventData,
            duration = 0f,
            cooldown = 0f
        });

        CallUpdate(manager);

        Assert.AreEqual(0, runningEvents.Count);
    }
}