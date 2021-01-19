using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnitClassData))]
public class UnitClassDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UnitClassData myScript = (UnitClassData)target;
        if (GUILayout.Button("Load Conversations"))
        {
            myScript.AutoLoad();
        }
    }
}