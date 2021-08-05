using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

public class Unit : MapObject
{
    [Header("Basic info")]
    public Marker MovementMarker;
    public Marker AttackMarker;
    public Team TheTeam;
    public string Name;
    public string Class;
    public AIType AIType;
    [Header("Stats")]
    public bool Flies;
    public Stats Stats;
    public Inclination Inclination;
    [Header("AI Values")]
    public int MaxAcceptableHitRisk = 50;
    public AIPriorities Priorities;
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
    [System.NonSerialized]
    public int Health;
    [HideInInspector]
    [System.NonSerialized]
    public Vector2Int PreviousPos;
    [HideInInspector]
    [System.NonSerialized]
    public int ReinforcementTurn;
    [HideInInspector]
    [System.NonSerialized]
    public bool Statue;
    public int BattleStatsStr { get { return Stats.Strength + Weapon.Damage; } }
    public int BattleStatsEnd { get { return Stats.MaxHP; } }
    public int BattleStatsPir { get { return Stats.Pierce; } }
    public int BattleStatsArm { get { return Stats.Armor; } }
    public int BattleStatsPre { get { return Stats.Precision * 10 + Weapon.Hit - 40; } }
    public int BattleStatsEva { get { return (Stats.Evasion - Weapon.Weight) * 10 - 40; } }
    private PalettedSprite palette;
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
    public void Init()
    {
        palette = GetComponent<PalettedSprite>();
        if (palette == null)
        {
            throw new System.Exception("No palette (Init)!");
        }
        palette.Awake();
        //Start(); // There is a very weird bug - units created with CreatePlayerUnit don't activate their Start function. This is a bad workaround.
    }
    protected override void Start()
    {
        base.Start();
        palette = palette ?? GetComponent<PalettedSprite>();
        if (palette == null)
        {
            throw new System.Exception("No palette (Start)!");
        }
        LoadIcon();
        Moved = Statue;
        Health = Stats.MaxHP;
    }
    private void LoadIcon()
    {
        switch (GameController.Current.LevelMetadata.TeamDatas[(int)TheTeam].PortraitLoadingMode)
        {
            case PortraitLoadingMode.Name:
                Icon = PortraitController.Current.FindPortrait(Name);
                break;
            case PortraitLoadingMode.Team:
                Icon = PortraitController.Current.FindPortrait(TheTeam.Name());
                Name = Class;
                break;
            case PortraitLoadingMode.Generic:
                Icon = PortraitController.Current.FindGenericPortrait();
                Name = Icon.Name;
                break;
            default:
                break;
        }
    }
    public void SetIcon(Portrait icon, bool changeName = true)
    {
        Icon = icon;
        Name = icon.Name;
    }
    public override void Interact(InteractState interactState)
    {
        switch (interactState)
        {
            case InteractState.None:
                GameController.Current.RemoveMarkers();
                if (TheTeam == Team.Player && !Moved)
                {
                    int[,] checkedTiles = new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
                    List<Vector2Int> attackFrom = new List<Vector2Int>();
                    MarkDangerArea(Pos.x, Pos.y, Movement, checkedTiles, attackFrom);
                    GameController.Current.InteractState = InteractState.Move;
                    GameController.Current.ShowPointerMarker(this, (int)TheTeam);
                    GameController.Current.Selected = this;
                }
                else if (TheTeam != Team.Player)
                {
                    int[,] checkedTiles = new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
                    List<Vector2Int> attackFrom = new List<Vector2Int>();
                    MovementMarker.GetComponent<PalettedSprite>().Palette = (int)TheTeam;
                    MarkDangerArea(Pos.x, Pos.y, Movement, checkedTiles, attackFrom, true);
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
    private void GetMovement(int x, int y, int range, int[,] checkedTiles, List<Vector2Int> attackFrom, bool ignoreAllies = false)
    {
        if (checkedTiles[x, y] > range)
        {
            return;
        }
        Unit atPos = GameController.Current.FindUnitAtPos(x, y);
        if (atPos == null || atPos == this || (ignoreAllies && atPos.TheTeam == TheTeam))
        {
            checkedTiles[x, y] = range + 1;
        }
        else if (atPos.TheTeam != TheTeam)
        {
            attackFrom.Add(new Vector2Int(x, y));
            return;
        }
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 || j == 0)
                {
                    if (x + i < 0 || y + j < 0 || x + i >= GameController.Current.MapSize.x || y + j >= GameController.Current.MapSize.y)
                    {
                        continue;
                    }
                    if (range - GameController.Current.Map[x + i, y + j].GetMovementCost(this) >= 0)
                    {
                        Unit atTargetPos = GameController.Current.FindUnitAtPos(x + i, y + j);
                        if (atTargetPos != null && atTargetPos.TheTeam != TheTeam && atPos != null && atPos != this && (!ignoreAllies || atPos.TheTeam != TheTeam))
                        {
                            continue;
                        }
                        GetMovement(x + i, y + j, range - GameController.Current.Map[x + i, y + j].GetMovementCost(this), checkedTiles, attackFrom, ignoreAllies);
                    }
                    else if (atPos == null || atPos == this || (ignoreAllies && atPos.TheTeam == TheTeam))
                    {
                        attackFrom.Add(new Vector2Int(x + i, y + j));
                    }
                }
            }
        }
    }
    private void GetDangerAreaPart(int x, int y, int range, int[,] checkedTiles)
    {
        if (checkedTiles[x, y] > 0 || -checkedTiles[x, y] > range || !GameController.Current.Map[x, y].Passable)
        {
            return;
        }
        checkedTiles[x, y] = -(range + 1);
        if (range - 1 > 0)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 || j == 0)
                    {
                        if (x + i < 0 || y + j < 0 || x + i >= GameController.Current.MapSize.x || y + j >= GameController.Current.MapSize.y)
                        {
                            continue;
                        }
                        GetDangerAreaPart(x + i, y + j, range - 1, checkedTiles);
                    }
                }
            }
        }
    }
    private int[,] GetDangerArea(int x, int y, int range, int[,] checkedTiles, List<Vector2Int> attackFrom, bool ignoreAllies = false)
    {
        GetMovement(x, y, range, checkedTiles, attackFrom, ignoreAllies);
        attackFrom = attackFrom.Distinct().ToList();
        foreach (Vector2Int pos in attackFrom)
        {
            GetDangerAreaPart(pos.x, pos.y, Weapon.Range, checkedTiles);
        }
        return checkedTiles;
    }
    public int[,] GetDangerArea()
    {
        int[,] checkedTiles = new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
        List<Vector2Int> attackFrom = new List<Vector2Int>();
        return GetDangerArea(Pos.x, Pos.y, Movement, checkedTiles, attackFrom);
    }
    public int[,] GetMovement(bool ignoreAllies = false)
    {
        int[,] checkedTiles = new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
        List<Vector2Int> attackFrom = new List<Vector2Int>();
        GetMovement(Pos.x, Pos.y, Movement, checkedTiles, attackFrom, ignoreAllies);
        return checkedTiles;
    }

    private void MarkDangerArea(int x, int y, int range, int[,] checkedTiles, List<Vector2Int> attackFrom, bool ignoreAllies = false)
    {
        GetDangerArea(x, y, range, checkedTiles, attackFrom, ignoreAllies);
        for (int i = 0; i < checkedTiles.GetLength(0); i++)
        {
            for (int j = 0; j < checkedTiles.GetLength(1); j++)
            {
                if (checkedTiles[i, j] > 0)
                {
                    Marker movementMarker = Instantiate(MovementMarker.gameObject).GetComponent<Marker>();
                    movementMarker.Pos = new Vector2Int(i, j);
                    movementMarker.Origin = this;
                    movementMarker.ShowArmorIcon();
                    movementMarker.gameObject.SetActive(true);
                }
                else if (checkedTiles[i, j] < 0)
                {
                    Marker attackMarker = Instantiate(AttackMarker.gameObject).GetComponent<Marker>();
                    attackMarker.Pos = new Vector2Int(i, j);
                    attackMarker.Origin = this;
                    attackMarker.gameObject.SetActive(true);
                }
            }
        }
    }

    public void MarkDangerArea()
    {
        int[,] checkedTiles = new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
        List<Vector2Int> attackFrom = new List<Vector2Int>();
        MovementMarker.GetComponent<PalettedSprite>().Palette = (int)TheTeam;
        MarkDangerArea(Pos.x, Pos.y, Movement, checkedTiles, attackFrom, true);
    }

    public void MarkAttack(int x = -1, int y = -1, int range = -1, bool[,] checkedTiles = null)
    {
        if (range == -1)
        {
            range = Weapon.Range + 1;
            x = Pos.x;
            y = Pos.y;
        }
        checkedTiles = checkedTiles ?? new bool[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
        if (checkedTiles[x, y] || !GameController.Current.Map[x, y].Passable)
        {
            return;
        }
        else
        {
            AttackMarker attackMarker = Instantiate(AttackMarker.gameObject).GetComponent<AttackMarker>();
            attackMarker.Pos = new Vector2Int(x, y);
            attackMarker.Origin = this;
            attackMarker.gameObject.SetActive(true);
        }
        checkedTiles[x, y] = true;
        if (range - 1 > 0)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 || j == 0)
                    {
                        if (!GameController.Current.IsValidPos(x + i,y + j))
                        {
                            continue;
                        }
                        MarkAttack(x + i, y + j, range - 1, checkedTiles);
                    }
                }
            }
        }
    }
    public void MoveTo(Vector2Int pos, bool immediate = false)
    {
        // Add animation etc.
        PreviousPos = Pos;
        if (!immediate)
        {
            if (TheTeam != Team.Player && pos == Pos) // Only delay when enemies stay in place
            {
                MapAnimationsController.Current.AnimateDelay();
            }
            else
            {
                MapAnimationsController.Current.AnimateMovement(this, pos);
            }
        }
        else
        {
            Pos = pos;
        }
    }
    public void Fight(Unit unit)
    {
        void ActualFight(Unit target)
        {
            if (SavedData.Load<int>("BattleAnimationsMode", 0, SaveMode.Global) == 0) // Real animations
            {
                CrossfadeMusicPlayer.Current.SwitchBattleMode(true);
                BattleAnimationController battleAnimationController = Instantiate(GameController.Current.Battle).GetComponentInChildren<BattleAnimationController>();
                GameController.Current.TransitionToMidBattleScreen(battleAnimationController);
                battleAnimationController.Attacker = this;
                battleAnimationController.Defender = target;
                battleAnimationController.StartBattle();
                GameController.Current.FinishMove(this);
            }
            else // Map animations
            {
                GameController.Current.RemoveMarkers();
                MapAnimationsController.Current.AnimateBattle(this, target);
                MapAnimationsController.Current.OnFinishAnimation = () =>
                {
                    if (this != null)
                    {
                        GameController.Current.FinishMove(this);
                    }
                    else
                    {
                        GameController.Current.FinishMoveDead();
                    }
                };
            }
        }
        if (BattleQuote != "" || unit.BattleQuote != "") // Battle quotes achieve the same effect as delays
        {
            ConversationPlayer.Current.OnFinishConversation = () => ActualFight(unit);
            ConversationPlayer.Current.PlayOneShot(BattleQuote != "" ? BattleQuote : unit.BattleQuote);
        }
        else if (TheTeam == Team.Player) // The player know who they're attacking, no need for a delay.
        {
            ActualFight(unit);
        }
        else // If an enemy attacks, however, a delay is needed in order to show the target.
        {
            MapAnimationsController.Current.OnFinishAnimation = () => ActualFight(unit);
            MapAnimationsController.Current.AnimateDelay();
        }
    }
    public void AI(List<Unit> units)
    {
        List<Unit> enemyUnits = units.Where(a => a.TheTeam != TheTeam && !a.Statue && Priorities.ShouldAttack(a)).ToList(); // Pretty much all AIs need enemy units.
        switch (AIType)
        {
            case AIType.Charge:
                // First, try the Hold AI.
                if (TryHoldAI(enemyUnits))
                {
                    break;
                }
                // If that failed, find the closest enemy.
                int[,] checkedTiles = new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
                List<Vector2Int> attackFrom = new List<Vector2Int>();
                int[,] fullDangerArea = GetDangerArea(Pos.x, Pos.y, 50, checkedTiles, attackFrom, true);
                enemyUnits = enemyUnits.Where(a => fullDangerArea[a.Pos.x, a.Pos.y] != 0).ToList();
                if (enemyUnits.Count <= 0) // Can't attack anyone - probably surrounded by scary enemies
                {
                    Debug.Log(ToString() + " can't attack anyone - probably surrounded by scary enemies - and retreats");
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
                // This is the only AI that can used ranged attacks (because I have no plans for ranged mobile enemies before the Guards)
                int[,] dangerArea = GetDangerArea();
                enemyUnits.Sort((a, b) => HoldAITargetValue(a).CompareTo(HoldAITargetValue(b)));
                foreach (Unit unit in enemyUnits)
                {
                    if (dangerArea[unit.Pos.x, unit.Pos.y] != 0)
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
        int[,] dangerArea = GetDangerArea();
        enemyUnits.Sort((a, b) => HoldAITargetValue(a).CompareTo(HoldAITargetValue(b)));
        foreach (Unit unit in enemyUnits)
        {
            if (dangerArea[unit.Pos.x, unit.Pos.y] != 0)
            {
                Vector2Int currentBest = new Vector2Int(-1, -1);
                for (int i = -Weapon.Range; i <= Weapon.Range; i++)
                {
                    for (int j = -Weapon.Range; j <= Weapon.Range; j++)
                    {
                        if (Mathf.Abs(i) + Mathf.Abs(j) <= Weapon.Range)
                        {
                            if (unit.Pos.x + i >= 0 && unit.Pos.x + i < GameController.Current.MapSize.x &&
                                unit.Pos.y + j >= 0 && unit.Pos.y + j < GameController.Current.MapSize.y &&
                                dangerArea[unit.Pos.x + i, unit.Pos.y + j] > 0)
                            {
                                // This is probably the longest, messiest if I've ever written. Let's hope it works.
                                if (currentBest.x < 0 ||
                                    ((!unit.CanAttackPos(unit.Pos.x + i, unit.Pos.y + j) || unit.CanAttackPos(currentBest.x, currentBest.y)) &&
                                    (GameController.Current.Map[currentBest.x, currentBest.y].ArmorModifier < GameController.Current.Map[unit.Pos.x + i, unit.Pos.y + j].ArmorModifier ||
                                    (GameController.Current.Map[currentBest.x, currentBest.y].ArmorModifier == GameController.Current.Map[unit.Pos.x + i, unit.Pos.y + j].ArmorModifier &&
                                     Vector2Int.Distance(currentBest, Pos) > Vector2Int.Distance(new Vector2Int(unit.Pos.x + i, unit.Pos.y + j), Pos)))) ||
                                    (!unit.CanAttackPos(unit.Pos.x + i, unit.Pos.y + j) && unit.CanAttackPos(currentBest.x, currentBest.y)))
                                {
                                    currentBest = new Vector2Int(unit.Pos.x + i, unit.Pos.y + j);
                                }
                            }
                        }
                    }
                }
                Debug.Log(this + " is moving to " + currentBest + " in order to attack " + unit + " (value " + HoldAITargetValue(unit) + ")");
                MapAnimationsController.Current.OnFinishAnimation = () => Fight(unit);
                MoveTo(currentBest);
                return true;
            }
        }
        return false;
    }
    private void RetreatAI(int[,] fullDangerArea)
    {
        Vector2Int minPoint = Vector2Int.zero;
        for (int x = 0; x < GameController.Current.MapSize.x; x++)
        {
            for (int y = 0; y < GameController.Current.MapSize.y; y++)
            {
                if (x == 0 || y == 0 || x == GameController.Current.MapSize.x - 1 || y == GameController.Current.MapSize.y - 1)
                {
                    if (fullDangerArea[x, y] > 0 && (fullDangerArea[minPoint.x, minPoint.y] <= 0 || fullDangerArea[minPoint.x, minPoint.y] < fullDangerArea[x, y]))
                    {
                        minPoint = new Vector2Int(x, y);
                    }
                }
            }
        }
        Vector2Int target = ClosestMoveablePointToTarget(minPoint, fullDangerArea);
        if (target.x == 0 || target.y == 0 || target.x == GameController.Current.MapSize.x - 1 || target.y == GameController.Current.MapSize.y - 1) // Can reach the end of the map, aka retreat, aka die
        {
            MapAnimationsController.Current.OnFinishAnimation = () =>
            {
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
    private Vector2Int ClosestMoveablePointToTarget(Vector2Int target, int[,] fullDangerArea)
    {
        List<Vector2Int> attackFrom = new List<Vector2Int>();
        int[,] trueDangerArea = GetDangerArea(Pos.x, Pos.y, Movement, trueDangerArea = new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y], attackFrom);
        Vector2Int currentMoveTarget = new Vector2Int(target.x, target.y);
        GameController.Current.RemoveMarkers();
        while (trueDangerArea[currentMoveTarget.x, currentMoveTarget.y] <= 0)
        {
            Vector2Int min = currentMoveTarget;
            for (int i = -Weapon.Range; i <= Weapon.Range; i++)
            {
                for (int j = -Weapon.Range; j <= Weapon.Range; j++)
                {
                    if (Mathf.Abs(i) + Mathf.Abs(j) <= Weapon.Range &&
                        GameController.Current.IsValidPos(currentMoveTarget.x + i, currentMoveTarget.y + j) &&
                        fullDangerArea[currentMoveTarget.x + i, currentMoveTarget.y + j] > 0 &&
                        fullDangerArea[min.x, min.y] < fullDangerArea[currentMoveTarget.x + i, currentMoveTarget.y + j])
                    {
                        min = new Vector2Int(currentMoveTarget.x + i, currentMoveTarget.y + j);
                    }
                }
            }
            if (min == currentMoveTarget)
            {
                throw new System.Exception("Path not found... to a target with a verified path! This should be impossible... Pos: " + currentMoveTarget + ", attacker: " + ToString() + ", target: " + target);
            }
            currentMoveTarget = min;
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
        //Debug.Log(ToString() + " AI values against " + unit.ToString() + " are: " + 
        //    "Damage (" + Priorities.TrueDamageWeight + "): " + Priorities.TrueDamageValue(unit) +
        //    ", Relative Damage (" + Priorities.RelativeDamageWeight + "): " + Priorities.RelativeDamageValue(unit) +
        //    ", Survival (" + Priorities.SurvivalWeight + "): " + Priorities.SurvivalValue(unit) +
        //    "; Final calculation: " + Priorities.GetTotalPriority(unit));
        return Priorities.GetTotalPriority(unit);
    }
    private int GetMoveRequiredToReachPos(Vector2Int pos, int movement, int[,] fullMoveRange)
    {
        int min = int.MaxValue;
        for (int i = -Weapon.Range; i <= Weapon.Range; i++)
        {
            for (int j = -Weapon.Range; j <= Weapon.Range; j++)
            {
                if ((Mathf.Abs(i) + Mathf.Abs(j) <= Weapon.Range && Mathf.Abs(i) + Mathf.Abs(j) > 0) && GameController.Current.IsValidPos(pos.x + i, pos.y + j) && fullMoveRange[pos.x + i, pos.y + j] > 0 && min > movement - fullMoveRange[pos.x + i, pos.y + j])
                {
                    min = movement - fullMoveRange[pos.x + i, pos.y + j];
                }
            }
        }
        if (min > movement)
        {
            Debug.LogWarning("Can't reach target position: " + pos + ", unit: " + ToString() + " at pos " + Pos);
        }
        return min;
    }
    public bool CanAttack(Unit other)
    {
        return other != null && CanAttackPos(other.Pos);
    }
    public bool CanAttackPos(Vector2Int pos)
    {
        return !Statue && Weapon.Range >= (Mathf.Abs(Pos.x - pos.x) + Mathf.Abs(Pos.y - pos.y));
    }
    public bool CanAttackPos(int x, int y)
    {
        return !Statue && Weapon.Range >= (Mathf.Abs(Pos.x - x) + Mathf.Abs(Pos.y - y));
    }
    public string AttackPreview(Unit other, int padding = 2, bool canAttack = true)
    {
        return "HP :" + Health.ToString().PadRight(padding) + 
            "\nDMG:" + (canAttack ? this.GetDamage(other).ToString() : "--").PadRight(padding) + 
            "\nHIT:" + (canAttack ? this.GetHitChance(other).ToString() : "--").PadRight(padding);
    }
    public string BattleStats()
    {
        return "ATK:" + (Weapon.Damage + Stats.Strength).ToString().PadRight(3) + "\nHIT:" + (Weapon.Hit + 10 * Stats.Precision - 40).ToString().PadRight(3) + "\nAVD:" + (10 * (Stats.Evasion - Weapon.Weight) - 40).ToString().PadRight(3);
    }
    public string State()
    {
        return Statue ? "Statue" : (Moved ? "Moved" : "Normal");
    }
    public bool? Attack(Unit unit)
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
        int a, b;
        if (((a = Random.Range(0, 100)) + (b = Random.Range(0, 50))) / 1.5f < percent) // 1.5RN system
        {
            Debug.Log(a + ", " + (b * 2) + " - " + ((a + b) / 1.5f) + " < " + percent + ": hit");
            unit.Health -= this.GetDamage(unit);
            // Kill?
            if (unit.Health <= 0)
            {
                GameController.Current.KillUnit(unit);
                return null;
            }
            return true;
        }
        else
        {
            Debug.Log(a + ", " + (b * 2) + " - " + ((a + b) / 1.5f) + " >= " + percent + ": miss");
            return false;
        }
    }
    public override string ToString()
    {
        return TheTeam == Team.Player ? Name : (Name == TheTeam.Name() ? Class : Name);
    }
    public void ChangeInclination(Inclination target)
    {
        for (int i = (int)Inclination * 2; i < (int)Inclination * 2 + 2; i++)
        {
            Stats.Growths[i]--;
        }
        Inclination = target;
        for (int i = (int)Inclination * 2; i < (int)Inclination * 2 + 2; i++)
        {
            Stats.Growths[i]++;
        }
    }
    public string Save()
    {
        return JsonUtility.ToJson(this);
    }
    public void Load(string json)
    {
        Marker movementMarker = MovementMarker; // Change to load one from GameController depending on player/enemy
        Marker attackMarker = AttackMarker; // Change to load one from GameController depending on player/enemy
        JsonUtility.FromJsonOverwrite(json, this);
        MovementMarker = movementMarker;
        AttackMarker = attackMarker;
        LoadIcon();
        moved = false;
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
        if (SurvivalWeight <= 0) // Monster units always attack
        {
            return true;
        }
        else
        {
            if (((CautionLevel & AICautionLevel.NoDamage) != 0) && (thisUnit.GetDamage(unit) <= 0 || thisUnit.GetHitChance(unit) <= 0))
            {
                //Debug.Log("Shouldn't try attack " + unit + " (No damage)");
                return false;
            }
            if (((CautionLevel & AICautionLevel.Suicide) != 0) && (SurvivalValue(unit) >= 0))
            {
                //Debug.Log("Shouldn't try attack " + unit + " (Suicide)");
                return false;
            }
            if (((CautionLevel & AICautionLevel.LittleDamage) != 0) && (GetAIDamageValue(unit) <= 0))
            {
                //Debug.Log("Shouldn't try attack " + unit + " (Little damage)");
                return false;
            }
            //Debug.Log("Should try attack " + unit);
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