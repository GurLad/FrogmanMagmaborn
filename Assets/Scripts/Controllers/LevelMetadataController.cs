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

#if UNITY_EDITOR || MODDABLE_BUILD
    public void AutoLoad()
    {
        // Load metadatas json
        string json = FrogForgeImporter.LoadTextFile("LevelMetadata.json").Text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("LevelMetadatas"), this);
        // Find default
        DefaultMetadata = LevelMetadatas[0];
        LevelMetadatas.RemoveAt(0);
        // Dirty
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }
#endif
}
