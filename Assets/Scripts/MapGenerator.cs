using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class TileData
{
    public Data data;
    public float height;

    public TileData(Data data, float height)
    {
        this.data = data;
        this.height = height;
    }
}

public class MapData
{
    public TileData[,] ground;
    public TileData[,] landform;
    public Vector2Int pos;
}

public class MapGenerator : MonoBehaviour
{
    [Header("Map Setting")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Vector2Int mapOffeset;
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
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap landformTilemap;

    [Header("Components")]
    [SerializeField] private PerlinNoise perlinNoise;

    [Header("Tiles")]
    [SerializeField] private Data[] tiles;

    private List<MapData> mapDatas;
    private Vector2Int currentPos;

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
        mapData.ground = new TileData[width, height];
        mapData.landform = new TileData[width, height];
        mapData.pos = new Vector2Int(pos.x, pos.y);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mapData.ground[x, y] = null;
                mapData.landform[x, y] = null;
            }
        }

        float[,] groundNoiseMap = perlinNoise.GetNoise(groundOrgX, groundOrgY, width, height, pos.x * width, pos.y * height);
        float[,] landformNoiseMap = perlinNoise.GetNoise(landformOrgX, landformOrgY, width, height, pos.x * width, pos.y * height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (groundNoiseMap[x, y] <= tiles[(int)TileName.water].prob)
                {
                    mapData.ground[x, y] = new TileData(tiles[(int)TileName.water], tiles[(int)TileName.water].prob);
                }
                else
                {
                    mapData.ground[x, y] = new TileData(tiles[(int)TileName.grass], groundNoiseMap[x, y]);
                }

                if (groundNoiseMap[x, y] >= tiles[(int)TileName.mountain].prob)
                {
                    mapData.landform[x, y] = new TileData(tiles[(int)TileName.mountain], groundNoiseMap[x, y]);
                }

                if (landformNoiseMap[x, y] <= tiles[(int)TileName.forest].prob && mapData.ground[x, y].data.name == TileName.grass)
                {
                    mapData.landform[x, y] = new TileData(tiles[(int)TileName.forest], groundNoiseMap[x, y]);
                }
                else if (landformNoiseMap[x, y] <= tiles[(int)TileName.tree].prob && mapData.ground[x, y].data.name == TileName.grass)
                {
                    mapData.landform[x, y] = new TileData(tiles[(int)TileName.tree], groundNoiseMap[x, y]);
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

                if (showAllMap)
                {
                    newPosition += new Vector3Int(mapData.pos.x * (width + mapInterval.x), mapData.pos.y * (height + mapInterval.y), 0);
                }

                if (mapData.ground[x, y] != null)
                {
                    newPosition.z = Mathf.FloorToInt(mapData.ground[x, y].height * layerNum);
                    groundTilemap.SetTileFlags(newPosition, TileFlags.None);
                    groundTilemap.SetTile(newPosition, mapData.ground[x, y].data.tile);
                }
                if (mapData.landform[x, y] != null)
                {
                    newPosition.z = Mathf.FloorToInt(mapData.landform[x, y].height * layerNum);
                    groundTilemap.SetTileFlags(newPosition, TileFlags.None);
                    landformTilemap.SetTile(newPosition, mapData.landform[x, y].data.tile);
                }
            }
        }
    }
}
