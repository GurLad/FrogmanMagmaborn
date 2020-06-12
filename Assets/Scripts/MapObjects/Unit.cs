using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum Team { Player, Enemy }
public class Unit : MapObject
{
    [Header("Basic info")]
    public MoveMarker MovementMarker;
    public AttackMarker AttackMarker;
    public Team TheTeam;
    public string Name;
    public Sprite Icon;
    [Header("Stats")]
    public int Movement;
    public Stats Stats;
    //TEMP!! Replace with Weapon class
    public int AttackRange;
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
        Moved = false;
    }
    public override void Interact(InteractState interactState)
    {
        switch (interactState)
        {
            case InteractState.None:
                if (TheTeam == Team.Player && !Moved)
                {
                    int[,] checkedTiles = new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
                    List<Vector2Int> attackFrom = new List<Vector2Int>();
                    MarkDangerArea(Pos.x, Pos.y, Movement, checkedTiles, attackFrom);
                    GameController.Current.InteractState = InteractState.Move;
                    GameController.Current.Selected = this;
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
            GetDangerAreaPart(pos.x, pos.y, AttackRange, checkedTiles);
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
                    MoveMarker movementMarker = Instantiate(MovementMarker.gameObject).GetComponent<MoveMarker>();
                    movementMarker.Pos = new Vector2Int(i, j);
                    movementMarker.Origin = this;
                    movementMarker.gameObject.SetActive(true);
                }
                else if (checkedTiles[i, j] < 0)
                {
                    AttackMarker attackMarker = Instantiate(AttackMarker.gameObject).GetComponent<AttackMarker>();
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
            range = AttackRange + 1;
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
    public string AttackPreview(Stats other, int padding = 2)
    {
        return "HP :" + Health.ToString().PadRight(padding) + "\nDMG:" + Stats.Damage(other).ToString().PadRight(padding) + "\nHIT:" + Stats.HitChance(other).ToString().Replace("100", padding <= 2 ? "99" : "100").PadRight(padding);
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
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 || j == 0)
                        {
                            // Currently only works with 1 range weapons
                            if (dangerArea[unit.Pos.x + i, unit.Pos.y + j] > 0)
                            {
                                MoveTo(new Vector2Int(unit.Pos.x + i, unit.Pos.y + j));
                                Fight(unit);
                                GameController.Current.FinishMove(this);
                                return;
                            }
                        }
                    }
                }
            }
        }
        GameController.Current.FinishMove(this);
    }
    public void Attack(Unit unit)
    {
        // Think of a way to implement this with combat animations.
        int percent = Stats.HitChance(unit.Stats);
        if (Random.Range(0, 100) < percent)
        {
            Debug.Log(Name + " dealt " + Stats.Damage(unit.Stats) + " damage to " + unit.Name);
            unit.Health -= Stats.Damage(unit.Stats);
            // Kill?
        }
        else
        {
            Debug.Log(Name + " missed " + unit.Name);
        }
    }
}
