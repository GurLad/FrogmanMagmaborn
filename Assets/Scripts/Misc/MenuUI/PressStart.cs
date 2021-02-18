using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressStart : MonoBehaviour
{
    public GameObject Menu;
    private void Update()
    {
        if (Control.GetButtonDown(Control.CB.Start))
        {
            Menu.SetActive(true);
            Destroy(gameObject);
        }
    }
}
