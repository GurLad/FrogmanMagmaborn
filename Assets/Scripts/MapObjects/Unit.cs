using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum Team { Player, Enemy }
public class Unit : MapObject
{
    [Header("Basic info")]
    public Marker MovementMarker;
    public Marker AttackMarker;
    public Team TheTeam;
    public string Name;
    public string Class;
    public Portrait Icon;
    [Header("Stats")]
    public int Movement;
    public Stats Stats;
    [HideInInspector]
    public Weapon Weapon;
    [HideInInspector]
    public int Health;
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
        Health = Stats.MaxHP;
        Icon = PortraitController.Current.FindPortrait(Name); // Change to load one depending on class (if enemy) or name (if player)
        Moved = false;
        Weapon = new Weapon(1);
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
                    MarkDangerArea(Pos.x, Pos.y, Movement, checkedTiles, attackFrom);
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
    private void GetMovement(int x, int y, int range, int[,] checkedTiles, List<Vector2Int> attackFrom)
    {
        if (checkedTiles[x, y] > range)
        {
            return;
        }
        checkedTiles[x, y] = range + 1;
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
                    if (range - GameController.Current.Map[x + i, y + j].MovementCost >= 0 && (GameController.Current.FindUnitAtPos(x + i, y + j) == null || GameController.Current.FindUnitAtPos(x + i, y + j).TheTeam == TheTeam))
                    {
                        GetMovement(x + i, y + j, range - GameController.Current.Map[x + i, y + j].MovementCost, checkedTiles, attackFrom);
                    }
                    else
                    {
                        attackFrom.Add(new Vector2Int(x + i, y + j));
                    }
                }
            }
        }
    }
    private void GetDangerAreaPart(int x, int y, int range, int[,] checkedTiles)
    {
        if (checkedTiles[x, y] > 0 || -checkedTiles[x, y] > range)
        {
            return;
        }
        checkedTiles[x, y] = -(range + 1);
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
                    if (range - 1 > 0)
                    {
                        GetDangerAreaPart(x + i, y + j, range - 1, checkedTiles);
                    }
                }
            }
        }
    }
    private int[,] GetDangerArea(int x, int y, int range, int[,] checkedTiles, List<Vector2Int> attackFrom)
    {
        GetMovement(x, y, range, checkedTiles, attackFrom);
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

    private void MarkDangerArea(int x, int y, int range, int[,] checkedTiles, List<Vector2Int> attackFrom)
    {
        GetDangerArea(x, y, range, checkedTiles, attackFrom);
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
        if (checkedTiles[x, y])
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
                    if (range - 1 > 0)
                    {
                        MarkAttack(x + i, y + j, range - 1, checkedTiles);
                    }
                }
            }
        }
    }
    public void MoveTo(Vector2Int pos)
    {
        // Add animation etc.
        Pos = pos;
    }
    public void Fight(Unit unit)
    {
        CrossfadeMusicPlayer.Instance.SwitchBattleMode(true);
        BattleAnimationController battleAnimationController = Instantiate(GameController.Current.Battle).GetComponentInChildren<BattleAnimationController>();
        GameController.Current.transform.parent.gameObject.SetActive(false);
        battleAnimationController.Attacker = this;
        battleAnimationController.Defender = unit;
        battleAnimationController.StartBattle();
    }
    public void AI(List<Unit> units)
    {
        // Charge
        int[,] dangerArea = GetDangerArea();
        List<Unit> enemyEnemies = units.Where(a => a.TheTeam != TheTeam).ToList();
        foreach (Unit unit in enemyEnemies)
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
                            if (dangerArea[unit.Pos.x + i, unit.Pos.y + j] > 0)
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
            }
        }
        GameController.Current.FinishMove(this);
    }
    private int GetHitChance(Unit other)
    {
        return Weapon.Hit - 5 * Mathf.Max(0, other.Stats.Evasion - Stats.Precision);
    }
    private int GetDamage(Unit other)
    {
        return Mathf.Max(0, Stats.Strength + Weapon.Damage - 2 * Mathf.Max(0, other.Stats.Armor - Stats.Pierce));
    }
    public string AttackPreview(Unit other, int padding = 2)
    {
        return "HP :" + Health.ToString().PadRight(padding) + "\nDMG:" + GetDamage(other).ToString().PadRight(padding) + "\nHIT:" + GetHitChance(other).ToString().Replace("100", padding <= 2 ? "99" : "100").PadRight(padding);
    }
    public bool? Attack(Unit unit)
    {
        int percent = GetHitChance(unit);
        if (Random.Range(0, 100) < percent)
        {
            Debug.Log(Name + " dealt " + GetDamage(unit) + " damage to " + unit.Name);
            unit.Health -= GetDamage(unit);
            // Kill?
            if (unit.Health <= 0)
            {
                GameController.Current.MapObjects.Remove(unit);
                Destroy(unit.gameObject);
                return null;
            }
            return true;
        }
        else
        {
            Debug.Log(Name + " missed " + unit.Name);
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
