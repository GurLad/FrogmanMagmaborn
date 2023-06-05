using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MAMovement : MapAnimation
{
    public float WalkSpeed;
    public AudioClip WalkSound;
    private Unit currentUnit;
    private SpriteRenderer unitSpriteRenderer;
    private List<Vector2Int> path = new List<Vector2Int>();

    public bool Init(System.Action onFinishAnimation, float walkSpeed, AudioClip walkSound, Unit unit, Vector2Int targetPos)
    {
        // Constructor
        OnFinishAnimation = onFinishAnimation;
        WalkSpeed = walkSpeed;
        WalkSound = walkSound;
        // Check if an animation is even needed
        if (OnFinishAnimation == null)
        {
            throw Bugger.FMError("No OnFinishAnimation command - probably animated before assigning it.", false);
        }
        if (unit.Pos == targetPos)
        {
            OnFinishAnimation();
            return false;
        }
        currentUnit = unit;
        if (path.Count > 0)
        {
            Bugger.Warning("Path isn't empty!");
            path.Clear();
        }
        path = unit.FindPath(targetPos);
        // Start animation
        unitSpriteRenderer = unit.gameObject.GetComponent<SpriteRenderer>();
        return init = true;
    }

    protected override void Animate()
    {
        if (count >= 1 / WalkSpeed)
        {
            count -= 1 / WalkSpeed;
            SoundController.PlaySound(WalkSound);
            FlipX(path[0] - currentUnit.Pos, unitSpriteRenderer);
            currentUnit.Pos = path[0];
            path.RemoveAt(0);
            if (path.Count <= 0)
            {
                EndAnimation();
            }
        }
    }
}
