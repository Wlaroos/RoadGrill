using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

[CustomEditor(typeof(ItemData), true)]
public class ItemDataEditor : Editor
{
    // Tracks which properties have already been drawn manually
    private HashSet<string> drawnProperties = new HashSet<string>();

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        drawnProperties.Clear();

        // Base Info & Main Fields
        DrawField("itemName");
        DrawField("itemID");

        EditorGUILayout.Space();
        DrawField("icon");
        DrawField("worldPrefab");

        // Conditional Groups (Toggle + Dependent Fields)
        EditorGUILayout.Space();
        DrawConditionalGroup("isChoppable", new string[] { "choppedItem", "choppedItemAmount" });

        EditorGUILayout.Space();
        DrawConditionalGroup("isGrillable", new string[] { "grilledItem", "cookTime" });

        EditorGUILayout.Space();
        DrawConditionalGroup("isStackable", new string[] { "maxStackSize" });

        EditorGUILayout.Space();
        DrawConditionalGroup("isSellable", new string[] { "value" });

        EditorGUILayout.Space();
        DrawConditionalGroup("isFlattenable", new string[] { "flatValueMultipliers" });

        // Automatically draws any new fields added to ItemData that weren't manually drawn above.
        EditorGUILayout.Space();
        DrawRemainingProperties();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeader(string title)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
    }

    private void DrawField(string propertyName)
    {
        SerializedProperty prop = serializedObject.FindProperty(propertyName);
        if (prop != null)
        {
            EditorGUILayout.PropertyField(prop, true);
            drawnProperties.Add(propertyName);
        }
    }

    private void HideField(string propertyName)
    {
        SerializedProperty prop = serializedObject.FindProperty(propertyName);
        if (prop != null)
        {
            drawnProperties.Add(propertyName);
        }
    }

    private void DrawConditionalGroup(string boolPropName, string[] dependentPropNames)
    {
        SerializedProperty boolProp = serializedObject.FindProperty(boolPropName);
        if (boolProp != null)
        {
            EditorGUILayout.PropertyField(boolProp);
            drawnProperties.Add(boolPropName);

            // If the toggle is true, indent and draw its dependent fields
            if (boolProp.boolValue)
            {
                EditorGUI.indentLevel++;
                foreach (string propName in dependentPropNames)
                {
                    DrawField(propName);
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                // If the toggle is false, hide its dependent fields
                foreach (string propName in dependentPropNames)
                {
                    HideField(propName);
                }
            }
        }
    }

    private void DrawRemainingProperties()
    {
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;

            // Skip the default m_Script property and anything we already drew manually
            if (iterator.name == "m_Script" || drawnProperties.Contains(iterator.name))
                continue;

            // Check if the field was declared in a subclass inheriting from ItemData
            FieldInfo fieldInfo = serializedObject.targetObject.GetType().GetField(
                iterator.name, 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            // If the field belongs to a derived class rather than base ItemData, skip it
            if (fieldInfo != null && fieldInfo.DeclaringType != typeof(ItemData))
                continue;

            Debug.Log("[ADD NEW FIELDS TO ItemDataEditor.cs]: " + iterator.name + "]");
            EditorGUILayout.PropertyField(iterator, true);
        }
    }
}