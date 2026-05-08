using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;

    [Header("Buttons")]
    public Button btnEasy;
    public Button btnCasual;
    public Button btnMedium;
    public Button btnHard;
    public Button btnExpert;
    public Button btnRestart;

    [Header("Status Text")]
    public Text statusText;

    void Start()
    {
        // Hook up button clicks
        if (btnEasy != null) btnEasy.onClick.AddListener(() => OnDifficultySelected(Difficulty.Easy));
        if (btnCasual != null) btnCasual.onClick.AddListener(() => OnDifficultySelected(Difficulty.Casual));
        if (btnMedium != null) btnMedium.onClick.AddListener(() => OnDifficultySelected(Difficulty.Medium));
        if (btnHard != null) btnHard.onClick.AddListener(() => OnDifficultySelected(Difficulty.Hard));
        if (btnExpert != null) btnExpert.onClick.AddListener(() => OnDifficultySelected(Difficulty.Expert));
        if (btnRestart != null) btnRestart.onClick.AddListener(OnRestartClicked);

        UpdateStatusText();
    }

    void Update()
    {
        UpdateStatusText();
    }

    private void OnDifficultySelected(Difficulty d)
    {
        if (gameManager == null) return;
        gameManager.SetDifficulty(d);
        gameManager.RestartGame();   // Apply immediately by starting a fresh game
    }

    private void OnRestartClicked()
    {
        if (gameManager == null) return;
        gameManager.RestartGame();
    }

    private void UpdateStatusText()
    {
        if (gameManager == null || statusText == null) return;

        if (gameManager.gameOver)
        {
            Player winner = gameManager.boardState.CheckWinner();
            if (winner == Player.None)
                statusText.text = "Draw — board full";
            else
                statusText.text = $"{winner} wins!";
        }
        else
        {
            string whoseTurn = gameManager.IsHumanTurn ? "Your turn" : "AI thinking...";
            statusText.text = $"{whoseTurn} ({gameManager.currentPlayer})    Difficulty: {gameManager.difficulty.ToDisplayName()}";
        }
    }
}