using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemData), true)]
public class ItemIconPreview : Editor
{
    public override void OnInspectorGUI()
    {
        // Update the serialized object representation
        serializedObject.Update();

        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;

        // Loop through all visible properties in the Inspector
        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            // Skip drawing the default MonoScript field
            if (prop.name == "m_Script")
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(prop, true);
                }
                continue;
            }

            // Draw the current property field
            EditorGUILayout.PropertyField(prop, true);

            // Check if this property is the icon field, then draw the preview underneath
            if (prop.name == "icon")
            {
                ItemData itemSO = (ItemData)target;
                
                if (itemSO != null && itemSO.icon != null)
                {
                    Texture2D spriteTex = AssetPreview.GetAssetPreview(itemSO.icon);

                    if (spriteTex != null)
                    {
                        // Allocate layout space for the preview box
                        Rect rect = GUILayoutUtility.GetRect(50, 50, GUILayout.ExpandWidth(false));
                        
                        // Draw the texture
                        GUI.DrawTexture(rect, spriteTex, ScaleMode.ScaleToFit, true);
                    }
                    else
                    {
                        // AssetPreviews load asynchronously
                        Repaint();
                    }
                }
            }
        }

        // Apply any changes made in the inspector
        serializedObject.ApplyModifiedProperties();
    }
}