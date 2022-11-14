using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugOptions : MonoBehaviour
{
    [SerializeField]
    private Options options;
    public bool Enabled => options.Enabled;
    public bool SkipIntro => options.SkipIntro;

    public void Apply(GameController gameController, List<Unit> playerUnitsCache)
    {
        gameController.LevelNumber = options.EndgameLevel;
        foreach (string unit in options.Units)
        {
            gameController.PlayerUnits.Add(gameController.CreatePlayerUnit(unit));
        }
        if (options.UnlimitedMove)
        {
            foreach (Unit unit in gameController.PlayerUnits)
            {
                unit.Movement = 50;
                unit.Flies = true;
            }
        }
        if (options.OPPlayers)
        {
            foreach (Unit unit in gameController.PlayerUnits)
            {
                unit.Stats += unit.AutoLevel(50);
            }
        }
        if (options.ExtraLevels > 0)
        {
            foreach (Unit unit in gameController.PlayerUnits)
            {
                unit.Stats += unit.AutoLevel(options.ExtraLevels);
            }
        }
        ConversationPlayer.Current.Play(gameController.CreateLevel(options.ForceConversation, options.ForceMap));
    }

#if UNITY_EDITOR || MODDABLE_BUILD
    public void AutoLoad()
    {
        // Load json
        string json = FrogForgeImporter.LoadTextFile("DebugOptions.json").Text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("options"), this);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }
#endif

    [System.Serializable]
    private class Options
    {
        public bool Enabled;
        [Header("General")]
        public bool SkipIntro;
        [Header("In-game")]
        public int EndgameLevel;
        public int ExtraLevels;
        public List<string> Units;
        public bool UnlimitedMove;
        public bool OPPlayers;
        public string ForceConversation;
        public string ForceMap;
    }
}
