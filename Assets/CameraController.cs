using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CameraController : MonoBehaviour
{
    public enum CameraMode { PixelPerfect, Stretch }
    public static CameraController Current;
    private PixelPerfectCamera pixelPerfectCamera;
    private void Awake()
    {
        Current = this;
        pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
    }
    public void UpdateMode(CameraMode mode)
    {
        switch (mode) // Add windowed with pixel ratio of X (unity doesn't let you change the pixel ratio of PixelPerfectCamera for some reason)
        {
            case CameraMode.PixelPerfect:
                pixelPerfectCamera.stretchFill = false;
                break;
            case CameraMode.Stretch:
                pixelPerfectCamera.stretchFill = true;
                break;
            default:
                break;
        }
    }
}
