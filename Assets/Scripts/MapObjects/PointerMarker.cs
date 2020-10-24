using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerMarker : Marker
{
    public PalettedSprite PalettedSprite;
    private void Reset()
    {
        PalettedSprite = GetComponent<PalettedSprite>();
    }
    public override void Interact(InteractState interactState)
    {
        // Do nothing
    }
}
