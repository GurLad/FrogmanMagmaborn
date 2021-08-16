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
public enum AICautionLevel { None = 0, NoDamage = 1, Suicide = 2, TEMP = 3, LittleDamage = 4 }
public enum TormentPowerState { None, I, II }
public enum PortraitLoadingMode { Name, Team, Generic }
public enum Objective { Rout, Boss, Escape, Survive }
public enum GameState { Normal, SideWon, ShowingEvent }

public static class StaticGlobals
{
    public const string MAIN_CHARACTER_NAME = "Frogman";
    // Extension methods
    public static string Name(this Team team)
    {
        return GameController.Current.LevelMetadata.TeamDatas[(int)team].Name;
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
    public static AIType ToAIType(this string aiName)
    {
        return aiName.ToLower() switch
        {
            "charge" => AIType.Charge,
            "guard" => AIType.Guard,
            "hold" => AIType.Hold,
            _ => throw new System.Exception("No matching AI type! (" + aiName + ")")
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
    public static T GetOrAddComponenet<T>(this GameObject gameObject) where T : Component
    {
        T temp = gameObject.GetComponent<T>();
        return temp != null ? temp : gameObject.AddComponent<T>();
    }
}