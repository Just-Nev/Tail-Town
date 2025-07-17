using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Refill : MonoBehaviour
{
    [Header("Assign the target SpriteRenderer here")]
    public SpriteRenderer targetSpriteRenderer;

    [Header("Fade Settings")]
    public float fadeDuration = 1f; // Time it takes to reach full opacity

    private bool isFading = false;
    private float fadeTimer = 0f;

    void OnMouseDown()
    {
        if (targetSpriteRenderer != null && !isFading)
        {
            isFading = true;
            fadeTimer = 0f;
        }
    }

    void Update()
    {
        if (isFading && targetSpriteRenderer != null)
        {
            fadeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fadeTimer / fadeDuration);

            Color color = targetSpriteRenderer.color;
            color.a = Mathf.Lerp(color.a, 1f, t);
            targetSpriteRenderer.color = color;

            if (t >= 1f || Mathf.Approximately(color.a, 1f))
            {
                color.a = 1f;
                targetSpriteRenderer.color = color;
                isFading = false;
            }
        }
    }
}
