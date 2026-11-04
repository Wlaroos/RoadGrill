using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
public class ConditionalHideDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalHideAttribute condHideAttr = (ConditionalHideAttribute)attribute;
        bool enabled = GetConditionalHideAttributeResult(condHideAttr, property);

        bool wasEnabled = GUI.enabled;
        GUI.enabled = enabled;

        if (!condHideAttr.hideInInspector || enabled)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        GUI.enabled = wasEnabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalHideAttribute condHideAttr = (ConditionalHideAttribute)attribute;
        bool enabled = GetConditionalHideAttributeResult(condHideAttr, property);

        if (condHideAttr.hideInInspector && !enabled)
        {
            return 0f;
        }

        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    private bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHideAttr, SerializedProperty property)
    {
        bool enabled = true;
        string propertyPath = property.propertyPath;
        string conditionPath = propertyPath.Contains(".") 
            ? propertyPath.Substring(0, propertyPath.LastIndexOf('.') + 1) + condHideAttr.conditionalSourceField 
            : condHideAttr.conditionalSourceField;

        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

        if (sourcePropertyValue != null)
        {
            if (sourcePropertyValue.propertyType == SerializedPropertyType.Boolean)
            {
                enabled = sourcePropertyValue.boolValue;
            }
            else
            {
                Debug.LogWarning("Attempting to use ConditionalHideAttribute on a field with a source property that is not a boolean: " + conditionPath);
            }
        }
        else
        {
            Debug.LogWarning("ConditionalHideAttribute could not find source property: " + conditionPath);
        }

        return enabled;
    }
}