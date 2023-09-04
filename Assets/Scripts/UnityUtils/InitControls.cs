using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitControls : MonoBehaviour
{
    private static bool initDone = false;

    private void Awake()
    {
        if (initDone)
        {
            return;
        }
        // Init Steam if it's the Steam build
#if STEAM_BUILD && !UNITY_EDITOR
        try
        {
            Steamworks.SteamClient.Init(1768830, true);
        }
        catch (System.Exception e)
        {
            // Something went wrong! Steam is closed?
            Bugger.Warning("Failed to initialize Steam");
        }
#endif
        // Load & init the save slot
        SavedData.InitFiles();
        SavedData.SaveSlot = SavedData.Load("DefaultSaveSlot", 0, SaveMode.Global);
        SavedData.CreateSaveSlotFiles();
        if (!SavedData.HasKey("InitControls", SaveMode.Global))
        {
            SavedData.Save("InitControls", 1, SaveMode.Global);
            Control.SetButton(Control.CB.A, KeyCode.X);
            Control.SetButton(Control.CB.B, KeyCode.Z);
            Control.SetButton(Control.CB.Select, KeyCode.C);
            Control.SetButton(Control.CB.Start, KeyCode.Return);
            Control.SetAxis(Control.Axis.X, KeyCode.RightArrow, KeyCode.LeftArrow);
            Control.SetAxis(Control.Axis.Y, KeyCode.UpArrow, KeyCode.DownArrow);
            SavedData.Save("MusicOn", 1, SaveMode.Global);
            SavedData.Save("SFXOn", 1, SaveMode.Global);
            SavedData.Save("GameSpeed", 1, SaveMode.Global); // Default game speed is fast
            SavedData.Save("BattleAnimationsMode", 1, SaveMode.Global); // Default animations mode is player
        }
        if (!SavedData.HasKey("TransitionsOn", SaveMode.Global))
        {
            SavedData.Save("TransitionsOn", 1, SaveMode.Global);
        }
        if (!SavedData.HasKey("ScreenShakeOn", SaveMode.Global))
        {
            SavedData.Save("ScreenShakeOn", 1, SaveMode.Global);
        }
        initDone = true;
    }

#if STEAM_BUILD && !UNITY_EDITOR
    private void OnApplicationQuit()
    {
        Steamworks.SteamClient.Shutdown();
    }
#endif
}
