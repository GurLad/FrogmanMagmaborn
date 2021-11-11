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
            return HasKnowledge(name.ToString());
        }

        public static bool HasKnowledge(string name)
        {
            return SavedData.Load<int>("Knowledge", "Upgrade" + name) > 0;
        }

        public static bool GetUpgradeActive(HardcodedKnowledge name)
        {
            return GetUpgradeActive(name.ToString());
        }

        public static bool GetUpgradeActive(string name)
        {
            return SavedData.Load<int>("Knowledge", "Upgrade" + name) == 1;
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
            return KnowledgeController.GetUpgradeActive(HardcodedKnowledge.LevelUpChoice) ? 3 : 2;
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
            return KnowledgeController.GetUpgradeActive(HardcodedKnowledge.InclinationBuff);
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
                    baseNum = 2;
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
                baseNum += GameController.Current.DeadPlayerUnits.Count;
            }
        }
        return baseNum;
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

    // Game events

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

    public static void EndMapEvents(List<Unit> units)
    {
        switch (KnowledgeController.TormentPower("WrathMercy"))
        {
            case TormentPowerState.None:
                break;
            case TormentPowerState.I:
                if (units.Find(a => a.TheTeam != Team.Player) == null)
                {
                    units.ForEach(a => a.Stats.Strength++);
                }
                break;
            case TormentPowerState.II:
                if (units.Find(a => a.TheTeam != Team.Player) != null)
                {
                    units.ForEach(a => a.Stats.Endurance++);
                }
                break;
            default:
                break;
        }
    }

    // Unit extension methods

    public static Stats AutoLevel(this Unit unit, int level)
    {
        Difficulty difficulty = (Difficulty)SavedData.Load("Knowledge", "UpgradeDifficulty", 0);
        if (unit.TheTeam == Team.Player)
        {
            if (difficulty != Difficulty.Insane && difficulty != Difficulty.NotSet)
            {
                level += difficulty == Difficulty.Normal ? 2 : 1;
            }
            switch (KnowledgeController.TormentPower("OrderChaos"))
            {
                case TormentPowerState.None:
                    break;
                case TormentPowerState.I:
                    level++;
                    break;
                case TormentPowerState.II:
                    level--;
                    break;
                default:
                    break;
            }
        }
        unit.Level = level;
        Stats temp = unit.Stats.GetMultipleLevelUps(level); // StatsPerLevel modifiers would be broken if they affected auto-levels
        if (unit.TheTeam == Team.Monster) // It wouldn't make sense for Torment to buff Guards, although might be too easy this way.
        {
            // Based on freedback from Dan, I'm removing the drawback - better making the game too easy, than making people not use Torment Powers.
            // Alternative: the Torment Power penalty only works on hard, or as an unlockable challange mode.
            //temp += unit.Stats.GetLevelUp(KnowledgeController.TotalTormentPowers);
        }
        return temp;
    }

    public static int GetHitChance(this Unit attacker, Unit defender)
    {
        int Hit(Unit attacker, Unit defender) => Mathf.Clamp(attacker.Weapon.Hit - 10 * (defender.Stats.Evasion - defender.Weapon.Weight - attacker.Stats.Precision), KnowledgeController.TormentPower("HonorGlory") == TormentPowerState.I ? 50 : 0, 100);
        if (attacker.EffectiveAgainst(defender))
        {
            attacker.Stats[(int)attacker.Inclination * 2] += 2;
            int hit = Hit(attacker, defender);
            attacker.Stats[(int)attacker.Inclination * 2] -= 2;
            return hit;
        }
        return Hit(attacker, defender);
    }

    public static int GetDamage(this Unit attacker, Unit defender)
    {
        int armorModifier = GameController.Current.Map[defender.Pos.x, defender.Pos.y].GetArmorModifier(defender);
        int Damage(Unit attacker, Unit defender)
        {
            int value = Mathf.Max(0, attacker.Stats.Strength + attacker.Weapon.Damage - 2 * Mathf.Max(0, defender.Stats.Armor + armorModifier - attacker.Stats.Pierce));
            switch (KnowledgeController.TormentPower("HonorGlory"))
            {
                case TormentPowerState.I:
                    return Mathf.Max(1, value);
                case TormentPowerState.II:
                    return value * 2;
                case TormentPowerState.None:
                default:
                    return value;
            }
        }
        if (attacker.EffectiveAgainst(defender))
        {
            attacker.Stats[(int)attacker.Inclination * 2] += 2;
            int damage = Damage(attacker, defender);
            attacker.Stats[(int)attacker.Inclination * 2] -= 2;
            return damage;
        }
        return Damage(attacker, defender);
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