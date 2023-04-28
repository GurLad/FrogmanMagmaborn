using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CameraController : MonoBehaviour
{
    public enum CameraMode { Default, Filter, Stretch }
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
    public Material PixelPerfectMaterial;
    public PPTScript PPTScript;
    [HideInInspector]
    public int CurrentMultiplier = 0;
    private Vector2 resolution;

    private void Awake()
    {
        Current = this;
        PPTScript.Material = PixelPerfectMaterial = Instantiate(PixelPerfectMaterial);
        FullScreen = SavedData.Load("Fullscreen", 1, SaveMode.Global) == 1;
        if (!FullScreen)
        {
            CurrentMultiplier = SavedData.Load("ScreenSize", 0, SaveMode.Global);
            ChangeSize(0);
        }
        UpdateMode((CameraMode)SavedData.Load("CameraMode", 0, SaveMode.Global));
        UpdateResolution();
    }

    private void Update()
    {
        if (resolution.y != Screen.currentResolution.height || resolution.x != Screen.currentResolution.width)
        {
            UpdateResolution();
        }
    }

    public void UpdateMode(CameraMode mode)
    {
        Mode = mode;
        PixelPerfectMaterial.SetInt("_Filter", (int)(mode & CameraMode.Filter));
        PixelPerfectMaterial.SetInt("_Stretch", !FullScreen ? 1 : (int)(mode & CameraMode.Stretch));
        SavedData.Save("CameraMode", (int)mode, SaveMode.Global);
        SavedData.SaveAll(SaveMode.Global);
    }

    public void ChangeSize(int increaseAmount = 0)
    {
        CurrentMultiplier += increaseAmount + MaxResolutionMultiplier;
        CurrentMultiplier %= MaxResolutionMultiplier;
        Screen.SetResolution(ReferenceResolution.x * (CurrentMultiplier + 1), ReferenceResolution.y * (CurrentMultiplier + 1), false);
        SavedData.Save("ScreenSize", CurrentMultiplier, SaveMode.Global);
        SavedData.SaveAll(SaveMode.Global);
        UpdateResolution();
    }

    public void ScreenShake(float strength, float duration)
    {
        if (GameCalculations.ScreenShakeOn)
        {
            ScreenShaker screenShaker = gameObject.AddComponent<ScreenShaker>();
            screenShaker.Strength = strength;
            screenShaker.Duration = duration;
        }
        SoundController.PlaySound(ScreenShakeSFX, 0.5f);
    }

    private void UpdateResolution()
    {
        PixelPerfectMaterial.SetInt("_ScreenSizeX", Screen.width);
        PixelPerfectMaterial.SetInt("_ScreenSizeY", Screen.height);
        resolution = new Vector2(Screen.width, Screen.height);
    }
}
