using NUnit.Framework;
using UnityEngine;

public class PassiveItemTests
{
    private class TestPassiveItem : PassiveItem
    {
        public bool modifierApplied = false;

        protected override void ApplyModifier()
        {
            modifierApplied = true;
        }

        public void CallStart()
        {
            typeof(PassiveItem)
                .GetMethod("Start", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(this, null);
        }
    }

    [Test]
    public void Start_ShouldApplyModifier()
    {
        GameObject playerObject = new GameObject("Player");
        playerObject.AddComponent<PlayerStats>();

        GameObject passiveObject = new GameObject("Passive");
        TestPassiveItem passiveItem = passiveObject.AddComponent<TestPassiveItem>();

        passiveItem.CallStart();

        Assert.IsTrue(passiveItem.modifierApplied);

        Object.DestroyImmediate(passiveObject);
        Object.DestroyImmediate(playerObject);
    }
}