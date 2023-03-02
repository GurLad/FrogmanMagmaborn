using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TUpdateVolume : Trigger
{
    public bool Music;

    public override void Activate()
    {
        if (Music)
        {
            CrossfadeMusicPlayer.Current.Volume = SavedData.Load("MusicOn", 1, SaveMode.Global);
        }
        else
        {
            SoundController.SetVolume(SavedData.Load("SFXOn", 1, SaveMode.Global));
        }
    }
}
