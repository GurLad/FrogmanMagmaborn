using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour // TBA: Move all map-related stuff here. Currently only a tileset controller
{
    [Header("Data")]
    public List<Tileset> Tilesets;
    public List<Map> Maps;
    [Header("Objects")]
    public Tile BaseTile;
    public Transform TilesetsContainer;
    [HideInInspector]
    [SerializeField]
    private MapData mapData;

#if UNITY_EDITOR || MODDABLE_BUILD
    public void AutoLoadTilesets()
    {
        // Clear previous data
        Tilesets.Clear();
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in TilesetsContainer)
        {
            toDestroy.Add(child.gameObject);
        }
        while (toDestroy.Count > 0)
        {
            DestroyImmediate(toDestroy[0]);
            toDestroy.RemoveAt(0);
        }
        // Load jsons
        string json = FrogForgeImporter.LoadTextFile("Tilesets.json").Text;
        JsonUtility.FromJsonOverwrite(json.ForgeJsonToUnity("Tilesets"), this);
        // Generate objects & load sprites
        for (int i = 0; i < Tilesets.Count; i++)
        {
            GameObject container = new GameObject(Tilesets[i].Name);
            container.transform.parent = TilesetsContainer;
            Tilesets[i].GenerateObjects(BaseTile, container);
            for (int j = 0; j < Tilesets[i].TileObjects.Count; j++)
            {
                Sprite file = FrogForgeImporter.LoadSpriteFile("Images/Tilesets/" + Tilesets[i].Name + "/" + j + ".png");
                FrogForgeImporter.LoadSpriteOrAnimationToObject(Tilesets[i].TileObjects[j].gameObject, file, 16);
            }
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }

    public void AutoLoadMaps()
    {
        // Clear previous data
        Maps.Clear();
        // Load jsons
        string[] fileNames = FrogForgeImporter.GetAllFilesAtPath("Maps");
        foreach (string fileName in fileNames)
        {
            TextFile file = FrogForgeImporter.LoadTextFile(fileName, true);
            JsonUtility.FromJsonOverwrite(file.Text.ForgeJsonToUnity("mapData"), this);
            Map map = new Map();
            // Level numer
            map.LevelNumber = mapData.LevelNumber;
            // Tileset
            map.Tileset = mapData.Tileset;
            // Objective
            string[] objectiveParts = mapData.Objective.Split(':');
            map.Objective = (Objective)System.Enum.Parse(typeof(Objective), objectiveParts[0]);
            map.ObjectiveData = mapData.Objective.Substring(mapData.Objective.IndexOf(':') + 1);
            // Map
            map.MapString = mapData.Tiles;
            // Units
            map.Units = new List<UnitPlacementData>(mapData.Units.ConvertAll(a => a.Clone()));
            // Map events
            map.MapEvents = mapData.MapEvents.ConvertAll(a => "~\n" + a.Requirements + "\n~\n~\n" + a.Event);
            // Name
            map.Name = mapData.Name;
            Maps.Add(map);
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
    }
#endif

    [System.Serializable]
    public class MapData
    {
        public string Tiles;
        public List<UnitPlacementData> Units;
        public List<MapEventData> MapEvents;
        public string Tileset;
        public int LevelNumber;
        public string Objective;
        public string Name;
    }

    [System.Serializable]
    public class UnitPlacementData
    {
        public Vector2Int Pos;
        public Team Team;
        public int Level;
        public string Class;
        public AIType AIType;
        public int ReinforcementTurn;
        public bool Statue;

        public UnitPlacementData Clone()
        {
            UnitPlacementData data = new UnitPlacementData();
            data.Pos = new Vector2Int(Pos.x, Pos.y);
            data.Team = Team;
            data.Level = Level;
            data.Class = Class;
            data.AIType = AIType;
            data.ReinforcementTurn = ReinforcementTurn;
            data.Statue = Statue;
            return data;
        }
    }

    [System.Serializable]
    public class MapEventData
    {
        public string Requirements;
        public string Event;
    }
}

[System.Serializable]
public class Tileset
{
    public string Name;
    public Palette Palette1 = new Palette();
    public Palette Palette2 = new Palette();
    public List<Tile> TileObjects;
    [HideInInspector]
    [SerializeField]
    private List<TileData> Tiles;

    public Tileset()
    {
        for (int i = 0; i < 4; i++)
        {
            Palette1.colors[i] = Color.black;
            Palette2.colors[i] = Color.black;
        }
    }

    public void GenerateObjects(Tile baseTile, GameObject container)
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            Tile tile = GameObject.Instantiate(baseTile.gameObject, container.transform).GetComponent<Tile>();
            tile.Name = Tiles[i].Name;
            tile.ArmorModifier = Tiles[i].ArmorMod;
            tile.MovementCost = Tiles[i].MoveCost;
            tile.High = Tiles[i].High;
            tile.GetComponent<PalettedSprite>().ForceSilentSetPalette(Tiles[i].Palette - 1);
            tile.name = i + " - " + tile.Name;
            TileObjects.Add(tile);
        }
    }

    [System.Serializable]
    public class TileData
    {
        public string Name;
        public int MoveCost;
        public int ArmorMod;
        public int Palette;
        public bool High;
    }
}

[System.Serializable]
public class Map
{
    public string Name;
    public int LevelNumber;
    public int[,] Tilemap;
    public List<MapController.UnitPlacementData> Units;
    public List<string> MapEvents;
    public string Tileset;
    public Objective Objective;
    public string ObjectiveData;
    public string MapString;
    public void Init()
    {
        string[] lines = MapString.Split(';');
        Vector2Int mapSize = GameController.MAP_SIZE; // TBA: Maps can have different size. For level 12, so future me will deal with it.
        Tilemap = new int[mapSize.x, mapSize.y];
        for (int k = 0; k < mapSize.x; k++)
        {
            string[] line = lines[k].Split('|');
            for (int j = 0; j < mapSize.y; j++)
            {
                Tilemap[k, j] = int.Parse(line[j]);
            }
        }
    }
    public bool MatchesDemands(ConversationData conversation)
    {
        if (LevelNumber != GameController.Current.LevelNumber)
        {
            return false;
        }
        foreach (var demand in conversation.Demands)
        {
            if (demand[0] == '!')
            {
                if (MeetsDemand(demand.Substring(1)))
                {
                    return false;
                }
            }
            else
            {
                if (!MeetsDemand(demand))
                {
                    return false;
                }
            }
        }
        return true;
    }
    private bool MeetsDemand(string demand)
    {
        string[] parts = demand.Split(':');
        switch (parts[0])
        {
            case "hasCharacter":
                return Units.Find(a => a.Class == parts[1]) != null;
            case "charactersAlive":
                // Find number of returning playable characters in map (excluding Frogman and recruitments)
                int count = 0;
                foreach (MapController.UnitPlacementData unit in Units)
                {
                    if (unit.Class == "P")
                    {
                        // Is player
                        count++;
                    }
                }
                int targetNumber = int.Parse(parts[1].Substring(1));
                // Format: charactersAlive:?X, ex. charactersAlive:>2
                switch (parts[1][0])
                {
                    case '>':
                        return count > targetNumber;
                    case '<':
                        return count < targetNumber;
                    case '=':
                        return count == targetNumber;
                    default:
                        break;
                }
                break;
            case "mapID":
                return Name == parts[1];
            default:
                break;
        }
        return true;
    }
}