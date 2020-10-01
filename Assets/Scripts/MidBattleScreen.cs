using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidBattleScreen : MonoBehaviour
{
    public static MidBattleScreen Current;
    public void Quit()
    {
        GameController.Current.transform.parent.gameObject.SetActive(true);
        Destroy(transform.parent.gameObject);
    }
}
