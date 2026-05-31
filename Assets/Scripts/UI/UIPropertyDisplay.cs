using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

public abstract class UIPropertyDisplay : MonoBehaviour
{
    public bool updateInEditor = false;
    protected TextMeshProUGUI propertyNames, propertyValues;
    public const string DASH = "-";

    protected virtual void OnEnable() { UpdateFields(); }
    protected virtual void OnDrawGizmosSelected() { if (updateInEditor) UpdateFields(); }

    public abstract object GetReadObject();

    protected virtual bool IsFieldShown(FieldInfo field) { return true; }

    protected virtual StringBuilder ProcessName(string name, StringBuilder output, FieldInfo field)
    {
        if (!IsFieldShown(field))
            return output;
        return output.AppendLine(name);
    }

    protected virtual StringBuilder ProcessValue(object value, StringBuilder output, FieldInfo field)
    {
        if (!IsFieldShown(field))
            return output;

        float floatValue = value is int ? (int)value : value is float ? (float)value : 0f;

        PropertyAttribute attribute = (PropertyAttribute)field.GetCustomAttribute<RangeAttribute>() ?? field.GetCustomAttribute<MinAttribute>();
        if (attribute != null && field.FieldType == typeof(float))
        {
            float percentage = Mathf.Round(floatValue * 100 - 100);

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
        }
        else
        {
            output.Append(value).Append('\n');
        }

        return output;
    }

    protected virtual StringBuilder[] GetProperties(BindingFlags flags, string targetedType)
    {
        return GetProperties(flags, System.Type.GetType(targetedType));
    }

    protected virtual StringBuilder[] GetProperties(BindingFlags flags, System.Type targetType)
    {
        StringBuilder names = new StringBuilder();
        StringBuilder values = new StringBuilder();

        if (targetType == null)
            return new StringBuilder[2] { PrettifyNames(names), values };

        object readObject = GetReadObject();
        FieldInfo[] fields = targetType.GetFields(flags);
        foreach (FieldInfo field in fields)
        {
            ProcessName(field.Name, names, field);
            ProcessValue(field.GetValue(readObject), values, field);
        }

        return new StringBuilder[2] { PrettifyNames(names), values };
    }

    public abstract void UpdateFields();

    public static StringBuilder PrettifyNames(StringBuilder input)
    {
        if (input.Length <= 0)
            return new StringBuilder();

        StringBuilder result = new StringBuilder();
        char last = '\0';
        for (int i = 0; i < input.Length; i++)
        {
            char current = input[i];
            if (last == '\0' || char.IsWhiteSpace(last))
            {
                current = char.ToUpper(current);
            }
            else if (char.IsUpper(current))
            {
                result.Append(' ');
            }
            result.Append(current);

            last = current;
        }
        return result;
    }
}
