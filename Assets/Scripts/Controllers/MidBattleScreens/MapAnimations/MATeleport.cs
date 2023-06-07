using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MATeleport : MapAnimation, IAdvancedSpriteSheetAnimationListener
{
    public AdvancedSpriteSheetAnimation TeleportAnimation;
    public AudioClip TeleportSFX;
    private Unit currentUnit;
    private bool skipOut;
    private Vector2Int target;

    public bool Init(System.Action onFinishAnimation, AdvancedSpriteSheetAnimation teleportAnimation, AudioClip teleportSFX, Unit unit, Vector2Int targetPos, bool move)
    {
        // Constructor
        OnFinishAnimation = onFinishAnimation;
        TeleportAnimation = teleportAnimation;
        TeleportSFX = teleportSFX;
        currentUnit = unit;
        skipOut = !move;
        target = targetPos.x >= 0 ? targetPos : unit.Pos;
        // Hide unit until teleport in if !move
        if (skipOut)
        {
            currentUnit.Pos = target;
            currentUnit.gameObject.SetActive(false);
        }
        // Create animation
        TeleportAnimation = CreateAnimationOnUnit(unit, TeleportAnimation);
        TeleportAnimation.Listeners.Add(this);
        return init = true;
    }

    public override void StartAnimation()
    {
        base.StartAnimation();
        if (skipOut)
        {
            TeleportAnimation.Activate("StartIn");
        }
        else
        {
            TeleportAnimation.Activate("StartOut");
        }
    }

    public void ChangedFrame(int id, string name, int newFrame)
    {
        // Do nothing
    }

    public void FinishedAnimation(int id, string name)
    {
        switch (name)
        {
            case "StartOut":
                currentUnit.gameObject.SetActive(false);
                currentUnit.Pos = target;
                TeleportAnimation.Activate("EndOut");
                break;
            case "EndOut":
                TeleportAnimation.transform.position = currentUnit.transform.position;
                TeleportAnimation.transform.position += new Vector3(0, 0, -0.5f);
                TeleportAnimation.Activate("StartIn");
                break;
            case "StartIn":
                currentUnit.gameObject.SetActive(true);
                TeleportAnimation.Activate("EndIn");
                break;
            case "EndIn":
                Destroy(TeleportAnimation.gameObject);
                EndAnimation();
                break;
            default:
                break;
        }
    }

    protected override void Animate()
    {
        // Do nothing
    }
}
