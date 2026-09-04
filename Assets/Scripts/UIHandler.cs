using TMPro;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tileCoordinates;

    private void Start()
    {
        GameEvents.Instance.onTileClicked += UpdateTileInformation;
        GameEvents.Instance.onTileHovered += UpdateTileInformation;
    }

    private void OnDisable()
    {
        GameEvents.Instance.onTileClicked -= UpdateTileInformation;
        GameEvents.Instance.onTileHovered -= UpdateTileInformation;
    }

    // tile's coordinate display
    private void UpdateTileInformation(Tile tile)
    {
        tileCoordinates.text = "X:" + tile.tileX +"\t"+ "Z:"+tile.tileZ;
    }
}
