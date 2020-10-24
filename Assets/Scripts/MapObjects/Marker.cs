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
        if (tile.GetArmorModifier(Origin) != 0)
        {
            AdvancedSpriteSheetAnimation advancedSpriteSheetAnimation = GetComponent<AdvancedSpriteSheetAnimation>();
            advancedSpriteSheetAnimation.Start();
            advancedSpriteSheetAnimation.Activate("Arm" + (tile.ArmorModifier > 0 ? "+" : "-"));
        }
    }
}
