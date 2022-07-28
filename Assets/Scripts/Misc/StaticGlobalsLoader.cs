using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticGlobalsLoader : MonoBehaviour
{
    [SerializeField]
    private GameSettings gameSettings;

#if UNITY_EDITOR || MODDABLE_BUILD
    public void AutoLoad()
    {
        // Load json
        string json = FrogForgeImporter.LoadTextFile("GameSettings.json").Text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("gameSettings"), this);
        // Set globals
        SavedData.Prefix = gameSettings.ModName;
        StaticGlobals.MainCharacterName = gameSettings.MainCharacterName;
        StaticGlobals.MainPlayerTeam = (Team)gameSettings.PlayerTeam;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }
#endif

    [System.Serializable]
    private class GameSettings
    {
        public string ModName;
        public string ModDescription; // Unused
        public string MainCharacterName;
        public int PlayerTeam;
    }
}
