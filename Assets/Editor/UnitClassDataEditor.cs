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
        if (GUILayout.Button("Load Class Data"))
        {
            myScript.AutoLoad();
        }
    }
}

[CustomEditor(typeof(BattleAnimationController))]
public class BattleAnimationControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BattleAnimationController myScript = (BattleAnimationController)target;
        if (GUILayout.Button("Load Animations"))
        {
            myScript.AutoLoad();
        }
    }
}