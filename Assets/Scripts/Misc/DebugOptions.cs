using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugOptions : MonoBehaviour
{
    public bool Enabled;
    [Header("General")]
    public bool SkipIntro;
    [Header("In-game")]
    public bool StartAtEndgame;
    public int EndgameLevel;
    public int ExtraLevels;
    public List<string> Units;
    public bool UnlimitedMove;
    public bool OPPlayers;
    public string ForceConversation;
    public string ForceMap;

    public void Apply(GameController gameController, List<Unit> playerUnitsCache)
    {
        gameController.LevelNumber = EndgameLevel;
        foreach (string unit in Units)
        {
            gameController.PlayerUnits.Add(gameController.CreatePlayerUnit(unit));
        }
        if (UnlimitedMove)
        {
            foreach (Unit unit in gameController.PlayerUnits)
            {
                unit.Movement = 50;
                unit.Flies = true;
            }
        }
        if (OPPlayers)
        {
            foreach (Unit unit in gameController.PlayerUnits)
            {
                unit.Stats += unit.AutoLevel(50);
            }
        }
        if (ExtraLevels > 0)
        {
            foreach (Unit unit in gameController.PlayerUnits)
            {
                unit.Stats += unit.AutoLevel(ExtraLevels);
            }
        }
        ConversationPlayer.Current.Play(gameController.CreateLevel(ForceConversation, ForceMap));
    }
}
