using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiTileMoveMarker : MoveMarker
{
    private Vector2Int _targetPos;
    public override Vector2Int TargetPos { get => _targetPos; set => _targetPos = value; }
}
