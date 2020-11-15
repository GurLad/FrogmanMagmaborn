using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConversationController))]
public class ConversationControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ConversationController myScript = (ConversationController)target;
        if (GUILayout.Button("Load Conversations"))
        {
            myScript.AutoLoad();
        }
    }
}