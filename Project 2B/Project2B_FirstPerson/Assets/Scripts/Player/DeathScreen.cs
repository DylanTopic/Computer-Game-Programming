using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    public PlayerHealth player;
    public GameObject deathPanel;
    public Button restartButton;

    void Start()
    {
        if (player != null)
            player.OnDied += ShowDeathScreen;

        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);

        // make sure the panel starts hidden
        if (deathPanel != null)
            deathPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (player != null)
            player.OnDied -= ShowDeathScreen;
    }

    void ShowDeathScreen()
    {
        if (deathPanel != null)
            deathPanel.SetActive(true);

        // unlock cursor so the player can click Restart
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // freeze the world
        Time.timeScale = 0f;
    }

    void Restart()
    {
        // reset time, then reload the current scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}