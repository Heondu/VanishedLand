using UnityEngine;
using UnityEngine.Tilemaps;

public class Marker : MonoBehaviour
{
    [SerializeField] private Grid grid;

    [SerializeField] private Tilemap markerTilemap;
    [SerializeField] private Tile moveMarkMovable;
    [SerializeField] private Tile moveMarkBlock;

    [SerializeField] private Tilemap cursorTilemap;
    [SerializeField] private Tile selectTile;

    private void Update()
    {
        cursorTilemap.ClearAllTiles();
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = MapGenerator.Instance.UseHeightMap() ? 2 : 0;
        Vector3Int cellPos = grid.WorldToCell(mousePos);
        cursorTilemap.SetTile(cellPos, selectTile);
    }
    public void ShowMoveMarker(Vector3Int position, int[,] map)
    {
        float width = map.GetLength(0);
        float height = map.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1)
                {
                    Vector3Int newPos = new Vector3Int(position.x - Mathf.FloorToInt(width / 2) + x, position.y - Mathf.FloorToInt(height / 2) + y, 0);

                    if (MapGenerator.Instance.IsMovableTile(newPos))
                    {
                        markerTilemap.SetTile(newPos, moveMarkMovable);
                    }
                    else
                    {
                        markerTilemap.SetTile(newPos, moveMarkBlock);
                    }
                }
            }
        }
    }
    public void Clear()
    {
        markerTilemap.ClearAllTiles();
    }
    public bool IsMoveMarker(Vector3Int position)
    {
        return markerTilemap.GetTile(position) != null;
    }
}
