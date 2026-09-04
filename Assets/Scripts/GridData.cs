using UnityEngine;

[CreateAssetMenu(fileName = "Grid",menuName = "Grid/GridData")]
public class GridData : ScriptableObject
{
    [HideInInspector] public int NoOfRows = 10;
    [HideInInspector] public int NoOfColumns = 10;
    [HideInInspector] public bool[] ObstaclePresent = new bool[100];

    //used when runtime ui generates grid
    public void RuntimeResize(int rows, int cols)
    {
        NoOfRows = rows;
        NoOfColumns = cols;
        ResizeGrid(); 
    }

    // editor side
    public void ResizeGrid()
    {
        if (ObstaclePresent.Length != NoOfColumns * NoOfRows)
        {
            ObstaclePresent = new bool[NoOfRows * NoOfColumns];
        }
    }
}
