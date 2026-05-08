using UnityEngine;

[System.Serializable]
public struct WinLine
{
    public Vector3Int[] cells;  // exactly 4 cells per line on a 4x4x4 board

    public WinLine(Vector3Int[] cells)
    {
        this.cells = cells;
    }
}