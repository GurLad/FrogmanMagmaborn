using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameCrystalExplode : MonoBehaviour
{
    public float Speed;
    public List<Transform> Parts = new List<Transform>();
    public List<Vector3> InitialPositions = new List<Vector3>();
    private bool exploding;
    private float count;

    private void Reset()
    {
        foreach (Transform child in transform)
        {
            Parts.Add(child);
            InitialPositions.Add(child.localPosition);
        }
    }

    private void Start()
    {
        // TEMP
        exploding = true;
    }

    private void Update()
    {
        if (exploding)
        {
            count += Time.unscaledDeltaTime * Speed;
            if (count >= 1)
            {
                Destroy(gameObject);
                return;
            }
            for (int i = 0; i < Parts.Count; i++)
            {
                Parts[i].localScale = Vector3.one * (1 - count);
                Parts[i].localPosition = (1 + count) * InitialPositions[i];
            }
        }
    }
}
