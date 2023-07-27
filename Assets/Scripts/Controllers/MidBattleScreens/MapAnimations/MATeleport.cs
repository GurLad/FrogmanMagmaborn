using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MATeleport : MAMultiTeleport
{
    public bool Init(System.Action onFinishAnimation, AdvancedSpriteSheetAnimation teleportAnimation, AudioClip teleportInSFX, AudioClip teleportOutSFX, Unit unit, Vector2Int targetPos, bool move)
    {
        return Init(onFinishAnimation, teleportAnimation, teleportInSFX, teleportOutSFX, new List<Unit> { unit }, new List<Vector2Int> { targetPos }, move);
    }
}
