using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameTormentModelBodyOutline : MonoBehaviour
{
    public List<EndgameTormentModelBodyPart> BodyParts;
    public List<Transform> OutlineParts;

    private void Update()
    {
        for (int i = 0; i < BodyParts.Count; i++)
        {
            BodyParts[i].UpdateRotation();
            OutlineParts[i].localEulerAngles = BodyParts[i].CurrentRotation();
        }
    }
}
