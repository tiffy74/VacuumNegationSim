using UnityEngine;

public class CellVisualiser : MonoBehaviour
{
    private SpriteRenderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        if (_renderer == null)
            Debug.LogError("CellVisualizer requires a SpriteRenderer!");
    }

    public void Initialize(Color color)
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        SetColor(color);
    }

    public void SetColor(Color color)
    {
        if (_renderer != null)
            _renderer.color = color;
    }
    public Color GetViabilityColor(float viability)
    {
        float t = Mathf.Clamp01(viability / 500f);

        if (t < 0.2f)
            return Color.Lerp(Color.gray, Color.blue, t / 0.2f);
        else if (t < 0.4f)
            return Color.Lerp(Color.blue, Color.cyan, (t - 0.2f) / 0.2f);
        else if (t < 0.6f)
            return Color.Lerp(Color.cyan, Color.green, (t - 0.4f) / 0.2f);
        else if (t < 0.8f)
            return Color.Lerp(Color.green, Color.yellow, (t - 0.6f) / 0.2f);
        else
            return Color.Lerp(Color.yellow, Color.red, (t - 0.8f) / 0.2f);
    }
    public void SetViabilityColor(float viability)
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        Color color = Color.white; // Default color

        float t = Mathf.Clamp01(viability / 100f);
        if (t < 0.2f)
            color = Color.Lerp(Color.gray, Color.blue, t / 0.2f);        // 0.0–0.2
        else if (t < 0.4f)
            color = Color.Lerp(Color.blue, Color.cyan, (t - 0.2f) / 0.2f); // 0.2–0.4
        else if (t < 0.6f)
            color = Color.Lerp(Color.cyan, Color.green, (t - 0.4f) / 0.2f); // 0.4–0.6
        else if (t < 0.8f)
            color = Color.Lerp(Color.green, Color.yellow, (t - 0.6f) / 0.2f); // 0.6–0.8
        else
            color = Color.Lerp(Color.yellow, Color.red, (t - 0.8f) / 0.2f);   // 0.8–1.0
        _renderer.color = color;
    }

    
    public void SetCombinedColor(float viability, float entropy)
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        float vNorm = Mathf.Clamp01(viability / 100f);
        float eNorm = Mathf.Clamp01(entropy);

        Color viabilityColor = Color.Lerp(Color.gray, Color.red, vNorm);     // Red = high viability
        Color entropyOverlay = Color.Lerp(Color.clear, Color.blue, eNorm);   // Blue overlay = high entropy

        Color finalColor = viabilityColor + entropyOverlay;
        finalColor.a = 1f; // ensure full opacity

        _renderer.color = finalColor;
    }
}
