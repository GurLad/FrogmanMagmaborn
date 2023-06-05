using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MADelay : MapAnimation
{
    public float DelayTime;

    public bool Init(System.Action onFinishAnimation, float delayTime)
    {
        // Constructor
        OnFinishAnimation = onFinishAnimation;
        DelayTime = delayTime;
        return true;
    }

    protected override void Animate()
    {
        if (count >= DelayTime)
        {
            EndAnimation();
        }
    }
}
