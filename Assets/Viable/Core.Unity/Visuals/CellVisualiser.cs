using UnityEngine;

namespace Viable.Core.Unity.Visuals
{
    /// <summary>
    /// Handles visual representation of a single simulation cell.
    /// Maps viability and complexity metrics to colors.
    /// Stage 8: Moved from Assets.Scripts.Visuals to Viable.Core.Unity.Visuals.
    /// </summary>
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
        }

        float NormalizeViability(float v)
        {
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
                return Color.Lerp(Color.yellow, new Color(1.0f, 0.7f, 0.2f), (t - 0.75f) / 0.15f);
            else
                return Color.Lerp(new Color(1.0f, 0.7f, 0.2f), new Color(1.0f, 0.85f, 0.6f), (t - 0.9f) / 0.1f);
        }
        
        public void SetViabilityColor(float viability)
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();

            Color color = Color.black;

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
            float shade = Mathf.Lerp(1.1f, 0.7f, rNorm);
            return new Color(
                baseColor.r * shade,
                baseColor.g * shade,
                baseColor.b * shade,
                1f
            );
        }
        
        public void SetViabilityWithComplexity(float viability, float complexity)
        {
            float vNorm = Mathf.Clamp01(viability);
            float cNorm = Mathf.Clamp01(complexity);

            Color viabilityColor = GetViabilityColor(vNorm);

            // Complexity as a subtle overlay
            float complexityBlend = Mathf.Pow(cNorm, 1.2f) * 0.4f;

            // Only fade to blue if viability is extremely low
            if (vNorm < 0.00005f)
            {
                Color complexityOverlay = Color.Lerp(viabilityColor, Color.blue, complexityBlend);
                viabilityColor = complexityOverlay;
            }

            SetCombinedColor(viabilityColor);
        }

        public void SetViability(float v)
        {
            SetViabilityWithComplexity(v, 0f);
        }

        public void SetCombinedColor(Color viabilityColor)
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();

            Color finalColor = viabilityColor;
            finalColor.a = 1f;
            _renderer.color = finalColor;
        }
        
        public void SetColor(Color c)
        { 
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
            _renderer.color = c;
            LastColor = c;
        }
    }
}
