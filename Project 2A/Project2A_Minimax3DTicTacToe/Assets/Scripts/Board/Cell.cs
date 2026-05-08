using UnityEngine;

public class Cell : MonoBehaviour
{
    [Header("Coordinates")]
    public int x;
    public int y;  // vertical layer
    public int z;

    [Header("State")]
    public CellState state = CellState.Empty;

    public void SetCoordinates(int xCoord, int yCoord, int zCoord)
    {
        x = xCoord;
        y = yCoord;
        z = zCoord;
    }

    public bool IsEmpty()
    {
        return state == CellState.Empty;
    }
}

public enum CellState
{
    Empty,
    PlayerX,
    PlayerO
}