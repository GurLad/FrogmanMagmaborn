using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MATeleport : MAMultiTeleport
{
    public bool Init(System.Action onFinishAnimation, AdvancedSpriteSheetAnimation teleportAnimation, AudioClip teleportSFX, Unit unit, Vector2Int targetPos, bool move)
    {
        return Init(onFinishAnimation, teleportAnimation, teleportSFX, new List<Unit> { unit }, new List<Vector2Int> { targetPos }, move);
    }
}
