using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EndgameSpikeSpawner : MonoBehaviour
{
    public EndgameSpike BaseSpike;
    public float TVWidth;
    public float XOffset;
    public float YOffset;
    public float ZOffset;
    public int XCount;
    public int ZCount;

    private void Start()
    {
        for (int i = 0; i < ZCount; i++)
        {
            for (int j = 0; j < XCount; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    for (int m = 0; m < 2; m++)
                    {
                        EndgameSpike newSpike = Instantiate(BaseSpike, BaseSpike.transform.parent);
                        newSpike.transform.position += new Vector3(j * XOffset * Mathf.Sign(k - 0.5f) + TVWidth * k, m * YOffset, i * ZOffset);
                        newSpike.transform.localEulerAngles = new Vector3(-90 + 180 * m, 0, 0);
                        newSpike.UpDownDirection = (int)Mathf.Sign(k - 0.5f);
                        MeshRenderer meshRenderer = newSpike.GetComponent<MeshRenderer>();
                        for (int n = 0; n < meshRenderer.materials.Length; n++)
                        {
                            Material a = meshRenderer.materials[n];
                            meshRenderer.materials[n] = Instantiate(meshRenderer.materials[n]);
                            meshRenderer.materials[n].color = new Color(a.color.r, a.color.g, a.color.b, a.color.a * (ZCount - i) / ZCount);
                        }
                        newSpike.gameObject.SetActive(true);
                        newSpike.name = "Spike" + i + j + k + m;
                    }
                }
            }
        }
    }
}
