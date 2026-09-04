
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomGridDataTool : EditorWindow
{
    private static GridData gridData;

    private VisualElement root;
    private VisualElement gridContainer;

    private const int MinGridSize = 2;
    private const int MaxGridSize = 20;
    private Label errorLabel;

    private bool[] buttonSelected;

    [MenuItem("Tools/CustomGridTool")]
    public static void ShowWindow()
    {
        gridData = AssetDatabase.LoadAssetAtPath<GridData>("Assets/Prefabs/GridData.asset");
        CustomGridDataTool window = GetWindow<CustomGridDataTool>();
        window.titleContent = new GUIContent("LevelEditor");
        window.minSize = new Vector2(1000, 1000);
    }

    public void CreateGUI()
    {
        root = rootVisualElement;

        var rows = new IntegerField("Rows [2-20]");
        rows.value = gridData != null ? gridData.NoOfRows : 10; //default 10
        root.Add(rows);
        
        var cols = new IntegerField("Columns[2-20]");
        cols.value = gridData != null ? gridData.NoOfColumns : 10;//default 10
        root.Add(cols);

        errorLabel = new Label(); // to display error messages for dev
        errorLabel.style.color = Color.red;
        root.Add(errorLabel);

        var generateButton = new Button(() =>
        {
            if (gridData == null) return;

            int r = Mathf.Clamp(rows.value, MinGridSize, MaxGridSize);
            int c = Mathf.Clamp(cols.value, MinGridSize, MaxGridSize);

            if (r != rows.value || c != cols.value) // entered value was clamped so display error message and show clamped value
            {
                errorLabel.text = "Please enter values between [2-20]";
                rows.value = r;
                cols.value = c;
            }
            else
            {
                errorLabel.text = "";
            }

            gridData.NoOfRows = r;
            gridData.NoOfColumns = c;
            gridData.ResizeGrid();
            EditorUtility.SetDirty(gridData); // scriptable item marked to save
            RebuildGrid(); // subscribing the button to grid building/generating

        });
        generateButton.text = "Generate Grid";
        root.Add(generateButton);                           
    
        gridContainer = new VisualElement();
        root.Add(gridContainer);
        RebuildGrid(); // generating grid from saved data of scriptable item when window is opened
    }

    private void RebuildGrid()
    {
        gridContainer.Clear();
        if (gridData == null) return;

        int ROWS = gridData.NoOfRows;
        int COLS = gridData.NoOfColumns;
        buttonSelected = (bool[])gridData.ObstaclePresent.Clone(); // to keep track of buttons that are selected

        gridContainer.style.flexDirection = FlexDirection.Column;
        gridContainer.style.flexGrow = 1;

        for (int i = 0; i < ROWS; i++)
        {
            VisualElement rowElement = new VisualElement();
            rowElement.style.flexDirection = FlexDirection.Row; 
            rowElement.style.flexGrow = 1;

            for (int j = 0; j < COLS; j++)
            {
                Button button = new Button(); // create button as much as rows * cols
                button.style.flexGrow = 1;
                int index = i * COLS + j;

                SetButtonColor(button, index);

                button.clicked += () =>
                {
                    bool wouldBeSelected = !buttonSelected[index];

                    if (wouldBeSelected) // if the button is not selected yet then..
                    {
                        // need minimum 2 free tile to spawn player and enemy otherwise this button can't be selected
                        int freeTilesAfterToggle = CountFreeTiles() - 1;
                        if (freeTilesAfterToggle < 2) 
                        {
                            errorLabel.text = "At least 2 free tiles are required for player and enemy spawn";
                            return;
                        }
                    }

                    buttonSelected[index] = !buttonSelected[index];
                    gridData.ObstaclePresent[index] = buttonSelected[index];
                    EditorUtility.SetDirty(gridData);
                    SetButtonColor(button, index);
                    errorLabel.text = "";   
                };
                rowElement.Add(button);

            }
            gridContainer.Add(rowElement);
        }
    }

    
    private int CountFreeTiles()
    {
        int count = 0;
        foreach (bool b in buttonSelected)
        {
            if (!b) count++;
        }
        return count;
    }


    private void SetButtonColor(Button button, int index)
    {
        if (buttonSelected[index])
        {
            button.style.backgroundColor = new StyleColor(Color.red); // already selected then show different color
        }
        else
        {
            button.style.backgroundColor = new StyleColor(Color.gray * 0.5f);
        }
    }
}
