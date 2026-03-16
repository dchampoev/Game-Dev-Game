using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BreakablePropsPlayModeTest
{
    [UnityTest]
    public IEnumerator TakeDamage_WhenHealthBelowZero_ShouldDestroy()
    {
        GameObject obj = new GameObject();
        var props = obj.AddComponent<BreakableProps>();

        props.health = 1;

        props.TakeDamage(2);

        yield return null;

        Assert.IsTrue(obj == null);
    }
}
