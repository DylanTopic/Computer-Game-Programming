using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public BoardBuilder boardBuilder;

    [Header("Materials")]
    public Material matEmpty;
    public Material matPlayerX;
    public Material matPlayerO;

    [Header("Players")]
    public ControllerType xController = ControllerType.Human;
    public ControllerType oController = ControllerType.AI;

    [Header("AI Settings")]
    [Tooltip("Delay (in seconds) before the AI makes its move — for UX, so it doesn't feel instant.")]
    public float aiMoveDelay = 0.5f;

    [Tooltip("AI difficulty level. Higher = looks further ahead, plays stronger, takes longer.")]
    public Difficulty difficulty = Difficulty.Medium;



    // Runtime state
    public BoardState boardState { get; private set; }
    public Player currentPlayer { get; private set; }
    public bool gameOver { get; private set; }

    // Convenience helpers
    public ControllerType CurrentController =>
        currentPlayer == Player.X ? xController : oController;

    public bool IsHumanTurn => CurrentController == ControllerType.Human;
    public bool IsAITurn => CurrentController == ControllerType.AI;

    void Start()
    {
        boardState = new BoardState();
        currentPlayer = Player.X;
        gameOver = false;
        Debug.Log("GameManager initialized. Current player: " + currentPlayer);
        Debug.Log($"X is {xController}, O is {oController}");

        if (IsAITurn)
        {
            StartCoroutine(AITurnRoutine());
        }
    }


    // Resets the board
    public void RestartGame()
    {
        StopAllCoroutines();  

        boardState = new BoardState();
        currentPlayer = Player.X;
        gameOver = false;

        // Reset every visual cell to empty
        for (int x = 0; x < BoardState.Size; x++)
            for (int y = 0; y < BoardState.Size; y++)
                for (int z = 0; z < BoardState.Size; z++)
                    UpdateCellVisual(x, y, z, Player.None);

        Debug.Log($"=== New game started. Difficulty: {difficulty.ToDisplayName()} ===");

        if (IsAITurn)
        {
            StartCoroutine(AITurnRoutine());
        }
    }


    // Public method for the UI to set difficulty mid-session
    public void SetDifficulty(Difficulty d)
    {
        difficulty = d;
        Debug.Log($"Difficulty set to {d.ToDisplayName()} (depth {d.ToDepth()})");
    }

    // Attempt to place a piece for the current player at the given coordinates
    // Returns true if the move was legal and applied; false otherwise
    public bool TryPlacePiece(int x, int y, int z)
    {
        if (gameOver)
        {
            Debug.Log("Game is over. No more moves allowed.");
            return false;
        }

        if (!boardState.InBounds(x, y, z))
        {
            Debug.LogWarning($"Move out of bounds: ({x}, {y}, {z})");
            return false;
        }

        if (!boardState.IsEmpty(x, y, z))
        {
            Debug.Log($"Cell ({x}, {y}, {z}) is already occupied.");
            return false;
        }

        // Update logical state
        boardState.Set(x, y, z, currentPlayer);

        // Update visual state
        UpdateCellVisual(x, y, z, currentPlayer);

        Debug.Log($"{currentPlayer} placed at ({x}, {y}, {z})");

        // Check for win
        Player winner = boardState.CheckWinner();
        if (winner != Player.None)
        {
            gameOver = true;
            Debug.Log($"=== {winner} WINS! ===");
            return true;
        }

        // Check for draw
        if (boardState.IsFull())
        {
            gameOver = true;
            Debug.Log("=== DRAW — board is full ===");
            return true;
        }

        // Swap turns
        currentPlayer = (currentPlayer == Player.X) ? Player.O : Player.X;
        Debug.Log("Now it's " + currentPlayer + "'s turn.");

        // If the new current player is the AI, schedule its move
        if (IsAITurn)
        {
            StartCoroutine(AITurnRoutine());
        }

        return true;
    }

    private System.Collections.IEnumerator AITurnRoutine()
    {
        Debug.Log($"AI ({currentPlayer}) is thinking...");
        yield return new WaitForSeconds(aiMoveDelay);

        // Don't act if the game ended in the meantime
        if (gameOver) yield break;

        Vector3Int? move = ChooseAIMove();
        if (move.HasValue)
        {
            Vector3Int m = move.Value;
            TryPlacePiece(m.x, m.y, m.z);
        }
        else
        {
            Debug.LogWarning("AI could not find a move — board may be full.");
        }
    }

    // Picks a move for the AI
    private Vector3Int? ChooseAIMove()
    {
        float blunderChance = difficulty.BlunderChance();
        if (blunderChance > 0f && Random.value < blunderChance)
        {
            Debug.Log($"AI blunder roll succeeded (chance {blunderChance:F2}) — playing random move");
            return ChooseRandomLegalMove();
        }

        return Minimax.FindBestMove(boardState, currentPlayer, difficulty.ToDepth());
    }
    private Vector3Int? ChooseRandomLegalMove()
    {
        List<Vector3Int> legal = new List<Vector3Int>();
        for (int x = 0; x < BoardState.Size; x++)
            for (int y = 0; y < BoardState.Size; y++)
                for (int z = 0; z < BoardState.Size; z++)
                    if (boardState.IsEmpty(x, y, z))
                        legal.Add(new Vector3Int(x, y, z));

        if (legal.Count == 0) return null;
        return legal[Random.Range(0, legal.Count)];
    }

    private void UpdateCellVisual(int x, int y, int z, Player p)
    {
        GameObject cellObj = boardBuilder.cells[x, y, z];
        if (cellObj == null) return;

        Renderer renderer = cellObj.GetComponent<Renderer>();
        if (renderer == null) return;

        Material targetMat = p switch
        {
            Player.X => matPlayerX,
            Player.O => matPlayerO,
            _ => matEmpty
        };

        renderer.material = targetMat;

        // Also update the Cell's state field so it stays in sync
        Cell cell = cellObj.GetComponent<Cell>();
        if (cell != null)
        {
            cell.state = p switch
            {
                Player.X => CellState.PlayerX,
                Player.O => CellState.PlayerO,
                _ => CellState.Empty
            };
        }
    }
}