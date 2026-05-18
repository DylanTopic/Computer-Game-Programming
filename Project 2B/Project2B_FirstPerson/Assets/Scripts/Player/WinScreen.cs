using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    public ExitLock exitLock;
    public GameObject winPanel;
    public Button playAgainButton;

    void Start()
    {
        if (exitLock != null)
            exitLock.OnExitReached += ShowWinScreen;

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(PlayAgain);

        // hide on start
        if (winPanel != null)
            winPanel.SetActive(false);
    }



    void OnDestroy()
    {
        if (exitLock != null)
            exitLock.OnExitReached -= ShowWinScreen;
    }

    void ShowWinScreen()
    {
        Debug.Log("ShowWinScreen called - winPanel null? " + (winPanel == null));
        if (winPanel != null)
        {
            // bring to front of canvas
            winPanel.transform.SetAsLastSibling();

            // force full-screen anchors in case they got messed up
            RectTransform rt = winPanel.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            winPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}