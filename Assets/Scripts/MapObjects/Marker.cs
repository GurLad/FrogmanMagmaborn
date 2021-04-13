using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Marker : MapObject
{
    [HideInInspector]
    public Unit Origin;
    public void ShowArmorIcon()
    {
        Tile tile = GameController.Current.Map[Pos.x, Pos.y];
        int armMod = tile.GetArmorModifier(Origin);
        if (armMod != 0)
        {
            AdvancedSpriteSheetAnimation advancedSpriteSheetAnimation = GetComponent<AdvancedSpriteSheetAnimation>();
            advancedSpriteSheetAnimation.Start();
            advancedSpriteSheetAnimation.Activate("Arm" + (armMod > 0 ? "+" : "-"));
        }
    }
}
