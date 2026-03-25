using NUnit.Framework;
using UnityEngine;

public class ItemDoLevelUpTests
{
    private class TestItem : Item
    {
        public bool wasCalled = false;

        public override bool AttemptEvolution(ItemData.Evolution evo, int levelUpAmount = 1)
        {
            wasCalled = true;
            return true;
        }

        public void SetEvolutionData(ItemData.Evolution[] data)
        {
            evolutionData = data;
        }
    }

    [Test]
    public void DoLevelUp_WhenNoEvolutionData_ShouldReturnTrue()
    {
        GameObject go = new GameObject();
        TestItem item = go.AddComponent<TestItem>();

        item.SetEvolutionData(null);

        bool result = item.DoLevelUp();

        Assert.IsTrue(result);
    }

    [Test]
    public void DoLevelUp_WhenAutoEvolutionExists_ShouldCallAttemptEvolution()
    {
        GameObject go = new GameObject();
        TestItem item = go.AddComponent<TestItem>();

        item.SetEvolutionData(new[]
        {
            new ItemData.Evolution
            {
                condition = ItemData.Evolution.Condition.auto
            }
        });

        item.DoLevelUp();

        Assert.IsTrue(item.wasCalled);
    }
}