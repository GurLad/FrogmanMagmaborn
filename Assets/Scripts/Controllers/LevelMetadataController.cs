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

    #if UNITY_EDITOR
    public void AutoLoad()
    {
        // Load metadatas json
        string json = FrogForgeImporter.LoadFile<TextAsset>("LevelMetadata.json").text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("LevelMetadatas"), this);
        // Find default
        DefaultMetadata = LevelMetadatas[0];
        LevelMetadatas.RemoveAt(0);
        // Dirty
        UnityEditor.EditorUtility.SetDirty(gameObject);
    }
    #endif
}
