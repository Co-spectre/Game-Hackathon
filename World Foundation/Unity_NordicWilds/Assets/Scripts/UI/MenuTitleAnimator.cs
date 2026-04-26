using UnityEngine;
using UnityEngine.UI;

namespace NordicWilds.UI
{
    // Keeps the start menu readable and steady. The previous intro/pulse animation drove
    // CanvasGroup alpha every frame, which made the menu feel like it was blinking.
    public class MenuTitleAnimator : MonoBehaviour
    {
        public CanvasGroup titleGroup;
        public RectTransform titleRect;
        public CanvasGroup subtitleGroup;
        public CanvasGroup pressPromptGroup;
        public CanvasGroup buttonsGroup;

        private bool fadingOut;

        void OnEnable()
        {
            fadingOut = false;
            ShowSteady();
        }

        void Update()
        {
            if (!fadingOut)
                ShowSteady();
        }

        public void FadeOutAll(float dt, float duration)
        {
            fadingOut = true;
            float drop = dt / Mathf.Max(0.0001f, duration);
            if (titleGroup != null)       titleGroup.alpha       = Mathf.Max(0f, titleGroup.alpha - drop);
            if (subtitleGroup != null)    subtitleGroup.alpha    = Mathf.Max(0f, subtitleGroup.alpha - drop);
            if (pressPromptGroup != null) pressPromptGroup.alpha = Mathf.Max(0f, pressPromptGroup.alpha - drop);
            if (buttonsGroup != null)     buttonsGroup.alpha     = Mathf.Max(0f, buttonsGroup.alpha - drop);
        }

        private void ShowSteady()
        {
            if (titleGroup != null) titleGroup.alpha = 1f;
            if (titleRect != null) titleRect.localScale = Vector3.one;
            if (subtitleGroup != null) subtitleGroup.alpha = 1f;
            if (pressPromptGroup != null) pressPromptGroup.alpha = 0.88f;
            if (buttonsGroup != null)
            {
                buttonsGroup.alpha = 1f;
                buttonsGroup.interactable = true;
                buttonsGroup.blocksRaycasts = true;
            }
        }
    }
}
