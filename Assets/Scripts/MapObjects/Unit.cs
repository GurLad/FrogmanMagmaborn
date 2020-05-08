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
        if (range >= 0)
        {
            checkedTiles = checkedTiles ?? new int[GameController.Current.MapSize.x, GameController.Current.MapSize.y];
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
                        MarkMovement(x + i, y + j, range - GameController.Current.Map[x + i, y + j].MovementCost, checkedTiles);
                    }
                }
            }
        }
    }
}
