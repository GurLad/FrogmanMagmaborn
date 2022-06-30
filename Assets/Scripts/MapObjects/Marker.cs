using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Marker : MapObject
{
    [HideInInspector]
    public Unit Origin;
    public PalettedSprite PalettedSprite { get; private set; }

    public void Init()
    {
        PalettedSprite = GetComponent<PalettedSprite>();
        PalettedSprite.Awake();
    }

    public void ShowArmorIcon()
    {
        Tile tile = GameController.Current.Map[Pos.x, Pos.y];
        int armMod = Origin.GetArmorModifier(Pos);
        if (armMod != 0)
        {
            AdvancedSpriteSheetAnimation advancedSpriteSheetAnimation = GetComponent<AdvancedSpriteSheetAnimation>();
            advancedSpriteSheetAnimation.Start();
            advancedSpriteSheetAnimation.Activate("Arm" + (armMod > 0 ? "+" : "-"));
        }
    }
}
