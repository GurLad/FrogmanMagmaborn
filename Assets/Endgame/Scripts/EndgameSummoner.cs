using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameSummoner : AGameControllerListener, ISuspendable<SuspendDataEndgameSummoner>
{
    private enum SummonOnUnitMode { Damage, Teleport, EndMarker }
    private enum SummonNoUnitMode { Magmaborn, DeadBoss, Generic, Monster, EndMarker }

    private static Sprite circleSpriteOn; // Terrible workaround but it works
    private static Sprite circleSpriteOff;

    public static EndgameSummoner Current;

    public string ChaosCircleName;
    public float ChaosModifierBaseIncrease;
    public float ChaosModifierTurnIncrease;
    public float ChaosModifierTormentHealthMultiplierIncrease;
    public Sprite CircleSpriteOn;
    public Sprite CircleSpriteOff;
    public List<EndgameCrystal> Crystals;
    // Summon no unit options
    public List<string> SummonOptionsMagmabornTeam2 { private get; set; } // Fashima
    public List<string> SummonOptionsMagmabornTeam3 { private get; set; } // Torment
    public List<string> SummonOptionsDeadBoss { private get; set; }
    public List<string> SummonOptionsGeneric { private get; set; }
    public List<string> SummonOptionsMonster { private get; set; }
    public string PostSummonConversation { private get; set; }
    public string PostCrystalShatterConversation { private get; set; }
    public int LastSummonMode { get; private set; }
    private Unit torment;
    private float chaosModifier = 0;
    private float chaosModifierIncrease =>
        ChaosModifierBaseIncrease +
        ChaosModifierTurnIncrease * (GameController.Current.Turn - 1) +
        ChaosModifierTormentHealthMultiplierIncrease * (torment.Stats.Base.MaxHP - torment.Health) / torment.Stats.Base.MaxHP;
    private List<SummonCircle> circles { get; } = new List<SummonCircle>();

    private void Awake()
    {
        Current = this;
    }

    public void Process(Tile[,] tiles, Vector2Int size)
    {
        circleSpriteOn ??= CircleSpriteOn;
        circleSpriteOff ??= CircleSpriteOff;
        List<Unit> torments = GameController.Current.GetNamedUnits(StaticGlobals.FinalBossName);
        if (torments.Count != 1)
        {
            throw Bugger.Error("There must be exactly 1 unit named Torment for the endgame summoner to work");
        }
        torment = GameController.Current.GetNamedUnits(StaticGlobals.FinalBossName)[0];
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (tiles[i, j].Name == ChaosCircleName)
                {
                    circles.Add(new SummonCircle(new Vector2Int(i, j), tiles[i, j].GetComponent<AdvancedSpriteSheetAnimation>()));
                }
            }
        }
    }

    public override void OnBeginPlayerTurn(List<Unit> units)
    {
        // TEMP
        if (torment == null)
        {
            Process(GameController.Current.Map, GameController.Current.MapSize);
        }
        // Shatter crystals
        if (GameController.Current.Turn % 2 == 1 && Crystals.Count > 0)
        {
            Crystals[0].Shatter(() =>
            {
                ConversationPlayer.Current.OnFinishConversation = () => MapAnimationsController.Current.TryPlayNextAnimation();
                ConversationPlayer.Current.PlayOneShot(":callOther:" + PostCrystalShatterConversation);
            });
            Crystals.RemoveAt(0);
        }
        // Store current summons
        List<SummonCircle> currentSummons = circles.FindAll(a => a.Summoning);
        // Activate old summons
        circles.ForEach(a => a.OnCircle = GameController.Current.FindUnitAtPos(a.Pos));
        currentSummons = currentSummons.Shuffle();
        foreach (SummonCircle circle in currentSummons)
        {
            if (!circle.Summoning)
            {
                circle.Summoning = false;
                continue;
            }
            if (circle.OnCircle != null)
            {
                SummonActionOnUnit(circle, circle.OnCircle);
            }
            else
            {
                SummonActionNoUnit(circle);
            }
        }
        // Begin new summons
        if (!MidBattleScreen.HasCurrent)
        {
            BeginNewSummons();
        }
        else
        {
            MapAnimationsController.Current.OnFinishAnimation = BeginNewSummons;
            MapAnimationsController.Current.AnimateDelay();
        }
    }

    public override void OnEndLevel(List<Unit> units, bool playerWon)
    {
        // Do nothing
    }

    public override void OnPlayerWin(List<Unit> units)
    {
        // Do nothing
    }

    public void SummonWisp()
    {
        bool yAxis = Random.Range(0, 2) == 0;
        Vector2Int size = GameController.Current.MapSize;
        Vector2Int pos;
        int attemps = 0; // Failsafe - shouldn't ever happen
        do
        {
            pos = new Vector2Int(Random.Range(0, yAxis ? size.x : 2) * (yAxis ? 1 : (size.x - 1)), Random.Range(0, yAxis ? 2 : size.y) * (yAxis ? (size.y - 1) : 1));
        } while (GameController.Current.FindUnitAtPos(pos) != null && attemps++ < 100);
        Unit summoned = GameController.Current.CreateUnit(StaticGlobals.FinalBossMinionName, 0, Team.Guard, false, PortraitLoadingMode.Team);
        summoned.Pos = pos;
        summoned.AIType = AIType.Beeline;
        summoned.AIData = StaticGlobals.FinalBossName;
        summoned.Stats.Base.Endurance = 1;
        summoned.Health = summoned.Stats.Base.MaxHP;
    }

    private void SummonActionOnUnit(SummonCircle circle, Unit target)
    {
        SummonOnUnitMode mode = (SummonOnUnitMode)Random.Range(0, (int)SummonOnUnitMode.EndMarker);
        switch (mode)
        {
            case SummonOnUnitMode.Damage: // TBA
            case SummonOnUnitMode.Teleport:
                SummonCircle other = circles.FindAll(a => a != circle).RandomItemInList();
                if (other.OnCircle != null)
                {
                    //Bugger.Info("Swapping " + target + " " + circle.Pos + " and " + other.OnCircle + " " + other.Pos);
                    MapAnimationsController.Current.OnFinishAnimation = () => circle.Summoning = false;
                    MapAnimationsController.Current.AnimateSwapTeleport(target, other.OnCircle, circle.Pos, other.Pos);
                    circle.OnCircle = other.OnCircle;
                    other.OnCircle = target;
                }
                else
                {
                    //Bugger.Info("Teleporting " + target + " " + circle.Pos + " to " + other.Pos);
                    MapAnimationsController.Current.OnFinishAnimation = () => circle.Summoning = false;
                    MapAnimationsController.Current.AnimateTeleport(target, other.Pos, true);
                    circle.OnCircle = null;
                    other.OnCircle = target;
                }
                break;
            case SummonOnUnitMode.EndMarker:
                break;
            default:
                break;
        }
    }

    private void SummonActionNoUnit(SummonCircle circle)
    {
        SummonNoUnitMode mode = (SummonNoUnitMode)Random.Range(0, (int)SummonNoUnitMode.EndMarker);
        List<string> options = null;
        Team team = Team.Player;
        PortraitLoadingMode portraitLoadingMode = PortraitLoadingMode.Team;
        switch (mode)
        {
            case SummonNoUnitMode.Magmaborn:
                int team2Count = GameController.Current.GetUnitNames(Team.Monster).Count;
                int team3Count = GameController.Current.GetUnitNames(Team.Guard).Count;
                team = Random.Range(0, team2Count + team3Count) >= team2Count ? Team.Monster : Team.Guard;
                options = team == Team.Monster ? SummonOptionsMagmabornTeam2 : SummonOptionsMagmabornTeam3;
                portraitLoadingMode = PortraitLoadingMode.Name;
                break;
            case SummonNoUnitMode.DeadBoss:
                options = SummonOptionsDeadBoss;
                team = Team.Player;
                portraitLoadingMode = PortraitLoadingMode.Name;
                break;
            case SummonNoUnitMode.Generic:
                options = SummonOptionsGeneric;
                team = Team.Player;
                portraitLoadingMode = PortraitLoadingMode.Generic;
                break;
            case SummonNoUnitMode.Monster:
                options = SummonOptionsMonster;
                team = Team.Player;
                portraitLoadingMode = PortraitLoadingMode.Team;
                break;
            case SummonNoUnitMode.EndMarker:
                break;
            default:
                break;
        }
        Unit summoned = GameController.Current.CreateUnit(options.RandomItemInList(), GameController.Current.LevelNumber, team, false, portraitLoadingMode);
        summoned.Moved = true;
        circle.OnCircle = summoned;
        MapAnimationsController.Current.OnFinishAnimation = () =>
        {
            circle.Summoning = false;
            LastSummonMode = (int)mode;
            PortraitController.Current.AddPortraitAlias("Summoned", summoned.Icon);
            ConversationPlayer.Current.OnFinishConversation = () => MapAnimationsController.Current.TryPlayNextAnimation();
            // TBA: Set the endgame var which stores the current SummonNoUnitMode
            ConversationPlayer.Current.PlayOneShot(":callOther:" + PostSummonConversation);
        };
        MapAnimationsController.Current.AnimateTeleport(summoned, circle.Pos, false);
    }

    private void BeginNewSummons()
    {
        chaosModifier += chaosModifierIncrease;
        for (int i = 0; i < Mathf.FloorToInt(chaosModifier); i++)
        {
            List<SummonCircle> availableCircles = circles.FindAll(a => !a.Summoning);
            if (availableCircles.Count > 0)
            {
                SummonCircle selected = availableCircles.RandomItemInList();
                //Bugger.Info("Available: " + string.Join(", ", availableCircles.ConvertAll(a => a.Pos)) + ", chose: " + selected.Pos);
                selected.Summoning = true;
            }
            else
            {
                break;
            }
            chaosModifier--;
        }
    }

    public SuspendDataEndgameSummoner SaveToSuspendData()
    {
        SuspendDataEndgameSummoner suspendData = new SuspendDataEndgameSummoner();
        suspendData.SummonOptionsMagmabornTeam2 = SummonOptionsMagmabornTeam2;
        suspendData.SummonOptionsMagmabornTeam3 = SummonOptionsMagmabornTeam3;
        suspendData.SummonOptionsDeadBoss = SummonOptionsDeadBoss;
        suspendData.SummonOptionsGeneric = SummonOptionsGeneric;
        suspendData.SummonOptionsMonster = SummonOptionsMonster;
        suspendData.PostSummonConversation = PostSummonConversation;
        suspendData.PostCrystalShatterConversation = PostCrystalShatterConversation;
        suspendData.ChaosModifier = chaosModifier;
        suspendData.Circles = circles.ConvertAll(a => new SummonCircleData(a.Pos, a.Summoning));
        suspendData.CrystalCount = Crystals.Count;
        suspendData.EndgameOn = true;
        return suspendData;
    }

    public void LoadFromSuspendData(SuspendDataEndgameSummoner data)
    {
        SummonOptionsMagmabornTeam2 = data.SummonOptionsMagmabornTeam2;
        SummonOptionsMagmabornTeam3 = data.SummonOptionsMagmabornTeam3;
        SummonOptionsDeadBoss = data.SummonOptionsDeadBoss;
        SummonOptionsGeneric = data.SummonOptionsGeneric;
        SummonOptionsMonster = data.SummonOptionsMonster;
        PostSummonConversation = data.PostSummonConversation;
        PostCrystalShatterConversation = data.PostCrystalShatterConversation;
        chaosModifier = data.ChaosModifier;
        Process(GameController.Current.Map, GameController.Current.MapSize);
        data.Circles.ForEach(a => circles.Find(b => b.Pos == a.Pos).Summoning = a.Summoning);
        while (Crystals.Count > data.CrystalCount)
        {
            Destroy(Crystals[0].gameObject);
            Crystals.RemoveAt(0);
        }
    }

    private class SummonCircle
    {
        public Vector2Int Pos;
        public AdvancedSpriteSheetAnimation Tile;
        public Unit OnCircle = null;
        private bool _summoning = false;
        public bool Summoning
        {
            get => _summoning;
            set
            {
                _summoning = value;
                Tile.Animations[0].SpriteSheet = value ? circleSpriteOn : circleSpriteOff;
                Tile.Animations[0].Split();
            }
        }

        public SummonCircle(Vector2Int pos, AdvancedSpriteSheetAnimation tile)
        {
            Pos = pos;
            Tile = tile;
            Tile.gameObject.transform.position -= new Vector3(0, 0, 0.1f);
        }
    }

    [System.Serializable]
    public class SummonCircleData
    {
        public Vector2Int Pos;
        public bool Summoning;

        public SummonCircleData() { }

        public SummonCircleData(Vector2Int pos, bool summoning)
        {
            Pos = pos;
            Summoning = summoning;
        }
    }
}

[System.Serializable]
public class SuspendDataEndgameSummoner
{
    public bool EndgameOn = false;
    public List<string> SummonOptionsMagmabornTeam2; // Fashima
    public List<string> SummonOptionsMagmabornTeam3; // Torment
    public List<string> SummonOptionsDeadBoss;
    public List<string> SummonOptionsGeneric;
    public List<string> SummonOptionsMonster;
    public string PostSummonConversation;
    public string PostCrystalShatterConversation;
    public float ChaosModifier = 0;
    public List<EndgameSummoner.SummonCircleData> Circles;
    public int CrystalCount;
}
