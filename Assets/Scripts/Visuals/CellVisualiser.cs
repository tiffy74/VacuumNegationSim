using UnityEngine;

public class CellVisualiser : MonoBehaviour
{
    private SpriteRenderer _renderer;
    public Color LastColor { get; private set; }
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
        // Broader range to avoid pushing most values to the hot end
        const float VmaxVisual = 2.0f;
        return Mathf.Clamp01(v / VmaxVisual);
    }

    public Color GetViabilityColor(float viability)
    {
        float t = Mathf.Clamp01(viability);

        if (t < 0.1f)
            return Color.Lerp(Color.black, Color.blue, t / 0.1f);
        else if (t < 0.25f)
            return Color.Lerp(Color.blue, Color.cyan, (t - 0.1f) / 0.15f);
        else if (t < 0.5f)
            return Color.Lerp(Color.cyan, Color.green, (t - 0.25f) / 0.25f);
        else if (t < 0.75f)
            return Color.Lerp(Color.green, Color.yellow, (t - 0.5f) / 0.25f);
        else if (t < 0.9f)
            return Color.Lerp(Color.yellow, new Color(1.0f, 0.7f, 0.2f), (t - 0.75f) / 0.15f); // yellow->amber
        else
            return Color.Lerp(new Color(1.0f, 0.7f, 0.2f), new Color(1.0f, 0.85f, 0.6f), (t - 0.9f) / 0.1f); // amber->light peach (avoid deep red)
    }
    public void SetViabilityColor(float viability)
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        Color color = Color.black; // Default color

        float t = NormalizeViability(viability);
        if (t < 0.1f)
            color = Color.Lerp(Color.black, Color.blue, t / 0.1f);
        else if (t < 0.25f)
            color = Color.Lerp(Color.blue, Color.cyan, (t - 0.1f) / 0.15f);
        else if (t < 0.5f)
            color = Color.Lerp(Color.cyan, Color.green, (t - 0.25f) / 0.25f);
        else if (t < 0.75f)
            color = Color.Lerp(Color.green, Color.yellow, (t - 0.5f) / 0.25f);
        else if (t < 0.9f)
            color = Color.Lerp(Color.yellow, new Color(1.0f, 0.7f, 0.2f), (t - 0.75f) / 0.15f);
        else
            color = Color.Lerp(new Color(1.0f, 0.7f, 0.2f), new Color(1.0f, 0.85f, 0.6f), (t - 0.9f) / 0.1f);

        _renderer.color = color;
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
    public void SetViabilityWithEntropy(float viability, float entropy)
    {
        float vNorm = Mathf.Clamp01(viability);
        float eNorm = Mathf.Clamp01(entropy);

        Color viabilityColor = GetViabilityColor(vNorm);

        // Entropy as a subtle overlay, not a full override
        float entropyBlend = Mathf.Pow(eNorm, 1.2f) * 0.4f;

        // Only fade to black if viability is extremely low
        if (vNorm < 0.00005f)
        {
            Color entropyOverlay = Color.Lerp(viabilityColor, Color.blue, entropyBlend);
            viabilityColor = entropyOverlay;
        }
            

        SetCombinedColor(viabilityColor);
    }

    public void SetViability(float v)
    {
        SetViabilityWithEntropy(v, 0f);
    }

    public void SetCombinedColor(Color viabilityColor)
    {
        if (_renderer == null)
            _renderer = GetComponent<SpriteRenderer>();

        // Blend the two colors (you can adjust the blend factor as needed)
        Color finalColor = viabilityColor;
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