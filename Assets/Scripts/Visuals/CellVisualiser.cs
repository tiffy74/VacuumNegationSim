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

    
    public Color GetViabilityColor(float viability)
    {
        float t = Mathf.Clamp01(viability);

        if (t < 0.1f)
            return Color.Lerp(Color.black, Color.red, t / 0.1f); // deep blue to blue
        else if (t < 0.2f)
            return Color.Lerp(Color.blue, Color.cyan, (t - 0.1f) / 0.1f);
        else if (t < 0.4f)
            return Color.Lerp(Color.cyan, Color.green, (t - 0.2f) / 0.2f);
        else if (t < 0.6f)
            return Color.Lerp(Color.green, Color.yellow, (t - 0.4f) / 0.2f);
        else if (t < 0.8f)
            return Color.Lerp(Color.yellow, new Color(1.0f, 0.5f, 0.0f), (t - 0.6f) / 0.2f); // yellow to orange
        else if (t < 0.95f)
            return Color.Lerp(new Color(1.0f, 0.5f, 0.0f), Color.red, (t - 0.8f) / 0.15f); // orange to red
        else
            return Color.Lerp(Color.whiteSmoke, Color.white, (t - 0.99f) / 0.05f); // red to white for very high viability
    }
    public void SetViabilityColor(float viability)
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        Color color = Color.grey; // Default color

        float t = Mathf.Clamp01(viability);
        if (t < 0.002f)
            color = Color.Lerp(Color.gray, Color.blue, t / 0.2f);        // 0.0–0.2
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

    public void SetViabilityWithEntropy(float viability, float entropy)
    {
        float vNorm = Mathf.Clamp01(viability);
        Color viabilityColor = GetViabilityColor(vNorm);
        // Fade toward blue as entropy increases
        // Color entropyColor = Color.Lerp(viabilityColor, Color.blue, Mathf.Clamp01(entropy));
        SetCombinedColor(viabilityColor,entropy);
    }
    public void SetCombinedColor(Color viabilityColor, float entropy)
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        float eNorm = Mathf.Clamp01(entropy);
        float entropySharp = Mathf.Pow(eNorm, 2.5f); // Sharper fade as entropy increases

        // Fade toward purple for high entropy (or choose another color for complexity)
        Color entropyColor = Color.Lerp(viabilityColor, new Color(0.5f, 0.0f, 0.7f), entropySharp);

        // Optionally, for very high entropy, fade further toward white or black
        if (eNorm > 0.95f)
            entropyColor = Color.Lerp(entropyColor, Color.black, (eNorm - 0.95f) / 0.05f);

        entropyColor.a = 1f;
        _renderer.color = entropyColor;
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
