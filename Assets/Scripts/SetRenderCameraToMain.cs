using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRenderCameraToMain : MonoBehaviour
{
    private Camera main;
    private void Awake()
    {
        main = Camera.main;
        GetComponent<Canvas>().worldCamera = main;
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
