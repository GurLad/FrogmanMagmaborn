using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameCalculations
{
    // Mainly because Knowledge ruins everything. Many public methods (aka Knowledge ones) should only be used here. Think of a solution.
    public static int NumLevelUpOptions
    {
        get
        {
            return KnowledgeController.TormentPower("OrderChaos") == TormentPowerState.II ? 1 : (KnowledgeController.HasKnowledge(HardcodedKnowledge.LevelUpChoice) ? 3 : 2);
        }
    }
    public static bool PermaDeath
    {
        get
        {
            return KnowledgeController.TormentPower("LifeDeath") != TormentPowerState.I;
        }
    }
    public static int StatsPerLevel(Team team, string unitName)
    {
        int baseNum = 3;
        if (team == Team.Player)
        {
            switch (KnowledgeController.TormentPower("OrderChaos"))
            {
                case TormentPowerState.I:
                    baseNum = 0;
                    break;
                case TormentPowerState.II:
                    baseNum = 4;
                    break;
                default:
                    baseNum = 3;
                    break;
            }
            if (unitName == StaticGlobals.MAIN_CHARACTER_NAME && KnowledgeController.TormentPower("LifeDeath") == TormentPowerState.II)
            {
                baseNum += GameController.Current.NumDeadPlayerUnits;
            }
        }
        return baseNum;
    }
    public static Stats AutoLevel(this Unit unit, int level)
    {
        Difficulty difficulty = (Difficulty)SavedData.Load("Difficulty", 0);
        if (unit.TheTeam == Team.Player)
        {
            if (difficulty != Difficulty.Hard && difficulty != Difficulty.NotSet)
            {
                level += difficulty == Difficulty.Easy ? 2 : 1;
            }
            if (KnowledgeController.TormentPower("OrderChaos") == TormentPowerState.I) // It would be more efficent to include this in the default, but less future-proof
            {
                level += 2;
            }
        }
        Stats temp = unit.Stats.GetMultipleLevelUps(level); // StatsPerLevel modifiers would be broken if they affected auto-levels
        if (unit.TheTeam == Team.Monster) // It wouldn't make sense for Torment to buff Guards, although might be too easy this way.
        {
            temp += unit.Stats.GetLevelUp(KnowledgeController.TotalTormentPowers);
        }
        return temp;
    }
}