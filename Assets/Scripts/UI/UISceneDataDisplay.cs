using System;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

public class UISceneDataDisplay : UIPropertyDisplay
{
    public UILevelSelector levelSelector;
    TextMeshProUGUI extraStageInfo;

    public override object GetReadObject()
    {
        if (levelSelector && UILevelSelector.selectedLevel >= 0)
        {
            return levelSelector.levels[UILevelSelector.selectedLevel];
        }
        return new UILevelSelector.SceneData();
    }

    public override void UpdateFields()
    {
        if (!propertyNames && transform.childCount > 0)
            propertyNames = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        if (!propertyValues && transform.childCount > 1)
            propertyValues = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        if (!extraStageInfo && transform.childCount > 2)
            extraStageInfo = transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        StringBuilder[] allStats = GetProperties(
            BindingFlags.Public | BindingFlags.Instance,
            typeof(UILevelSelector.SceneData)
        );

        UILevelSelector.SceneData data = (UILevelSelector.SceneData)GetReadObject();

        allStats[0].AppendLine("Move Speed").AppendLine("Gold Bonus").AppendLine("Luck Bonus").AppendLine("XP Bonus").AppendLine("Enemy Health");

        Type characterDataStats = typeof(CharacterData.Stats);
        ProcessValue(data.playerModifier.moveSpeed, allStats[1], characterDataStats.GetField("moveSpeed"));
        ProcessValue(data.playerModifier.greed, allStats[1], characterDataStats.GetField("greed"));
        ProcessValue(data.playerModifier.luck, allStats[1], characterDataStats.GetField("luck"));
        ProcessValue(data.playerModifier.growth, allStats[1], characterDataStats.GetField("growth"));

        Type enemyStats = typeof(EnemyStats.Stats);
        ProcessValue(data.enemyModifier.maxHealth, allStats[1], enemyStats.GetField("maxHealth"));

        if (propertyNames)
            propertyNames.text = allStats[0].ToString();
        if (propertyValues)
            propertyValues.text = allStats[1].ToString();
    }

    protected override bool IsFieldShown(FieldInfo field)
    {
        switch (field.Name)
        {
            default:
                return false;
            case "timeLimit":
            case "clockSpeed":
            case "moveSpeed":
            case "greed":
            case "luck":
            case "growth":
            case "maxHealth":
                return true;
        }
    }

    protected override StringBuilder ProcessName(string name, StringBuilder output, FieldInfo field)
    {
        if (field.Name == "extraNotes")
            return output;
        return base.ProcessName(name, output, field);
    }

    protected override StringBuilder ProcessValue(object value, StringBuilder output, FieldInfo field)
    {
        float floatValue;
        switch (field.Name)
        {
            case "timeLimit":
                floatValue = value is int ? (int)value : (float)value;
                if (floatValue == 0)
                {
                    output.Append(DASH).Append('\n');
                }
                else
                {
                    string minutes = Mathf.FloorToInt(floatValue / 60).ToString();
                    string seconds = (floatValue % 60).ToString();
                    if (floatValue % 60 < 10)
                        seconds = "0" + seconds;
                    output.Append(minutes).Append(':').Append(seconds).Append('\n');
                }
                return output;

            case "clockSpeed":
                floatValue = value is int ? (int)value : (float)value;
                output.Append(floatValue).Append('x').Append('\n');
                return output;

            case "maxHealth":
            case "moveSpeed":
            case "greed":
            case "luck":
            case "growth":
                floatValue = value is int ? (int)value : (float)value;
                float percentage = Mathf.Round(floatValue * 100);
                if (Mathf.Approximately(percentage, 0))
                {
                    output.Append(DASH).Append('\n');
                }
                else
                {
                    if (percentage > 0)
                        output.Append('+');
                    output.Append(percentage).Append('%').Append('\n');
                }
                return output;

            case "extraNotes":
                if (value == null)
                    return output;
                string message = value.ToString();
                if (extraStageInfo)
                    extraStageInfo.text = string.IsNullOrWhiteSpace(message) ? DASH : message;
                return output;
        }

        return base.ProcessValue(value, output, field);
    }

    void Reset()
    {
        levelSelector = FindAnyObjectByType<UILevelSelector>();
    }
}
