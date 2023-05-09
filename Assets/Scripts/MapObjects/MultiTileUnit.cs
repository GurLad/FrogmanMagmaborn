using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiTileUnit : Unit
{
    [Header("MultiTile extra fields")]
    public MultiTileMoveMarker MultiTileMoveMarker;
    public override Vector2Int Pos
    {
        get
        {
            return base.Pos;
        }
        set
        {
            base.Pos = value;
            transform.position = new Vector3((Pos.x + (Size.x - 1) / 2.0f) * GameController.Current.TileSize, -(Pos.y + (Size.y - 1) / 2.0f) * GameController.Current.TileSize, transform.position.z);
        }
    }
    [SerializeField]
    private Vector2Int size = Vector2Int.one;
    public Vector2Int Size
    {
        get => size;
        set
        {
            size = value;
            transform.localScale = new Vector3(size.x, size.y, 1);
            Pos = Pos;
        }
    }

    protected override void GenerateMultiTileMoveMarker(int i, int j, DangerArea dangerArea)
    {
        MultiTileMoveMarker movementMarker = Instantiate(MultiTileMoveMarker.gameObject).GetComponent<MultiTileMoveMarker>();
        movementMarker.Pos = new Vector2Int(i, j);
        movementMarker.TargetPos = dangerArea[i, j].Parent?.Pos ?? throw Bugger.FMError("Multi-tile move marker without a parent?");
        movementMarker.Origin = this;
        movementMarker.ShowArmorIcon();
        movementMarker.gameObject.SetActive(true);
    }

    public override bool AtPos(Vector2Int pos)
    {
        return base.AtPos(pos) || (Size != Vector2Int.one && pos.x >= Pos.x && pos.x <= Pos.x + Size.x - 1 && pos.y >= Pos.y && pos.y <= Pos.y + Size.y - 1);
    }

    protected override DangerArea GetDangerArea(int x, int y, int range, bool includePassThroughMoves = false)
    {
        return MultiTileDangerArea.Generate(this, x, y, range, includePassThroughMoves);
    }

    public class MultiTileDangerArea : Unit.DangerArea
    {
        private MultiTileUnit multiTileUnit;

        protected MultiTileDangerArea(MultiTileUnit unit) : base(unit)
        {
            multiTileUnit = unit;
        }

        protected MultiTileDangerArea(MultiTileUnit unit, int x, int y, int range, bool includePassThroughMoves) : base(unit, x, y, range, includePassThroughMoves)
        {
            multiTileUnit = unit;
        }

        public static MultiTileDangerArea Generate(MultiTileUnit unit, int x, int y, int range, bool includePassThroughMoves)
        {
            return new MultiTileDangerArea(unit, x, y, range, includePassThroughMoves);
        }

        protected override void PostProcessMovement(List<Vector2Int> attackFrom)
        {
            List<Vector2Int> attackFromClone = new List<Vector2Int>(attackFrom);
            foreach (Vector2Int pos in attackFromClone)
            {
                for (int i = 0; i < multiTileUnit.Size.x; i++)
                {
                    for (int j = 0; j < multiTileUnit.Size.y; j++)
                    {
                        if (i != 0 || j != 0)
                        {
                            if (this[pos.x + i, pos.y + j].Value > 0 ||
                                !GameController.Current.Map[pos.x + i, pos.y + j].Passable ||
                                attackFromClone.Contains(new Vector2Int(pos.x + i, pos.y + j)))
                            {
                                continue;
                            }
                            this[pos.x + i, pos.y + j].Type = TileDataType.MultiTileMove;
                            this[pos.x + i, pos.y + j].Value = this[pos.x, pos.y].Value;
                            this[pos.x + i, pos.y + j].Parent = this[pos.x, pos.y];
                            attackFrom.Add(new Vector2Int(pos.x + i, pos.y + j));
                        }
                    }
                }
            }
        }

        protected override int FindMovementGetCost(int x, int y)
        {
            int max = 0;
            for (int i = 0; i < multiTileUnit.Size.x; i++)
            {
                for (int j = 0; j < multiTileUnit.Size.y; j++)
                {
                    max = Mathf.Max(max, GameController.Current.Map[x + i, y + j].GetMovementCost(unit));
                }
            }
            return max;
        }

        protected override Unit FindMovementGetUnit(int x, int y)
        {
            Unit max = null, temp;
            for (int i = 0; i < multiTileUnit.Size.x; i++)
            {
                for (int j = 0; j < multiTileUnit.Size.y; j++)
                {
                    temp = GameController.Current.FindUnitAtPos(x + i, y + j);
                    if (temp != null && (max == null || (max == unit && temp != unit) || (!unit.IsEnemy(max) && unit.IsEnemy(temp))))
                    {
                        max = temp;
                    }
                }
            }
            return max;
        }
    }
}
