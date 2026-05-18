using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardState
{
    public const int Size = 4;          // 4x4x4
    public const int WinLength = 4;     // need 4 in a row to win

    // [x, y, z] — y is the vertical layer
    private Player[,,] grid;

    public BoardState()
    {
        grid = new Player[Size, Size, Size];
        // Default value of Player enum is None (0), so grid starts empty
    }

    public BoardState(BoardState other)
    {
        grid = new Player[Size, Size, Size];
        Array.Copy(other.grid, grid, other.grid.Length);
    }

    public Player Get(int x, int y, int z)
    {
        return grid[x, y, z];
    }

    public void Set(int x, int y, int z, Player p)
    {
        grid[x, y, z] = p;
    }

    public bool IsEmpty(int x, int y, int z)
    {
        return grid[x, y, z] == Player.None;
    }

    public bool InBounds(int x, int y, int z)
    {
        return x >= 0 && x < Size
            && y >= 0 && y < Size
            && z >= 0 && z < Size;
    }
    private static List<WinLine> cachedLines;

    private static List<WinLine> GetLines()
    {
        if (cachedLines == null)
        {
            cachedLines = WinLineGenerator.Generate(Size);
        }
        return cachedLines;
    }

    // If a player has won, returns that player. Otherwise returns Player.None.
    public Player CheckWinner()
    {
        foreach (WinLine line in GetLines())
        {
            Player first = grid[line.cells[0].x, line.cells[0].y, line.cells[0].z];
            if (first == Player.None) continue;

            bool allSame = true;
            for (int i = 1; i < line.cells.Length; i++)
            {
                var c = line.cells[i];
                if (grid[c.x, c.y, c.z] != first)
                {
                    allSame = false;
                    break;
                }
            }

            if (allSame)
            {
                return first;
            }
        }

        return Player.None;
    }

    // Returns true if the board is full 
    public bool IsFull()
    {
        for (int x = 0; x < Size; x++)
            for (int y = 0; y < Size; y++)
                for (int z = 0; z < Size; z++)
                    if (grid[x, y, z] == Player.None)
                        return false;
        return true;
    }


    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        for (int y = Size - 1; y >= 0; y--)  // print top layer first
        {
            sb.AppendLine($"Layer y={y}:");
            for (int z = 0; z < Size; z++)
            {
                for (int x = 0; x < Size; x++)
                {
                    char c = grid[x, y, z] switch
                    {
                        Player.X => 'X',
                        Player.O => 'O',
                        _ => '.'
                    };
                    sb.Append(c).Append(' ');
                }
                sb.AppendLine();
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
    // Returns the 76 winning lines on a 4x4x4 board
    public static List<WinLine> GetAllLines()
    {
        return GetLines();  // reuses the existing private cached generator
    }
    private static int[,,] cachedCellRichness;

    // Returns a 3D array where [x, y, z] = number of winning lines that include this cell
    // Cached after first call. Cells in the interior appear in more lines than corners/edges
    public static int[,,] GetCellRichness()
    {
        if (cachedCellRichness != null) return cachedCellRichness;

        int[,,] richness = new int[Size, Size, Size];
        foreach (WinLine line in GetAllLines())
        {
            foreach (Vector3Int c in line.cells)
            {
                richness[c.x, c.y, c.z]++;
            }
        }
        cachedCellRichness = richness;
        return cachedCellRichness;
    }
}