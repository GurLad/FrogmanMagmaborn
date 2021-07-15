using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameCalculations
{
    // Mainly because Knowledge ruins everything. Many public methods (aka internal unit/tile/whatever properties) should only be used here. Think of a solution.

    private static class KnowledgeController
    {
        public static int TotalTormentPowers
        {
            get
            {
                return SavedData.Load<int>("Knowledge", "TormentPowerCount");
            }
        }

        public static bool HasKnowledge(HardcodedKnowledge name)
        {
            return SavedData.Load<int>("Knowledge", "Upgrade" + name) > 0;
        }

        public static bool HasKnowledge(string name)
        {
            return SavedData.Load<int>("Knowledge", "Upgrade" + name) > 0;
        }

        public static bool FoundKnowledge(string name)
        {
            return SavedData.Load<int>("Knowledge", "Upgrade" + name) >= 0;
        }

        public static int GetInclination(string characterName)
        {
            return SavedData.Load<int>("Knowledge", "UpgradeInclination" + characterName);
        }

        public static void UnlockKnowledge(string name)
        {
            SavedData.Save("Knowledge", "Upgrade" + name, 0);
        }

        public static TormentPowerState TormentPower(string name)
        {
            return (TormentPowerState)Mathf.Max(0, SavedData.Load<int>("Knowledge", "UpgradeTorment" + name));
        }
    }

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

    public static bool HasInclinationUpgrade
    {
        get
        {
            return KnowledgeController.HasKnowledge(HardcodedKnowledge.InclinationBuff);
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

    public static void EndTurnEvents(List<Unit> units)
    {
        switch (KnowledgeController.TormentPower("HurtHelp"))
        {
            case TormentPowerState.None:
                break;
            case TormentPowerState.I:
                units.ForEach(a => a.Health -= (a.TheTeam != Team.Player && !a.Moved && a.Health > 1) ? 1 : 0);
                break;
            case TormentPowerState.II:
                units.ForEach(a => a.Health += (a.TheTeam == Team.Player && a.Health < a.Stats.MaxHP) ? 1 : 0);
                break;
            default:
                break;
        }
    }

    public static bool HasKnowledge(string name) // For conversations. Pretty bad idea to allow access to these - GameCalculations should be the only class using Knowledge.
    {
        return KnowledgeController.HasKnowledge(name);
    }

    public static bool FoundKnowledge(string name) // For conversations. Pretty bad idea to allow access to these - GameCalculations should be the only class using Knowledge.
    {
        return KnowledgeController.FoundKnowledge(name);
    }

    public static void UnlockKnowledge(string name)
    {
        KnowledgeController.UnlockKnowledge(name);
    }

    // Unit extension methods

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
            // Based on freedback from Dan, I'm removing the drawback - better making the game too easy, than making people not use Torment Powers.
            // Alternative: the Torment Power penalty only works on hard, or as an unlockable challange mode.
            //temp += unit.Stats.GetLevelUp(KnowledgeController.TotalTormentPowers);
        }
        return temp;
    }

    public static int GetHitChance(this Unit attacker, Unit defender) // Technically, this doesn't use KnowledgeController, but it's still an important game calculation.
    {
        if (attacker.EffectiveAgainst(defender))
        {
            attacker.Stats[(int)attacker.Inclination * 2] += 2;
            int hit = Mathf.Min(100, attacker.Weapon.Hit - 10 * (defender.Stats.Evasion - defender.Weapon.Weight - attacker.Stats.Precision));
            attacker.Stats[(int)attacker.Inclination * 2] -= 2;
            return hit;
        }
        return Mathf.Clamp(attacker.Weapon.Hit - 10 * (defender.Stats.Evasion - defender.Weapon.Weight - attacker.Stats.Precision), 0, 100);
    }

    public static int GetDamage(this Unit attacker, Unit defender) // See above.
    {
        int armorModifier = GameController.Current.Map[defender.Pos.x, defender.Pos.y].GetArmorModifier(defender);
        if (attacker.EffectiveAgainst(defender))
        {
            attacker.Stats[(int)attacker.Inclination * 2] += 2;
            int damage = Mathf.Max(0, attacker.Stats.Strength + attacker.Weapon.Damage - 2 * Mathf.Max(0, defender.Stats.Armor + armorModifier - attacker.Stats.Pierce));
            attacker.Stats[(int)attacker.Inclination * 2] -= 2;
            return damage;
        }
        return Mathf.Max(0, attacker.Stats.Strength + attacker.Weapon.Damage - 2 * Mathf.Max(0, defender.Stats.Armor + armorModifier - attacker.Stats.Pierce));
    }

    public static void LoadInclination(this Unit unit)
    {
        int inclination = KnowledgeController.GetInclination(unit.Name);
        if (inclination > 0)
        {
            Debug.Log("Changing inclination!");
            unit.ChangeInclination((Inclination)(inclination - 1));
        }
    }

    public static bool EffectiveAgainst(this Unit attacker, Unit defender) // Might change effectiveness to triangle
    {
        return attacker.TheTeam == Team.Player && defender != null && attacker.Inclination == defender.Inclination && HasInclinationUpgrade;
    }

    // Tile extension methods

    public static int GetMovementCost(this Tile tile, Unit unit)
    {
        if (unit.Flies || (unit.TheTeam == Team.Player && KnowledgeController.TormentPower("SpeedSafety") == TormentPowerState.I))
        {
            return tile.High ? tile.MovementCost : 1;
        }
        else
        {
            return tile.MovementCost;
        }
    }

    public static int GetArmorModifier(this Tile tile, Unit unit)
    {
        return (unit.Flies && !tile.High) ? 0 :
            (unit.TheTeam == Team.Player && KnowledgeController.TormentPower("SpeedSafety") == TormentPowerState.II) ? Mathf.Abs(tile.ArmorModifier) : tile.ArmorModifier;
    }
}