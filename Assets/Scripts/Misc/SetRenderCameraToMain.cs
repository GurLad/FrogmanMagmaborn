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
        Destroy(this);
    }
}
