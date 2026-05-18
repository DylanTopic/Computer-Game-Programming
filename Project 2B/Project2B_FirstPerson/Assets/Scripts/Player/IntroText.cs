using UnityEngine;
using TMPro;
using System.Collections;

public class IntroText : MonoBehaviour
{
    public TMP_Text introText;
    public float holdDuration = 4f;    // how long it stays before fading
    public float fadeDuration = 1.2f;  // fade out time
    public float shakeAmount = 6f;     // pixels of random offset

    public string message = "Dodge the enemies, solve the puzzle, and escape the dungeon";

    private Vector3 originalPos;

    void Start()
    {
        if (introText == null) return;
        originalPos = introText.rectTransform.localPosition;
        introText.text = message;
        SetAlpha(1f);
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        float elapsed = 0f;
        float total = holdDuration + fadeDuration;

        while (elapsed < total)
        {
            // jitter the position every frame
            float x = (Random.value - 0.5f) * 2f * shakeAmount;
            float y = (Random.value - 0.5f) * 2f * shakeAmount;
            introText.rectTransform.localPosition = originalPos + new Vector3(x, y, 0f);

            // start fading once we're past the hold period
            if (elapsed > holdDuration)
            {
                float t = (elapsed - holdDuration) / fadeDuration;
                SetAlpha(1f - t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // clean up
        introText.rectTransform.localPosition = originalPos;
        SetAlpha(0f);
        introText.gameObject.SetActive(false);
    }

    void SetAlpha(float a)
    {
        Color c = introText.color;
        c.a = Mathf.Clamp01(a);
        introText.color = c;
    }
}