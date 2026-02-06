using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Viable.Core.Unity.UI
{
    /// <summary>
    /// Notification UI - Shows temporary popups in bottom-right corner.
    /// Used for feedback on user actions (preset loaded, simulation restarted, errors, etc.).
    /// Stage 14: Added for better UX feedback.
    /// </summary>
    public class NotificationUI : MonoBehaviour
    {
        public enum NotificationType
        {
            Info,
            Success,
            Warning,
            Error
        }

        [Header("UI Elements")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private Image notificationBackground;

        [Header("Animation Settings")]
        [SerializeField] private float displayDuration = 2.5f;
        [SerializeField] private float fadeInDuration = 2.3f;
        [SerializeField] private float fadeOutDuration = 2.3f;

        [Header("Colors")]
        [SerializeField] private Color infoColor = new Color(0f, 0f, 0.4f);       // #3380CC (Dark Blue)
        [SerializeField] private Color successColor = new Color(0.2f, 0.7f, 0.3f, 1.0f);    // #33B34D (Green)
        [SerializeField] private Color warningColor = new Color(0.9f, 0.6f, 0.1f, 1.0f);    // #E69919 (Orange)
        [SerializeField] private Color errorColor = new Color(0.8f, 0.2f, 0.2f, 1.0f);      // #CC3333 (Red)
        [SerializeField] private Color textColor = Color.white;                              // #FFFFFF (White)

        private Coroutine currentNotificationCoroutine;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            // Get or add CanvasGroup for fade animations
            if (notificationPanel != null)
            {
                canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
                }
            }

            // Ensure text color is set
            if (notificationText != null)
            {
                notificationText.color = textColor;
            }

            // Hide initially
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Show a notification message with specified type.
        /// Automatically hides after displayDuration seconds.
        /// </summary>
        public void ShowNotification(string message, NotificationType type = NotificationType.Info)
        {
            // Cancel any existing notification
            if (currentNotificationCoroutine != null)
            {
                StopCoroutine(currentNotificationCoroutine);
            }

            // Start new notification
            currentNotificationCoroutine = StartCoroutine(ShowNotificationCoroutine(message, type));
        }

        private IEnumerator ShowNotificationCoroutine(string message, NotificationType type)
        {
            if (notificationPanel == null || notificationText == null)
            {
                Debug.LogWarning("[NotificationUI] UI elements not assigned!");
                yield break;
            }

            // Set message and color
            notificationText.text = message;
            notificationText.color = textColor; // Ensure text is always readable
            
            if (notificationBackground != null)
            {
                notificationBackground.color = GetColorForType(type);
            }

            // Show panel
            notificationPanel.SetActive(true);
            // Fade in
            // yield return FadeCanvasGroup(canvasGroup, 0f, 1f, fadeInDuration);

            // Wait
            yield return new WaitForSeconds(displayDuration);

            // Fade out
            // yield return FadeCanvasGroup(canvasGroup, 1f, 0f, fadeOutDuration);

            // Hide panel
            notificationPanel.SetActive(false);

            currentNotificationCoroutine = null;
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup group, float startAlpha, float endAlpha, float duration)
        {
            if (group == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                group.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                yield return null;
            }

            group.alpha = endAlpha;
        }

        private Color GetColorForType(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Info:
                    return infoColor;
                case NotificationType.Success:
                    return successColor;
                case NotificationType.Warning:
                    return warningColor;
                case NotificationType.Error:
                    return errorColor;
                default:
                    return infoColor;
            }
        }

        /// <summary>
        /// Hide current notification immediately (if any).
        /// </summary>
        public void HideNotification()
        {
            if (currentNotificationCoroutine != null)
            {
                StopCoroutine(currentNotificationCoroutine);
                currentNotificationCoroutine = null;
            }

            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
        }
    }
}
