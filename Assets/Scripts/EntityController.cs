using UnityEngine;
using AIAlgorithm.AStar;

public class EntityController : MonoBehaviour
{
    [SerializeField] protected AStar aStar;

    protected int offsetZ = 3;
    protected int[,] moveRange =
{
        { 0, 0, 1, 0, 0 },
        { 0, 1, 1, 1, 0 },
        { 1, 1, 0, 1, 1 },
        { 0, 1, 1, 1, 0 },
        { 0, 0, 1, 0, 0 },
    };
    protected Grid grid;
    public Vector3Int currentPos;
    protected bool isMove = false;

    public void Init(Grid grid)
    {
        this.grid = grid;
        TileData tileData = MapGenerator.Instance.GetMapData().ground.Get(new Vector2Int(0, 0));
        offsetZ = (int)transform.position.z;
        currentPos = new Vector3Int((int)transform.position.x, (int)transform.position.y, offsetZ + MapGenerator.Instance.GetTileHeight(tileData));
        Vector3 worldPos = grid.CellToWorld(currentPos);
        transform.position = worldPos;
    }
    public int GetOffsetZ()
    {
        return offsetZ;
    }
    public bool IsInMoveRange(int x, int y)
    {
        int width = moveRange.GetLength(0);
        int height = moveRange.GetLength(1);

        if (Mathf.Abs(x - currentPos.x) > width / 2 || Mathf.Abs(y - currentPos.y) > height / 2) return false;
        if (moveRange[x - currentPos.x + width / 2, y - currentPos.y + height / 2] == 0) return false;
        else return true;
    }
    protected virtual void Move()
    {

    }
    public virtual void MoveEnd()
    {
        isMove = false;
        GameManager.Instance.TurnEnd();
    }
}
