using System.Collections.Generic;
using UnityEngine;

public static class WinLineGenerator
{
    public static List<WinLine> Generate(int size = 4)
    {
        List<WinLine> lines = new List<WinLine>();

        // Along X axis: for each (y, z)
        for (int y = 0; y < size; y++)
            for (int z = 0; z < size; z++)
                lines.Add(MakeLine(size, i => new Vector3Int(i, y, z)));

        // Along Y axis: for each (x, z)
        for (int x = 0; x < size; x++)
            for (int z = 0; z < size; z++)
                lines.Add(MakeLine(size, i => new Vector3Int(x, i, z)));

        // Along Z axis: for each (x, y)
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                lines.Add(MakeLine(size, i => new Vector3Int(x, y, i)));

        // Diagonals on xy-plane (z fixed)
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