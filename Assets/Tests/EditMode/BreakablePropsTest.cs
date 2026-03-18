using NUnit.Framework;
using UnityEngine;

public class BreakablePropsTests
{
    [Test]
    public void TakeDamage_ShouldReduceHealth()
    {
        GameObject obj = new GameObject("Breakable");
        BreakableProps props = obj.AddComponent<BreakableProps>();
        props.health = 10f;

        props.TakeDamage(3f);

        Assert.AreEqual(7f, props.health);

        Object.DestroyImmediate(obj);
    }
}