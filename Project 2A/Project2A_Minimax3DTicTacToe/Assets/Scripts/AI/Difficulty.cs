public enum Difficulty
{
    Easy = 1,
    Casual = 2,
    Medium = 3,
    Hard = 4,
    Expert = 5
}

public static class DifficultyExtensions
{
    public static int ToDepth(this Difficulty d) => (int)d;

    public static string ToDisplayName(this Difficulty d) => d switch
    {
        Difficulty.Easy => "Easy",
        Difficulty.Casual => "Casual",
        Difficulty.Medium => "Medium",
        Difficulty.Hard => "Hard",
        Difficulty.Expert => "Expert",
        _ => d.ToString()
    };

    // Probability (0..1) that the AI plays a random move instead of the Minimax-best move.
    // Higher values = weaker, more human-like blunders. Used to dumb down lower difficulties
    // so the strong heuristic doesn't make even depth 1 feel competent.

    public static float BlunderChance(this Difficulty d) => d switch
    {
        Difficulty.Easy => 0.50f,   // mostly random
        Difficulty.Casual => 0.25f,   // frequent blunders
        Difficulty.Medium => 0.05f,   // occasional slip
        Difficulty.Hard => 0.00f,
        Difficulty.Expert => 0.00f,
        _ => 0f
    };
}