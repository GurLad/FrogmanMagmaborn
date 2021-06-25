using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMetadataController : MonoBehaviour
{
    [SerializeField]
    private LevelMetadata DefaultMetadata = new LevelMetadata();
    [SerializeField]
    private List<LevelMetadata> LevelMetadatas;

    public LevelMetadata this[int i]
    {
        get
        {
            return i <= 0 ? DefaultMetadata : LevelMetadatas[i - 1];
        }
    }
}
