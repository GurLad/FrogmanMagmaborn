using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameSpike : MonoBehaviour
{
    public Vector2 RotSpeed;
    public Vector2 UpDownSpeed;
    public Vector2 UpDownStrength;
    [HideInInspector]
    public int UpDownDirection;
    private Vector3 basePos;
    private Vector3 baseRot;
    private float rotOffset;
    private float rotSpeed;
    private float upDownOffset;
    private float upDownSpeed;
    private float upDownStrength;

    private void Start()
    {
        basePos = transform.position;
        baseRot = transform.localEulerAngles;
        rotOffset = Random.Range(0, 360f);
        rotSpeed = RotSpeed.RandomValueInRange();
        upDownOffset = Random.Range(0, 2 * Mathf.PI);
        upDownSpeed = UpDownSpeed.RandomValueInRange();
        upDownStrength = UpDownStrength.RandomValueInRange();
    }

    private void Update()
    {
        transform.position = basePos + new Vector3(0, UpDownDirection * (Mathf.Sin(Time.time * upDownSpeed + upDownOffset) + 1) * upDownStrength / 2);
        transform.localEulerAngles = baseRot + new Vector3(0, 360 * rotSpeed * Time.time + rotOffset);
    }
}
