using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[ExcludeFromCodeCoverage]
[DisallowMultipleComponent]
[CustomEditor(typeof(UILevelSelector))]
public class UILevelSelectorEditor : Editor
{
    UILevelSelector selector;

    void OnEnable()
    {
        selector = target as UILevelSelector;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!selector.toggleTemplate)
        {
            EditorGUILayout.HelpBox(
                "You need to assign a Toggle Template for the button to work properly.",
                MessageType.Warning
            );
        }

        if (GUILayout.Button("Find and Populate Levels"))
        {
            PopulateLevelsList();
            CreateLevelSelectToggles();
        }
    }

    public void PopulateLevelsList()
    {
        Undo.RecordObject(selector, "Create New SceneData structs");
        SceneAsset[] maps = UILevelSelector.GetAllMaps();

        selector.levels.RemoveAll(levels => levels.scene == null);

        foreach (SceneAsset map in maps)
        {
            if (!selector.levels.Any(sceneData => sceneData.scene == map))
            {
                Match m = Regex.Match(map.name, UILevelSelector.MAP_NAME_FORMAT, RegexOptions.IgnoreCase);
                string mapLabel = "Level", mapName = "New Map";
                if (m.Success)
                {
                    if (m.Groups.Count > 1) mapLabel = m.Groups[1].Value;
                    if (m.Groups.Count > 2) mapName = m.Groups[2].Value;
                }

                selector.levels.Add(new UILevelSelector.SceneData
                {
                    scene = map,
                    sceneName = map.name,
                    label = mapLabel,
                    displayName = mapName
                });
            }
            else
            {
                UILevelSelector.SceneData existingLevel = selector.levels.First(sceneData => sceneData.scene == map);
                existingLevel.sceneName = map.name;
            }
        }
    }

    public void CreateLevelSelectToggles()
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

        Undo.RecordObject(selector, "Updates to UILevelSelector.");
        selector.selectableToggles.Clear();

        for (int i = 0; i < selector.levels.Count; i++)
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
                Undo.RegisterCreatedObjectUndo(toggle.gameObject, "Created a new toggle.");
            }

            toggle.gameObject.name = selector.levels[i].SceneName;

            Transform levelName = toggle.transform.Find(selector.levelImagePath).Find("Name Holder").Find(selector.levelNamePath);
            if (levelName && levelName.TryGetComponent(out TextMeshProUGUI nameText))
            {
                nameText.text = selector.levels[i].displayName;
            }

            Transform levelNumber = toggle.transform.Find(selector.levelImagePath).Find(selector.levelNumberPath);
            if (levelNumber && levelNumber.TryGetComponent(out TextMeshProUGUI numberText))
            {
                numberText.text = selector.levels[i].label;
            }

            Transform levelDescription = toggle.transform.Find(selector.levelDiscriptionPath);
            if (levelDescription && levelDescription.TryGetComponent(out TextMeshProUGUI descriptionText))
            {
                descriptionText.text = selector.levels[i].description;
            }

            Transform levelImage = toggle.transform.Find(selector.levelImagePath);
            if (levelImage && levelImage.TryGetComponent(out Image image))
            {
                image.sprite = selector.levels[i].icon;
            }

            selector.selectableToggles.Add(toggle);
        }

        EditorUtility.SetDirty(selector);
    }
}
#endif
