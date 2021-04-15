using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressStart : MonoBehaviour
{
    public MenuController Menu;
    private void Update()
    {
        if (Control.GetButtonDown(Control.CB.Start))
        {
            Menu.Begin();
            Destroy(gameObject);
        }
    }
}
