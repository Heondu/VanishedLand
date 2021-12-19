using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class TileData
{
    public Data data;
    public Vector3Int pos;

    public TileData(Vector3Int pos, Data data)
    {
        this.pos = pos;
        this.data = data;
    }
}

public class MapData
{
    public List<TileData> ground;
    public List<TileData> landform;
    public Vector2Int pos;
}

public static class ExtensionMethod
{
    public static TileData Get(this List<TileData> tileDatas, Vector2Int pos)
    {
        return tileDatas.FirstOrDefault(t => t.pos.x == pos.x && t.pos.y == pos.y);
    }
}

public class EntityData
{
    public List<EntityController> entity = new List<EntityController>();

    public EntityController Get(Vector2Int pos)
    {
        return entity.FirstOrDefault(n => n.currentPos.x == pos.x && n.currentPos.y == pos.y);
    }
}

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MapGenerator>();
            }
            return instance;
        }
    }
    private static MapGenerator instance;

    [Header("Map Setting")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Vector2Int mapOffeset;
    [SerializeField] private bool useHeightMap;
    [Space(10)]
    [SerializeField] private int layerNum = 10;
    [Space(10)]
    [SerializeField] private bool showAllMap;
    [SerializeField] private Vector2Int mapInterval;
    [Space(10)]
    [SerializeField] private int seed;
    [SerializeField] private bool useRandomSeed;
    private float groundOrgX = 0;
    private float groundOrgY = 0;
    private float landformOrgX = 0;
    private float landformOrgY = 0;

    [Header("Tilemap Setting")]
    public Tilemap groundTilemap;
    public Tilemap landformTilemap;

    [Header("Components")]
    [SerializeField] private PerlinNoise perlinNoise;

    [Header("Tiles")]
    [SerializeField] private Data[] tiles;

    private List<MapData> mapDatas;
    public EntityData entityData;
    private Vector2Int currentPos;

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Setup();
        ClearMap();
        ShowMap(GenerateMap(currentPos));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Setup();
            ClearMap();
            ShowMap(GenerateMap(currentPos));
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveMap(Vector2Int.up);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveMap(Vector2Int.down);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveMap(Vector2Int.left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveMap(Vector2Int.right);
        }
    }

    public void MoveButton(string direction)
    {
        switch (direction)
        {
            case "Up": MoveMap(Vector2Int.up); break;
            case "Down": MoveMap(Vector2Int.down); break;
            case "Left": MoveMap(Vector2Int.left); break;
            case "Right": MoveMap(Vector2Int.right); break;
        }
    }

    private void MoveMap(Vector2Int direction)
    {
        if (!showAllMap) ClearMap();
        MapData mapData = mapDatas.FirstOrDefault(x => x.pos == currentPos + direction);
        if (mapData == null)
        {
            mapData = GenerateMap(currentPos + direction);
        }
        ShowMap(mapData);
        currentPos += direction;
    }

    private void Setup()
    {
        if (!useRandomSeed) Random.InitState(seed);
        groundOrgX = Random.Range(0, 99999);
        groundOrgY = Random.Range(0, 99999);
        landformOrgX = Random.Range(0, 99999);
        landformOrgY = Random.Range(0, 99999);

        mapDatas = new List<MapData>();
        currentPos = Vector2Int.zero;
    }

    private void ClearMap()
    {
        groundTilemap.ClearAllTiles();
        landformTilemap.ClearAllTiles();
    }

    private MapData GenerateMap(Vector2Int pos)
    {
        MapData mapData = new MapData();
        mapData.ground = new List<TileData>();
        mapData.landform = new List<TileData>();
        mapData.pos = new Vector2Int(pos.x, pos.y);

        float[,] groundNoiseMap = perlinNoise.GetNoise(groundOrgX, groundOrgY, width, height, pos.x * width, pos.y * height);
        float[,] landformNoiseMap = perlinNoise.GetNoise(landformOrgX, landformOrgY, width, height, pos.x * width, pos.y * height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int p = new Vector2Int(x - width / 2 + mapOffeset.x, y - height / 2 + mapOffeset.y);

                if (groundNoiseMap[x, y] < tiles[(int)TileName.water].prob)
                {
                    mapData.ground.Add(new TileData(new Vector3Int(p.x, p.y, Mathf.FloorToInt(tiles[(int)TileName.water].prob) * layerNum + 1), tiles[(int)TileName.water]));
                }
                else
                {
                    mapData.ground.Add(new TileData(new Vector3Int(p.x, p.y, Mathf.FloorToInt(groundNoiseMap[x, y] * layerNum)), tiles[(int)TileName.grass]));
                }

                if (groundNoiseMap[x, y] >= tiles[(int)TileName.mountain].prob)
                {
                    mapData.landform.Add(new TileData(new Vector3Int(p.x, p.y, Mathf.FloorToInt(groundNoiseMap[x, y] * layerNum)), tiles[(int)TileName.mountain]));
                }

                if (landformNoiseMap[x, y] <= tiles[(int)TileName.forest].prob && mapData.ground.Get(p).data.name == TileName.grass)
                {
                    mapData.landform.Add(new TileData(new Vector3Int(p.x, p.y, Mathf.FloorToInt(groundNoiseMap[x, y] * layerNum)), tiles[(int)TileName.forest]));
                }
                else if (landformNoiseMap[x, y] <= tiles[(int)TileName.tree].prob && mapData.ground.Get(p).data.name == TileName.grass)
                {
                    mapData.landform.Add(new TileData(new Vector3Int(p.x, p.y, Mathf.FloorToInt(groundNoiseMap[x, y] * layerNum)), tiles[(int)TileName.tree]));
                }
            }
        }

        mapDatas.Add(mapData);

        return mapData;
    }

    private void ShowMap(MapData mapData)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int newPosition = new Vector3Int(x - width / 2 + mapOffeset.x, y - height / 2 + mapOffeset.y, 0);
                Vector2Int p = new Vector2Int(newPosition.x, newPosition.y);

                if (showAllMap) newPosition += new Vector3Int(mapData.pos.x * (width + mapInterval.x), mapData.pos.y * (height + mapInterval.y), 0);

                if (mapData.ground.Get(p) != null)
                {
                    newPosition.z = GetTileHeight(mapData.ground.Get(p));
                    groundTilemap.SetTileFlags(newPosition, TileFlags.None);
                    groundTilemap.SetTile(newPosition, mapData.ground.Get(p).data.tile);
                }
                if (mapData.landform.Get(p) != null)
                {
                    newPosition.z = GetTileHeight(mapData.landform.Get(p));
                    groundTilemap.SetTileFlags(newPosition, TileFlags.None);
                    landformTilemap.SetTile(newPosition, mapData.landform.Get(p).data.tile);
                }
            }
        }
    }

    public MapData GetMapData()
    {
        return mapDatas.FirstOrDefault(x => x.pos == currentPos);
    }

    public int GetTileHeight(TileData tileData)
    {
        if (useHeightMap) return tileData.pos.z;
        else return 0;
    }

    public bool UseHeightMap()
    {
        return useHeightMap;
    }

    public bool IsMovableTile(Vector3Int position)
    {
        if (!useHeightMap) position.z = 0;

        TileData tileData = GetMapData().ground.Get(new Vector2Int(position.x, position.y));
        if (tileData == null) return false;
        if (tileData.data.moveType == TileMoveType.block) return false;

        tileData = GetMapData().landform.Get(new Vector2Int(position.x, position.y));
        if (tileData != null && tileData.data.moveType == TileMoveType.block) return false;

        if (entityData.Get(new Vector2Int(position.x, position.y)) != null) return false;

        return true;
    }
}
