using UnityEngine;

public class CellVisualiser : MonoBehaviour
{
    private SpriteRenderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (_renderer == null)
            Debug.LogError("CellVisualiser requires a SpriteRenderer!");
    }

    public void Initialize(Color color)
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        SetColor(color);
        Debug.Log($"[VISUAL] Initialized CellVisualiser with color {color}");
    }

    float NormalizeViability(float v)
    {
        // Log compression: preserves gradients above 1
        // v = 1 -> ~0.5, v = 10 -> ~0.8, v < 1 smoothly spreads out
        return Mathf.Clamp01(Mathf.Log10(1f + Mathf.Max(0f, v)));
    }
    public Color GetViabilityColor(float viability)
    {
        float t = NormalizeViability(viability);

        // Use a smooth gradient: blue (low) -> cyan -> green -> yellow -> orange -> red -> white (high)
        if (t < 0.2f)
            return Color.Lerp(Color.blue, Color.cyan, t / 0.2f);
        else if (t < 0.4f)
            return Color.Lerp(Color.cyan, Color.green, (t - 0.2f) / 0.2f);
        else if (t < 0.6f)
            return Color.Lerp(Color.green, Color.yellow, (t - 0.4f) / 0.2f);
        else if (t < 0.8f)
            return Color.Lerp(Color.yellow, new Color(1.0f, 0.5f, 0.0f), (t - 0.6f) / 0.2f); // yellow to orange
        else if (t < 0.95f)
            return Color.Lerp(new Color(1.0f, 0.5f, 0.0f), Color.red, (t - 0.8f) / 0.15f); // orange to red
        else
            return Color.Lerp(Color.red, Color.white, (t - 0.95f) / 0.05f); // red to white for very high viability
    }
    public void SetViabilityColor(float viability)
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        Color color = Color.black; // Default color

        float t = NormalizeViability(viability);
        if (t < 0.002f)
            color = Color.Lerp(Color.black, Color.blue, t / 0.2f);        // 0.0–0.2
        else if (t < 0.04f)
            color = Color.Lerp(Color.blue, Color.cyan, (t - 0.2f) / 0.2f); // 0.2–0.4
        else if (t < 0.6f)
            color = Color.Lerp(Color.cyan, Color.green, (t - 0.4f) / 0.2f); // 0.4–0.6
        else if (t < 0.8f)
            color = Color.Lerp(Color.green, Color.yellow, (t - 0.6f) / 0.2f); // 0.6–0.8
        else
            color = Color.Lerp(Color.yellow, Color.red, (t - 0.8f) / 0.2f);   // 0.8–1.0
        _renderer.color = color;
        Debug.Log($"[VISUAL] Set color for viability {viability:F3} to {_renderer.color}");
    }
    Color ApplyRadialShading(Color baseColor, float rNorm)
    {
        // Slight darkening toward the boundary to reveal geometry
        float shade = Mathf.Lerp(1.1f, 0.7f, rNorm);
        return new Color(
            baseColor.r * shade,
            baseColor.g * shade,
            baseColor.b * shade,
            1f
        );
    }
    public void SetViabilityWithEntropy(float viability, float entropy, float rNorm)
    {
        float vNorm = NormalizeViability(viability);
        float eNorm = Mathf.Clamp01(entropy);

        Color viabilityColor = GetViabilityColor(vNorm);

        float entropyBlend = Mathf.Pow(eNorm, 1.2f) * 0.4f;
        Color entropyOverlay = Color.Lerp(viabilityColor, Color.black, entropyBlend);

        if (vNorm < 0.05f)
            entropyOverlay = Color.Lerp(entropyOverlay, Color.black, 1f - vNorm / 0.05f);

        Color finalColor = Color.Lerp(viabilityColor, entropyOverlay, 0.5f);
        finalColor = ApplyRadialShading(finalColor, rNorm);

        _renderer.color = finalColor;
    }


    public void SetCombinedColor(Color viabilityColor, Color entropyOverlay)
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        // Blend the two colors (you can adjust the blend factor as needed)
        Color finalColor = Color.Lerp(viabilityColor, entropyOverlay, 0.5f);
        finalColor.a = 1f;
        _renderer.color = finalColor;
    }
    // Add near the bottom of CellVisualiser.cs
    public void SetColor(Color c)
    {
        // If you already cache SpriteRenderer as _sr, use that.
        // Otherwise:
        var sr = GetComponent<SpriteRenderer>();
        sr.color = c;
    }
}