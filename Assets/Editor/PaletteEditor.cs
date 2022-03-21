using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Palette))]
public class PaletteDrawer : PropertyDrawer
{
    private static List<Color32> AllPossibleColors { get; } = new List<Color32>(new Color32[]
    {
        // Generated in Frogman Magmaborn
        new Color32(255, 255, 255, 255),
        new Color32(164, 232, 252, 255),
        new Color32(188, 184, 252, 255),
        new Color32(220, 184, 252, 255),
        new Color32(252, 184, 252, 255),
        new Color32(244, 192, 224, 255),
        new Color32(244, 208, 180, 255),
        new Color32(252, 224, 180, 255),
        new Color32(252, 216, 132, 255),
        new Color32(220, 248, 120, 255),
        new Color32(184, 248, 120, 255),
        new Color32(176, 240, 216, 255),
        new Color32(0, 248, 252, 255),
        new Color32(0, 0, 0, 255),
        new Color32(252, 248, 252, 255),
        new Color32(56, 192, 252, 255),
        new Color32(104, 136, 252, 255),
        new Color32(156, 120, 252, 255),
        new Color32(252, 120, 252, 255),
        new Color32(252, 88, 156, 255),
        new Color32(252, 120, 88, 255),
        new Color32(252, 160, 72, 255),
        new Color32(252, 184, 0, 255),
        new Color32(188, 248, 24, 255),
        new Color32(88, 216, 88, 255),
        new Color32(88, 248, 156, 255),
        new Color32(0, 232, 228, 255),
        new Color32(0, 0, 0, 255),
        new Color32(188, 192, 196, 255),
        new Color32(0, 120, 252, 255),
        new Color32(0, 136, 252, 255),
        new Color32(104, 72, 252, 255),
        new Color32(220, 0, 212, 255),
        new Color32(228, 0, 96, 255),
        new Color32(252, 56, 0, 255),
        new Color32(228, 96, 24, 255),
        new Color32(172, 128, 0, 255),
        new Color32(0, 184, 0, 255),
        new Color32(0, 168, 0, 255),
        new Color32(0, 168, 72, 255),
        new Color32(0, 136, 148, 255),
        new Color32(0, 0, 0, 255),
        new Color32(120, 128, 132, 255),
        new Color32(0, 0, 252, 255),
        new Color32(0, 0, 196, 255),
        new Color32(64, 40, 196, 255),
        new Color32(148, 0, 140, 255),
        new Color32(172, 0, 40, 255),
        new Color32(172, 16, 0, 255),
        new Color32(140, 24, 0, 255),
        new Color32(80, 48, 0, 255),
        new Color32(0, 120, 0, 255),
        new Color32(0, 104, 0, 255),
        new Color32(0, 88, 0, 255),
        new Color32(0, 64, 88, 255),
        new Color32(0, 0, 0, 255),
        new Color32(0, 0, 0, 0)
    });
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 10f * 2;
    }
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        //var colors = property.FindPropertyRelative("Colors");
        //for (int i = 0; i < 4; i++)
        //{
        //    Debug.Log(colors);
        //    EditorGUI.ColorField(new Rect(i * 50, 0, 50, 30), colors.GetArrayElementAtIndex(i).colorValue);
        //}


        //EditorGUI.BeginProperty(rect, label, property);
        //SerializedProperty list = property.FindPropertyRelative("Colors");
        //Rect posRect = new Rect(rect);
        //posRect.width = rect.width / 4;
        //posRect.height = 20;

        //for (int i = 0; i < 4; i++)
        //{
        //    list.GetArrayElementAtIndex(i).colorValue = EditorGUI.ColorField(posRect, list.GetArrayElementAtIndex(i).colorValue);
        //    posRect.x += posRect.width;
        //}

        //EditorGUI.EndProperty();

        EditorGUI.BeginProperty(rect, label, property);
        SerializedProperty list = property.FindPropertyRelative("Colors");
        Rect posRect = new Rect(rect);
        posRect.width = rect.width / 4;
        posRect.height = 20;

        for (int i = 0; i < 4; i++)
        {
            SerializedProperty id = list.GetArrayElementAtIndex(i).FindPropertyRelative("id");
            id.intValue = Mathf.Clamp(id.intValue, 0, AllPossibleColors.Count - 1);
            GUIStyle style = new GUIStyle();
            style.normal.background = Texture2DFromColor(1, 1, AllPossibleColors[id.intValue]);
            style.normal.textColor = style.normal.background.GetPixel(0, 0).maxColorComponent < 0.55f ? Color.white : Color.black;
            id.intValue = EditorGUI.IntField(posRect, id.intValue, style);
            posRect.x += posRect.width;
        }

        EditorGUI.EndProperty();

        //var positionProperty = property.FindPropertyRelative("Position");
        //var normalProperty = property.FindPropertyRelative("NormalRotation");

        //EditorGUIUtility.wideMode = true;
        //EditorGUIUtility.labelWidth = 70;
        //rect.height /= 2;
        //positionProperty.vector3Value = EditorGUI.Vector3Field(rect, "Position", positionProperty.vector3Value);
        //rect.y += rect.height;
        //normalProperty.vector3Value = EditorGUI.Vector3Field(rect, "Normal", normalProperty.vector3Value);
    }
    private Texture2D Texture2DFromColor(int width, int height, Color color)
    {
        Texture2D tex = new Texture2D(width, height);
        Color[] fillColorArray = tex.GetPixels();

        for (int i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] = color;
        }

        tex.SetPixels(fillColorArray);

        tex.Apply();

        return tex;
    }
}