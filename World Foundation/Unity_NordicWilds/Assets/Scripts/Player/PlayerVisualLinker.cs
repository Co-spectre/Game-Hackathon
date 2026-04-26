using UnityEngine;

namespace NordicWilds.Player
{
    public class PlayerVisualLinker : MonoBehaviour
    {
        [Header("Meshy Model")]
        [Tooltip("The visual child object. The Setup script assigns this automatically.")]
        [SerializeField] private GameObject visualInstance;

        private void Awake()
        {
            // Disable legacy procedural visual if it exists
            var legacyVisual = transform.Find("RuntimeVisualModel");
            if (legacyVisual != null)
                legacyVisual.gameObject.SetActive(false);

            if (visualInstance == null)
            {
                var existing = transform.Find("PlayerVisual");
                if (existing != null) visualInstance = existing.gameObject;
            }

            if (visualInstance != null)
            {
                // Make sure the visual doesn't interfere with physics
                DisablePhysicsOnVisual(visualInstance);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────────
        private void DisablePhysicsOnVisual(GameObject visual)
        {
            foreach (var col in visual.GetComponentsInChildren<Collider>())
                col.enabled = false;

            foreach (var rb in visual.GetComponentsInChildren<Rigidbody>())
                rb.isKinematic = true;
        }
    }
}
