
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private GridData gridData;
    private Vector3 playerStartingPosition;
    private Vector3 enemyStartingPosition;
    private HashSet<Node> nodes = new HashSet<Node>();
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;
    private float tileXWidth;
    private float tileZWidth;

    public bool UnitMoving;
    public GameObject playerUnit;
    public GameObject enemyUnit;

    private BaseMovement playerMovement;
    private BaseMovement enemyMovement;

    private IAI enemyAI;

    private void Start()
    {
        Instance = this;
    }

    public void Spawn()
    {
        if (playerUnit != null) Destroy(playerUnit);
        if (enemyUnit != null) Destroy(enemyUnit);

        gridData = GridManager.Instance.gridData;
        nodes = GridManager.Instance.pathFindingBase.gridMap.Nodes;
        tileXWidth = GridManager.Instance.tileXWidth;
        tileZWidth = GridManager.Instance.tileZWidth;
        
        playerStartingPosition = CheckForSpawnPosition(out Tile playerStartingTile);
        enemyStartingPosition = CheckForSpawnPosition(out Tile enemyStartingTile,true);

        playerUnit = Instantiate(playerPrefab, playerStartingPosition, Quaternion.identity);
        enemyUnit = Instantiate(enemyPrefab, enemyStartingPosition, Quaternion.identity);

        playerUnit.GetComponent<BaseMovement>().InitializeTile(playerStartingTile);
        enemyUnit.GetComponent<BaseMovement>().InitializeTile(enemyStartingTile);

        enemyAI = enemyUnit.GetComponent<IAI>();
        StartCoroutine(TriggerInitialEnemyMove(enemyUnit, playerStartingTile)); // in the beginning enemy moves to player 
    }

    private IEnumerator TriggerInitialEnemyMove(GameObject enemy, Tile playerTile)
    {
        yield return null; 
        enemyAI?.OnPlayerMoved(playerTile);
    }
    public void NotifyEnemyOfPlayerMove(Tile playerTile)
    {
        enemyAI?.OnPlayerMoved(playerTile);
    }

    
    private Vector3 CheckForSpawnPosition(out Tile spawnTile, bool forEnemy = false)
    {
        Node node = forEnemy ? nodes.Last() : nodes.First(); // spawns player at first empty node and enemy at last

        int index = node.NodeId;
        int tileX = index / GridManager.Instance.noOfColumns;
        int tileZ = index % GridManager.Instance.noOfColumns;

        spawnTile = GridManager.Instance.GetTileAt(tileX, tileZ);//gets the starting tile of player and enemy

        return new Vector3(tileX * tileXWidth + 0.1f, 0.045f, tileZ * tileZWidth + 0.1f);

    }

    // this is soley for avoiding player moving through the enemy
    public HashSet<int> GetOccupiedNodeIds(Tile excludeSelf)
    {
        var occupied = new HashSet<int>();
        int cols = GridManager.Instance.noOfColumns;

        playerMovement = playerUnit.GetComponent<Player>();
        enemyMovement = enemyUnit.GetComponent<Enemy>();
        if (playerMovement?.currentTile != null && playerMovement.currentTile != excludeSelf)
        {
            occupied.Add(playerMovement.currentTile.tileX * cols + playerMovement.currentTile.tileZ);
        }
        if (enemyMovement?.currentTile != null && enemyMovement.currentTile != excludeSelf)
        {
            occupied.Add(enemyMovement.currentTile.tileX * cols + enemyMovement.currentTile.tileZ);
        }

        return occupied; // returns the nodeid or tile id where enemy stands on so that player avoids him during a*
    }
}
