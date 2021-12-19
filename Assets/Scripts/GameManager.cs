using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    PlayerTurn,
    EnemyTurn,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }
    private static GameManager instance;

    [SerializeField] private Grid grid;

    private PlayerController[] playerList;
    public EnemyController[] enemyList;
    private GameState gameState;
    private int playerTurnCount = 0;
    private int enemyTurnCount = 0;

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        gameState = GameState.PlayerTurn;

        playerList = FindObjectsOfType<PlayerController>();
        enemyList = FindObjectsOfType<EnemyController>();

        MapGenerator.Instance.entityData = new EntityData();
        for (int i = 0; i < playerList.Length; i++)
        {
            MapGenerator.Instance.entityData.entity.Add(playerList[i]);
            playerList[i].Init(grid);
        }
        for (int i = 0; i < enemyList.Length; i++)
        {
            MapGenerator.Instance.entityData.entity.Add(enemyList[i]);
            enemyList[i].Init(grid);
        }

        playerList[0].Activate();
    }

    private void Update()
    {

    }

    private void ChangeState(GameState newState)
    {
        gameState = newState;
    }

    public void TurnEnd()
    {
        if (gameState == GameState.PlayerTurn)
        {
            playerTurnCount++;
            if (playerTurnCount < playerList.Length)
            {
                playerList[playerTurnCount].Activate();
            }
            else
            {
                playerTurnCount = 0;
                ChangeState(GameState.EnemyTurn);
                enemyList[0].Activate();
            }
        }
        else if (gameState == GameState.EnemyTurn)
        {
            enemyTurnCount++;
            if (enemyTurnCount < enemyList.Length)
            {
                enemyList[enemyTurnCount].Activate();
            }
            else
            {
                enemyTurnCount = 0;
                ChangeState(GameState.PlayerTurn);
                playerList[0].Activate();
            }
        }
    }

    public GameState GetState()
    {
        return gameState;
    }
}
