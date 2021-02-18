using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TSetFullscreen : Trigger
{
    public MenuController Menu;
    public MenuItem CameraMode;
    public MenuItem ScreenSize;
    private Text Text;
    private void Start()
    {
        Text = GetComponent<Text>();
        UpdateText();
    }
    public override void Activate()
    {
        CameraController.FullScreen = !CameraController.FullScreen;
        UpdateText();
        SavedData.Save("Fullscreen", CameraController.FullScreen ? 1 : 0, SaveMode.Global);
    }
    private void UpdateText()
    {
        Text.text = CameraController.FullScreen ? "Fullscreen" : "Windowed";
        if (CameraController.FullScreen)
        {
            Menu.MenuItems[1] = CameraMode;
            CameraMode.gameObject.SetActive(true);
            ScreenSize.gameObject.SetActive(false);
        }
        else
        {
            Menu.MenuItems[1] = ScreenSize;
            ScreenSize.gameObject.SetActive(true);
            CameraMode.gameObject.SetActive(false);
            CameraController.Current.ChangeSize(0);
        }
    }
}
