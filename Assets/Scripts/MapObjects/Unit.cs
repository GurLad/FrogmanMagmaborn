using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum Team { Player, Enemy }
public enum AIType { Charge, Hold, Guard }
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
    public int Movement;
    public bool Flies;
    public Stats Stats;
    [HideInInspector]
    public Portrait Icon;
    [HideInInspector]
    public Weapon Weapon;
    [HideInInspector]
    public int Health;
    [HideInInspector]
    public int Level;
    [HideInInspector]
    public Vector2Int PreviousPos;
    public bool Moved
    {
        get
        {
            return moved;
        }
        set
        {
            moved = value;
            palette.Palette = moved ? 3 : (int)TheTeam;
        }
    }
    private bool moved;
    private PalettedSprite palette;
    protected override void Start()
    {
        base.Start();
        palette = GetComponent<PalettedSprite>();
        Icon = PortraitController.Current.FindPortrait(Name); // Change to load one depending on class (if enemy) or name (if player)
        Moved = false;
        Health = Stats.MaxHP;
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
                    GameController.Current.Selected = this;
                }
                else if (TheTeam != Team.Player)
                {
                    int[,] checkedTiles = new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
                    List<Vector2Int> attackFrom = new List<Vector2Int>();
                    MarkDangerArea(Pos.x, Pos.y, Movement, checkedTiles, attackFrom, true);
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
                    if (range - GetMovementCost(GameController.Current.Map[x + i, y + j]) >= 0)
                    {
                        Unit atTargetPos = GameController.Current.FindUnitAtPos(x + i, y + j);
                        if (atTargetPos != null && atTargetPos.TheTeam != TheTeam && atPos != null && atPos != this && (!ignoreAllies || atPos.TheTeam != TheTeam))
                        {
                            continue;
                        }
                        GetMovement(x + i, y + j, range - GetMovementCost(GameController.Current.Map[x + i, y + j]), checkedTiles, attackFrom, ignoreAllies);
                    }
                    else if (atPos == null || atPos == this || (ignoreAllies && atPos.TheTeam == TheTeam))
                    {
                        attackFrom.Add(new Vector2Int(x + i, y + j));
                    }
                }
            }
        }
    }
    private int GetMovementCost(Tile tile)
    {
        if (Flies)
        {
            return tile.High ? 99 : 1;
        }
        else
        {
            return tile.MovementCost;
        }
    }
    private void GetDangerAreaPart(int x, int y, int range, int[,] checkedTiles)
    {
        if (checkedTiles[x, y] > 0 || -checkedTiles[x, y] > range || GameController.Current.Map[x, y].High)
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
    public void MarkAttack(int x = -1, int y = -1, int range = -1, bool[,] checkedTiles = null)
    {
        if (range == -1)
        {
            range = Weapon.Range + 1;
            x = Pos.x;
            y = Pos.y;
        }
        checkedTiles = checkedTiles ?? new bool[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
        if (checkedTiles[x, y] || GameController.Current.Map[x, y].High)
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
                        if (!IsValidPos(x + i,y + j))
                        {
                            continue;
                        }
                        MarkAttack(x + i, y + j, range - 1, checkedTiles);
                    }
                }
            }
        }
    }
    public void MoveTo(Vector2Int pos)
    {
        // Add animation etc.
        PreviousPos = Pos;
        Pos = pos;
    }
    public void Fight(Unit unit)
    {
        CrossfadeMusicPlayer.Current.SwitchBattleMode(true);
        BattleAnimationController battleAnimationController = Instantiate(GameController.Current.Battle).GetComponentInChildren<BattleAnimationController>();
        GameController.Current.transform.parent.gameObject.SetActive(false);
        battleAnimationController.Attacker = this;
        battleAnimationController.Defender = unit;
        battleAnimationController.StartBattle();
    }
    public void AI(List<Unit> units)
    {
        List<Unit> enemyUnits = units.Where(a => a.TheTeam != TheTeam).ToList(); // Pretty much all AIs nead enemy units.
        switch (AIType)
        {
            case AIType.Charge:
                // First, try the Hold AI.
                if (HoldAI(enemyUnits))
                {
                    GameController.Current.FinishMove(this);
                    break;
                }
                // If that failed, find the closest enemy.
                int[,] checkedTiles = new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
                List<Vector2Int> attackFrom = new List<Vector2Int>();
                int[,] fullDangerArea = GetDangerArea(Pos.x, Pos.y, 50, checkedTiles, attackFrom, true);
                enemyUnits = enemyUnits.Where(a => fullDangerArea[a.Pos.x, a.Pos.y] != 0).ToList();
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
                attackFrom.Clear();
                int[,] trueDangerArea = GetDangerArea(Pos.x, Pos.y, Movement, trueDangerArea = new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y], attackFrom);
                Vector2Int currentMoveTarget = new Vector2Int(target.Pos.x, target.Pos.y);
                GameController.Current.RemoveMarkers();
                while (trueDangerArea[currentMoveTarget.x, currentMoveTarget.y] <= 0)
                {
                    Vector2Int min = currentMoveTarget;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if ((i == 0 || j == 0) && IsValidPos(currentMoveTarget.x + i, currentMoveTarget.y + j) &&
                                fullDangerArea[currentMoveTarget.x + i, currentMoveTarget.y + j] > 0 &&
                                fullDangerArea[min.x, min.y] < fullDangerArea[currentMoveTarget.x + i, currentMoveTarget.y + j])
                            {
                                min = new Vector2Int(currentMoveTarget.x + i, currentMoveTarget.y + j);
                            }
                        }
                    }
                    if (min == currentMoveTarget)
                    {
                        throw new System.Exception("Path not found... to a target with a verified path! This should be impossible... Pos: " + currentMoveTarget);
                    }
                    currentMoveTarget = min;
                }
                // Finally, move to the target location.
                MoveTo(currentMoveTarget);
                GameController.Current.FinishMove(this);
                break;
            case AIType.Hold:
                HoldAI(enemyUnits);
                GameController.Current.FinishMove(this);
                break;
            case AIType.Guard:
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// Does the hold AI.
    /// </summary>
    /// <returns>True if attacked, false otherwise.</returns>
    private bool HoldAI(List<Unit> enemyUnits)
    {
        int[,] dangerArea = GetDangerArea();
        enemyUnits.Sort((a, b) => (a.Health - GetDamage(a)).CompareTo(b.Health - GetDamage(b)));
        foreach (Unit unit in enemyUnits)
        {
            if (dangerArea[unit.Pos.x, unit.Pos.y] != 0)
            {
                Vector2Int currentBest = new Vector2Int(-1, -1);
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 || j == 0)
                        {
                            // Currently only works with 1 range weapons
                            if (unit.Pos.x + i >= 0 && unit.Pos.x + i < GameController.Current.MapSize.x &&
                                unit.Pos.y + j >= 0 && unit.Pos.y + j < GameController.Current.MapSize.y &&
                                dangerArea[unit.Pos.x + i, unit.Pos.y + j] > 0)
                            {
                                if (currentBest.x < 0 ||
                                    GameController.Current.Map[currentBest.x, currentBest.y].ArmorModifier < GameController.Current.Map[unit.Pos.x + i, unit.Pos.y + j].ArmorModifier ||
                                    (GameController.Current.Map[currentBest.x, currentBest.y].ArmorModifier == GameController.Current.Map[unit.Pos.x + i, unit.Pos.y + j].ArmorModifier &&
                                     Vector2Int.Distance(currentBest, Pos) > Vector2Int.Distance(new Vector2Int(unit.Pos.x + i, unit.Pos.y + j), Pos)))
                                {
                                    currentBest = new Vector2Int(unit.Pos.x + i, unit.Pos.y + j);
                                }
                            }
                        }
                    }
                }
                MoveTo(currentBest);
                Fight(unit);
                return true;
            }
        }
        return false;
    }
    private int GetMoveRequiredToReachPos(Vector2Int pos, int movement, int[,] fullMoveRange)
    {
        int min = int.MaxValue;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if ((i == 0 || j == 0) && IsValidPos(pos.x + i, pos.y + j) && fullMoveRange[pos.x + i, pos.y + j] > 0 && min > movement - fullMoveRange[pos.x + i, pos.y + j])
                {
                    min = movement - fullMoveRange[pos.x + i, pos.y + j];
                }
            }
        }
        if (min > movement)
        {
            throw new System.Exception("Can't reach target position: " + pos + ", unit: " + Name + " at pos " + Pos);
        }
        return min;
    }
    private bool IsValidPos(int x, int y)
    {
        return x >= 0 && y >= 0 && x < GameController.Current.MapSize.x && y < GameController.Current.MapSize.y;
    }
    private bool IsValidPos(Vector2Int pos)
    {
        return IsValidPos(pos.x, pos.y);
    }
    private int GetHitChance(Unit other)
    {
        return Mathf.Min(100, Weapon.Hit - 10 * (other.Stats.Evasion - other.Weapon.Weight - Stats.Precision));
    }
    private int GetDamage(Unit other)
    {
        return Mathf.Max(0, Stats.Strength + Weapon.Damage - 2 * Mathf.Max(0, other.Stats.Armor + GameController.Current.Map[other.Pos.x, other.Pos.y].ArmorModifier - Stats.Pierce));
    }
    public string AttackPreview(Unit other, int padding = 2)
    {
        return "HP :" + Health.ToString().PadRight(padding) + "\nDMG:" + GetDamage(other).ToString().PadRight(padding) + "\nHIT:" + GetHitChance(other).ToString().Replace("100", padding <= 2 ? "99" : "100").PadRight(padding);
    }
    public string BattleStats()
    {
        return "ATK:" + (Weapon.Damage + Stats.Strength).ToString().PadRight(3) + "\nHIT:" + (Weapon.Hit + 10 * Stats.Precision - 40).ToString().PadRight(3) + "\nAVD:" + (10 * (Stats.Evasion - Weapon.Weight) - 40).ToString().PadRight(3);
    }
    public bool? Attack(Unit unit)
    {
        int percent = GetHitChance(unit);
        int a, b;
        if (((a = Random.Range(0, 100)) + (b = Random.Range(0, 50))) / 1.5f < percent) // 1.5RN system
        {
            Debug.Log(a + ", " + (b * 2) + " - " + ((a + b) / 1.5f) + " < " + percent + ": hit");
            unit.Health -= GetDamage(unit);
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
    public string Save()
    {
        Debug.Log(JsonUtility.ToJson(this, true));
        return JsonUtility.ToJson(this);
    }
    public void Load(string json)
    {
        Marker movementMarker = MovementMarker; // Change to load one from GameController depending on player/enemy
        Marker attackMarker = AttackMarker; // Change to load one from GameController depending on player/enemy
        Portrait icon = Icon; // Move the find portrait code to a function
        JsonUtility.FromJsonOverwrite(json, this);
        MovementMarker = movementMarker;
        AttackMarker = attackMarker;
        Icon = icon;
    }
}
