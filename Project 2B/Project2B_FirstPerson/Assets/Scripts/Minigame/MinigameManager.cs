using UnityEngine;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    // singleton so other scripts can reach this
    public static MinigameManager Instance;

    // UI refs - assigned in the Inspector
    public GameObject difficultyPanel;
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;
    public Button winButton;
    public Button loseButton;  

    // chosen Minimax depth
    public int chosenDepth = 1;

    // callback fired when the minigame ends (true = player won)
    private System.Action<bool> onResolved;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // wire button clicks
        easyButton.onClick.AddListener(() => StartGame(1));
        mediumButton.onClick.AddListener(() => StartGame(2));
        hardButton.onClick.AddListener(() => StartGame(4));

        // debug placeholders
        winButton.onClick.AddListener(() => Resolve(true));
        loseButton.onClick.AddListener(() => Resolve(false));
    }

    void Update()
    {
    }

    // called by ExitLock when the player enters the trigger
    public void Open(System.Action<bool> callback)
    {
        onResolved = callback;

        // freeze the FPS world
        Time.timeScale = 0f;

        // free the cursor for clicking
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // show the difficulty picker
        difficultyPanel.SetActive(true);
    }

    // called when a difficulty button is clicked
    void StartGame(int depth)
    {
        chosenDepth = depth;
        difficultyPanel.SetActive(false);

        TicTacToeBoardUI ui = GetComponent<TicTacToeBoardUI>();

        // map depth to blunder chance
        // Easy (1)  = 70% random  - very forgiving
        // Medium (2) = 15% random - some mistakes
        // Hard (4)  = 0% random  - plays perfectly
        float blunder = depth == 1 ? 0.7f : depth == 3 ? 0.15f : 0f;

        ui.Show(depth, blunder, OnBoardGameOver);
    }

    // called when the Tic-Tac-Toe game ends
    void OnBoardGameOver(bool playerWon)
    {
        Resolve(playerWon);
    }

    // called when the minigame ends - true = player won, false = lost
    public void Resolve(bool playerWon)
    {
        difficultyPanel.SetActive(false);

        // resume the FPS world
        Time.timeScale = 1f;

        // re-lock the cursor for first-person play
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // notify whoever opened us (ExitLock)
        if (onResolved != null) onResolved(playerWon);
        onResolved = null;
    }

    // debug self-play test
    void TestMinimax()
    {
        BoardState state = new BoardState();
        Debug.Log("=== Minimax self-play test (4x4x4) ===");

        // simulate: player X plays a center-ish cell
        state.Set(1, 1, 1, Player.X);

        // AI plays O at each difficulty
        foreach (int depth in new[] { 1, 2, 4 })
        {
            BoardState copy = new BoardState(state);
            float t = Time.realtimeSinceStartup;
            Vector3Int? move = Minimax.FindBestMove(copy, Player.O, depth);
            float elapsed = Time.realtimeSinceStartup - t;
            Debug.Log($"Depth {depth}: AI picks {move} (time {elapsed:F3}s)");
        }
    }
}