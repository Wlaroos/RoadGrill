using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Crosshair.CrosshairConfig))]
public class CrosshairConfigDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float singleLineHeight = EditorGUIUtility.singleLineHeight;
        float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
        
        // Find the relative properties from the struct
        SerializedProperty interactionProp = property.FindPropertyRelative("interactionType");
        SerializedProperty colorProp = property.FindPropertyRelative("color");
        SerializedProperty spriteProp = property.FindPropertyRelative("sprite");

        Rect fieldRect = new Rect(position.x, position.y, position.width, singleLineHeight);

        // Draw InteractionType
        EditorGUI.PropertyField(fieldRect, interactionProp);
        fieldRect.y += singleLineHeight + verticalSpacing;

        // Draw Color field
        EditorGUI.PropertyField(fieldRect, colorProp);
        fieldRect.y += singleLineHeight + verticalSpacing;

        // Draw Sprite field
        EditorGUI.PropertyField(fieldRect, spriteProp);
        fieldRect.y += singleLineHeight + verticalSpacing;

        // Draw the sprite preview underneath, tinted with the config's color
        if (spriteProp.objectReferenceValue != null)
        {
            Sprite sprite = spriteProp.objectReferenceValue as Sprite;
            Texture2D spriteTex = AssetPreview.GetAssetPreview(sprite);

            Rect previewRect = new Rect(fieldRect.x, fieldRect.y, 50, 50);

            if (spriteTex != null)
            {
                // Cache the original GUI color, then apply the crosshair config's color tint
                Color previousColor = GUI.color;
                GUI.color = colorProp.colorValue;
                
                // Draw the texture with alpha blending and color tint applied
                GUI.DrawTexture(previewRect, spriteTex, ScaleMode.ScaleToFit, true);
                
                // Restore the original GUI color so it doesn't affect other UI elements
                GUI.color = previousColor;
            }
            else
            {
                // AssetPreviews load asynchronously; request a repaint until loaded
                EditorWindow.focusedWindow?.Repaint();
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 3 standard lines for interactionType, color, and sprite + extra space for the preview box
        float baseHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 3;
        
        SerializedProperty spriteProp = property.FindPropertyRelative("sprite");
        if (spriteProp.objectReferenceValue != null)
        {
            baseHeight += 55; // 50px preview + padding
        }

        return baseHeight;
    }
}