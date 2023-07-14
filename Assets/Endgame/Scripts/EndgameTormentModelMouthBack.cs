using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameTormentModelMouthBack : MonoBehaviour
{
    public float Multiplier;
    public float Offset;
    public List<Transform> Parts;
    public Transform BottomLip;

    private void Update()
    {
        Parts.ForEach(a => a.localScale = new Vector3(-Multiplier * (BottomLip.localPosition.y + Offset), a.localScale.y, a.localScale.z));
    }
}
