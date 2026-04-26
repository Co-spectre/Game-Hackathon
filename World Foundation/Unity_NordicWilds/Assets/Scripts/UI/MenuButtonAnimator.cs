using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NordicWilds.UI
{
    public class MenuButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private float hoverScale = 1.045f;
        [SerializeField] private float pressScale = 0.975f;
        [SerializeField] private float spring = 16f;

        private RectTransform rect;
        private Selectable selectable;
        private Vector3 targetScale = Vector3.one;
        private Vector3 velocity;
        private bool hovering;
        private bool pressing;

        private void Awake()
        {
            rect = transform as RectTransform;
            selectable = GetComponent<Selectable>();
        }

        private void OnEnable()
        {
            targetScale = Vector3.one;
            velocity = Vector3.zero;
            hovering = false;
            pressing = false;
            if (rect != null)
                rect.localScale = Vector3.one;
        }

        private void OnDisable()
        {
            hovering = false;
            pressing = false;
            targetScale = Vector3.one;
            velocity = Vector3.zero;
            if (rect != null)
                rect.localScale = Vector3.one;
        }

        private void Update()
        {
            if (rect == null)
                return;

            if (!IsUsable())
                targetScale = Vector3.one;

            rect.localScale = Vector3.SmoothDamp(
                rect.localScale,
                targetScale,
                ref velocity,
                1f / Mathf.Max(0.01f, spring),
                Mathf.Infinity,
                Time.unscaledDeltaTime);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsUsable()) return;

            hovering = true;
            targetScale = Vector3.one * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovering = false;
            if (!pressing)
                targetScale = Vector3.one;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsUsable()) return;

            pressing = true;
            targetScale = Vector3.one * pressScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pressing = false;
            targetScale = Vector3.one * (hovering ? hoverScale : 1f);
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!IsUsable()) return;

            hovering = true;
            targetScale = Vector3.one * hoverScale;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            hovering = false;
            pressing = false;
            targetScale = Vector3.one;
        }

        private bool IsUsable()
        {
            return selectable == null || selectable.IsInteractable();
        }
    }
}
