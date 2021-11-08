using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CameraController : MonoBehaviour
{
    public enum CameraMode { PixelPerfect, Stretch }
    public static CameraController Current;
    public static bool FullScreen
    {
        get
        {
            return fullScreen;
        }
        set
        {
            fullScreen = value;
            if (fullScreen)
            {
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
            }
            else
            {
                Screen.fullScreenMode = FullScreenMode.Windowed;
            }
        }
    }
    private static bool fullScreen;
    public CameraMode Mode;
    public int MaxResolutionMultiplier;
    public Vector2Int ReferenceResolution;
    public AudioClip ScreenShakeSFX;
    public Camera PixelPerfectCamera;
    [HideInInspector]
    public int CurrentMultiplier = 0;
    private void Awake()
    {
        Current = this;
        FullScreen = SavedData.Load("Fullscreen", 1, SaveMode.Global) == 1;
        if (FullScreen)
        {
            UpdateMode((CameraMode)SavedData.Load("CameraMode", 0, SaveMode.Global));
        }
        else
        {
            CurrentMultiplier = SavedData.Load("ScreenSize", 0, SaveMode.Global);
            ChangeSize(0);
        }
    }
    public void UpdateMode(CameraMode mode)
    {
        Mode = mode;
        // TBA
        SavedData.Save("CameraMode", (int)mode, SaveMode.Global);
    }
    public void ChangeSize(int increaseAmount = 0)
    {
        CurrentMultiplier += increaseAmount + MaxResolutionMultiplier;
        CurrentMultiplier %= MaxResolutionMultiplier;
        Screen.SetResolution(ReferenceResolution.x * (CurrentMultiplier + 1), ReferenceResolution.y * (CurrentMultiplier + 1), false);
        SavedData.Save("ScreenSize", CurrentMultiplier, SaveMode.Global);
    }
    public void ScreenShake(float strength, float duration)
    {
        ScreenShaker screenShaker = gameObject.AddComponent<ScreenShaker>();
        screenShaker.Strength = strength;
        screenShaker.Duration = duration;
        SoundController.PlaySound(ScreenShakeSFX, 0.5f);
    }
}
