using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PressStart : MonoBehaviour
{
    public MenuController Menu;
    public Text InitA;
    public Text InitB;
    public Text InitStart;

    private void Start()
    {
        if (!SavedData.HasKey("HasASaveSlot", SaveMode.Global))
        {
            if (InitA != null)
            {
                InitA.gameObject.SetActive(true);
                InitB.gameObject.SetActive(true);
                InitA.text = InitA.text.Replace("[A]", Control.DisplayShortButtonName(Control.CB.A));
                InitB.text = InitB.text.Replace("[B]", Control.DisplayShortButtonName(Control.CB.B));
                InitStart.text = InitStart.text.Replace("Start", Control.DisplayShortButtonName(Control.CB.Start));
            }
        }
    }

    private void Update()
    {
        if (Control.GetButtonDown(Control.CB.Start) || Control.GetButtonDown(Control.CB.A))
        {
            Menu.Begin();
            Destroy(gameObject);
        }
    }
}
