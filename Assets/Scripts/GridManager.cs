
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class GridManager : MonoBehaviour
{

    public static GridManager Instance;

    public float tileXWidth = 0.5f;
    public float tileZWidth = 0.5f;

    [HideInInspector] public int noOfRows = 10;
    [HideInInspector] public int noOfColumns = 10;

    private const int MinGridSize = 2;
    private const int MaxGridSize = 20;

    // runtime ui side grid dimension manipulating variables
    [SerializeField] private TMP_InputField rowsField;
    [SerializeField] private TMP_InputField columnsField;
    [SerializeField] private Button generateButton;

    [SerializeField] private GameObject tile;
    [SerializeField] private GameObject obstacle;
    [HideInInspector]public List<GameObject> tiles = new List<GameObject>();
    private List<GameObject> obstacles = new List<GameObject>();

    public LayerMask GroundTile; // ray can only hit these
    public GridData gridData;
    public Pathfindingbase pathFindingBase;

    [SerializeField] private GameManager gameManager;   

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        GenerateGrid();

        // responsible for the generate grid button interactible status at runtime
        rowsField.onValueChanged.AddListener(_ => ValidateInputs()); 
        columnsField.onValueChanged.AddListener(_ => ValidateInputs());
        ValidateInputs();
    }

    private void ValidateInputs()
    {
        bool rowsValid = int.TryParse(rowsField.text, out int rows) && rows >= MinGridSize && rows <= MaxGridSize;
        bool colsValid = int.TryParse(columnsField.text, out int cols) && cols >= MinGridSize && cols <= MaxGridSize;
        generateButton.interactable = rowsValid && colsValid;
    }

    public void OnGenerateClicked()
    {
        noOfRows = Mathf.Clamp(Convert.ToInt32(rowsField.text), MinGridSize, MaxGridSize); 
        noOfColumns = Mathf.Clamp(Convert.ToInt32(columnsField.text), MinGridSize, MaxGridSize);
        gridData.RuntimeResize(noOfRows, noOfColumns); // updating size here
        pathFindingBase.RebuildGraph(); //updating graph for a*
        GenerateGrid(); 
    }
    public void GenerateGrid()
    {
        noOfRows = gridData.NoOfRows;
        noOfColumns = gridData.NoOfColumns;
        ClearGrid();
        tiles.Clear();
        //obstacles.Clear();
        for (int i = 0; i < noOfRows; i++)
        {
            for (int j = 0; j < noOfColumns; j++)
            {
                int index = i * noOfColumns + j;

                //spawning tile
                GameObject tileGO = GameObject.Instantiate(tile, new Vector3(i * tileXWidth + 0.1f, 0, j * tileZWidth + 0.1f), Quaternion.identity);
                tileGO.transform.localScale = new Vector3(tileXWidth, 0.1f, tileZWidth);
                Tile currentTile = tileGO.GetComponent<Tile>();
                currentTile.tileX = i; currentTile.tileZ = j; // assigns tile data for each tile
                tiles.Add(tileGO);

                // spawning obstacle
                if (gridData.ObstaclePresent[index]) 
                {
                    GameObject obstacleGO = GameObject.Instantiate(obstacle, new Vector3(i * tileXWidth + 0.1f, 0.1f, j * tileZWidth + 0.1f), Quaternion.identity);
                    obstacleGO.transform.localScale = new Vector3(tileXWidth, 0.1f, tileZWidth);
                    obstacles.Add(obstacleGO);
                }
            }
        }
        gameManager.Spawn();
    }

    private void ClearGrid()
    {
        if (tiles.Count > 0)
        {
            foreach(GameObject tile in  tiles)
            {
                Destroy(tile);
            }
        }
        if (obstacles.Count > 0)
        {
            foreach (GameObject obstacle in obstacles)
            {
                Destroy(obstacle);
            }
        }
    }

    private void Update()
    {
        CheckMouseHover();
        CheckMouseClick();
    }

    private void CheckMouseClick()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitPoint;

            if (Physics.Raycast(ray, out hitPoint, Mathf.Infinity, GroundTile))
            {
                Tile currentTile = hitPoint.transform.gameObject.GetComponent<Tile>();
                GameEvents.Instance.TileClicked(currentTile); // tile click signal is sent then based on a* result player is moved
                Debug.Log("current tile = " + currentTile.tileX + "  " + currentTile.tileZ);
            }
            else
            {
                Debug.Log("nohit");
            }
        }
    }

    private void CheckMouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitPoint;

        if (Physics.Raycast(ray, out hitPoint, Mathf.Infinity, GroundTile))
        {
            Tile currentTile = hitPoint.transform.gameObject.GetComponent<Tile>();
            GameEvents.Instance.TileHovered(currentTile); // sends signal to ui to display the coordinate of a tile
        }
        else
        {
            //Debug.Log("nohit");
        }
    }

    public Tile GetTileAt(int tileX,int tileZ)
    {
        int index = tileX * noOfColumns + tileZ;
        return tiles[index].GetComponent<Tile>();
    }
}

