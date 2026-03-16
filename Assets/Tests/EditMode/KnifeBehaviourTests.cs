using NUnit.Framework;
using UnityEngine;
using System.Reflection;

public class KnifeBehaviourTests
{
    private class TestKnifeBehaviour : KnifeBehaviour
    {
        public void SetPrivateField(string fieldName, object value)
        {
            typeof(ProjectileWeaponBehaviour)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(this, value);
        }

        public void CallUpdate()
        {
            typeof(KnifeBehaviour)
                .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(this, null);
        }
    }

    [Test]
    public void Update_ShouldMoveKnifeForward()
    {
        GameObject knifeObject = new GameObject("Knife");
        TestKnifeBehaviour knife = knifeObject.AddComponent<TestKnifeBehaviour>();

        knife.SetPrivateField("travelDirection", Vector3.right);
        knife.SetPrivateField("currentSpeed", 10f);

        Vector3 startPosition = knife.transform.position;

        knife.CallUpdate();

        Assert.AreNotEqual(startPosition, knife.transform.position);

        Object.DestroyImmediate(knifeObject);
    }
}