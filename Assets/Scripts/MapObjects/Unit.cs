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
    public int Health;
    //TEMP!! Replace with Stats class
    public int MaxHealth;
    //TEMP!! Replace with Weapon class
    public int AttackRange;
    public override void Interact(InteractState interactState)
    {
        switch (interactState)
        {
            case InteractState.None:
                if (TheTeam == Team.Player)
                {
                    int[,] checkedTiles = new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
                    List<Vector2Int> attackFrom = new List<Vector2Int>();
                    MarkMovement(Pos.x, -Pos.y, Movement, checkedTiles, attackFrom);
                    attackFrom = attackFrom.Distinct().ToList();
                    foreach (Vector2Int pos in attackFrom)
                    {
                        MarkAttack(pos.x, pos.y, AttackRange, checkedTiles);
                    }
                    GameController.Current.InteractState = InteractState.Move;
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
            movementMarker.Pos = new Vector2Int(x, -y);
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
                    if (range - GameController.Current.Map[x + i, y + j].MovementCost >= 0)
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
    private void MarkAttack(int x, int y, int range, int[,] checkedTiles)
    {
        if (checkedTiles[x, y] > 0 || -checkedTiles[x, y] > range)
        {
            return;
        }
        else if (checkedTiles[x, y] == 0)
        {
            AttackMarker attackMarker = Instantiate(AttackMarker.gameObject).GetComponent<AttackMarker>();
            attackMarker.Pos = new Vector2Int(x, -y);
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
}
