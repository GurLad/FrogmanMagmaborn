using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameMapAnimator : MonoBehaviour
{
    [Header("Base")]
    public AdvancedSpriteSheetAnimation PseudoTile;
    [Header("Floor")]
    public string FloorName;
    public EndgameFloorAnimator FloorBaseHolder;
    public Sprite FloorBottomL;
    public Sprite FloorBottomM;
    public Sprite FloorBottomR;
    [Header("Bridge")]
    public string BridgeName;
    public GameObject BridgeBaseHolder;
    public Sprite BridgeTransparent;
    public Sprite BridgeTop;
    public Sprite BridgeBottom;

    private void Start()
    {
        // TEMP
        Process(GameController.Current.Map, GameController.Current.MapSize);
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
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (tiles[i, j].Processed)
                {
                    continue;
                }
                if (tiles[i, j].Tile.Name == FloorName)
                {
                    GroupFloor(tiles, size, i, j);
                }
                if (tiles[i, j].Tile.Name == BridgeName)
                {
                    GroupBridge(tiles, size, i, j);
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
            if (tiles[i, y].Tile.Name != FloorName)
            {
                width = i;
                break;
            }
            for (int j = y; j < height; j++)
            {
                if (tiles[i, j].Tile.Name != FloorName)
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
        holder.transform.position -= new Vector3(0, 0, 0.1f);
        holder.gameObject.SetActive(true);
    }

    private void GroupBridge(ProcessedTile[,] tiles, Vector2Int size, int x, int y, bool vertical = true)
    {
        Transform holder = Instantiate(BridgeBaseHolder, BridgeBaseHolder.transform.parent).transform;
        int height = 1, width = 1;
        if (vertical)
        {
            // Group
            for (int j = y; j < size.y; j++)
            {
                if (tiles[x, j].Tile.Name != BridgeName)
                {
                    height = j;
                    break;
                }
                tiles[x, j].Processed = true;
                tiles[x, j].Tile.transform.parent = holder.transform;
                tiles[x, j].Tile.GetComponent<AdvancedSpriteSheetAnimation>().Animations[0].SpriteSheet = BridgeTransparent;
            }
            // Add top/bottom
            PalettedSprite top = CreateNewPseudoTile(holder.transform, BridgeTop, new Vector2Int(x, y - 1)).GetComponent<PalettedSprite>();
            top.Awake();
            top.Palette = 1;
            PalettedSprite bottom = CreateNewPseudoTile(holder.transform, BridgeBottom, new Vector2Int(x, height)).GetComponent<PalettedSprite>();
            bottom.Awake();
            bottom.Palette = 1;
        }
        else
        {
            // TBA
        }
        holder.transform.position -= new Vector3(0, 0, 0.2f);
        holder.gameObject.SetActive(true);
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
        newTile.transform.position = new Vector2(GameController.Current.TileSize * pos.x, -GameController.Current.TileSize * pos.y);
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
}
