using UnityEngine;

public class PlayerController : EntityController
{
    [SerializeField] private GameObject selectMarker;
    [SerializeField] private Marker marker;

    private bool isSelected = false;

    private void Awake()
    {
        marker = FindObjectOfType<Marker>();
    }

    private void Update()
    {
        if (GameManager.Instance.GetState() == GameState.PlayerTurn)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (isSelected)
                {
                    Move();
                }
            }
        }
    }

    public void Select()
    {
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f, 1 << LayerMask.NameToLayer("Object"));
        if (hit.collider != null)
        {
            PlayerController playerController = hit.collider.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.Activate();
            }
        }
    }

    protected override void Move()
    {
        if (isMove) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = MapGenerator.Instance.UseHeightMap() ? 2 : 0;
        Vector3Int cellPos = grid.WorldToCell(mousePos);

        if (IsMovableTile(cellPos))
        {
            isMove = true;

            Vector3Int start = currentPos;
            Vector3Int end = new Vector3Int(cellPos.x, cellPos.y, currentPos.z);
            aStar.SetTargetPosition(start, end, grid, this);
            Deactivate();
        }
    }

    private bool IsMovableTile(Vector3Int cellPos)
    {
        cellPos.z = MapGenerator.Instance.UseHeightMap() ? cellPos.z : 0;

        if (!marker.IsMoveMarker(cellPos)) return false;
        if (!MapGenerator.Instance.IsMovableTile(cellPos)) return false;

        return true;
    }

    public void Activate()
    {
        if (isSelected)
        {
            Deactivate();
            return;
        }

        isSelected = true;
        selectMarker.SetActive(true);

        marker.ShowMoveMarker(currentPos, moveRange);
    }

    public void Deactivate()
    {
        isSelected = false;
        selectMarker.SetActive(false);

        marker.Clear();
    }
}