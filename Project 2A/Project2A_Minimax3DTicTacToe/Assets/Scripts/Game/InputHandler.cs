using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public GameManager gameManager;

    [Header("Raycast Settings")]
    public float maxRayDistance = 100f;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRayDistance))
        {
            Cell clickedCell = hit.collider.GetComponent<Cell>();
            if (clickedCell == null) return;

            if (gameManager == null)
            {
                Debug.LogError("InputHandler.gameManager is not assigned in the Inspector!");
                return;
            }

            if (gameManager.gameOver)
            {
                Debug.Log("Game is over — clicks ignored.");
                return;
            }

            if (!gameManager.IsHumanTurn)
            {
                Debug.Log("Not your turn — AI is moving.");
                return;
            }

            gameManager.TryPlacePiece(clickedCell.x, clickedCell.y, clickedCell.z);
        }
    }
}