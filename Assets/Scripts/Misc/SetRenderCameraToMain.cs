using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRenderCameraToMain : MonoBehaviour
{
    private Camera main;
    private void Start()
    {
        main = CameraController.Current.PixelPerfectCamera;
        GetComponent<Canvas>().worldCamera = main;
        if (GameController.Current == null)
        {
            Destroy(this);
        }
    }
    private void FixedUpdate()
    {
        if (GameController.Current.CameraBlackScreen.activeSelf)
        {
            GameController.Current.CameraBlackScreen.SetActive(false);
            Destroy(this);
        }
    }
}
