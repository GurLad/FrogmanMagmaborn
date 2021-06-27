using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FrogForgeImporter))]
public class FrogForgeImporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FrogForgeImporter myScript = (FrogForgeImporter)target;
        if (myScript.ConversationController != null && GUILayout.Button("Load Conversations"))
        {
            myScript.ConversationController.AutoLoad();
        }
        if (myScript.UnitClassData != null && GUILayout.Button("Load Class + Unit Data"))
        {
            myScript.UnitClassData.AutoLoad();
        }
        if (myScript.BattleAnimationController != null && GUILayout.Button("Load Battle Animations"))
        {
            myScript.BattleAnimationController.AutoLoad();
        }
        if (myScript.PortraitController != null && GUILayout.Button("Load Portraits"))
        {
            myScript.PortraitController.AutoLoad();
        }
        if (myScript.LevelMetadataController != null && GUILayout.Button("Load Level Metadata"))
        {
            myScript.LevelMetadataController.AutoLoad();
        }
    }
}