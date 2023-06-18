using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameTormentModelAnimator : MonoBehaviour
{
    private void Start()
    {
        transform.parent = GameController.Current.transform;
    }
}
