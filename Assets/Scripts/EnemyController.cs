using UnityEngine;

public class EnemyController : EntityController
{
    private PlayerController target;

    private void Start()
    {
        target = FindObjectOfType<PlayerController>();
    }
    
    public void Activate()
    {
        Move();
    }

    protected override void Move()
    {
        if (isMove) return;

        isMove = true;
        aStar.SetTargetPosition(currentPos, target.currentPos, grid, this);
    }
}
