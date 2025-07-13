using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Outline : MonoBehaviour
{
    [Header("Tint Settings")]
    [Range(0f, 1f)] public float tintAmount = 0.1f; // How much brighter
    private Color originalColor;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    private void OnMouseEnter()
    {
        sr.color = GetTintedColor(originalColor, tintAmount);
    }

    private void OnMouseExit()
    {
        sr.color = originalColor;
    }

    private Color GetTintedColor(Color baseColor, float amount)
    {
        // Lighten the color slightly
        return new Color(
            Mathf.Clamp01(baseColor.r + amount),
            Mathf.Clamp01(baseColor.g + amount),
            Mathf.Clamp01(baseColor.b + amount),
            baseColor.a
        );
    }
}




