using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEditor.Events;

#if UNITY_EDITOR
[ExcludeFromCodeCoverage]
[DisallowMultipleComponent]
[CustomEditor(typeof(UICharacterSelector))]
public class UICharacterSelectorEditor : Editor
{
    UICharacterSelector selector;

    void OnEnable()
    {
        selector = target as UICharacterSelector;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Selectable Characters"))
        {
            CreateTogglesForCharacterData();
        }
    }

    public void CreateTogglesForCharacterData()
    {
        if (!selector.toggleTemplate)
        {
            Debug.LogWarning("Toggle template is not assigned.");
            return;
        }

        for (int i = selector.toggleTemplate.transform.parent.childCount - 1; i >= 0; i--)
        {
            Toggle toggle = selector.toggleTemplate.transform.parent.GetChild(i).GetComponent<Toggle>();
            if (toggle == selector.toggleTemplate) continue;
            Undo.DestroyObjectImmediate(toggle.gameObject);
        }

        Undo.RecordObject(selector, "Updates to UICharacterSelector.");
        selector.selectableToggles.Clear();
        CharacterData[] characters = UICharacterSelector.GetAllCharacterDataAssets();

        for (int i = 0; i < characters.Length; i++)
        {
            Toggle toggle;
            if (i == 0)
            {
                toggle = selector.toggleTemplate;
                Undo.RecordObject(toggle, "Modifying the template.");
            }
            else
            {
                toggle = Instantiate(selector.toggleTemplate, selector.toggleTemplate.transform.parent);
                Undo.RegisterCreatedObjectUndo(toggle.gameObject, "Creating a new toggle.");
            }

            Transform characterName = toggle.transform.Find(selector.characterNamePath);
            if (characterName && characterName.TryGetComponent(out TextMeshProUGUI nameText))
            {
                nameText.text = toggle.gameObject.name = characters[i].Name;
            }

            Transform characterIcon = toggle.transform.Find(selector.characterIconPath);
            if (characterIcon && characterIcon.TryGetComponent(out Image iconImage))
            {
                iconImage.sprite = characters[i].Icon;
            }

            Transform weaponIcon = toggle.transform.Find(selector.weaponIconPath);
            if (weaponIcon && weaponIcon.TryGetComponent(out Image weaponImage))
            {
                weaponImage.sprite = characters[i].StartingWeapon.icon;
            }

            selector.selectableToggles.Add(toggle);

            for (int j = 0; j < toggle.onValueChanged.GetPersistentEventCount(); j++)
            {
                if (toggle.onValueChanged.GetPersistentMethodName(j) == "Select")
                {
                    UnityEventTools.RemovePersistentListener(toggle.onValueChanged, j);
                }
            }
            UnityEventTools.AddObjectPersistentListener(toggle.onValueChanged, selector.Select, characters[i]);
        }

        EditorUtility.SetDirty(selector);
    }
}
#endif