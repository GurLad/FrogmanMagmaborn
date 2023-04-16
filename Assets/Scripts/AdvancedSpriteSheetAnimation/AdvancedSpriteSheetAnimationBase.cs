using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AdvancedSpriteSheetAnimationBase : MonoBehaviour
{
    public List<SpriteSheetData> Animations;
    public bool Active { get; private set; }
    public bool FixedSpeed;
    public float BaseSpeed;
    public bool ActivateOnStart;
    public List<IAdvancedSpriteSheetAnimationListener> Listeners = new List<IAdvancedSpriteSheetAnimationListener>();
    private static float fixedBaseSpeed = 2;
    private float speed;
    private bool loop;
    private float count = 0;
    private int currentAnimation = -1;
    private int currentFrame = 0;

    private void Reset()
    {
        FindRenderer();
    }

    public void Start()
    {
        if (Active)
        {
            return;
        }
        if (!HasRenderer())
        {
            FindRenderer();
        }
        Animations.ForEach(a => a.Split());
        if (FixedSpeed && BaseSpeed <= 0)
        {
            BaseSpeed = 1;
        }
        if (ActivateOnStart)
        {
            Activate(0, true);
        }
    }

    private void Update()
    {
        void UpdateFrame(int nextFrame)
        {
            currentFrame = nextFrame;
            if (currentFrame >= Animations[currentAnimation].Frames.Count)
            {
                if (loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    Active = false;
                    Listeners.ForEach(a => a.FinishedAnimation(currentAnimation, Animations[currentAnimation].Name));
                    return;
                }
                Listeners.ForEach(a => a.FinishedAnimation(currentAnimation, Animations[currentAnimation].Name));
            }
            else
            {
                Listeners.ForEach(a => a.ChangedFrame(currentAnimation, Animations[currentAnimation].Name, currentFrame));
            }
            SetSprite(Animations[currentAnimation].Frames[currentFrame]);
        }

        if (Active)
        {
            if (FixedSpeed)
            {
                int fixedFrame = (int)(Time.time * fixedBaseSpeed * BaseSpeed) % Animations[currentAnimation].Frames.Count;
                if (fixedFrame != currentFrame)
                {
                    UpdateFrame(fixedFrame);
                }
            }
            else
            {
                count += Time.deltaTime * speed;
                if (count >= 1)
                {
                    count--;
                    UpdateFrame(currentFrame + 1);
                }
            }
        }
    }
    /// <summary>
    /// Switches to the specified animation.
    /// </summary>
    /// <param name="animation">Animation ID to change to.</param>
    /// <param name="forceRestart">Restart the animation if the new ID is equal to the current one.</param>
    public void Activate(int animation, bool forceRestart = false)
    {
        if (!forceRestart && currentAnimation == animation)
        {
            return;
        }
        currentAnimation = animation;
        speed = Animations[currentAnimation].Speed > 0 ? Animations[currentAnimation].Speed : BaseSpeed;
        loop = Animations[currentAnimation].Loop;
        count = 0;
        currentFrame = FixedSpeed ? (int)(Time.time * fixedBaseSpeed) % Animations[currentAnimation].Frames.Count : 0;
        Active = true;
        SetSprite(Animations[currentAnimation].Frames[currentFrame]);
    }
    /// <summary>
    /// Switches to the specified animation.
    /// </summary>
    /// <param name="animationName">Name of the animation to switch to.</param>
    /// <param name="forceRestart">Restart the animation if the new animation is equal to the current one.</param>
    /// <exception cref="System.Exception">No matching animation</exception>
    public void Activate(string animationName, bool forceRestart = false)
    {
        int newID = Animations.FindIndex(a => a.Name == animationName);
        if (newID < 0)
        {
            throw Bugger.Error("No matching animation! (" + animationName + ")");
        }
        if (!forceRestart && currentAnimation == newID)
        {
            return;
        }
        currentAnimation = newID;
        speed = Animations[currentAnimation].Speed > 0 ? Animations[currentAnimation].Speed : BaseSpeed;
        loop = Animations[currentAnimation].Loop;
        count = 0;
        currentFrame = FixedSpeed ? (int)(Time.time * fixedBaseSpeed) % Animations[currentAnimation].Frames.Count : 0;
        Active = true;
        SetSprite(Animations[currentAnimation].Frames[currentFrame]);
    }
    /// <summary>
    /// Returns whether an animation by the given name exists.
    /// </summary>
    /// <param name="animationName">Name of the animation to check.</param>
    /// <returns>Whether an animation by the given name exists.</returns>
    public bool HasAnimation(string animationName)
    {
        return Animations.Find(a => a.Name == animationName) != null;
    }
    /// <summary>
    /// Restarts the current animation.
    /// </summary>
    public void Restart()
    {
        count = 0;
        currentFrame = FixedSpeed ? (int)(Time.time * fixedBaseSpeed) % Animations[currentAnimation].Frames.Count : 0;
        Active = true;
        SetSprite(Animations[currentAnimation].Frames[currentFrame]);
    }

    [ContextMenu("Assign first frame to renderer")]
    public void EditorPreview()
    {
        Animations[0].Split();
        SetSprite(Animations[0].Frames[0]);
    }

    // Abstract renderer commands

    protected abstract bool HasRenderer();
    protected abstract void FindRenderer();
    protected abstract void SetSprite(Sprite sprite);
}
