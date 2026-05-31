using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

public class UIStatsDisplay : UIPropertyDisplay
{
    public PlayerStats player;
    public CharacterData character;
    public bool displayCurrentHealth = false;
    const string CurseFieldName = nameof(CharacterData.Stats.curse);

    public override object GetReadObject()
    {
        if (player)
            return player.Stats;
        else if (character)
            return character.stats;
        return new CharacterData.Stats();
    }

    protected override StringBuilder ProcessValue(object value, StringBuilder output, FieldInfo field)
    {
        if (field.Name != CurseFieldName || field.FieldType != typeof(float))
        {
            return base.ProcessValue(value, output, field);
        }

        float percentage = Mathf.Round((float)value * 100);
        if (Mathf.Approximately(percentage, 0))
        {
            return output.Append(DASH).Append('\n');
        }

        if (percentage > 0)
            output.Append('+');
        return output.Append(percentage).Append('%').Append('\n');
    }

    public override void UpdateFields()
    {
        if (!player && !character)
            return;

        StringBuilder[] allStats = GetProperties(
            BindingFlags.Public | BindingFlags.Instance,
            typeof(CharacterData.Stats)
        );

        if (!propertyNames && transform.childCount > 0)
            propertyNames = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        if (!propertyValues && transform.childCount > 1)
            propertyValues = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        if (displayCurrentHealth)
        {
            allStats[0].Insert(0, "Health\n");
            allStats[1].Insert(0, (player ? player.CurrentHealth.ToString() : DASH) + "\n");
        }

        if (propertyNames)
            propertyNames.text = allStats[0].ToString();
        if (propertyValues)
            propertyValues.text = allStats[1].ToString();

        if (propertyNames && propertyValues)
            propertyValues.fontSize = propertyNames.fontSize;
    }

    void Reset()
    {
        player = FindAnyObjectByType<PlayerStats>();
    }
}
