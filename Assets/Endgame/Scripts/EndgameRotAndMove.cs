using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameRotAndMove : MonoBehaviour
{
    public enum Axis { X, Y, Z };

    public Axis RotAxis = Axis.Y;
    public Vector2 RotSpeed;
    public Axis UpDownAxis = Axis.Y;
    public Vector2 UpDownSpeed;
    public Vector2 UpDownStrength;
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
        if (upDownSpeed > 0)
        {
            transform.position = basePos + AxisToVector3(UpDownAxis) * (UpDownDirection * (Mathf.Sin(Time.unscaledTime * upDownSpeed + upDownOffset) + 1) * upDownStrength / 2);
        }
        if (rotSpeed > 0)
        {
            transform.localEulerAngles = baseRot + AxisToVector3(RotAxis) * (360 * rotSpeed * Time.unscaledTime + rotOffset);
        }
    }

    private Vector3 AxisToVector3(Axis axis)
    {
        return axis switch
        {
            Axis.X => Vector3.right,
            Axis.Y => Vector3.up,
            Axis.Z => Vector3.forward,
            _ => Vector3.zero
        };
    }
}
