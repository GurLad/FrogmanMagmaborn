using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Team { Player, Enemy }
public class Unit : MapObject
{
    [Header("Basic info")]
    public MoveMarker MovementMarker;
    public Team TheTeam;
    [Header("Stats")]
    public int Movement;
    public override void Interact(InteractState interactState)
    {
        switch (interactState)
        {
            case InteractState.None:
                if (TheTeam == Team.Player)
                {
                    MarkMovement(Pos.x, -Pos.y, Movement);
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
    public void MarkMovement(int x, int y, int range, int[,] checkedTiles = null)
    {
        if (x < 0 || y < 0 || x >= GameController.Current.MapSize.x || y >= GameController.Current.MapSize.y)
        {
            return;
        }
        if (range >= GameController.Current.Map[x, y].MovementCost)
        {
            checkedTiles = checkedTiles ?? new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
            if (checkedTiles[x, y] >= range)
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
            checkedTiles[x, y] = range;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 || j == 0)
                    {
                        MarkMovement(x + i, y + j, range - GameController.Current.Map[x, y].MovementCost, checkedTiles);
                    }
                }
            }
        }
    }
}
