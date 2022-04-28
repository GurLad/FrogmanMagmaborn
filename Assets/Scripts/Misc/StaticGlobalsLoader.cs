using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticGlobalsLoader : MonoBehaviour
{
    public string ModName;
    public string ModDescription; // Unused
    public string MainCharacterName;
    public int PlayerTeam;

#if UNITY_EDITOR || MODDABLE_BUILD
    public void AutoLoad()
    {
        // Load json
        string json = FrogForgeImporter.LoadTextFile("GameSettings.json").Text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("GameSettings"), this);
        // Set globals
        SavedData.Prefix = ModName;
        StaticGlobals.MainCharacterName = MainCharacterName;
        StaticGlobals.MainPlayerTeam = (Team)PlayerTeam;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }
#endif
}
