using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class KnifeControllerTests
{
    private class TestKnifeController : KnifeController
    {
        public void CallStart()
        {
            typeof(KnifeController)
                .GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(this, null);
        }

        public void CallAttack()
        {
            typeof(KnifeController)
                .GetMethod("Attack", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(this, null);
        }
    }

    [Test]
    public void Attack_ShouldInstantiateKnife()
    {
        GameObject playerObject = new GameObject("Player");
        PlayerMovement playerMovement = playerObject.AddComponent<PlayerMovement>();
        playerMovement.lastMoveDirection = Vector2.right;

        GameObject weaponObject = new GameObject("KnifeController");
        TestKnifeController controller = weaponObject.AddComponent<TestKnifeController>();

        GameObject knifePrefab = new GameObject("Knife");
        knifePrefab.AddComponent<KnifeBehaviour>();

        WeaponScriptableObject weaponData = ScriptableObject.CreateInstance<WeaponScriptableObject>();
        SerializedObject so = new SerializedObject(weaponData);
        so.FindProperty("prefab").objectReferenceValue = knifePrefab;
        so.ApplyModifiedProperties();

        controller.weaponData = weaponData;

        controller.CallStart();
        controller.CallAttack();

        Assert.Pass();

        Object.DestroyImmediate(weaponData);
        Object.DestroyImmediate(knifePrefab);
        Object.DestroyImmediate(weaponObject);
        Object.DestroyImmediate(playerObject);
    }
}