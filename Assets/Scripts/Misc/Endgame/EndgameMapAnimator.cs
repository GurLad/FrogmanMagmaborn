using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameMapAnimator : MonoBehaviour
{
    [Header("Floor")]
    public string FloorName;
    public EndgameFloorAnimator FloorBaseHolder;

    private void Start()
    {
        // TEMP
        Process(GameController.Current.Map, GameController.Current.MapSize);
    }

    public void Process(Tile[,] map, Vector2Int size)
    {
        // Clone map
        // Item1 - was this tile processed already?
        // Item2 - the tile itself
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
            }
        }
    }

    private void GroupFloor(ProcessedTile[,] tiles, Vector2Int size, int x, int y)
    {
        EndgameFloorAnimator holder = Instantiate(FloorBaseHolder, FloorBaseHolder.transform.parent);
        int height = size.y;
        for (int i = x; i < size.x; i++)
        {
            for (int j = y; j < height; j++)
            {
                if (tiles[i, j].Tile.Name != FloorName)
                {
                    height = j;
                    continue;
                }
                tiles[i, j].Processed = true;
                tiles[i, j].Tile.transform.parent = holder.transform;
            }
            if (tiles[i, y].Tile.Name != FloorName)
            {
                break;
            }
        }
        holder.transform.position -= new Vector3(0, 0, 0.1f);
        holder.gameObject.SetActive(true);
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
