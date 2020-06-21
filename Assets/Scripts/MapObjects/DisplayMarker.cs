using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayMarker : Marker
{
    public override void Interact(InteractState interactState)
    {
        if (interactState == InteractState.None)
        {
            GameController.Current.RemoveMarkers();
        }
    }
}