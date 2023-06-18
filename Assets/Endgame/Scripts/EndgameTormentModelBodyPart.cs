using Parabox.CSG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameTormentModelBodyPart : MonoBehaviour
{
    [Header("Animation")]
    public int FrameRate = 30;
    public int FrameCount;
    [Range(-1, 1)]
    public int Direction;
    public List<GameObject> Parts;
    [Header("Generator")]
    public GameObject ApplyOn;
    public GameObject CutWith;
    public Material None;
    private float count = 0;
    private int current = 0;

    private void Start()
    {
        Generate(); // Probably a better idea than massively increasing the file size by pre-generating all models
    }

    public void UpdateRotation()
    {
        count += Time.unscaledDeltaTime;
        while (count >= 1f / FrameRate)
        {
            count -= 1f / FrameRate;
            Parts[current].SetActive(false);
            Parts[current = (current + 1) % FrameCount].SetActive(true);
        }
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        Clear();
        Transform holder = transform;
        Material baseMaterial = ApplyOn.GetComponent<MeshRenderer>().sharedMaterial;
        for (int i = 0; i < FrameCount; i++)
        {
            ApplyOn.transform.localEulerAngles = CurrentRotation(i);
            Model result = CSG.Subtract(ApplyOn, CutWith);
            GameObject composite = new GameObject();
            composite.AddComponent<MeshFilter>().sharedMesh = result.mesh;
            composite.AddComponent<MeshRenderer>().sharedMaterials = new Material[] { baseMaterial, None }; // Weird fix
            composite.transform.parent = holder;
            composite.name = holder.gameObject.name + "-" + i;
            composite.gameObject.SetActive(i == 0);
            Parts.Add(composite);
        }
        ApplyOn.transform.localEulerAngles = Vector3.zero;
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        Transform holder = transform;
        ApplyOn.name = "_" + holder.name + "Base";
        ApplyOn.transform.parent = holder;
        ApplyOn.SetActive(false);
        Parts.ForEach(a => DestroyImmediate(a));
        Parts.Clear();
    }

    public Vector3 CurrentRotation(int i = -1)
    {
        i = i >= 0 ? i : current;
        return new Vector3(Mathf.Sign(Direction) * 360 * i / (float)FrameCount, 0, 0);
    }
}
