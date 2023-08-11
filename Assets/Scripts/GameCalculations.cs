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
            return SavedData.Load<int>("Knowledge", "Upgrade" + name, -1) == 1;
        }

        public static bool FoundKnowledge(string name)
        {
            return SavedData.Load<int>("Knowledge", "Upgrade" + name, -1) >= 0;
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
            return KnowledgeController.TormentPower("DeathLife") != TormentPowerState.II;
        }
    }

    public static bool HasInclinationUpgrade
    {
        get
        {
            return KnowledgeController.GetUpgradeActive(HardcodedKnowledge.InclinationBuff);
        }
    }

    public static bool Debug
    {
        get
        {
            return true;
        }
    }

    public static bool TransitionsOn
    {
        get
        {
            return SavedData.Load("TransitionsOn", 0, SaveMode.Global) == 1;
        }
    }

    public static bool ExtraSymbolsOn
    {
        get
        {
            return SavedData.Load("ExtraSymbolsOn", 0, SaveMode.Global) == 1;
        }
    }

    public static bool ScreenShakeOn
    {
        get
        {
            return SavedData.Load("ScreenShakeOn", 1, SaveMode.Global) == 1;
        }
    }

    public static Team FirstTurnTeam
    {
        get
        {
            return StaticGlobals.MainPlayerTeam; // Temp - I need to think of a better way to set this (LevelMetadata perhaps?)
        }
    }

    public static bool BattleAnimationsOn(Unit attacker, Unit defender)
    {
        switch (SavedData.Load<int>("BattleAnimationsMode", 0, SaveMode.Global))
        {
            case 0: // Always
                return true;
            case 1: // Player
                return attacker.TheTeam.PlayerControlled() || defender.TheTeam.PlayerControlled();
            case 2: // Bosses
                return GameController.Current.IsBoss(attacker) || GameController.Current.IsBoss(defender);
            case 3: // None
            default:
                return false;
        }
    }

    public static int GameSpeed(bool affectedByInput = true)
    {
        return (SavedData.Load("GameSpeed", 0, SaveMode.Global) == 1 ^ (affectedByInput && Control.GetButton(Control.CB.B))) ? 2 : 1;
    }

    public static int StatsPerLevel(Team team, string unitName)
    {
        int baseNum = 3;
        if (team.IsMainPlayerTeam())
        {
            switch (KnowledgeController.TormentPower("ChaosOrder"))
            {
                case TormentPowerState.I:
                    baseNum = 4;
                    break;
                case TormentPowerState.II:
                    baseNum = 2;
                    break;
                default:
                    baseNum = 3;
                    break;
            }
            if (unitName == StaticGlobals.MainCharacterName && KnowledgeController.TormentPower("DeathLife") == TormentPowerState.I)
            {
                baseNum += GameController.Current.DeadPlayerUnits.Count;
            }
        }
        return baseNum;
    }

    public static int MVPValue(int mapCount, int battleCount, int killCount, int deathCount)
    {
        return battleCount + killCount * 4 - mapCount - deathCount * 4;
    }

    public static float GetRandomHitResult()
    {
        //int a, b;
        //((a = Random.Range(0, 100)) + (b = Random.Range(0, 50))) / 1.5f;
        return (Random.Range(0, 100) + Random.Range(0, 50)) / 1.5f;
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
                units = units.FindAll(a => a.TheTeam.IsEnemy(StaticGlobals.MainPlayerTeam) && !a.Moved && a.Health > 1);
                if (units.Count > 0)
                {
                    Unit selected = units.RandomItemInList();
                    selected.Health -= Mathf.Min(selected.Health - 1, 2);
                }
                break;
            case TormentPowerState.II:
                units = units.FindAll(a => a.TheTeam.IsMainPlayerTeam() && !a.Moved && a.Health < a.Stats.Base.MaxHP);
                if (units.Count > 0)
                {
                    Unit selected = units.RandomItemInList();
                    selected.Health += Mathf.Min(selected.Stats.Base.MaxHP - selected.Health, 2);
                }
                break;
            default:
                break;
        }
    }

    public static void EndLevelEvents(List<Unit> units)
    {

    }

    public static void PlayerWinEvents(List<Unit> units)
    {
        //switch (KnowledgeController.TormentPower("WrathMercy"))
        //{
        //    case TormentPowerState.None:
        //        break;
        //    case TormentPowerState.I:
        //        if (units.Find(a => a.TheTeam != Team.Player) == null)
        //        {
        //            units.ForEach(a => a.Stats.Strength++);
        //        }
        //        break;
        //    case TormentPowerState.II:
        //        if (units.Find(a => a.TheTeam != Team.Player) != null)
        //        {
        //            units.ForEach(a => a.Stats.Endurance++);
        //        }
        //        break;
        //    default:
        //        break;
        //}
    }

    // Unit extension methods

    public static void LoadSkills(this Unit unit)
    {
        if (unit.TheTeam.IsMainPlayerTeam())
        {
            switch (KnowledgeController.TormentPower("SpeedSafety"))
            {
                case TormentPowerState.None:
                    break;
                case TormentPowerState.I:
                    unit.AddSkill(Skill.Acrobat);
                    break;
                case TormentPowerState.II:
                    unit.AddSkill(Skill.NaturalCover);
                    break;
                default:
                    break;
            }
            switch (KnowledgeController.TormentPower("PushPull"))
            {
                case TormentPowerState.None:
                    break;
                case TormentPowerState.I:
                    unit.AddSkill(Skill.Push);
                    break;
                case TormentPowerState.II:
                    unit.AddSkill(Skill.Pull);
                    break;
                default:
                    break;
            }
            switch (KnowledgeController.TormentPower("AngerCalm"))
            {
                case TormentPowerState.None:
                    break;
                case TormentPowerState.I:
                    unit.AddSkill(Skill.Vantage);
                    unit.AddSkill(Skill.AntiDragonskin);
                    break;
                case TormentPowerState.II:
                    unit.AddSkill(Skill.AntiVantage);
                    unit.AddSkill(Skill.Dragonskin);
                    break;
                default:
                    break;
            }
        }
    }

    public static void LoadStatModifiers(this Unit unit)
    {
        unit.AddStatModifier(new SMTerrainArmor(unit));
        unit.AddStatModifier(new SMWeapon(unit));
        if (HasInclinationUpgrade)
        {
            unit.AddStatModifier(new SMInclinationBonus(unit));
        }
        switch (KnowledgeController.TormentPower("TakeGive"))
        {
            case TormentPowerState.None:
                break;
            case TormentPowerState.I:
                unit.AddStatModifier(new SMTake(unit));
                break;
            case TormentPowerState.II:
                unit.AddStatModifier(new SMGive(unit));
                break;
            default:
                break;
        }
        // TBA: Fatigue
    }

    public static Stats AutoLevel(this Unit unit, int level)
    {
        Difficulty difficulty = (Difficulty)SavedData.Load("Knowledge", "UpgradeDifficulty", 0);
        if (unit.TheTeam.IsMainPlayerTeam())
        {
            if (difficulty != Difficulty.Insane && difficulty != Difficulty.NotSet)
            {
                level += difficulty == Difficulty.Normal ? 2 : 1;
            }
            switch (KnowledgeController.TormentPower("ChaosOrder"))
            {
                case TormentPowerState.None:
                    break;
                case TormentPowerState.I:
                    level--;
                    break;
                case TormentPowerState.II:
                    level++;
                    break;
                default:
                    break;
            }
        }
        unit.Level = level;
        Stats temp = unit.Stats.Base.GetMultipleLevelUps(level); // StatsPerLevel modifiers would be broken if they affected auto-levels
        //if (unit.TheTeam == Team.Monster) // It wouldn't make sense for Torment to buff Guards, although might be too easy this way.
        //{
        //    // Based on freedback from Dan, I'm removing the drawback - better making the game too easy, than making people not use Torment Powers.
        //    // Alternative: the Torment Power penalty only works on hard, or as an unlockable challange mode.
        //    //temp += unit.Stats.GetLevelUp(KnowledgeController.TotalTormentPowers);
        //}
        return temp;
    }

    public static int GetHitChance(this Unit attacker, Unit defender)
    {
        Stats attackerStats = attacker.Stats.Total;
        Stats defenderStats = defender.Stats.Total;
        return Mathf.Clamp(attackerStats.GetHit() - defenderStats.GetAvoid(), KnowledgeController.TormentPower("HonorGlory") == TormentPowerState.I ? 50 : 0, 100);
    }

    public static int GetDamage(this Unit attacker, Unit defender)
    {
        Stats attackerStats = attacker.Stats.Total;
        Stats defenderStats = defender.Stats.Total;
        int value = Mathf.Max(0, attackerStats.Strength - 2 * Mathf.Max(0, defenderStats.Armor - attackerStats.Pierce));
        if (defender.HasSkill(Skill.Dragonskin))
        {
            value = Mathf.CeilToInt(value / 2.0f);
        }
        else if (defender.HasSkill(Skill.AntiDragonskin))
        {
            value *= 2;
        }
        switch (KnowledgeController.TormentPower("HonorGlory"))
        {
            case TormentPowerState.I:
                return Mathf.Max(1, value);
            case TormentPowerState.II:
                //return value * 2;
            case TormentPowerState.None:
            default:
                return value;
        }
    }

    public static int GetMaxHP(this Stats stats)
    {
        return stats.Endurance * (KnowledgeController.TormentPower("HonorGlory") == TormentPowerState.II ? 1 : 2);
    }

    public static int GetHit(this Stats stats)
    {
        return stats.Precision * 10;
    }

    public static int GetAvoid(this Stats stats)
    {
        return stats.Evasion * 10;
    }

    public static void LoadInclination(this Unit unit)
    {
        int inclination = KnowledgeController.GetInclination(unit.Name);
        if (inclination > 0)
        {
            unit.ChangeInclination((Inclination)(inclination - 1));
        }
    }

    public static bool EffectiveAgainst(this Unit attacker, Unit defender) // Might change effectiveness to triangle
    {
        return HasInclinationUpgrade && attacker.TheTeam.IsMainPlayerTeam() && defender != null && attacker.IsEnemy(defender) && attacker.Inclination == defender.Inclination;
    }

    // Tile extension methods

    public static int GetMovementCost(this Tile tile, Unit unit)
    {
        if (unit.Flies)
        {
            return tile.High ? tile.MovementCost : 1;
        }
        else if (unit.HasSkill(Skill.Acrobat))
        {
            return (tile.High || tile.MovementCost > 5) ? tile.MovementCost : 1;
        }
        else
        {
            return tile.MovementCost;
        }
    }

    public static int GetArmorModifier(this Unit unit, Vector2Int pos)
    {
        Tile tile = GameController.Current.Map[pos.x, pos.y];
        return 
            ((unit.Flies && !tile.High) ? 0 :
            (unit.HasSkill(Skill.NaturalCover) ? Mathf.Abs(tile.ArmorModifier) : tile.ArmorModifier)) + 
            (KnowledgeController.TormentPower("TakeGive") == TormentPowerState.II && unit.TheTeam.IsMainPlayerTeam() ? unit.CountAdjacentAllies(pos) : 
            (KnowledgeController.TormentPower("TakeGive") == TormentPowerState.I && !unit.TheTeam.IsMainPlayerTeam() ? -unit.CountAdjacentAllies(pos) : 0));
    }
}
