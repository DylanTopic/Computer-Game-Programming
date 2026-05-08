using System.Collections.Generic;
using UnityEngine;

public static class WinLineGenerator
{
    public static List<WinLine> Generate(int size = 4)
    {
        List<WinLine> lines = new List<WinLine>();

        // === Axis-aligned lines (48 total) ===

        // Along X axis: for each (y, z), the 4 cells (0..size-1, y, z)
        for (int y = 0; y < size; y++)
            for (int z = 0; z < size; z++)
                lines.Add(MakeLine(size, i => new Vector3Int(i, y, z)));

        // Along Y axis: for each (x, z), the 4 cells (x, 0..size-1, z)
        for (int x = 0; x < size; x++)
            for (int z = 0; z < size; z++)
                lines.Add(MakeLine(size, i => new Vector3Int(x, i, z)));

        // Along Z axis: for each (x, y), the 4 cells (x, y, 0..size-1)
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                lines.Add(MakeLine(size, i => new Vector3Int(x, y, i)));

        // === 2D-face diagonals (24 total) ===

        // Diagonals on xy-plane (z fixed): each of 4 z-slices has 2 diagonals = 8
        for (int z = 0; z < size; z++)
        {
            lines.Add(MakeLine(size, i => new Vector3Int(i, i, z)));
            lines.Add(MakeLine(size, i => new Vector3Int(i, size - 1 - i, z)));
        }

        // Diagonals on xz-plane (y fixed)
        for (int y = 0; y < size; y++)
        {
            lines.Add(MakeLine(size, i => new Vector3Int(i, y, i)));
            lines.Add(MakeLine(size, i => new Vector3Int(i, y, size - 1 - i)));
        }

        // Diagonals on yz-plane (x fixed)
        for (int x = 0; x < size; x++)
        {
            lines.Add(MakeLine(size, i => new Vector3Int(x, i, i)));
            lines.Add(MakeLine(size, i => new Vector3Int(x, i, size - 1 - i)));
        }

        // === Space (3D) diagonals through the cube (4 total) ===
        lines.Add(MakeLine(size, i => new Vector3Int(i, i, i)));
        lines.Add(MakeLine(size, i => new Vector3Int(i, i, size - 1 - i)));
        lines.Add(MakeLine(size, i => new Vector3Int(i, size - 1 - i, i)));
        lines.Add(MakeLine(size, i => new Vector3Int(size - 1 - i, i, i)));

        return lines;
    }

    private static WinLine MakeLine(int size, System.Func<int, Vector3Int> indexer)
    {
        Vector3Int[] cells = new Vector3Int[size];
        for (int i = 0; i < size; i++)
        {
            cells[i] = indexer(i);
        }
        return new WinLine(cells);
    }
}