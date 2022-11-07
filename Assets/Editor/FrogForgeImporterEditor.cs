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
            myScript.BattleAnimationController.AutoLoadAnimations();
        }
        if (myScript.BattleAnimationController != null && GUILayout.Button("Load Battle Backgrounds"))
        {
            myScript.BattleAnimationController.AutoLoadBackgrounds();
        }
        if (myScript.PortraitController != null && GUILayout.Button("Load Portraits"))
        {
            myScript.PortraitController.AutoLoad();
        }
        if (myScript.CGController != null && GUILayout.Button("Load CGs"))
        {
            myScript.CGController.AutoLoad();
        }
        if (myScript.LevelMetadataController != null && GUILayout.Button("Load Level Metadata"))
        {
            myScript.LevelMetadataController.AutoLoad();
        }
        if (myScript.MapController != null && GUILayout.Button("Load Tilesets"))
        {
            myScript.MapController.AutoLoadTilesets();
        }
        if (myScript.MapController != null && GUILayout.Button("Load Maps"))
        {
            myScript.MapController.AutoLoadMaps();
        }
        if (myScript.DebugOptions != null && GUILayout.Button("Load Debug Options"))
        {
            myScript.DebugOptions.AutoLoad();
        }
        if (myScript.CrossfadeMusicPlayer != null && GUILayout.Button("Load Musics"))
        {
            myScript.CrossfadeMusicPlayer.AutoLoad();
        }
    }
}