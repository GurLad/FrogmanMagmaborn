using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This is my code for 2D animations (AdvancedAnimation's counterpart). This one allows you
/// to assign multiple animations and change which one is currently playing through different
/// scripts. You can also use SpriteSheetAnimator for single animations.
/// Version 1.2, 12/04/2020
/// </summary>
public class AdvancedSpriteSheetAnimation : AdvancedSpriteSheetAnimationBase
{
    public SpriteRenderer Renderer;

    protected override void FindRenderer()
    {
        Renderer = GetComponent<SpriteRenderer>();
    }

    protected override bool HasRenderer()
    {
        return Renderer != null;
    }

    protected override void SetSprite(Sprite sprite)
    {
        Renderer.sprite = sprite;
    }
}

[System.Serializable]
public class SpriteSheetData
{
    public string Name;
    public Sprite SpriteSheet;
    public int NumberOfFrames;
    public float Speed = -1;
    public bool Loop = false;
    [HideInInspector]
    public List<Sprite> Frames = new List<Sprite>();
    public void Split()
    {
        Frames.Clear();
        float Width = SpriteSheet.rect.width / NumberOfFrames;
        for (int i = 0; i < NumberOfFrames; i++)
        {
            Frames.Add(Sprite.Create(SpriteSheet.texture, new Rect(Width * i, 0, Width, SpriteSheet.rect.height), new Vector2(0.5f, 0.5f), SpriteSheet.pixelsPerUnit));
        }
    }
}
