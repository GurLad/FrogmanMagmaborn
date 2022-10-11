using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedSpriteSheetAnimationUI : AdvancedSpriteSheetAnimationBase
{
    public Image Renderer;

    protected override void FindRenderer()
    {
        Renderer = GetComponent<Image>();
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
