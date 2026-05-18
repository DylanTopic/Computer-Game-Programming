using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    public PlayerHealth player;
    public TMP_Text hpText;

    void Start()
    {
        if (player != null)
        {
            // subscribe to HP changes
            player.OnHealthChanged += UpdateHP;
            // initial paint
            UpdateHP(player.currentHealth, player.maxHealth);
        }
    }

    void OnDestroy()
    {
        if (player != null)
            player.OnHealthChanged -= UpdateHP;
    }

    void UpdateHP(int current, int max)
    {
        if (hpText == null) return;
        hpText.text = $"HP: {current} / {max}";

        // color shift as HP drops
        float t = (float)current / max;
        if (t > 0.5f) hpText.color = Color.white;
        else if (t > 0.25f) hpText.color = new Color(1f, 0.7f, 0.2f); // orange
        else hpText.color = new Color(1f, 0.3f, 0.3f); // red
    }
}