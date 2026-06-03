#if UNITY_EDITOR
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

[ExcludeFromCodeCoverage]
[CustomEditor(typeof(UIPowerUpSelector))]
public class UIPowerSelectorEditor : Editor
{
    const string ContentName = "Content";
    const string IconName = "Power Up Icon";
    const string NameTextName = "Power Up Name";

    UIPowerUpSelector selector;

    void OnEnable()
    {
        selector = target as UIPowerUpSelector;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate Selectable PowerUps"))
        {
            CreateTogglesForPowerUpData();
        }
    }

    public void CreateTogglesForPowerUpData()
    {
        Toggle toggleTemplate = selector.toggleTemplate;
        if (toggleTemplate == null)
        {
            Debug.LogWarning("Toggle template is not assigned. Please assign a toggle template before generating toggles.");
            return;
        }

        for (int i = selector.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = selector.transform.GetChild(i);
            Toggle toggle = child.GetComponent<Toggle>();
            if (toggle == toggleTemplate)
                continue;
            Undo.DestroyObjectImmediate(child.gameObject);
        }

        Undo.RecordObject(selector, "Updates to UIPowerSelector.");

        PowerUpData[] powerUps = UIPowerUpSelector.GetAllPowerUps();
        selector.selectableToggles.Clear();

        for (int i = 0; i < powerUps.Length; i++)
        {
            Toggle toggle;
            if (i == 0)
            {
                toggle = toggleTemplate;
                Undo.RecordObject(toggle, "Modifying the template.");
            }
            else
            {
                toggle = Instantiate(toggleTemplate, selector.transform);
                Undo.RegisterCreatedObjectUndo(toggle.gameObject, "Creating a new toggle.");
            }
            toggle.group = selector.GetComponent<ToggleGroup>();

            toggle.gameObject.name = powerUps[i].name;

            Transform content = toggle.transform.Find(ContentName);
            if (content != null)
            {
                Image icon = FindChildComponent<Image>(content, IconName);
                if (icon)
                {
                    Undo.RecordObject(icon, "Set power-up icon.");
                    icon.sprite = powerUps[i].icon;
                    EditorUtility.SetDirty(icon);
                }

                TextMeshProUGUI nameText = FindChildComponent<TextMeshProUGUI>(content, NameTextName);
                if (nameText)
                {
                    Undo.RecordObject(nameText, "Set power-up name.");
                    nameText.text = powerUps[i].name;
                    EditorUtility.SetDirty(nameText);
                }

                Transform tickBoxesContainer = content.Find(selector.tickContainerName);
                if (tickBoxesContainer != null)
                {
                    Transform boxTemplate = tickBoxesContainer.childCount > 0 ? tickBoxesContainer.GetChild(0) : null;

                    if (boxTemplate != null)
                    {
                        for (int j = tickBoxesContainer.childCount - 1; j >= 1; j--)
                        {
                            Undo.DestroyObjectImmediate(tickBoxesContainer.GetChild(j).gameObject);
                        }

                        int maxLevel = powerUps[i].maxLevel;

                        for (int j = 0; j < maxLevel; j++)
                        {
                            Transform box;
                            if (j == 0)
                            {
                                box = boxTemplate;
                                Undo.RecordObject(box, "Modifying tick box template.");
                            }
                            else
                            {
                                box = Instantiate(boxTemplate, tickBoxesContainer);
                                Undo.RegisterCreatedObjectUndo(box.gameObject, "Created tick box.");
                            }

                            box.gameObject.name = $"Box {j + 1}";

                            Transform tick = box.Find(selector.tickImageName);
                            if (tick != null)
                            {
                                tick.gameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Tick box template not found for power-up {powerUps[i].name}. Please ensure there is a tick box template in the tick container.");
                    }
                }
            }

            for (int j = toggle.onValueChanged.GetPersistentEventCount() - 1; j >= 0; j--)
            {
                if (toggle.onValueChanged.GetPersistentMethodName(j) == "Select")
                {
                    UnityEventTools.RemovePersistentListener(toggle.onValueChanged, j);
                }
            }
            UnityEventTools.AddObjectPersistentListener(toggle.onValueChanged, selector.Select, powerUps[i]);
            selector.selectableToggles.Add(toggle);
        }

        EditorUtility.SetDirty(selector);

        Debug.Log("Finished generating toggles for power-ups. Please review the generated toggles and make any necessary adjustments to the layout or styling.");
    }

    static T FindChildComponent<T>(Transform parent, string childName) where T : Component
    {
        Transform namedChild = parent.Find(childName);
        if (namedChild && namedChild.TryGetComponent(out T namedComponent))
            return namedComponent;

        return parent.GetComponentInChildren<T>(true);
    }
}
#endif
