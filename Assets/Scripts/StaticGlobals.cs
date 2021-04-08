using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Difficulty { NotSet, Easy, Medium, Hard }
public enum KnowledgeUpgradeType { Toggle, Inclination, TormentPower }
public enum HardcodedKnowledge { LevelUpChoice, InclinationBuff } // For convenience only
public enum Team { Player, Monster, Guard }
public enum AIType { Charge, Hold, Guard }
public enum Inclination { Physical, Technical, Skillful } // Bad names
public enum VoiceType { Square50, Square25, Square12, Triangle }
public enum InteractState { None, Move, Attack }
public enum AICautionLevel { None = 0, NoDamage = 1, Suicide = 2, LittleDamage = 4 }
public enum TormentPowerState { None, I, II }

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
    public static Team? ToTeam(this string teamName)
    {
        return teamName.ToLower() switch
        {
            "player" => Team.Player,
            "monster" => Team.Monster,
            "guard" => Team.Guard,
            _ => null
        };
    }
    public static string ToColoredString(this string str, int paletteID, int colorIndex = 1)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(PaletteController.Current.SpritePalettes[paletteID][colorIndex]) + ">" + str + "</color>";
    }
    public static string ForgeJsonToUnity(this string json, string propertyName)
    {
        return "{" + '"' + propertyName + '"' + ":" + json + "}";
    }
}
