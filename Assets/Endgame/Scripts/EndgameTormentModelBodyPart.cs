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

    public Vector3 CurrentRotation(int i = -1)
    {
        i = i >= 0 ? i : current;
        return new Vector3(Mathf.Sign(Direction) * 360 * i / (float)FrameCount, 0, 0);
    }

#if UNITY_EDITOR
    [ContextMenu("Generate")]
    public void Generate()
    {
        Clear();
        Transform holder = transform;
        Material baseMaterial = ApplyOn.GetComponent<MeshRenderer>().sharedMaterial;
        GameObject tempApply = RemoveTransform(Instantiate(ApplyOn));
        GameObject tempCutWith = RemoveTransform(Instantiate(CutWith));
        for (int i = 0; i < FrameCount; i++)
        {
            string path = @"Assets/Endgame/Models/GeneratedParts/" + holder.name + "-" + ((decimal)i/FrameCount) + ".asset";
            Mesh found = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (found == null)
            {
                tempApply.transform.localEulerAngles = CurrentRotation(i) * Mathf.Sign(Direction);
                Model result = CSG.Subtract(tempApply, tempCutWith);
                UnityEditor.MeshUtility.Optimize(result.mesh);
                UnityEditor.AssetDatabase.CreateAsset(result.mesh, path);
                UnityEditor.AssetDatabase.SaveAssets();
                found = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(path);
            }
            GameObject composite = new GameObject();
            composite.AddComponent<MeshFilter>().sharedMesh = found;
            composite.AddComponent<MeshRenderer>().sharedMaterials = new Material[] { baseMaterial, None }; // Weird fix
            composite.transform.parent = holder;
            composite.transform.localScale = ApplyOn.transform.localScale;
            composite.transform.localPosition = ApplyOn.transform.localPosition;
            composite.transform.localRotation = ApplyOn.transform.localRotation;
            composite.name = holder.gameObject.name + "-" + i;
            composite.gameObject.SetActive(i == 0);
            Parts.Add(composite);
        }
        ApplyOn.transform.localEulerAngles = Vector3.zero;
        if (Direction < 0)
        {
            GameObject first = Parts[0];
            Parts.RemoveAt(0);
            Parts.Reverse();
            Parts.Insert(0, first);
        }
        DestroyImmediate(tempApply);
        DestroyImmediate(tempCutWith);
        UnityEditor.EditorUtility.SetDirty(gameObject);
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
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }

    private GameObject RemoveTransform(GameObject target)
    {
        target.transform.localScale = Vector3.one;
        target.transform.position = target.transform.localPosition;
        target.transform.rotation = Quaternion.identity;
        return target;
    }

#endif
}
