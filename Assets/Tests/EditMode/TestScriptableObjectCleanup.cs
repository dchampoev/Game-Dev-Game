using UnityEditor;
using UnityEngine;

public static class TestScriptableObjectCleanup
{
    public static void DestroyRuntimeObjects<T>() where T : ScriptableObject
    {
        foreach (T obj in Resources.FindObjectsOfTypeAll<T>())
        {
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(obj)))
                continue;

            Object.DestroyImmediate(obj);
        }
    }
}
