using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameMapAnimator : MonoBehaviour
{
    [Header("Base")]
    public AdvancedSpriteSheetAnimation PseudoTile;
    [Header("Floor")]
    public List<string> FloorNames;
    public EndgameFloorAnimator FloorBaseHolder;
    public Sprite FloorBottomL;
    public Sprite FloorBottomM;
    public Sprite FloorBottomR;
    [Header("Bridge")]
    public List<BridgeType> BridgeTypes;
    public GameObject BridgeBaseHolder;
    [Header("Void")]
    public string VoidName;
    public Vector2 VoidSpawnRate;
    public List<Sprite> VoidSpawns; // Might add the ability for void spawns bigger than 1x1, we'll see
    public Transform VoidHolder;
    private List<Vector2Int> voidPoints = new List<Vector2Int>();
    private float voidCount;

    private void Start()
    {
        VoidHolder.transform.position -= new Vector3(0, 0, 0.1f);
        // TEMP
        Process(GameController.Current.Map, GameController.Current.MapSize);
    }

    private void Update()
    {
        voidCount -= Time.deltaTime;
        if (voidCount <= 0)
        {
            SpawnVoidTile();
            voidCount = VoidSpawnRate.RandomValueInRange();
        }
    }

    public void Process(Tile[,] map, Vector2Int size)
    {
        // Clone map
        ProcessedTile[,] tiles = new ProcessedTile[size.x, size.y];
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                tiles[i, j] = new ProcessedTile(map[i, j]);
            }
        }
        // Process
        BridgeType type;
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (tiles[i, j].Processed)
                {
                    continue;
                }
                if (FloorNames.Contains(tiles[i, j].Tile.Name))
                {
                    GroupFloor(tiles, size, i, j);
                }
                if ((type = BridgeTypes.Find(a => tiles[i, j].Tile.Name == a.Name)) != null)
                {
                    GroupBridge(tiles, type, size, i, j);
                }
                if (tiles[i, j].Tile.Name == VoidName)
                {
                    voidPoints.Add(new Vector2Int(i, j));
                }
            }
        }
    }

    private void GroupFloor(ProcessedTile[,] tiles, Vector2Int size, int x, int y)
    {
        // Group
        EndgameFloorAnimator holder = Instantiate(FloorBaseHolder, FloorBaseHolder.transform.parent);
        int height = size.y;
        int width = size.x;
        for (int i = x; i < size.x; i++)
        {
            if (!FloorNames.Contains(tiles[i, y].Tile.Name))
            {
                width = i;
                break;
            }
            for (int j = y; j < height; j++)
            {
                if (!FloorNames.Contains(tiles[i, j].Tile.Name))
                {
                    height = j;
                    continue;
                }
                tiles[i, j].Processed = true;
                tiles[i, j].Tile.transform.parent = holder.transform;
                tiles[i, j].Tile.transform.position -= new Vector3(0, 0, 0.2f);
            }
        }
        // Add bottom
        for (int i = x; i < width; i++)
        {
            CreateNewPseudoTile(holder.transform, i == x ? FloorBottomL : (i == width - 1 ? FloorBottomR : FloorBottomM), new Vector2Int(i, height));
        }
        holder.transform.position -= new Vector3(0, 0, 0.2f);
        holder.gameObject.SetActive(true);
    }

    private void GroupBridge(ProcessedTile[,] tiles, BridgeType type, Vector2Int size, int x, int y)
    {
        Transform holder = Instantiate(BridgeBaseHolder, BridgeBaseHolder.transform.parent).transform;
        int height = 1, width = 1;
        if (type.Vertical)
        {
            // Group
            for (int j = y; j < size.y; j++)
            {
                if (tiles[x, j].Tile.Name != type.Name)
                {
                    height = j;
                    break;
                }
                tiles[x, j].Processed = true;
                tiles[x, j].Tile.transform.parent = holder.transform;
                tiles[x, j].Tile.GetComponent<AdvancedSpriteSheetAnimation>().Animations[0].SpriteSheet = type.Transparent;
            }
            // Add top/bottom
            PalettedSprite top = CreateNewPseudoTile(holder.transform, type.TopLeft, new Vector2Int(x, y - 1)).GetComponent<PalettedSprite>();
            top.Awake();
            top.Palette = 1;
            PalettedSprite bottom = CreateNewPseudoTile(holder.transform, type.BottomRight, new Vector2Int(x, height)).GetComponent<PalettedSprite>();
            bottom.Awake();
            bottom.Palette = 1;
        }
        else
        {
            // Group
            for (int i = x; i < size.x; i++)
            {
                if (tiles[i, y].Tile.Name != type.Name)
                {
                    width = i;
                    break;
                }
                tiles[i, y].Processed = true;
                tiles[i, y].Tile.transform.parent = holder.transform;
                tiles[i, y].Tile.GetComponent<AdvancedSpriteSheetAnimation>().Animations[0].SpriteSheet = type.Transparent;
            }
            // Add left/right
            if (GameController.Current.IsValidPos(new Vector2Int(x - 1, y)) && BridgeTypes.Find(a => tiles[x - 1, y].Tile.Name == a.Name) == null)
            {
                PalettedSprite left = CreateNewPseudoTile(holder.transform, type.TopLeft, new Vector2Int(x - 1, y)).GetComponent<PalettedSprite>();
                left.Awake();
                left.Palette = 1;
            }
            if (GameController.Current.IsValidPos(new Vector2Int(width, y)) && BridgeTypes.Find(a => tiles[width, y].Tile.Name == a.Name) == null)
            {
                PalettedSprite right = CreateNewPseudoTile(holder.transform, type.BottomRight, new Vector2Int(width, y)).GetComponent<PalettedSprite>();
                right.Awake();
                right.Palette = 1;
            }
        }
        holder.transform.position -= new Vector3(0, 0, 0.3f);
        holder.gameObject.SetActive(true);
    }

    private void SpawnVoidTile()
    {
        Vector2Int pos = voidPoints.RandomItemInList();
        Sprite spawn = VoidSpawns.RandomItemInList();
        AdvancedSpriteSheetAnimation newTile = CreateNewPseudoTile(VoidHolder, spawn, pos);
        DestroyAnimationOnFinish destroyer = newTile.gameObject.AddComponent<DestroyAnimationOnFinish>();
        newTile.Listeners.Add(destroyer);
        newTile.FixedSpeed = false;
        newTile.BaseSpeed *= 2; // fixedBaseSpeed
    }

    private AdvancedSpriteSheetAnimation CreateNewPseudoTile(Transform holder, Sprite targetSprite, Vector2Int pos)
    {
        AdvancedSpriteSheetAnimation newTile = Instantiate(PseudoTile, holder);
        SpriteSheetData newData = new SpriteSheetData();
        newData.SpriteSheet = targetSprite;
        newData.NumberOfFrames = (int)newData.SpriteSheet.rect.width / 16;
        newData.Speed = 0;
        newData.Name = "FloorBottom";
        newData.Loop = true;
        newTile.Animations.Add(newData);
        newTile.transform.localPosition = new Vector2(GameController.Current.TileSize * pos.x, -GameController.Current.TileSize * pos.y);
        newTile.gameObject.name = targetSprite.name;
        newTile.gameObject.SetActive(true);
        return newTile;
    }

    private class ProcessedTile
    {
        public bool Processed { get; set; } = false;
        public Tile Tile { get; private set; }

        public ProcessedTile(Tile tile)
        {
            Tile = tile;
        }
    }

    [System.Serializable]
    public class BridgeType
    {
        public string Name;
        public Sprite Transparent;
        public Sprite TopLeft;
        public Sprite BottomRight;
        public bool Vertical;
    }

    public class DestroyAnimationOnFinish : MonoBehaviour, IAdvancedSpriteSheetAnimationListener
    {
        public void ChangedFrame(int id, string name, int newFrame)
        {
            // Do nothing
        }

        public void FinishedAnimation(int id, string name)
        {
            Destroy(gameObject);
        }
    }
}
