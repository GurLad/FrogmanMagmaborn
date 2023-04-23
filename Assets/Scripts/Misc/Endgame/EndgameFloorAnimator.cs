using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameFloorAnimator : MonoBehaviour
{
    public float Speed;
    public float Strength;
    private float ratio;
    private float cosTheta;
    private float sinTheta;
    private Vector3 basePos;
    private float offest;

    private void Start()
    {
        basePos = transform.position;
        // Generate x to y ratio
        ratio = Random.Range(0.0001f, 1);
        // Rotate by a random angle
        float theta = Random.Range(0, 2 * Mathf.PI);
        // Add random offset
        offest = Random.Range(0, 2 * Mathf.PI);
        cosTheta = Mathf.Cos(theta);
        sinTheta = Mathf.Sin(theta);
    }

    private void Update()
    {
        Vector2 modifier = GetPosModifier(Time.time * Speed + offest) * Strength;
        transform.position = basePos + new Vector3(modifier.x, modifier.y, 0);
    }

    private Vector2 GetPosModifier(float t)
    {
        return new Vector2(cosTheta * Mathf.Cos(t) - sinTheta * Mathf.Sin(t) * ratio, sinTheta * Mathf.Cos(t) + cosTheta * Mathf.Sin(t) * ratio);
    }
}
