using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Difficulty { NotSet, Easy, Medium, Hard }
public enum KnowledgeUpgradeType { Toggle, Choice }
public enum HardcodedKnowledge { LevelUpChoice, InclinationBuff } // For convenience only
public enum Team { Player, Monster, Guard }
public enum AIType { Charge, Hold, Guard }
public enum Inclination { Physical, Technical, Skillful } // Bad names
public enum VoiceType { Square50, Square25, Square12, Triangle }
public enum InteractState { None, Move, Attack }
public enum AICautionLevel { None = 0, NoDamage = 1, Suicide = 2, LittleDamage = 4 }

public static class StaticGlobals
{
    // Extension methods
    public static string Name(this Team team)
    {
        if (GameController.Current.LevelNumber % 4 == 0 && team == Team.Monster)
        {
            return "Torment";
        }
        return team.ToString();
    }
}
