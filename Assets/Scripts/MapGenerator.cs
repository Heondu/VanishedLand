using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Setting")]
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Vector2Int mapOffeset;

    [Header("Grids")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap objectTilemap;

    [Header("Tiles")]
    [SerializeField] private Tile groundTile;
    [SerializeField] private Tile forestTile;
    [SerializeField] private Tile mountainTile;

    [Header("Spawn Prob Setting")]
    [Range(0f, 1f)]
    [SerializeField] private float forestProb;
    [Range(0f, 1f)]
    [SerializeField] private float mountainProb;

    private void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int position = new Vector3Int(x - width / 2 + mapOffeset.x, y - height / 2 + mapOffeset.y, 0);

                groundTilemap.SetTile(position, groundTile);

                float rand = Random.value;
                if (rand <= forestProb)
                {
                    objectTilemap.SetTile(position, forestTile);
                }
                if (rand <= mountainProb)
                {
                    objectTilemap.SetTile(position, mountainTile);
                }
            }
        }
    }
}
