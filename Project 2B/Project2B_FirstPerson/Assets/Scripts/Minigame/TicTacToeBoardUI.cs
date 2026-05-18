using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TicTacToeBoardUI : MonoBehaviour
{
    // assigned in Inspector
    public GameObject boardPanel;
    public TMP_Text statusText;
    public Transform layersContainer;
    public Button resignButton;

    // sides
    private Player humanPlayer = Player.X;
    private Player aiPlayer = Player.O;

    // state
    private BoardState board;
    private int aiDepth;
    private bool inputLocked;

    // callback to MinigameManager
    private System.Action<bool> onGameOver;

    // cell refs by [x,y,z]
    private Button[,,] cellButtons;
    private TMP_Text[,,] cellLabels;


    private float aiBlunderChance;

    void Awake()
    {
        if (resignButton != null)
            resignButton.onClick.AddListener(OnResign);
    }

    // open the board at the chosen depth
    public void Show(int minimaxDepth, float blunderChance, System.Action<bool> callback)
    {
        aiDepth = minimaxDepth;
        aiBlunderChance = blunderChance;
        onGameOver = callback;
        board = new BoardState();
        inputLocked = false;

        BuildBoardUI();
        boardPanel.SetActive(true);
        UpdateAllCells();
        UpdateStatus("Your turn (X). Click a cell.");
    }

    public void Hide()
    {
        boardPanel.SetActive(false);
    }

    // builds 4 horizontal rows, each with a label + 4x4 grid of cells
    void BuildBoardUI()
    {
        for (int i = layersContainer.childCount - 1; i >= 0; i--)
            Destroy(layersContainer.GetChild(i).gameObject);

        int n = BoardState.Size;
        cellButtons = new Button[n, n, n];
        cellLabels = new TMP_Text[n, n, n];

        // build top layer first (y = n-1) so up = up
        for (int y = n - 1; y >= 0; y--)
        {
            // one row per layer
            GameObject layer = new GameObject($"Layer_y{y}", typeof(RectTransform));
            layer.transform.SetParent(layersContainer, false);
            HorizontalLayoutGroup hlg = layer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 12;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            LayoutElement layerLe = layer.AddComponent<LayoutElement>();
            layerLe.minHeight = 180;

            // layer label
            GameObject labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(layer.transform, false);
            TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = $"Floor {y}";
            label.fontSize = 22;
            label.alignment = TextAlignmentOptions.Center;
            LayoutElement labelLe = labelGo.AddComponent<LayoutElement>();
            labelLe.minWidth = 60;
            labelLe.minHeight = 180;

            // grid container
            GameObject grid = new GameObject("Grid", typeof(RectTransform));
            grid.transform.SetParent(layer.transform, false);
            GridLayoutGroup glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(40, 40);
            glg.spacing = new Vector2(3, 3);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = n;
            LayoutElement gridLe = grid.AddComponent<LayoutElement>();
            gridLe.minWidth = n * 44;
            gridLe.minHeight = n * 44;

            // grid traversal rows are z (top to bottom), cols are x (left to right)
            for (int z = 0; z < n; z++)
            {
                for (int x = 0; x < n; x++)
                {
                    int xc = x, yc = y, zc = z;

                    GameObject btnGo = new GameObject($"Cell_{x}_{y}_{z}", typeof(RectTransform));
                    btnGo.transform.SetParent(grid.transform, false);

                    Image img = btnGo.AddComponent<Image>();
                    img.color = new Color(0.18f, 0.22f, 0.30f, 1f);

                    Button btn = btnGo.AddComponent<Button>();
                    btn.targetGraphic = img;
                    btn.onClick.AddListener(() => OnCellClicked(xc, yc, zc));

                    // text child fills the button
                    GameObject txtGo = new GameObject("Text", typeof(RectTransform));
                    txtGo.transform.SetParent(btnGo.transform, false);
                    RectTransform rt = txtGo.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    TextMeshProUGUI txt = txtGo.AddComponent<TextMeshProUGUI>();
                    txt.text = "";
                    txt.fontSize = 30;
                    txt.alignment = TextAlignmentOptions.Center;

                    cellButtons[x, y, z] = btn;
                    cellLabels[x, y, z] = txt;
                }
            }
        }
    }

    void OnCellClicked(int x, int y, int z)
    {
        if (inputLocked) return;
        if (!board.IsEmpty(x, y, z)) return;

        // human plays X
        board.Set(x, y, z, humanPlayer);
        UpdateAllCells();

        Player winner = board.CheckWinner();
        if (winner != Player.None || board.IsFull())
        {
            EndGame(winner);
            return;
        }

        // hand off to AI
        inputLocked = true;
        UpdateStatus("AI thinking...");
        StartCoroutine(AIMoveRoutine());
    }

    System.Collections.IEnumerator AIMoveRoutine()
    {
        yield return null;

        Vector3Int? move;
        // roll the dice - sometimes the AI plays randomly
        if (Random.value < aiBlunderChance)
        {
            move = PickRandomLegalMove();
            Debug.Log("AI blundered (random move)");
        }
        else
        {
            move = Minimax.FindBestMove(board, aiPlayer, aiDepth);
        }

        if (move.HasValue)
        {
            Vector3Int m = move.Value;
            board.Set(m.x, m.y, m.z, aiPlayer);
        }
        UpdateAllCells();

        Player winner = board.CheckWinner();
        if (winner != Player.None || board.IsFull())
        {
            EndGame(winner);
            yield break;
        }

        inputLocked = false;
        UpdateStatus("Your turn (X). Click a cell.");
    }

    void EndGame(Player winner)
    {
        inputLocked = true;
        bool playerWon = winner == humanPlayer;

        if (winner == Player.None) UpdateStatus("Draw.");
        else if (playerWon) UpdateStatus("You win! Exit unlocked.");
        else UpdateStatus("AI wins. Try again.");

        StartCoroutine(EndGameDelay(playerWon));
    }

    System.Collections.IEnumerator EndGameDelay(bool playerWon)
    {
        yield return new WaitForSecondsRealtime(2.5f);
        Hide();
        if (onGameOver != null) onGameOver(playerWon);
    }

    void OnResign()
    {
        EndGame(aiPlayer);
    }

    void UpdateAllCells()
    {
        int n = BoardState.Size;
        for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
                for (int z = 0; z < n; z++)
                {
                    Player p = board.Get(x, y, z);
                    string mark = p == Player.X ? "X" : p == Player.O ? "O" : "";
                    cellLabels[x, y, z].text = mark;
                    cellLabels[x, y, z].color =
                        p == Player.X ? Color.cyan :
                        p == Player.O ? new Color(1f, 0.5f, 0.5f) :
                        Color.white;
                }
    }

    void UpdateStatus(string s)
    {
        if (statusText != null) statusText.text = s;
    }

    // pick a random empty cell
    Vector3Int? PickRandomLegalMove()
    {
        List<Vector3Int> empties = new List<Vector3Int>();
        int n = BoardState.Size;
        for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
                for (int z = 0; z < n; z++)
                    if (board.IsEmpty(x, y, z))
                        empties.Add(new Vector3Int(x, y, z));
        if (empties.Count == 0) return null;
        return empties[Random.Range(0, empties.Count)];
    }
}