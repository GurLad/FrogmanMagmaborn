using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TShowOpeningCutscene : Trigger
{
    public OpeningCutscene OpeningCutscene;
    public MenuController MenuController;

    public override void Activate()
    {
        PaletteController.Current.FadeOut(() =>
        {
            MenuController.Finish();
            OpeningCutscene.BeginCutscene();
        });
    }
}
