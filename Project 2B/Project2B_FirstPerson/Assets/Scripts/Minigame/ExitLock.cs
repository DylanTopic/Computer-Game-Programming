using UnityEngine;

public class ExitLock : MonoBehaviour
{
    public Renderer slabRenderer;
    public Color unlockedColor = Color.green;

    // damage dealt when the player loses the minigame
    public int losePenalty = 50;

    // fires when the player walks through after unlocking
    public System.Action OnExitReached;

    private bool unlocked = false;
    private bool active = false;
    private bool escaped = false;

    private PlayerHealth cachedPlayerHealth;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // already beaten - this is the escape
        if (unlocked && !escaped)
        {
            escaped = true;
            Debug.Log("Player escaped through the exit.");
            OnExitReached?.Invoke();
            return;
        }

        // already in the minigame
        if (active) return;

        // open the minigame
        active = true;
        cachedPlayerHealth = other.GetComponent<PlayerHealth>();
        MinigameManager.Instance.Open(OnMinigameResolved);
    }

    void OnMinigameResolved(bool playerWon)
    {
        active = false;

        if (playerWon)
        {
            unlocked = true;
            Debug.Log("Exit unlocked! Walk through to escape.");
            if (slabRenderer != null)
            {
                Material mat = slabRenderer.material;
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", unlockedColor);
                if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", unlockedColor);
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", unlockedColor * 2f);
                    mat.EnableKeyword("_EMISSION");
                }
            }
        }
        else
        {
            Debug.Log("Lock holds. Try again.");
            // hurt the player for failing
            if (cachedPlayerHealth != null)
                cachedPlayerHealth.TakeDamage(losePenalty);
        }
    }
}