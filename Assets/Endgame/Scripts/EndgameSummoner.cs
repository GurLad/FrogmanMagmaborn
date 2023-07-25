using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameSummoner : AGameControllerListener
{
    private enum SummonOnUnitMode { Damage, Teleport, EndMarker }
    private enum SummonNoUnitMode { Magmaborn, DeadBoss, Generic, Monster, EndMarker }

    private static Sprite circleSpriteOn; // Terrible workaround but it works
    private static Sprite circleSpriteOff;

    public static EndgameSummoner Current;

    public string ChaosCircleName;
    public float ChaosModifierBaseIncrease;
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
    public string PostCrystalShatterConversation { private get; set; } // TBA
    private Unit torment;
    private float chaosModifier = 0;
    private float chaosModifierIncrease => ChaosModifierBaseIncrease + ChaosModifierTormentHealthMultiplierIncrease * (torment.Stats.Base.MaxHP - torment.Health) / torment.Stats.Base.MaxHP;
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
                ConversationPlayer.Current.PlayOneShot(":callOther:" + PostSummonConversation);
            });
            Crystals.RemoveAt(0);
        }
        // Store current summons
        List<SummonCircle> currentSummons = circles.FindAll(a => a.Summoning);
        // Activate old summons
        foreach (SummonCircle circle in currentSummons)
        {
            if (!circle.Summoning)
            {
                circle.Summoning = false;
                continue;
            }
            Unit onCircle = GameController.Current.FindUnitAtPos(circle.Pos);
            if (onCircle != null)
            {
                SummonActionOnUnit(circle, onCircle);
            }
            else
            {
                SummonActionNoUnit(circle);
            }
        }
        // Begin new summons
        chaosModifier += chaosModifierIncrease;
        for (int i = 0; i < Mathf.FloorToInt(chaosModifier); i++)
        {
            List<SummonCircle> availableCircles = circles.FindAll(a => !a.Summoning && !currentSummons.Contains(a));
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
        Bugger.Info("Chaos is now " + chaosModifier);
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
    }

    private void SummonActionOnUnit(SummonCircle circle, Unit target)
    {
        SummonOnUnitMode mode = (SummonOnUnitMode)Random.Range(0, (int)SummonOnUnitMode.EndMarker);
        switch (mode)
        {
            case SummonOnUnitMode.Damage: // TBA
            case SummonOnUnitMode.Teleport:
                SummonCircle other = circles.FindAll(a => a != circle).RandomItemInList();
                Unit onOtherCircle = GameController.Current.FindUnitAtPos(other.Pos);
                if (onOtherCircle != null) // TBA: Fix animation
                {
                    MapAnimationsController.Current.OnFinishAnimation = () => other.Summoning = circle.Summoning = false;
                    MapAnimationsController.Current.AnimateSwapTeleport(target, onOtherCircle);
                }
                else
                {
                    MapAnimationsController.Current.OnFinishAnimation = () => circle.Summoning = false;
                    MapAnimationsController.Current.AnimateTeleport(target, other.Pos, true);
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
        MapAnimationsController.Current.OnFinishAnimation = () =>
        {
            circle.Summoning = false;
            ConversationPlayer.Current.OnFinishConversation = () => MapAnimationsController.Current.TryPlayNextAnimation();
            // TBA: Set the endgame var which stores the current SummonNoUnitMode
            ConversationPlayer.Current.PlayOneShot(":callOther:" + PostSummonConversation);
        };
        MapAnimationsController.Current.AnimateTeleport(summoned, circle.Pos, false);
    }

    private class SummonCircle
    {
        public Vector2Int Pos;
        public AdvancedSpriteSheetAnimation Tile;
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
}
