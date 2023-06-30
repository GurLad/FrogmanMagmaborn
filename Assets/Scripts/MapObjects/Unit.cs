using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AttackFrom = System.Collections.Generic.List<UnityEngine.Vector2Int>;

public class Unit : MapObject
{
    [Header("Basic info")]
    public Team TheTeam;
    public string Name;
    public string DisplayName;
    public string Class;
    public AIType AIType;
    [Header("Stats")]
    public bool Flies;
    public UnitStats Stats;
    public Inclination Inclination;
    [Header("AI Values")]
    public int MaxAcceptableHitRisk = 50;
    public AIPriorities Priorities;
    [Header("Objects")]
    public MoveMarker MovementMarker;
    public AttackMarker AttackMarker;
    public SpriteRenderer Symbol;
    [Header("MultiTile extra fields")]
    public MultiTileMoveMarker MultiTileMoveMarker;
    [Header("Misc")]
    public PortraitLoadingMode PortraitMode = PortraitLoadingMode.None; // Cannot use PortraitLoadingMode? for some reason...
    [HideInInspector]
    public Weapon Weapon;
    [HideInInspector]
    public int Level;
    [HideInInspector]
    public string BattleQuote = "";
    [HideInInspector]
    public string DeathQuote = "";
    [HideInInspector]
    [System.NonSerialized]
    public Portrait Icon;
    [HideInInspector]
    public int Health;
    [HideInInspector]
    public Vector2Int PreviousPos;
    [HideInInspector]
    public int ReinforcementTurn;
    [HideInInspector]
    public bool Statue;
    public bool InsideMap => !(Pos == -Vector2Int.one && ReinforcementTurn > 0 && !Statue);
    private PalettedSprite palette;
    private bool started;
    private List<IUnitListener> listeners { get; } = new List<IUnitListener>();
    [SerializeField]
    private SkillSet skills = new SkillSet();
    [SerializeField]
    private List<string> enemyOverrides = new List<string>();
    [SerializeField]
    private bool moved;
    public bool Moved
    {
        get
        {
            return Statue || moved;
        }
        set
        {
            moved = value;
            palette.Palette = moved ? 3 : (int)TheTeam;
            Symbol.sprite = moved ? GameController.Current.LevelMetadata.TeamDatas[(int)TheTeam].MovedSymbol : GameController.Current.LevelMetadata.TeamDatas[(int)TheTeam].BaseSymbol;
        }
    }
    private int movement = 5;
    public int Movement
    {
        get
        {
            return AIType == AIType.Guard ? 0 : movement;
        }
        set
        {
            movement = value;
        }
    }

    public Unit()
    {
        Priorities = new AIPriorities(this);
    }

    public virtual void Init(bool callStart = false)
    {
        palette = GetComponent<PalettedSprite>();
        //if (palette == null)
        //{
        //    throw new System.Exception("No palette (Init)!");
        //}
        palette.Awake();
        //Start(); // There is a very weird bug - units created with CreatePlayerUnit don't activate their Start function. This is a bad workaround.
        if (callStart)
        {
            Start();
        }
    }

    protected override void Start()
    {
        ReflectSettingsUpdate();
        if (started)
        {
            return;
        }
        started = true;
        base.Start();
        palette = palette ?? GetComponent<PalettedSprite>();
        //if (palette == null)
        //{
        //    throw new System.Exception("No palette (Start)!");
        //}
        LoadIcon();
        Moved = Statue;
        Health = Stats.Base.MaxHP; // Max HP cannot be affected by modifiers (otherwise it would be weird)
    }

    private void LoadIcon()
    {
        switch (PortraitMode == PortraitLoadingMode.None ? (PortraitMode = GameController.Current.LevelMetadata.TeamDatas[(int)TheTeam].PortraitLoadingMode) : PortraitMode)
        {
            case PortraitLoadingMode.Name:
                Icon = PortraitController.Current.FindPortrait(Name);
                break;
            case PortraitLoadingMode.Team:
                Icon = PortraitController.Current.FindPortrait(TheTeam.Name());
                Name = Class;
                break;
            case PortraitLoadingMode.Generic:
                Icon = PortraitController.Current.GenerateGenericPortrait().Portrait;
                DisplayName = Icon.TheDisplayName;
                break;
            default:
                throw Bugger.Error("Invalid portrait loading mode!");
        }
    }

    public void SetIcon(Portrait icon, bool changeName = true)
    {
        Icon = icon;
        if (changeName)
        {
            DisplayName = icon.TheDisplayName;
        }
    }

    public override void Interact(InteractState interactState)
    {
        switch (interactState)
        {
            case InteractState.None:
                GameController.Current.RemoveMarkers();
                MovementMarker.PalettedSprite.Palette = (int)TheTeam;
                if (TheTeam == GameController.Current.CurrentPhase && !Moved)
                {
                    SelectOrder();
                    SystemSFXController.Play(SystemSFXController.Type.UnitSelect);
                }
                else if (TheTeam != GameController.Current.CurrentPhase)
                {
                    MarkDangerArea(Pos.x, Pos.y, Movement, true);
                    GameController.Current.ShowPointerMarker(this, (int)TheTeam);
                }
                break;
            case InteractState.Move:
                break;
            case InteractState.Attack:
                break;
            default:
                break;
        }
    }

    protected virtual DangerArea GetDangerArea(int x, int y, int range, bool includePassThroughMoves = false)
    {
        return DangerArea.Generate(this, x, y, range, includePassThroughMoves);
    }

    protected virtual void GenerateMultiTileMoveMarker(int i, int j, DangerArea dangerArea)
    {
        throw Bugger.FMError("Non-multi-tile unit tried to generate a danger area with multi-tiles?!");
    }

    private DangerArea GetDangerArea(bool includePassThroughMoves = false)
    {
        return GetDangerArea(Pos.x, Pos.y, Movement, includePassThroughMoves);
    }

    private void MarkDangerArea(int x, int y, int range, bool includePassThroughMoves = false)
    {
        void GenerateMoveMarker(int i, int j)
        {
            MoveMarker movementMarker = Instantiate(MovementMarker.gameObject).GetComponent<MoveMarker>();
            movementMarker.Pos = new Vector2Int(i, j);
            movementMarker.Origin = this;
            movementMarker.ShowArmorIcon();
            movementMarker.gameObject.SetActive(true);
        }

        void GenerateAttackMarker(int i, int j, DangerArea dangerArea)
        {
            AttackMarker attackMarker = Instantiate(AttackMarker.gameObject).GetComponent<AttackMarker>();
            attackMarker.Pos = new Vector2Int(i, j);
            attackMarker.Origin = this;
            attackMarker.DangerArea = dangerArea;
            attackMarker.gameObject.SetActive(true);
        }

        MovementMarker.PalettedSprite.Palette = (int)TheTeam;
        DangerArea dangerArea = GetDangerArea(x, y, range, includePassThroughMoves);
        for (int i = 0; i < GameController.Current.MapSize.x; i++)
        {
            for (int j = 0; j < GameController.Current.MapSize.y; j++)
            {
                switch (dangerArea[i, j].Type)
                {
                    case DangerArea.TileDataType.Inaccessible:
                        break;
                    case DangerArea.TileDataType.Move:
                        GenerateMoveMarker(i, j);
                        break;
                    case DangerArea.TileDataType.PassThrough:
                        if (!includePassThroughMoves)
                        {
                            throw Bugger.FMError("Pass through tiles found when ignoring pass throughs.", false);
                        }
                        else
                        {
                            GenerateMoveMarker(i, j);
                        }
                        break;
                    case DangerArea.TileDataType.Attack:
                        GenerateAttackMarker(i, j, dangerArea);
                        break;
                    case DangerArea.TileDataType.MultiTileMove:
                        GenerateMultiTileMoveMarker(i, j, dangerArea);
                        break;
                    default:
                        break;
                }
            }
        }
    }
    /// <summary>
    /// Select the unit, then show move markers (aka select playable unit).
    /// </summary>
    public void SelectOrder()
    {
        MarkDangerArea(Pos.x, Pos.y, Movement, false);
        GameController.Current.InteractState = InteractState.Move;
        GameController.Current.ShowPointerMarker(this, (int)TheTeam);
        GameController.Current.Selected = this;
    }
    /// <summary>
    /// Moves the unit to the target pos, then shows attack markers (aka move playable unit).
    /// </summary>
    /// <param name="Pos">Target pos to move to.</param>
    public void MoveOrder(Vector2Int Pos)
    {
        GameController.Current.RemoveMarkers();
        MapAnimationsController.Current.OnFinishAnimation = () =>
        {
            MarkAttack();
            GameController.Current.ShowPointerMarker(this, 3);
            GameController.Current.InteractState = InteractState.Attack;
        };
        MoveTo(Pos);
    }

    public List<Vector2Int> FindPath(Vector2Int targetPos)
    {
        if (targetPos == Pos) // The path is empty, as the target pos is this pos
        {
            return new List<Vector2Int>();
        }
        DangerArea dangerArea = GetDangerArea(true); // Cannot rely on given one, as will probably not ignore allies.
        // Recover path (slightly different from the AI one, find a way to merge them?)
        List<Vector2Int> path = new List<Vector2Int>();
        int counter = 0;
        do
        {
            if (counter++ > 50)
            {
                throw Bugger.FMError("Infinite loop in AnimatedMovement! Path: " + string.Join(", ", path), false);
            }
            path.Add(targetPos);
            Vector2Int currentBest = Vector2Int.zero;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if ((i != 0 && j == 0) || (j != 0 && i == 0))
                    {
                        if (GameController.Current.IsValidPos(targetPos.x + i, targetPos.y + j) &&
                            (dangerArea[targetPos.x + i, targetPos.y + j].Value > dangerArea[targetPos.x + currentBest.x, targetPos.y + currentBest.y].Value ||
                             (dangerArea[targetPos.x + i, targetPos.y + j].Value == dangerArea[targetPos.x + currentBest.x, targetPos.y + currentBest.y].Value &&
                              (targetPos + new Vector2Int(i, j)).TileDist(Pos) < (targetPos + currentBest).TileDist(Pos))))
                        {
                            currentBest = new Vector2Int(i, j);
                        }
                    }
                }
            }
            if (currentBest == Vector2Int.zero)
            {
                throw Bugger.FMError("Path recovery failed! Path: " + string.Join(", ", path), false);
            }
            targetPos += currentBest;
        } while (targetPos != Pos);
        path.Reverse();
        return path;
    }

    public void MarkDangerArea()
    {
        MarkDangerArea(Pos.x, Pos.y, Movement, true);
    }

    public void MarkAttack() //(int x = -1, int y = -1, int range = -1, bool[,] checkedTiles = null)
    {
        DangerArea dangerArea = GetDangerArea(Pos.x, Pos.y, 0, false);
        if (HasSkill(Skill.SiegeWeapon)) // Just mark everything
        {
            for (int i = 0; i < GameController.Current.MapSize.x; i++)
            {
                for (int j = 0; j < GameController.Current.MapSize.y; j++)
                {
                    AttackMarker attackMarker = Instantiate(AttackMarker.gameObject).GetComponent<AttackMarker>();
                    attackMarker.Pos = new Vector2Int(i, j);
                    attackMarker.Origin = this;
                    attackMarker.gameObject.SetActive(true);
                }
            }
            return;
        }
        for (int i = Pos.x - Weapon.Range; i <= GameController.Current.MapSize.x; i++) // In case of multi-tiles...
        {
            for (int j = Pos.y - Weapon.Range; j <= GameController.Current.MapSize.y; j++)
            {
                if (GameController.Current.IsValidPos(i, j) && dangerArea[i, j].Value != 0)
                {
                    AttackMarker attackMarker = Instantiate(AttackMarker.gameObject).GetComponent<AttackMarker>();
                    attackMarker.Pos = new Vector2Int(i, j);
                    attackMarker.Origin = this;
                    attackMarker.gameObject.SetActive(true);
                }
            }
        }
        //if (range == -1)
        //{
        //    range = Weapon.Range + 1;
        //    x = Pos.x;
        //    y = Pos.y;
        //}
        //checkedTiles = checkedTiles ?? new bool[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
        //if (checkedTiles[x, y] || !GameController.Current.Map[x, y].Passable)
        //{
        //    return;
        //}
        //else
        //{
        //    AttackMarker attackMarker = Instantiate(AttackMarker.gameObject).GetComponent<AttackMarker>();
        //    attackMarker.Pos = new Vector2Int(x, y);
        //    attackMarker.Origin = this;
        //    attackMarker.gameObject.SetActive(true);
        //}
        //checkedTiles[x, y] = true;
        //if (range - 1 > 0)
        //{
        //    for (int i = -1; i <= 1; i++)
        //    {
        //        for (int j = -1; j <= 1; j++)
        //        {
        //            if (i == 0 || j == 0)
        //            {
        //                if (!GameController.Current.IsValidPos(x + i,y + j))
        //                {
        //                    continue;
        //                }
        //                MarkAttack(x + i, y + j, range - 1, checkedTiles);
        //            }
        //        }
        //    }
        //}
    }
    /// <summary>
    /// Moves the unit to the given pos. If immediate is false, also animates the movement.
    /// </summary>
    /// <param name="pos">Target pos to move to.</param>
    /// <param name="immediate">Skip the move animation?</param>
    public void MoveTo(Vector2Int pos, bool immediate = false)
    {
        // Add animation etc.
        PreviousPos = Pos;
        if (!immediate)
        {
            if (!TheTeam.PlayerControlled() && pos == Pos) // Only delay when enemies stay in place
            {
                MapAnimationsController.Current.AnimateDelay();
            }
            else
            {
                GameController.Current.AutoSaveSaveAction(SuspendDataGameController.CurrentAction.ActionType.Move, Pos, pos, "");
                MapAnimationsController.Current.AnimateMovement(this, pos);
            }
        }
        else
        {
            Pos = pos;
        }
    }

    public void Fight(Unit unit, float attackerRandomResult = -1, float defenderRandomResult = -1)
    {
        // Stats - increase battle count if either unit is a player
        if (TheTeam.PlayerControlled())
        {
            SavedData.Append("Statistics", ToString() + "BattleCount", 1);
        }
        else if (unit.TheTeam.PlayerControlled())
        {
            SavedData.Append("Statistics", unit.ToString() + "BattleCount", 1);
        }
        // Determine who attacks first
        Unit attacker, defender;
        if (unit.CanAttack(this) && ((unit.HasSkill(Skill.Vantage) && !HasSkill(Skill.Vantage)) || (HasSkill(Skill.AntiVantage) && !unit.HasSkill(Skill.AntiVantage))))
        {
            attacker = unit;
            defender = this;
        }
        else
        {
            attacker = this;
            defender = unit;
        }
        // Save the result beforehand to prevent cheating
        attackerRandomResult = attackerRandomResult < 0 ? GameCalculations.GetRandomHitResult() : attackerRandomResult;
        defenderRandomResult = defenderRandomResult < 0 ? GameCalculations.GetRandomHitResult() : defenderRandomResult;
        GameController.Current.AutoSaveSaveAction(
            SuspendDataGameController.CurrentAction.ActionType.Combat,
            attacker.Pos, defender.Pos, Pos.x + "," + Pos.y + ";" + attackerRandomResult + "," + defenderRandomResult);
        // Actual fight
        if (BattleQuote != "" || unit.BattleQuote != "") // Battle quotes achieve the same effect as delays
        {
            GameController.Current.RemoveMarkers();
            PortraitController.Current.AddPortraitAlias("Attacker", Icon);
            PortraitController.Current.AddPortraitAlias("Defender", unit.Icon);
            ConversationPlayer.Current.OnFinishConversation = () =>
                GameController.Current.Fight(this, attacker, defender, attackerRandomResult, defenderRandomResult);
            string quote;
            if (BattleQuote != "")
            {
                quote = BattleQuote;
                BattleQuote = "";
            }
            else
            {
                quote = unit.BattleQuote;
                unit.BattleQuote = "";
            }
            ConversationPlayer.Current.PlayOneShot(quote);
        }
        else if (TheTeam.PlayerControlled()) // The player know who they're attacking, no need for a delay.
        {
            GameController.Current.Fight(this, attacker, defender, attackerRandomResult, defenderRandomResult);
        }
        else // If an enemy attacks, however, a delay is needed in order to show the target.
        {
            MapAnimationsController.Current.OnFinishAnimation = () =>
                GameController.Current.Fight(this, attacker, defender, attackerRandomResult, defenderRandomResult);
            MapAnimationsController.Current.AnimateDelay();
        }
    }

    public bool CanPush(Unit unit)
    {
        Vector2Int truePos = -(Pos - unit.Pos) + unit.Pos;
        return
            HasSkill(Skill.Push) && unit.Pos.TileDist(Pos) <= 1 &&
            GameController.Current.IsValidPos(truePos.x, truePos.y) && GameController.Current.Map[truePos.x, truePos.y].GetMovementCost(unit) <= unit.Movement &&
            !GameController.Current.FindUnitAtPos(truePos.x, truePos.y);
    }

    public bool CanPull(Unit unit)
    {
        Vector2Int truePos = (Pos - unit.Pos) + Pos;
        return
            HasSkill(Skill.Pull) && unit.Pos.TileDist(Pos) <= 1 &&
            GameController.Current.IsValidPos(truePos.x, truePos.y) && GameController.Current.Map[truePos.x, truePos.y].GetMovementCost(this) <= Movement &&
            !GameController.Current.FindUnitAtPos(truePos.x, truePos.y);
    }

    public void Push(Unit unit)
    {
        GameController.Current.AutoSaveSaveAction(SuspendDataGameController.CurrentAction.ActionType.Push, Pos, unit.Pos, "");
        GameController.Current.RemoveMarkers();
        MapAnimationsController.Current.OnFinishAnimation = () => GameController.Current.FinishMove(this);
        MapAnimationsController.Current.AnimatePushPull(this, unit, true);
    }

    public void Pull(Unit unit)
    {
        GameController.Current.AutoSaveSaveAction(SuspendDataGameController.CurrentAction.ActionType.Pull, Pos, unit.Pos, "");
        GameController.Current.RemoveMarkers();
        MapAnimationsController.Current.OnFinishAnimation = () => GameController.Current.FinishMove(this);
        MapAnimationsController.Current.AnimatePushPull(this, unit, false);
    }

    public int CountAdjacentAllies(Vector2Int pos)
    {
        int count = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                Unit unitAtPos = GameController.Current.FindUnitAtPos(pos.x + i, pos.y + j);
                if (Mathf.Abs(i) + Mathf.Abs(j) <= 1 && Mathf.Abs(i) + Mathf.Abs(j) > 0 &&
                    !(unitAtPos?.IsEnemy(this) ?? true) && ((unitAtPos ?? this) != this))
                {
                    count++;
                }
            }
        }
        return count;
    }

    protected virtual Vector2Int InDangerArea(DangerArea dangerArea)
    {
        return dangerArea[Pos.x, Pos.y].Value != 0 ? Pos : -Vector2Int.one;
    }

    public void AI(List<Unit> units)
    {
        List<Unit> enemyUnits = units.Where(a => a.IsEnemy(this) && Priorities.ShouldAttack(a) && a.InsideMap).ToList(); // Pretty much all AIs need enemy units.
        switch (AIType)
        {
            case AIType.Charge:
                // First, try the Hold AI.
                if (TryHoldAI(enemyUnits))
                {
                    break;
                }
                // If that failed, find the closest enemy.
                DangerArea fullDangerArea = GetDangerArea(Pos.x, Pos.y, 50, true);
                enemyUnits = enemyUnits.Where(a => fullDangerArea[a.Pos.x, a.Pos.y].Value != 0).ToList();
                if (enemyUnits.Count <= 0) // Can't attack anyone - probably surrounded by scary enemies
                {
                    //Bugger.Info(ToString() + " can't attack anyone - probably surrounded by scary enemies - and retreats");
                    RetreatAI(fullDangerArea);
                    return;
                }
                Unit target = enemyUnits[0];
                enemyUnits.RemoveAt(0);
                if (enemyUnits.Count > 0)
                {
                    int targetDist = GetMoveRequiredToReachPos(target.Pos, 50, fullDangerArea);
                    foreach (Unit enemy in enemyUnits)
                    {
                        int dist = GetMoveRequiredToReachPos(enemy.Pos, 50, fullDangerArea);
                        if (dist < targetDist)
                        {
                            target = enemy;
                            targetDist = dist;
                        }
                    }
                }
                // Now, recover a path.
                Vector2Int moveTarget = ClosestMoveablePointToTarget(target.Pos, fullDangerArea);
                // Finally, move to the target location.
                MapAnimationsController.Current.OnFinishAnimation = () => GameController.Current.FinishMove(this);
                MoveTo(moveTarget);
                break;
            case AIType.Hold:
                if (!TryHoldAI(enemyUnits))
                {
                    MapAnimationsController.Current.OnFinishAnimation = () => GameController.Current.FinishMove(this);
                    MapAnimationsController.Current.AnimateDelay();
                }
                break;
            case AIType.Guard:
                DangerArea dangerArea = GetDangerArea();
                enemyUnits.Sort((a, b) => HoldAITargetValue(a).CompareTo(HoldAITargetValue(b)));
                foreach (Unit unit in enemyUnits)
                {
                    if (dangerArea[unit.Pos.x, unit.Pos.y].Type == DangerArea.TileDataType.Attack)
                    {
                        Fight(unit);
                        return;
                    }
                }
                MapAnimationsController.Current.OnFinishAnimation = () => GameController.Current.FinishMove(this);
                MapAnimationsController.Current.AnimateDelay();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Tries the Hold AI. If successful (returns true), starts the animations and ends the turn - the caller MUSTN'T activate another AI.
    /// Otherwise, does nothing - the caller MUST activate a fallback AI (or end the turn).
    /// </summary>
    /// <returns>True if found and attacked a unit, false if did nothing.</returns>
    private bool TryHoldAI(List<Unit> enemyUnits)
    {
        DangerArea dangerArea = GetDangerArea();
        //enemyUnits.Sort((a, b) => HoldAITargetValue(a).CompareTo(HoldAITargetValue(b)));
        enemyUnits = enemyUnits.OrderBy(a => HoldAITargetValue(a)).ToList();
        foreach (Unit unit in enemyUnits)
        {
            //Bugger.Info(this + " is trying " + unit + ", which has AI value " + HoldAITargetValue(unit));
            Vector2Int pos = unit.InDangerArea(dangerArea);
            if (pos != -Vector2Int.one)
            {
                Vector2Int currentBest = dangerArea.GetBestPosToAttackTargetFrom(pos, -1);
                //Bugger.Info(this + " is moving to " + currentBest + " in order to attack " + unit + " (value " + HoldAITargetValue(unit) + ")");
                MapAnimationsController.Current.OnFinishAnimation = () => Fight(unit);
                MoveTo(currentBest);
                return true;
            }
        }
        return false;
    }

    private void RetreatAI(DangerArea fullDangerArea)
    {
        Vector2Int minPoint = -Vector2Int.one;
        for (int x = 0; x < GameController.Current.MapSize.x; x++)
        {
            for (int y = 0; y < GameController.Current.MapSize.y; y++)
            {
                if (x == 0 || y == 0 || x == GameController.Current.MapSize.x - 1 || y == GameController.Current.MapSize.y - 1)
                {
                    if (fullDangerArea[x, y].Type == DangerArea.TileDataType.Move)
                    {
                        if (minPoint == -Vector2Int.one || fullDangerArea[minPoint.x, minPoint.y].Value < fullDangerArea[x, y].Value)
                        {
                            minPoint = new Vector2Int(x, y);
                        }
                    }
                }
            }
        }
        if (minPoint == -Vector2Int.one)
        {
            //Bugger.Info(this + " can't move :(");
            MapAnimationsController.Current.OnFinishAnimation = () => GameController.Current.FinishMove(this);
            MoveTo(Pos);
            return;
        }
        Vector2Int target = ClosestMoveablePointToTarget(minPoint, fullDangerArea);
        if (target.x == 0 || target.y == 0 || target.x == GameController.Current.MapSize.x - 1 || target.y == GameController.Current.MapSize.y - 1) // Can reach the end of the map, aka retreat, aka die
        {
            MapAnimationsController.Current.OnFinishAnimation = () =>
            {
                DeathQuote = ""; // Retreating doesn't use a death quote
                MapAnimationsController.Current.OnFinishAnimation = () => GameController.Current.KillUnit(this);
                MapAnimationsController.Current.AnimateDelay();
            };
        }
        else
        {
            MapAnimationsController.Current.OnFinishAnimation = () => GameController.Current.FinishMove(this);
        }
        MoveTo(target);
    }

    private Vector2Int ClosestMoveablePointToTarget(Vector2Int target, DangerArea fullDangerArea)
    {
        DangerArea trueDangerArea = GetDangerArea(Pos.x, Pos.y, Movement);
        Vector2Int currentMoveTarget = new Vector2Int(target.x, target.y);
        GameController.Current.RemoveMarkers();
        // This is problematic, because here's how the algorithm works for 1 range:
        //  - Start at the target pos
        //  - In each iteration, try all 4 directions. The one with the highest remaining mov (aka min mov required to reach) is the new target
        //  - If we can reach the target this turn, do so and end the loop
        // However, in >1 range, there might be locations you can only acces diagonaly, ex (T is target, X is low wall, - is nothing):
        // - X
        // X T
        // So we must check Weapon.Range instead of just the 4 adjacent squares.
        // But then there's a problem - the algorithm prefers the target with the highest amount of remaining mov.
        // Which means it won't use the full move, and move slower than 1 range units, in most situations if we check Weapon.Range
        // So we use Weapon.Range for only the first iteration, to fix the above situation...
        // ...and then change to 1 range checking to utilize the full movement.
        int range = Mathf.Max(Weapon.Range, 1); // So that 0 range units can retreat properly
        while (trueDangerArea[currentMoveTarget.x, currentMoveTarget.y].Type != DangerArea.TileDataType.Move)
        {
            Vector2Int min = currentMoveTarget;
            for (int i = -range; i <= range; i++)
            {
                for (int j = -range; j <= range; j++)
                {
                    if (Mathf.Abs(i) + Mathf.Abs(j) <= range && Mathf.Abs(i) + Mathf.Abs(j) > 0 &&
                        GameController.Current.IsValidPos(currentMoveTarget.x + i, currentMoveTarget.y + j) &&
                        fullDangerArea[currentMoveTarget.x + i, currentMoveTarget.y + j].Value > 0 &&
                        (fullDangerArea[min.x, min.y].Value < fullDangerArea[currentMoveTarget.x + i, currentMoveTarget.y + j].Value ||
                         (fullDangerArea[min.x, min.y].Value == fullDangerArea[currentMoveTarget.x + i, currentMoveTarget.y + j].Value &&
                          min.TileDist(Pos) > currentMoveTarget.TileDist(Pos))) &&
                        (range <= 1 || CanAttackPos(target, new Vector2Int(currentMoveTarget.x + i, currentMoveTarget.y + j))))
                    {
                        min = new Vector2Int(currentMoveTarget.x + i, currentMoveTarget.y + j);
                        //Bugger.Info("Updated min to " + min + ", value: " + fullDangerArea[currentMoveTarget.x + i, currentMoveTarget.y + j].Value);
                    }
                }
            }
            if (min == currentMoveTarget)
            {
                throw Bugger.FMError("Path not found... to a target with a verified path! This should be impossible... Pos: " + Pos + ", attacker: " + ToString() + ", target: " + target, false);
            }
            currentMoveTarget = min;
            range = 1;
        }
        return currentMoveTarget;
    }

    private float HoldAITargetValue(Unit unit)
    {
        int trueDamage = this.GetDamage(unit);
        int hit = this.GetHitChance(unit);
        int damage = Priorities.GetAIDamageValue(unit);

        // If can kill, value is -100, so AI will always prioritize killing
        if (unit.Health - damage <= 0 && hit >= MaxAcceptableHitRisk)
        {
            return -100;
        }
        // If can kill with a risky move, return -unit.Health / 2, so enemy will be more chaotic (but still prefer a more consistent kill)
        if (unit.Health - trueDamage <= 0 && hit >= MaxAcceptableHitRisk)
        {
            return -unit.Health / 2 + Priorities.GetTotalPriority(unit);
        }
        // If can't damage, return 100, so enemy will never choose to deal no damage over actually dealing damage
        if (damage <= 0)
        {
            if (trueDamage > 0)
            {
                return 100 - hit / 10;
            }
            else
            {
                return 100;
            }
        }
        // Otherwise, time to calculate true weight!
        //Bugger.Info(ToString() + " AI values against " + unit.ToString() + " are: " +
        //    "Damage (" + Priorities.TrueDamageWeight + "): " + Priorities.TrueDamageValue(unit) +
        //    ", Relative Damage (" + Priorities.RelativeDamageWeight + "): " + Priorities.RelativeDamageValue(unit) +
        //    ", Survival (" + Priorities.SurvivalWeight + "): " + Priorities.SurvivalValue(unit) +
        //    "; Final calculation: " + Priorities.GetTotalPriority(unit));
        return Priorities.GetTotalPriority(unit);
    }

    private int GetMoveRequiredToReachPos(Vector2Int pos, int movement, DangerArea fullMoveRange)
    {
        int min = int.MaxValue;
        for (int i = -Weapon.Range; i <= Weapon.Range; i++)
        {
            for (int j = -Weapon.Range; j <= Weapon.Range; j++)
            {
                if ((Mathf.Abs(i) + Mathf.Abs(j) <= Weapon.Range && Mathf.Abs(i) + Mathf.Abs(j) > 0) &&
                    GameController.Current.IsValidPos(pos.x + i, pos.y + j) &&
                    CanAttackPos(pos.x, pos.y, pos.x + i, pos.y + j) &&
                    fullMoveRange[pos.x + i, pos.y + j].Value > 0 &&
                    min > movement - fullMoveRange[pos.x + i, pos.y + j].Value)
                {
                    min = movement - fullMoveRange[pos.x + i, pos.y + j].Value;
                }
            }
        }
        if (min > movement)
        {
            Bugger.Warning("Can't reach target position: " + pos + ", unit: " + ToString() + " at pos " + Pos);
        }
        return min;
    }

    public bool CanAttack(Unit other)
    {
        return other != null && other.IsEnemy(this) && CanAttackPos(other.GetClosetPosToUnit(this));
    }

    private bool CanAttackPos(Vector2Int pos)
    {
        return CanAttackPos(pos.x, pos.y, Pos.x, Pos.y);
    }

    private bool CanAttackPos(int x, int y)
    {
        return CanAttackPos(x, y, Pos.x, Pos.y);
    }

    private bool CanAttackPos(Vector2Int pos, Vector2Int fromPos)
    {
        return CanAttackPos(pos.x, pos.y, fromPos.x, fromPos.y);
    }

    private bool CanAttackPos(int x, int y, int fromX, int fromY) // TBA: fix for multi-tile units
    {
        //return !Statue && Weapon.Range >= (Mathf.Abs(Pos.x - x) + Mathf.Abs(Pos.y - y));
        Vector2Int difference = new Vector2Int(fromX - x, fromY - y);
        int differenceSize = difference.TileSize();
        if (differenceSize <= 1)
        {
            return !Statue && Weapon.Range >= differenceSize;
        }
        else
        {
            return GetDangerArea(fromX, fromY, 0, false)[x, y].Type == DangerArea.TileDataType.Attack;
        }
    }

    public string AttackPreview(Unit other, int padding = 2, bool canAttack = true)
    {
        return "HP :" + Health.ToString().PadRight(padding) + 
            "\nDMG:" + (canAttack ? this.GetDamage(other).ToString() : "--").PadRight(padding) + 
            "\nHIT:" + (canAttack ? this.GetHitChance(other).ToString() : "--").PadRight(padding);
    }

    public string State()
    {
        return Statue ? "Statue" : (Moved ? "Moved" : "Normal");
    }

    public bool? Attack(Unit unit, float randomResult)
    {
        int percent = this.GetHitChance(unit);
        if (percent >= 80)
        {
            percent += 10;
        }
        else if (percent <= 20)
        {
            percent -= 10;
        }
        if (randomResult < percent) // 1.5RN system
        {
            //Bugger.Info(a + ", " + (b * 2) + " - " + ((a + b) / 1.5f) + " < " + percent + ": hit");
            NotifyListeners((a) => a.OnHit());
            int damage = this.GetDamage(unit);
            if (damage > 0)
            {
                unit.Health -= damage;
                unit.NotifyListeners((a) => a.OnDamaged());
                // Kill?
                if (unit.Health <= 0)
                {
                    // Stats - increase kill count if it's a player unit, increase death count if a player unit was killed
                    if (TheTeam.PlayerControlled())
                    {
                        SavedData.Append("Statistics", ToString() + "KillCount", 1);
                    }
                    else if (unit.TheTeam.PlayerControlled())
                    {
                        SavedData.Append("Statistics", unit.ToString() + "DeathCount", 1);
                    }
                    GameController.Current.KillUnit(unit);
                    return null;
                }
                return true;
            }
            else
            {
                unit.NotifyListeners((a) => a.OnBlocked());
                return true; // I think true means the attack connected, regardless of damage
            }
        }
        else
        {
            //Bugger.Info(a + ", " + (b * 2) + " - " + ((a + b) / 1.5f) + " >= " + percent + ": miss");
            NotifyListeners((a) => a.OnMiss());
            unit.NotifyListeners((a) => a.OnDodged());
            return false;
        }
    }

    public override string ToString()
    {
        return (Icon?.Name == TheTeam.Name() ? Class : DisplayName) ?? DisplayName;
    }

    public bool IsEnemy(Unit target)
    {
        return enemyOverrides.Contains(target.Name) || target.enemyOverrides.Contains(Name) || TheTeam.IsEnemy(target.TheTeam);
    }

    public void ChangeInclination(Inclination target)
    {
        for (int i = (int)Inclination * 2; i < (int)Inclination * 2 + 2; i++)
        {
            Stats.Base.Growths[i]--;
        }
        Inclination = target;
        for (int i = (int)Inclination * 2; i < (int)Inclination * 2 + 2; i++)
        {
            Stats.Base.Growths[i]++;
        }
    }

    public void AddEnemyOverride(string target)
    {
        enemyOverrides.Add(target);
    }

    public void AddSkill(Skill skill)
    {
        skills.AddSkill(skill);
    }

    public bool HasSkill(Skill skill)
    {
        return skills.HasSkill(skill);
    }

    public virtual bool AtPos(Vector2Int pos)
    {
        return Pos == pos;
    }

    public virtual Vector2Int GetClosetPosToUnit(Unit unit) // For multi-tiles
    {
        return Pos;
    }

    public string Save()
    {
        return JsonUtility.ToJson(this);
    }

    public void Load(string json, bool resetMoved = false)
    {
        MoveMarker movementMarker = MovementMarker; // Change to load one from GameController depending on player/enemy
        AttackMarker attackMarker = AttackMarker; // Change to load one from GameController depending on player/enemy
        SpriteRenderer symbol = Symbol;
        JsonUtility.FromJsonOverwrite(json, this);
        MovementMarker = movementMarker;
        AttackMarker = attackMarker;
        Symbol = symbol;
        LoadIcon();
        moved = !resetMoved && moved;
        // Fix stats. TBA: think of a better solution, one which would also allow saving the data of StatModifiers between maps/save & quits
        Stats baseStats = Stats.Base;
        Stats = new UnitStats(this);
        Stats.Base = baseStats;
        this.LoadStatModifiers();
    }

    /// <summary>
    /// Updates all units for settings change. Currently only exists for the extra symbols accessibility option.
    /// </summary>
    public void ReflectSettingsUpdate()
    {
        Symbol.gameObject.SetActive(GameCalculations.ExtraSymbolsOn);
        Symbol.sprite = Moved ? GameController.Current.LevelMetadata.TeamDatas[(int)TheTeam].MovedSymbol : GameController.Current.LevelMetadata.TeamDatas[(int)TheTeam].BaseSymbol;
    }

    public void AddStatModifier<T>(T modifier) where T: AStatModifier
    {
        Stats.AddStatModifier(modifier);
        if (modifier is IUnitListener listener)
        {
            AddListener(listener);
        }
    }

    public void AddListener(IUnitListener listener)
    {
        listeners.Add(listener);
    }

    public void RemoveListener(IUnitListener listener)
    {
        listeners.Remove(listener);
    }

    private void NotifyListeners(System.Action<IUnitListener> action)
    {
        listeners.ForEach(a => action(a));
    }

    public class DangerArea
    {
        public enum TileDataType { Inaccessible, Move, PassThrough, Attack, MultiTileMove }

        private TileData[,] data;
        protected Unit unit;

        public TileData this[int x, int y]
        {
            get => data[x, y];
            set => data[x, y] = value;
        }

        protected DangerArea(Unit unit)
        {
            this.unit = unit;
            data = new TileData[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
            for (int x = 0; x < GameController.Current.MapSize.x; x++)
            {
                for (int y = 0; y < GameController.Current.MapSize.y; y++)
                {
                    data[x, y] = new TileData();
                    data[x, y].Pos = new Vector2Int(x, y);
                }
            }
        }

        protected void Init(int x, int y, int range, bool includePassThroughMoves)
        {
            AttackFrom attackFrom;
            if (range > 0)
            {
                attackFrom = FindMovement(x, y, range);
            }
            else
            {
                MarkMovementTile(x, y, 0, TileDataType.Move);
                attackFrom = new AttackFrom();
                attackFrom.Add(new Vector2Int(x, y));
                PostProcessMovement(attackFrom);
            }
            if (!includePassThroughMoves) // Remove all pass through tiles
            {
                attackFrom.RemoveAll(a => this[a.x, a.y].Type == TileDataType.PassThrough);
                ClearPassThroughs();
            }
            else // Merely low priority
            {
                attackFrom.Sort((a, b) => this[a.x, a.y].Type.CompareTo(this[b.x, b.y].Type));
            }

            //Bugger.Info(string.Join("\n", attackFrom.ConvertAll(a => a.x + ", " + a.y + ": " + dangerArea[a.x, a.y].Type)));

            if (!unit.HasSkill(Skill.SiegeWeapon)) // Infinite range instead of 3-10
            {
                if (unit.Weapon.Range > 0)
                {
                    attackFrom.ForEach(a => FindAttackPart(a.x, a.y, unit.Weapon.Range));
                }
            }
            else
            {
                for (int i = 0; i < GameController.Current.MapSize.x; i++)
                {
                    for (int j = 0; j < GameController.Current.MapSize.y; j++)
                    {
                        if (this[i, j].Value > 0 || !GameController.Current.Map[i, j].Passable)
                        {
                            continue;
                        }
                        this[i, j].Type = TileDataType.Attack;
                        this[i, j].Value = -1;
                    }
                }
            }
        }

        public static DangerArea Generate(Unit unit, int x, int y, int range, bool includePassThroughMoves)
        {
            DangerArea dangerArea = new DangerArea(unit);
            dangerArea.Init(x, y, range, includePassThroughMoves);
            return dangerArea;
        }

        protected virtual void PostProcessMovement(AttackFrom attackFrom) { }

        protected virtual int FindMovementGetCost(int x, int y)
        {
            return GameController.Current.Map[x, y].GetMovementCost(unit);
        }

        protected virtual Unit FindMovementGetUnit(int x, int y)
        {
            return GameController.Current.FindUnitAtPos(x, y);
        }

        private AttackFrom FindMovement(int x, int y, int range)
        {
            void Inner(int x, int y, int range, AttackFrom attackFrom) // Recursion
            {
                bool added = false;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if ((i != 0 && j == 0) || (j != 0 && i == 0)) // Check cross shape
                        {
                            if (OutOfBounds(x + i, y + j)) // Out of bounds
                            {
                                continue;
                            }
                            string posName = "Pos: " + (x + i) + ", " + (y + j) + "; ";
                            int cost = FindMovementGetCost(x + i, y + j);
                            if (range - cost >= 0) // Isn't blocked by terrain
                            {
                                if (this[x + i, y + j].Value > range - cost) // Found a better/same path, pointless
                                {
                                    continue;
                                }
                                Unit atPos = FindMovementGetUnit(x + i, y + j);
                                if (atPos == null || atPos == unit) // Empty space/self - can move
                                {
                                    MarkMovementTile(x + i, y + j, range - cost, TileDataType.Move);
                                }
                                else if (!unit.IsEnemy(atPos)) // Ally - can pass through
                                {
                                    MarkMovementTile(x + i, y + j, range - cost, TileDataType.PassThrough);
                                    if (!added) // In case PassThroughs are ignores
                                    {
                                        attackFrom.Add(new Vector2Int(x, y));
                                        added = true;
                                    }
                                }
                                else // Enemy - can be attacked from here
                                {
                                    if (!added)
                                    {
                                        attackFrom.Add(new Vector2Int(x, y));
                                        added = true;
                                    }
                                    continue;
                                }
                                // If Move or PassThrough, continue moving
                                Inner(x + i, y + j, range - cost, attackFrom);
                            }
                            else // Blocked by terrain - can attack from here
                            {
                                attackFrom.Add(new Vector2Int(x, y));
                            }
                        }
                    }
                }
            }

            AttackFrom attackFrom = new AttackFrom();
            if (OutOfBounds(x, y))
            {
                throw Bugger.FMError("Checking movement of an out-of-bounds unit!", false);
            }
            MarkMovementTile(x, y, range, TileDataType.Move);
            Inner(x, y, range, attackFrom);
            PostProcessMovement(attackFrom);
            return attackFrom;
        }

        private void FindAttackPart(int x, int y, int range)
        {
            void Inner (int x, int y, int range, TileData parent)
            {
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if ((i != 0 && j == 0) || (j != 0 && i == 0))
                        {
                            if (OutOfBounds(x + i, y + j))
                            {
                                continue;
                            }
                            if (this[x + i, y + j].Value > 0 || -this[x + i, y + j].Value > range || !GameController.Current.Map[x + i, y + j].Passable)
                            {
                                continue;
                            }
                            this[x + i, y + j].Type = TileDataType.Attack;
                            this[x + i, y + j].Value = -(range + 1);
                            if (range - 1 > 0)
                            {
                                Inner(x + i, y + j, range - 1, parent);
                            }
                        }
                    }
                }
                if (this[x, y].Value > 0 || -this[x, y].Value > range || !GameController.Current.Map[x, y].Passable)
                {
                    return;
                }
            }

            TileData parent = this[x, y];
            Inner(x, y, range, parent);
        }

        private bool OutOfBounds(int x, int y)
        {
            return x < 0 || y < 0 || x >= GameController.Current.MapSize.x || y >= GameController.Current.MapSize.y;
        }

        private void MarkMovementTile(int x, int y, int range, TileDataType type)
        {
            this[x, y].Type = type;
            this[x, y].Value = range + 1;
        }

        private void ClearPassThroughs()
        {
            for (int x = 0; x < GameController.Current.MapSize.x; x++)
            {
                for (int y = 0; y < GameController.Current.MapSize.y; y++)
                {
                    if (data[x, y].Type == TileDataType.PassThrough)
                    {
                        data[x, y].Type = TileDataType.Inaccessible;
                        data[x, y].Value = 0;
                    }
                }
            }
        }

        public override string ToString()
        {
            string result = "";
            for (int y = 0; y < GameController.Current.MapSize.y; y++)
            {
                for (int x = 0; x < GameController.Current.MapSize.x; x++)
                {
                    result += this[x, y].Value;
                }
                result += "\n";
            }
            return result;
        }

        /// <summary>
        /// Gets the best place to attack the target position from (for auto-attack & AI)
        /// </summary>
        /// <param name="target">The target position (will automatically detect the unit if there is one)</param>
        /// <param name="distanceWeight">Negative for prioritizing attacking from far places (enemies), positive for least distance walked (player)</param>
        /// <returns></returns>
        public Vector2Int GetBestPosToAttackTargetFrom(Vector2Int target, float distanceWeight = 1) // Different from the other one, as it doesn't take into acount the unit at that pos
        {
            if (this[target.x, target.y].Value != 0)
            {
                if (unit.HasSkill(Skill.SiegeWeapon)) // No need for "favourable pos" when you have infinite range...
                {
                    return unit.Pos;
                }
                Vector2Int currentBest = new Vector2Int(-1, -1);
                float currentBestWeight = -Mathf.Infinity;
                Unit targetUnit = GameController.Current.FindUnitAtPos(target.x, target.y);
                for (int i = -unit.Weapon.Range; i <= unit.Weapon.Range; i++)
                {
                    for (int j = -unit.Weapon.Range; j <= unit.Weapon.Range; j++)
                    {
                        if (Mathf.Abs(i) + Mathf.Abs(j) <= unit.Weapon.Range && Mathf.Abs(i) + Mathf.Abs(j) > 0)
                        {
                            if (!OutOfBounds(target.x + i, target.y + j) &&
                                (this[target.x + i, target.y + j].Type == TileDataType.Move ||
                                 this[target.x + i, target.y + j].Type == TileDataType.MultiTileMove))
                            {
                                Vector2Int checking = new Vector2Int(target.x + i, target.y + j);
                                if (this[target.x + i, target.y + j].Type == TileDataType.MultiTileMove)
                                {
                                    checking.x = this[target.x + i, target.y + j].Parent.Pos.x;
                                    checking.y = this[target.x + i, target.y + j].Parent.Pos.y;
                                }
                                if (!unit.CanAttackPos(target.x, target.y, checking.x, checking.y))
                                {
                                    //Bugger.Info(unit + " can't attack from " + new Vector2Int(target.x + i, target.y + j));
                                    continue;
                                }
                                float weight = 50; // Make sure it's positive
                                weight += GameController.Current.Map[checking.x, checking.y].ArmorModifier * 10;
                                weight += this[checking.x, checking.y].Value * distanceWeight;
                                if (targetUnit != null && targetUnit.IsEnemy(unit))
                                {
                                    weight += targetUnit.CanAttackPos(checking.x, checking.y) ? 0 : 100; // Always prioritize attacking where enemy can't counter
                                }
                                //Bugger.Info("Pos " + new Vector2Int(target.x + i, target.y + j) + " weight: " + weight + ", best pos " + currentBest + " weight: " + currentBestWeight);
                                if (weight > currentBestWeight)
                                {
                                    currentBest = new Vector2Int(checking.x, checking.y);
                                    currentBestWeight = weight;
                                }
                            }
                        }
                    }
                }
                if (currentBest == new Vector2Int(-1, -1))
                {
                    throw Bugger.FMError(unit + " couldn't find a favorable place to attack pos " + target, false);
                }
                return currentBest;
            }
            else
            {
                throw Bugger.FMError(unit + " cannot even attack pos " + target, false);
            }
        }

        public class TileData
        {
            public int Value = 0;
            public TileDataType Type = TileDataType.Inaccessible;
            public Vector2Int Pos;
            public TileData Parent; // Only for multi-tiles
        }
    }
}


[System.Serializable]
public class AIPriorities
{
    public float TrueDamageWeight, RelativeDamageWeight, SurvivalWeight;
    public AICautionLevel CautionLevel;
    private Unit thisUnit;

    public AIPriorities(Unit unit)
    {
        thisUnit = unit;
    }

    public void Set(float trueDamageWeight, float relativeDamageWeight, float survivalWeight, AICautionLevel cautionLevel)
    {
        TrueDamageWeight = trueDamageWeight;
        RelativeDamageWeight = relativeDamageWeight;
        SurvivalWeight = survivalWeight;
        CautionLevel = cautionLevel;
    }

    public void Set(AIPriorities copyFrom)
    {
        TrueDamageWeight = copyFrom.TrueDamageWeight;
        RelativeDamageWeight = copyFrom.RelativeDamageWeight;
        SurvivalWeight = copyFrom.SurvivalWeight;
        CautionLevel = copyFrom.CautionLevel;
    }

    public bool ShouldAttack(Unit unit)
    {
        if (thisUnit.Weapon.Range <= 0) // Units who can't attack shouldn't attack anyone
        {
            return false;
        }
        else if (SurvivalWeight <= 0) // Monster units always attack
        {
            return true;
        }
        else
        {
            if (((CautionLevel & AICautionLevel.NoDamage) != 0) && (thisUnit.GetDamage(unit) <= 0 || thisUnit.GetHitChance(unit) <= 0))
            {
                //ErrorController.Info("Shouldn't try attack " + unit + " (No damage)");
                return false;
            }
            if (((CautionLevel & AICautionLevel.Suicide) != 0) && (SurvivalValue(unit) >= 0))
            {
                //ErrorController.Info("Shouldn't try attack " + unit + " (Suicide)");
                return false;
            }
            if (((CautionLevel & AICautionLevel.LittleDamage) != 0) && (GetAIDamageValue(unit) <= 0))
            {
                //ErrorController.Info("Shouldn't try attack " + unit + " (Little damage)");
                return false;
            }
            //ErrorController.Info("Should try attack " + unit);
            return true;
        }
    }

    public float GetTotalPriority(Unit unit)
    {
        return TrueDamageWeight * TrueDamageValue(unit) + RelativeDamageWeight * RelativeDamageValue(unit) + SurvivalWeight * SurvivalValue(unit);
    }

    public int GetAIDamageValue(Unit other)
    {
        return Mathf.RoundToInt(thisUnit.GetDamage(other) * thisUnit.GetHitChance(other) / 100.0f + 0.01f);
    }
    // These should be private. Public for debug purposes
    public int TrueDamageValue(Unit unit)
    {
        return -GetAIDamageValue(unit);
    }

    public int RelativeDamageValue(Unit unit)
    {
        return unit.Health - GetAIDamageValue(unit);
    }

    public int SurvivalValue(Unit unit)
    {
        int survivalValue = Mathf.RoundToInt(unit.GetDamage(thisUnit) * unit.GetHitChance(thisUnit) / 100.0f + 0.01f) - thisUnit.Health;
        return survivalValue > 0 ? survivalValue + 5 : survivalValue;
    }
}