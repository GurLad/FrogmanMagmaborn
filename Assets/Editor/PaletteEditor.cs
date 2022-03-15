﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Palette))]
public class PaletteDrawer : PropertyDrawer
{
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
            list.GetArrayElementAtIndex(i).intValue = EditorGUI.IntField(posRect, list.GetArrayElementAtIndex(i).intValue);
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
}