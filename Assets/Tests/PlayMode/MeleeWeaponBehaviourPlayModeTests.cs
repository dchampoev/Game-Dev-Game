using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;

public class MeleeWeaponBehaviourPlayModeTests
{
    [UnityTest]
    public IEnumerator Start_ShouldDestroyObjectAfterLifetime()
    {
        GameObject weaponObject = new GameObject("MeleeWeapon");
        MeleeWeaponBehaviour behaviour = weaponObject.AddComponent<MeleeWeaponBehaviour>();

        WeaponScriptableObject weaponData = ScriptableObject.CreateInstance<WeaponScriptableObject>();
        var so = new SerializedObject(weaponData);
        so.FindProperty("damage").floatValue = 1f;
        so.FindProperty("speed").floatValue = 1f;
        so.FindProperty("cooldownDuration").floatValue = 1f;
        so.FindProperty("pierce").intValue = 1;
        so.ApplyModifiedProperties();

        behaviour.weaponData = weaponData;
        behaviour.InitializeStats();
        behaviour.lifetimeSeconds = 0.01f;

        yield return new WaitForSeconds(0.05f);

        Assert.IsTrue(weaponObject == null);

        Object.Destroy(weaponData);
    }
}