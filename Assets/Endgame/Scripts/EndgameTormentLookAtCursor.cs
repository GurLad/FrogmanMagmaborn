using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameTormentLookAtCursor : MonoBehaviour
{
    public Transform Model;
    public Vector3 Offset;

    private void Update()
    {
        Quaternion previousRotation = Model.rotation;
        Model.LookAt(GameController.Current.Cursor.transform.position + Offset);
        Model.rotation = Quaternion.Lerp(previousRotation, Model.rotation, Time.unscaledDeltaTime);
    }
}
