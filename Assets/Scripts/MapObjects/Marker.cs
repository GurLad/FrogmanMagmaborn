using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Marker : MapObject
{
    public Unit Origin;
    protected override void Start()
    {
        base.Start();
        Tile tile = GameController.Current.Map[Pos.x, Pos.y];
        if (Origin.TheTeam == Team.Player && tile.GetArmorModifier(Origin) != 0)
        {
            AdvancedSpriteSheetAnimation advancedSpriteSheetAnimation = GetComponent<AdvancedSpriteSheetAnimation>();
            advancedSpriteSheetAnimation.Start();
            advancedSpriteSheetAnimation.Activate("Arm" + (tile.ArmorModifier > 0 ? "+" : "-"));
        }
    }
}
