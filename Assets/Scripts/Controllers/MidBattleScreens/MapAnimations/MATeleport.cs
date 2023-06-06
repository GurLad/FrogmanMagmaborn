using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MATeleport : MapAnimation, IAdvancedSpriteSheetAnimationListener
{
    public AdvancedSpriteSheetAnimation TeleportAnimation;
    public AudioClip TeleportSFX;
    private Unit currentUnit;

    public bool Init(System.Action onFinishAnimation, AdvancedSpriteSheetAnimation teleportAnimation, AudioClip teleportSFX, Unit unit)
    {
        // Constructor
        OnFinishAnimation = onFinishAnimation;
        TeleportAnimation = teleportAnimation;
        TeleportSFX = teleportSFX;
        currentUnit = unit;
        // Create animation
        TeleportAnimation = CreateAnimationOnUnit(unit, TeleportAnimation);
        TeleportAnimation.Listeners.Add(this);
        return true;
    }

    public override void StartAnimation()
    {
        base.StartAnimation();
        currentUnit.gameObject.SetActive(false);
        TeleportAnimation.Activate("Start");
    }

    public void ChangedFrame(int id, string name, int newFrame)
    {
        // Do nothing
    }

    public void FinishedAnimation(int id, string name)
    {
        if (name == "Start")
        {
            currentUnit.gameObject.SetActive(true);
        }
        else // if (name == "End")
        {
            Destroy(TeleportAnimation.gameObject);
            EndAnimation();
        }
    }

    protected override void Animate()
    {
        // Do nothing
    }
}
