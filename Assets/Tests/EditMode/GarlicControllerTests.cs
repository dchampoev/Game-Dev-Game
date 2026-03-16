using NUnit.Framework;
using UnityEngine;

public class GarlicControllerTests
{
    class TestGarlicController : GarlicController
    {
        public void CallAttack()
        {
            Attack();
        }
    }

    [Test]
    public void Attack_ShouldSpawnGarlic()
    {
        GameObject weaponObject = new GameObject("GarlicController");
        TestGarlicController controller = weaponObject.AddComponent<TestGarlicController>();

        GameObject garlicPrefab = new GameObject("Garlic");

        WeaponScriptableObject weaponData = ScriptableObject.CreateInstance<WeaponScriptableObject>();

        var so = new UnityEditor.SerializedObject(weaponData);
        so.FindProperty("prefab").objectReferenceValue = garlicPrefab;
        so.ApplyModifiedProperties();

        controller.weaponData = weaponData;

        controller.CallAttack();

        Assert.IsTrue(true);

        Object.DestroyImmediate(weaponObject);
        Object.DestroyImmediate(garlicPrefab);
        Object.DestroyImmediate(weaponData);
    }
}