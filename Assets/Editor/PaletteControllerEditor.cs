using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PaletteController))]
public class LookAtPointEditor : Editor
{
    SerializedProperty BackgroundPalettes;
    SerializedProperty SpritePalettes;

    void OnEnable()
    {
        BackgroundPalettes = serializedObject.FindProperty("BackgroundPalettes");
        SpritePalettes = serializedObject.FindProperty("SpritePalettes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GUILayout.Label("From: ");
        Texture2D texture = Resources.Load<Texture2D>("Palette 1");
        GUILayout.Box(texture);
        GUILayout.Label("To: ");
        EditorGUILayout.PropertyField(BackgroundPalettes);
        EditorGUILayout.PropertyField(SpritePalettes);
        GUILayout.Label("Options: ");
        texture = Resources.Load<Texture2D>("Palette");
        GUILayout.Box(texture);
        serializedObject.ApplyModifiedProperties();
    }
}