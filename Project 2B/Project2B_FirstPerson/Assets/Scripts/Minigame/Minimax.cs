using System.Collections.Generic;
using UnityEngine;

public static class Minimax
{
    private const int WIN_SCORE = 100000;
    private const int LOSE_SCORE = -100000;
    private const int DRAW_SCORE = 0;

    public static int NodesEvaluated;
    public static int Cutoffs;

    public static Vector3Int? FindBestMove(BoardState state, Player aiPlayer, int maxDepth)
    {
        NodesEvaluated = 0;
        Cutoffs = 0;

        Player opponent = Opponent(aiPlayer);
        int bestScore = int.MinValue;
        Vector3Int? bestMove = null;

        List<Vector3Int> moves = GetOrderedLegalMoves(state);
        if (moves.Count == 0) return null;

        int alpha = int.MinValue;
        int beta = int.MaxValue;

        foreach (Vector3Int move in moves)
        {
            BoardState child = new BoardState(state);
            child.Set(move.x, move.y, move.z, aiPlayer);

            int score = AlphaBeta(child, maxDepth - 1, alpha, beta, false, aiPlayer, opponent);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }

            alpha = Mathf.Max(alpha, bestScore);
        }

        Debug.Log($"Minimax(αβ) chose {bestMove} score={bestScore} depth={maxDepth} " +
                  $"nodes={NodesEvaluated} cutoffs={Cutoffs}");
        return bestMove;
    }

    private static int AlphaBeta(BoardState state, int depth, int alpha, int beta,
                                 bool isMaximizing, Player aiPlayer, Player opponent)
    {
        NodesEvaluated++;

        Player winner = state.CheckWinner();
        // depth thing prefer faster wins / slower losses
        if (winner == aiPlayer) return WIN_SCORE - (10 - depth);
        if (winner == opponent) return LOSE_SCORE + (10 - depth);
        if (state.IsFull()) return DRAW_SCORE;

        if (depth <= 0) return Evaluate(state, aiPlayer, opponent);

        List<Vector3Int> moves = GetOrderedLegalMoves(state);

        if (isMaximizing)
        {
            int best = int.MinValue;
            foreach (Vector3Int move in moves)
            {
                BoardState child = new BoardState(state);
                child.Set(move.x, move.y, move.z, aiPlayer);

                int score = AlphaBeta(child, depth - 1, alpha, beta, false, aiPlayer, opponent);
                if (score > best) best = score;
                if (best > alpha) alpha = best;

                if (alpha >= beta) { Cutoffs++; break; }
            }
            return best;
        }
        else
        {
            int best = int.MaxValue;
            foreach (Vector3Int move in moves)
            {
                BoardState child = new BoardState(state);
                child.Set(move.x, move.y, move.z, opponent);

                int score = AlphaBeta(child, depth - 1, alpha, beta, true, aiPlayer, opponent);
                if (score < best) best = score;
                if (best < beta) beta = best;

                if (alpha >= beta) { Cutoffs++; break; }
            }
            return best;
        }
    }

    // try center cells first since theyre in more winning lines, makes pruning work better
    private static List<Vector3Int> GetOrderedLegalMoves(BoardState state)
    {
        List<(Vector3Int move, float dist)> scored = new List<(Vector3Int, float)>();
        Vector3 center = new Vector3((BoardState.Size - 1) * 0.5f,
                                     (BoardState.Size - 1) * 0.5f,
                                     (BoardState.Size - 1) * 0.5f);

        for (int x = 0; x < BoardState.Size; x++)
            for (int y = 0; y < BoardState.Size; y++)
                for (int z = 0; z < BoardState.Size; z++)
                    if (state.IsEmpty(x, y, z))
                    {
                        float d = Vector3.Distance(new Vector3(x, y, z), center);
                        scored.Add((new Vector3Int(x, y, z), d));
                    }

        scored.Sort((a, b) => a.dist.CompareTo(b.dist));

        List<Vector3Int> result = new List<Vector3Int>();
        foreach (var s in scored) result.Add(s.move);
        return result;
    }

    private static Player Opponent(Player p) => p == Player.X ? Player.O : Player.X;

    // index = piece count on a line. big jump at 3 because thats a real threat
    private static readonly int[] LINE_SCORE_AI = { 0, 1, 25, 1500, 0 };
    // opponent values slightly higher so AI prefers blocking
    private static readonly int[] LINE_SCORE_OPPONENT = { 0, 1, 30, 1800, 0 };
    private const int CELL_RICHNESS_WEIGHT = 1;

    private static int Evaluate(BoardState state, Player aiPlayer, Player opponent)
    {
        int score = 0;

        foreach (WinLine line in BoardState.GetAllLines())
        {
            int aiCount = 0, oppCount = 0;
            foreach (Vector3Int c in line.cells)
            {
                Player cell = state.Get(c.x, c.y, c.z);
                if (cell == aiPlayer) aiCount++;
                else if (cell == opponent) oppCount++;
            }

            if (aiCount > 0 && oppCount > 0) continue;

            if (aiCount > 0) score += LINE_SCORE_AI[aiCount];
            else if (oppCount > 0) score -= LINE_SCORE_OPPONENT[oppCount];
        }

        int[,,] richness = BoardState.GetCellRichness();
        for (int x = 0; x < BoardState.Size; x++)
            for (int y = 0; y < BoardState.Size; y++)
                for (int z = 0; z < BoardState.Size; z++)
                {
                    Player cell = state.Get(x, y, z);
                    if (cell == aiPlayer)
                        score += richness[x, y, z] * CELL_RICHNESS_WEIGHT;
                    else if (cell == opponent)
                        score -= richness[x, y, z] * CELL_RICHNESS_WEIGHT;
                }

        return score;
    }
}