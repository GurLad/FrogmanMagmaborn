using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TReflectSettingsUpdate : Trigger
{
    public override void Activate()
    {
        GameController.Current.ReflectSettingsUpdate();
    }
}
