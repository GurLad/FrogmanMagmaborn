using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum Team { Player, Enemy }
public class Unit : MapObject
{
    [Header("Basic info")]
    public MoveMarker MovementMarker;
    public AttackMarker AttackMarker;
    public Team TheTeam;
    [Header("Stats")]
    public string Name;
    public int Movement;
    public Stats Stats;
    //TEMP!! Replace with Weapon class
    public int AttackRange;
    [HideInInspector]
    public int Health;
    protected override void Start()
    {
        base.Start();
        Health = Stats.MaxHP;
    }
    public override void Interact(InteractState interactState)
    {
        switch (interactState)
        {
            case InteractState.None:
                if (TheTeam == Team.Player)
                {
                    int[,] checkedTiles = new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
                    List<Vector2Int> attackFrom = new List<Vector2Int>();
                    MarkMovement(Pos.x, Pos.y, Movement, checkedTiles, attackFrom);
                    attackFrom = attackFrom.Distinct().ToList();
                    foreach (Vector2Int pos in attackFrom)
                    {
                        MarkAttack(pos.x, pos.y, AttackRange, checkedTiles);
                    }
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
    public void MarkMovement(int x, int y, int range, int[,] checkedTiles, List<Vector2Int> attackFrom)
    {
        if (checkedTiles[x, y] > range)
        {
            return;
        }
        else if (checkedTiles[x, y] == 0)
        {
            MoveMarker movementMarker = Instantiate(MovementMarker.gameObject).GetComponent<MoveMarker>();
            movementMarker.Pos = new Vector2Int(x, y);
            movementMarker.Origin = this;
            movementMarker.gameObject.SetActive(true);
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
                        MarkMovement(x + i, y + j, range - GameController.Current.Map[x + i, y + j].MovementCost, checkedTiles, attackFrom);
                    }
                    else
                    {
                        attackFrom.Add(new Vector2Int(x + i, y + j));
                    }
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
    private void MarkAttack(int x, int y, int range, int[,] checkedTiles)
    {
        if (checkedTiles[x, y] > 0 || -checkedTiles[x, y] > range)
        {
            return;
        }
        else if (checkedTiles[x, y] == 0)
        {
            AttackMarker attackMarker = Instantiate(AttackMarker.gameObject).GetComponent<AttackMarker>();
            attackMarker.Pos = new Vector2Int(x, y);
            attackMarker.Origin = this;
            attackMarker.gameObject.SetActive(true);
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
                        MarkAttack(x + i, y + j, range - 1, checkedTiles);
                    }
                }
            }
        }
    }
    public void Attack(Unit unit)
    {
        // Think of a way to implement this with combat animations.
        int percent = Stats.HitChance(unit.Stats);
        if (Random.Range(0, 100) < percent)
        {
            Debug.Log(Name + " dealt " + Stats.Damage(unit.Stats) + " damage to " + unit.Name);
            unit.Health -= Stats.Damage(unit.Stats);
        }
        else
        {
            Debug.Log(Name + " missed " + unit.Name);
        }
    }
}
