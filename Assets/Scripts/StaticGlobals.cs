using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Difficulty { NotSet, Normal, Hard, Insane }
public enum KnowledgeUpgradeType { Toggle, Inclination, TormentPower, Difficulty }
public enum HardcodedKnowledge { LevelUpChoice, InclinationBuff } // For convenience only
public enum Team { Player, Monster, Guard }
public enum AIType { Charge, Hold, Guard }
public enum Inclination { Physical, Technical, Skillful } // Bad names
public enum VoiceType { Square50, Square25, Square12, Triangle }
public enum InteractState { None, Move, Attack }
public enum AICautionLevel { None = 0, NoDamage = 1, Suicide = 2, TEMP = 3, LittleDamage = 4 }
public enum TormentPowerState { None, I, II }
public enum PortraitLoadingMode { Name, Team, Generic, None }
public enum Objective { Rout, Boss, Escape, Survive, Custom }
public enum GameState { Normal, SideWon, ShowingEvent }
public enum BattleAnimationMode { Walk, Projectile, Teleport }
public enum Skill { Acrobat, NaturalCover, SiegeWeapon, Push, Pull, Vantage, AntiVantage, Dragonskin, AntiDragonskin } // Charisma, Shade, HitAndRun, FinishingTouch }

public static class StaticGlobals
{
    public static string MainCharacterName = "Frogman";
    public static Team MainPlayerTeam = Team.Player;
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
    public static AIType? ToAIType(this string aiName)
    {
        return aiName.ToLower() switch
        {
            "charge" => AIType.Charge,
            "guard" => AIType.Guard,
            "hold" => AIType.Hold,
            _ => null
        };
    }
    public static Skill? ToSkill(this string skillName)
    {
        return skillName.ToLower() switch
        {
            "acrobat" => Skill.Acrobat,
            "naturalcover" => Skill.NaturalCover,
            "siegeweapon" => Skill.SiegeWeapon,
            "push" => Skill.Push,
            "pull" => Skill.Pull,
            "vantage" => Skill.Vantage,
            "antivantage" => Skill.AntiVantage,
            "dragonskin" => Skill.Dragonskin,
            "antidragonskin" => Skill.AntiDragonskin,
            _ => null
        };
    }
    public static string ToColoredString(this string str, int paletteID)
    {
        return "<color=#" + paletteID + "00000>" + str + "</color>";
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
    public static bool IsEnemy(this Team origin, Team target)
    {
        return origin != target && !GameController.Current.LevelMetadata.Alliances[Mathf.Abs((int)origin | (int)target) - 1];
    }
    public static bool PlayerControlled(this Team origin)
    {
        return GameController.Current.LevelMetadata.TeamDatas[(int)origin].PlayerControlled;
    }
    public static bool IsMainPlayerTeam(this Team origin)
    {
        return origin == MainPlayerTeam;
    }
    public static int TileSize(this Vector2Int vector2Int)
    {
        return Mathf.Abs(vector2Int.x) + Mathf.Abs(vector2Int.y);
    }
    public static int TileDist(this Vector2Int one, Vector2Int two)
    {
        return Mathf.Abs(one.x - two.x) + Mathf.Abs(one.y - two.y);
    }
    public static List<T> Shuffle<T>(this List<T> values) // Fisher–Yates
    {
        int n = values.Count;
        for (int i = 0; i < n - 2; i++)
        {
            int j = Random.Range(i, n);
            T temp = values[i];
            values[i] = values[j];
            values[j] = temp;
        }
        return values;
    }
}