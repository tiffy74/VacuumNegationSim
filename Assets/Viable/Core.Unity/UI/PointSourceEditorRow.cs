using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Represents a single row in the Point Source Editor.
    /// Allows editing X, Y, and Strength values for a point source.
    /// </summary>
    public class PointSourceEditorRow : MonoBehaviour
    {
        [Header("Input Fields")]
        [SerializeField] private TMP_InputField xInput;
        [SerializeField] private TMP_InputField yInput;
        [SerializeField] private TMP_InputField strengthInput;

        [Header("Remove Button")]
        [SerializeField] private Button removeButton;

        private Action onRemoveCallback;

        /// <summary>
        /// Initialize the row with source data and a remove callback.
        /// </summary>
        public void Initialize(Configuration.PointSourceData sourceData, Action onRemove)
        {
            // Set input values
            if (xInput != null)
                xInput.text = sourceData.X.ToString();

            if (yInput != null)
                yInput.text = sourceData.Y.ToString();

            if (strengthInput != null)
                strengthInput.text = FormatScientific(sourceData.Strength);

            // Wire remove button
            onRemoveCallback = onRemove;
            if (removeButton != null)
            {
                removeButton.onClick.RemoveAllListeners();
                removeButton.onClick.AddListener(OnRemoveClicked);
            }
        }

        /// <summary>
        /// Get the current source data from input fields.
        /// </summary>
        public Configuration.PointSourceData GetSourceData()
        {
            int x = 0;
            int y = 0;
            double strength = 1e5;

            if (xInput != null && int.TryParse(xInput.text, out int parsedX))
                x = parsedX;

            if (yInput != null && int.TryParse(yInput.text, out int parsedY))
                y = parsedY;

            if (strengthInput != null && TryParseDouble(strengthInput.text, out double parsedStrength))
                strength = parsedStrength;

            return new Configuration.PointSourceData(x, y, strength);
        }

        private void OnRemoveClicked()
        {
            onRemoveCallback?.Invoke();
        }

        private string FormatScientific(double value)
        {
            if (value >= 1e6 || value <= -1e6 || (value != 0 && Math.Abs(value) < 0.001))
            {
                return value.ToString("E2"); // Scientific notation
            }
            else
            {
                return value.ToString("F2"); // Standard notation
            }
        }

        private bool TryParseDouble(string text, out double result)
        {
            return double.TryParse(text, System.Globalization.NumberStyles.Any,
                                  System.Globalization.CultureInfo.InvariantCulture,
                                  out result);
        }

        private void OnDestroy()
        {
            if (removeButton != null)
                removeButton.onClick.RemoveAllListeners();
        }
    }
}
