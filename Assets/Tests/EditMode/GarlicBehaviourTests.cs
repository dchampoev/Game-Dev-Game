using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class GarlicBehaviourTests
{
    private class TestGarlicBehaviour : GarlicBehaviour
    {
        public void InitializeMarkedEnemies()
        {
            typeof(GarlicBehaviour)
                .GetField("markedEnemies", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(this, new List<GameObject>());
        }

        public List<GameObject> GetMarkedEnemies()
        {
            return (List<GameObject>)typeof(GarlicBehaviour)
                .GetField("markedEnemies", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(this);
        }

        public void CallOnTriggerEnter2D(Collider2D collider)
        {
            typeof(GarlicBehaviour)
                .GetMethod("OnTriggerEnter2D", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(this, new object[] { collider });
        }
    }

    [Test]
    public void OnTriggerEnter2D_WhenEnemy_ShouldAddEnemyToMarkedEnemies()
    {
        GameObject garlicObject = new GameObject("Garlic");
        TestGarlicBehaviour garlic = garlicObject.AddComponent<TestGarlicBehaviour>();
        garlic.InitializeMarkedEnemies();

        GameObject enemyObject = new GameObject("Enemy");
        enemyObject.tag = "Enemy";
        BoxCollider2D enemyCollider = enemyObject.AddComponent<BoxCollider2D>();

        EnemyStats enemyStats = enemyObject.AddComponent<EnemyStats>();
        enemyStats.currentHealth = 10f;

        garlic.CallOnTriggerEnter2D(enemyCollider);

        Assert.AreEqual(1, garlic.GetMarkedEnemies().Count);
        Assert.AreEqual(enemyObject, garlic.GetMarkedEnemies()[0]);

        Object.DestroyImmediate(enemyObject);
        Object.DestroyImmediate(garlicObject);
    }

    [Test]
    public void OnTriggerEnter2D_WhenProp_ShouldAddPropToMarkedEnemies()
    {
        GameObject garlicObject = new GameObject("Garlic");
        TestGarlicBehaviour garlic = garlicObject.AddComponent<TestGarlicBehaviour>();
        garlic.InitializeMarkedEnemies();

        GameObject propObject = new GameObject("Prop");
        propObject.tag = "Prop";
        BoxCollider2D propCollider = propObject.AddComponent<BoxCollider2D>();
        BreakableProps breakable = propObject.AddComponent<BreakableProps>();
        breakable.health = 10f;

        garlic.CallOnTriggerEnter2D(propCollider);

        Assert.AreEqual(1, garlic.GetMarkedEnemies().Count);
        Assert.AreEqual(propObject, garlic.GetMarkedEnemies()[0]);

        Object.DestroyImmediate(propObject);
        Object.DestroyImmediate(garlicObject);
    }

    [Test]
    public void OnTriggerEnter2D_WhenSamePropHitsTwice_ShouldNotAddItTwice()
    {
        GameObject garlicObject = new GameObject("Garlic");
        TestGarlicBehaviour garlic = garlicObject.AddComponent<TestGarlicBehaviour>();
        garlic.InitializeMarkedEnemies();

        GameObject propObject = new GameObject("Prop");
        propObject.tag = "Prop";
        BoxCollider2D propCollider = propObject.AddComponent<BoxCollider2D>();
        BreakableProps breakable = propObject.AddComponent<BreakableProps>();
        breakable.health = 10f;

        garlic.CallOnTriggerEnter2D(propCollider);
        garlic.CallOnTriggerEnter2D(propCollider);

        Assert.AreEqual(1, garlic.GetMarkedEnemies().Count);

        Object.DestroyImmediate(propObject);
        Object.DestroyImmediate(garlicObject);
    }
}