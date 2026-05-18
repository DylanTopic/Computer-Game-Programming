using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    // event the HUD subscribes to
    public System.Action<int, int> OnHealthChanged;
    public System.Action OnDied;

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log("Player HP: " + currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        Debug.Log("Player died.");
        OnDied?.Invoke();
        Time.timeScale = 0f;
    }
}