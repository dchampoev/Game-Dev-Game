using NUnit.Framework;
using UnityEngine;
using UnityEditor;

public class WeaponControllerTests
{
    private class TestWeaponController : WeaponController
    {
        public int attackCalls = 0;

        public void CallStart() => Start();
        public void CallUpdate() => Update();

        protected override void Attack()
        {
            attackCalls++;
            base.Attack();
        }
    }

    private WeaponScriptableObject CreateWeaponData(float cooldown)
    {
        WeaponScriptableObject data = ScriptableObject.CreateInstance<WeaponScriptableObject>();
        SerializedObject so = new SerializedObject(data);
        so.FindProperty("cooldownDuration").floatValue = cooldown;
        so.ApplyModifiedProperties();
        return data;
    }

    [Test]
    public void Start_ShouldInitializeCooldownFromWeaponData()
    {
        GameObject playerObject = new GameObject("PlayerMovement");
        playerObject.AddComponent<PlayerMovement>();

        GameObject weaponObject = new GameObject("Weapon");
        TestWeaponController controller = weaponObject.AddComponent<TestWeaponController>();
        controller.weaponData = CreateWeaponData(2f);

        controller.CallStart();

        Assert.AreEqual(0, controller.attackCalls);

        Object.DestroyImmediate(controller.weaponData);
        Object.DestroyImmediate(weaponObject);
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void Update_WhenCooldownExpires_ShouldCallAttack()
    {
        GameObject playerObject = new GameObject("PlayerMovement");
        playerObject.AddComponent<PlayerMovement>();

        GameObject weaponObject = new GameObject("Weapon");
        TestWeaponController controller = weaponObject.AddComponent<TestWeaponController>();
        controller.weaponData = CreateWeaponData(0f);

        controller.CallStart();
        controller.CallUpdate();

        Assert.AreEqual(1, controller.attackCalls);

        Object.DestroyImmediate(controller.weaponData);
        Object.DestroyImmediate(weaponObject);
        Object.DestroyImmediate(playerObject);
    }
}