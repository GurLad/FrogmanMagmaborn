using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RunStatsController : MonoBehaviour, ISuspendable<SuspendDataRunStatsController>
{
    public enum UnitEvent { Spawn, Battle, Won, Lost }
    public enum GameEvent { TurnPassed, Won, Lost, RecordPlayTime }

    public static RunStatsController Current; // One should use Current? everywhere, to avoid recording stats during the tutorial
    public GameStatsData GameStats { get; private set; } = new GameStatsData();
    private Dictionary<string, UnitBattleStatsData> unitStats { get; } = new Dictionary<string, UnitBattleStatsData>();

    private void Awake()
    {
        Current = this;
    }

    public void RecordUnitEvent(Unit unit, UnitEvent unitEvent)
    {
        string name = unit.ToString();
        if (!unitStats.ContainsKey(name))
        {
            unitStats.Add(name, new UnitBattleStatsData(name));
        }
        switch (unitEvent)
        {
            case UnitEvent.Spawn:
                unitStats[name].Maps++;
                break;
            case UnitEvent.Battle:
                unitStats[name].Battles++;
                break;
            case UnitEvent.Won:
                unitStats[name].Wins++;
                break;
            case UnitEvent.Lost:
                unitStats[name].Losses++;
                break;
            default:
                break;
        }
    }

    public void RecordGameEvent(GameEvent gameEvent)
    {
        switch (gameEvent)
        {
            case GameEvent.TurnPassed:
                GameStats.TotalTurns++;
                break;
            case GameEvent.Won:
                GameStats.TotalWins++;
                break;
            case GameEvent.Lost:
                GameStats.Lost = true;
                break;
            case GameEvent.RecordPlayTime:
                GameStats.PlayTime += Time.timeSinceLevelLoad;
                break;
            default:
                break;
        }
    }

    public void AddToTotal(bool game = true, bool units = true) // Adds the current run stats to the total stats
    {
        if (game)
        {
            SavedData.Append("Statistics", "GameTurnsCount", GameStats.TotalTurns);
            SavedData.Append("Statistics", "GameWinCount", GameStats.TotalWins);
            SavedData.Append("Statistics", "GameLossCount", GameStats.Lost ? 1 : 0);
            SavedData.Append("Statistics", "GamePlayTime", GameStats.PlayTime);
            GameStats = new GameStatsData();
        }
        if (units)
        {
            unitStats.Values.ToList().ForEach(a =>
            {
                SavedData.Append("Statistics", a.ToString() + "MapsCount", a.Maps);
                SavedData.Append("Statistics", a.ToString() + "BattleCount", a.Battles);
                SavedData.Append("Statistics", a.ToString() + "KillCount", a.Wins);
                SavedData.Append("Statistics", a.ToString() + "DeathCount", a.Losses);
            });
            unitStats.Clear();
        }
    }

    public SuspendDataRunStatsController SaveToSuspendData()
    {
        SuspendDataRunStatsController suspendData = new SuspendDataRunStatsController();
        suspendData.GameStats = GameStats;
        suspendData.UnitStats = unitStats.Values.ToList();
        return suspendData;
    }

    public void LoadFromSuspendData(SuspendDataRunStatsController data)
    {
        GameStats = data.GameStats;
        data.UnitStats.ForEach(a => unitStats.Add(a.Name, a));
    }

    [System.Serializable]
    public class UnitBattleStatsData
    {
        public string Name;
        public int Maps = 0;
        public int Battles = 0;
        public int Wins = 0;
        public int Losses = 0;

        public UnitBattleStatsData(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    [System.Serializable]
    public class GameStatsData
    {
        public int TotalTurns = 0;
        public int TotalWins = 0; // Aka level
        public bool Lost = false;
        public float PlayTime = 0;
    }
}

[System.Serializable]
public class SuspendDataRunStatsController
{
    public RunStatsController.GameStatsData GameStats;
    public List<RunStatsController.UnitBattleStatsData> UnitStats;
}
