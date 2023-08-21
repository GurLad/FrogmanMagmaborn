using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShaker : MonoBehaviour
{
    public float Strength;
    public float Duration;
    private float count;
    private Vector3 pos;

    private void Start()
    {
        pos = transform.position;
    }

    private void Update()
    {
        count += Time.deltaTime;
        if (count >= Duration)
        {
            End();
        }
        else
        {
            float angel = Random.Range(0, 2 * Mathf.PI);
            float radius = Random.Range(0, Strength * (1 - count / Duration));
            transform.position = pos + new Vector3(radius * Mathf.Sin(angel), radius * Mathf.Cos(angel), 0);
        }
    }

    public void End()
    {
        transform.position = pos;
        Destroy(this);
    }
}
