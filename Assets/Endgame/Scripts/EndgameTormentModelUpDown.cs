using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameTormentModelUpDown : MonoBehaviour
{
    public Vector2 UpDownSpeed;
    public Vector2 UpDownStrength;
    //[HideInInspector]
    //public int UpDownDirection;
    private Vector3 basePos;
    private float upDownOffset;
    private float upDownSpeed;
    private float upDownStrength;

    private void Start()
    {
        basePos = transform.position;
        upDownOffset = Random.Range(0, 2 * Mathf.PI);
        upDownSpeed = UpDownSpeed.RandomValueInRange();
        upDownStrength = UpDownStrength.RandomValueInRange();
    }

    private void Update()
    {
        transform.position = basePos + new Vector3(0, 1 * (Mathf.Sin(Time.unscaledTime * upDownSpeed + upDownOffset) + 1) * upDownStrength / 2);
    }
}
