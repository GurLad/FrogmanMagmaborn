using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EndgameHalfCrystal : MonoBehaviour
{
    private enum State { Idle, WarmUp, Explode }
    [Header("Explode")]
    public float ExplodeSpeed;
    public float ExplodeRotStrength;
    public float ExplodeKnockbackStrength;
    public List<Transform> Parts = new List<Transform>();
    public List<Vector3> InitialPositions = new List<Vector3>();
    [Header("Warm Up")]
    public float WarmUpSpeed;
    [Range(0, 1)]
    public float WarmUpStrength = 0.1f;
    public bool Exploding => state == State.Explode;
    private List<Vector3> rotations = new List<Vector3>();
    private State state = State.Idle;
    private float count;

    private void Reset()
    {
        foreach (Transform child in transform)
        {
            Parts.Add(child);
            InitialPositions.Add(child.localPosition);
        }
    }

    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                break;
            case State.WarmUp:
                count += Time.unscaledDeltaTime * WarmUpSpeed;
                if (count >= 1)
                {
                    transform.localScale = Vector3.one;
                    count = 0;
                    state = State.Explode;
                }
                transform.localScale = Vector3.one * (2 - 2 * WarmUpStrength * Mathf.Sin(count * Mathf.PI)) / 2;
                break;
            case State.Explode:
                count += Time.unscaledDeltaTime * ExplodeSpeed;
                if (count >= 1)
                {
                    Destroy(gameObject);
                    return;
                }
                for (int i = 0; i < Parts.Count; i++)
                {
                    Parts[i].localScale = Vector3.one * (1 - count);
                    Parts[i].localPosition = (1 + count * ExplodeKnockbackStrength) * InitialPositions[i];
                    Parts[i].localEulerAngles = rotations[i] * count * ExplodeRotStrength;
                }
                break;
            default:
                break;
        }
    }

    private void Start()
    {
        //Explode();
    }

    public void Explode()
    {
        Parts.ForEach(a => rotations.Add(new Vector3(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f)).normalized)); // Probably a bad implemntation but eh
        count = 0;
        state = State.WarmUp;
    }

    [ContextMenu("Fix normals & bounds")]
    public void FixNormalsAndBounds()
    {
        Parts.ForEach(a =>
        {
            Mesh mesh = a.GetComponent<MeshFilter>().sharedMesh;
            mesh.RecalculateNormals();
            Bugger.Info("Before: " + mesh.bounds);
            mesh.RecalculateBounds();
            Bugger.Info("After: " + mesh.bounds);
            a.GetComponent<MeshFilter>().sharedMesh = mesh;
        });
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }

    [ContextMenu("Generate meshes")]
    public void GenerateMeshes()
    {
        foreach (Transform child in transform)
        {
            // Clear the previous attempt
            foreach (Transform subChild in child)
            {
                DestroyImmediate(subChild.gameObject);
            }
            // Make it double-faced
            Transform clone = Instantiate(child, child);
            clone.localPosition = Vector3.zero;
            MeshFilter cloneFilter = clone.GetComponent<MeshFilter>();
            string path = @"Assets/Endgame/Models/GeneratedParts/" + name + "/" + child.name + ".asset";
            Mesh found = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (found == null)
            {
                found = Instantiate(cloneFilter.sharedMesh);
                found.triangles = found.triangles.Reverse().ToArray();
                UnityEditor.MeshUtility.Optimize(found);
                UnityEditor.AssetDatabase.CreateAsset(found, path);
                UnityEditor.AssetDatabase.SaveAssets();
                found = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(path);
            }
            cloneFilter.sharedMesh = found;
            UnityEditor.EditorUtility.SetDirty(child);
        }
    }
}
