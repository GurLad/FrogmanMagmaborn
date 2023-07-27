using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MAMultiTeleport : MapAnimation, IAdvancedSpriteSheetAnimationListener
{
    public AdvancedSpriteSheetAnimation TeleportAnimation;
    public AudioClip TeleportSFX;
    private List<Unit> currentUnits;
    private List<Vector2Int> targets;
    private List<AdvancedSpriteSheetAnimation> animations = new List<AdvancedSpriteSheetAnimation>();
    private bool skipOut;

    public bool Init(System.Action onFinishAnimation, AdvancedSpriteSheetAnimation teleportAnimation, AudioClip teleportSFX, List<Unit> units, List<Vector2Int> targetPositions, bool move)
    {
        // Constructor
        OnFinishAnimation = onFinishAnimation;
        TeleportAnimation = teleportAnimation;
        TeleportSFX = teleportSFX;
        currentUnits = units;
        skipOut = !move;
        targets = targetPositions;
        // Hide unit until teleport in if !move
        if (skipOut)
        {
            for (int i = 0; i < currentUnits.Count; i++)
            {
                currentUnits[i].Pos = targets[i];
                currentUnits[i].gameObject.SetActive(false);
            }
        }
        return init = true;
    }

    public override void StartAnimation()
    {
        base.StartAnimation();
        // Create animation
        for (int i = 0; i < currentUnits.Count; i++)
        {
            animations.Add(CreateAnimationOnUnit(currentUnits[i], TeleportAnimation));
        }
        animations[0].Listeners.Add(this); // Need to listen to only one animations - the rest will (hopefully) stay in sync
        if (skipOut)
        {
            animations.ForEach(a => a.Activate("StartIn"));
        }
        else
        {
            animations.ForEach(a => a.Activate("StartOut"));
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
                for (int i = 0; i < currentUnits.Count; i++)
                {
                    currentUnits[i].gameObject.SetActive(false);
                    currentUnits[i].Pos = targets[i];
                }
                animations.ForEach(a => a.Activate("EndOut"));
                break;
            case "EndOut":
                for (int i = 0; i < currentUnits.Count; i++)
                {
                    animations[i].transform.position = currentUnits[i].transform.position;
                    animations[i].transform.position += new Vector3(0, 0, -0.5f);
                    animations[i].Activate("StartIn");
                }
                break;
            case "StartIn":
                currentUnits.ForEach(a => a.gameObject.SetActive(true));
                animations.ForEach(a => a.Activate("EndIn"));
                break;
            case "EndIn":
                animations.ForEach(a => Destroy(a.gameObject));
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
