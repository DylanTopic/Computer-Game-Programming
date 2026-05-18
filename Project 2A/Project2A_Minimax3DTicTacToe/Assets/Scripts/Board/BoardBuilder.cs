using UnityEngine;

public class BoardBuilder : MonoBehaviour
{
    [Header("Board Configuration")]
    public GameObject cellPrefab;
    public int boardSize = 4;          // 4x4x4
    public float cellSpacing = 1.0f;   // distance between cell centers
    public float layerSpacing = 1.5f;  // vertical distance between the 4 layers

    // 3D array storing references to every cell GameObject.
    // Indexing: cells[x, y, z] where y is the vertical layer (0 = bottom, 3 = top).
    public GameObject[,,] cells;

    void Awake()
    {
        BuildBoard();
    }

    void BuildBoard()
    {
        cells = new GameObject[boardSize, boardSize, boardSize];

        float offset = (boardSize - 1) * cellSpacing * 0.5f;

        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int z = 0; z < boardSize; z++)
                {
                    Vector3 position = new Vector3(
                        x * cellSpacing - offset,
                        y * layerSpacing,
                        z * cellSpacing - offset
                    );

                    GameObject cellObj = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                    cellObj.name = $"Cell_{x}_{y}_{z}";

                    Cell cellComponent = cellObj.GetComponent<Cell>();
                    if (cellComponent != null)
                    {
                        cellComponent.SetCoordinates(x, y, z);
                    }

                    cells[x, y, z] = cellObj;
                }
            }
        }
    }
}