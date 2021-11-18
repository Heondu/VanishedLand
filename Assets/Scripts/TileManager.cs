using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileName
{
    grass,
    water,
    mountain,
    tree,
    forest,
}

public enum TilemapType
{
    ground,
    landform
}

public enum TileMoveType
{
    move,
    block
}

[System.Serializable]
public class Data
{
    public Tile tile;
    public TileName name;
    public TileMoveType moveType;
    [Range(0f, 1f)]
    public float prob;
}

public class TileManager : MonoBehaviour
{

}
