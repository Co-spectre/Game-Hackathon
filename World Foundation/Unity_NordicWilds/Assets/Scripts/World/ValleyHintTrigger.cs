using UnityEngine;

namespace NordicWilds.World
{
    /// <summary>
    /// Displays a brief on-screen hint message when the player enters the valley
    /// pass leading to the Woods Realm, guiding them toward the portal.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ValleyHintTrigger : MonoBehaviour
    {
        [SerializeField] private string message = "A dark path winds between the hills... a portal glows ahead.";
        [SerializeField] private float duration = 4.0f;
        [SerializeField] private bool oneShot = true;

        private bool triggered;
        private string activeMessage;
        private float activeMessageUntil;
        private GUIStyle style;
        private Texture2D bg;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
        }

        private void OnDestroy()
        {
            if (bg != null)
                Destroy(bg);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (oneShot && triggered)
                return;

            triggered = true;
            activeMessage = message;
            activeMessageUntil = Time.time + duration;
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(activeMessage) || Time.time > activeMessageUntil)
                return;

            EnsureStyles();

            float width = Mathf.Min(700f, Screen.width - 40f);
            Rect panel = new Rect((Screen.width - width) * 0.5f, 80f, width, 56f);
            GUI.DrawTexture(panel, bg);
            GUI.Label(panel, activeMessage, style);
        }

        private void EnsureStyles()
        {
            if (style != null)
                return;

            style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = new Color(0.96f, 0.92f, 0.78f, 1f);

            bg = new Texture2D(2, 2);
            Color tint = new Color(0.05f, 0.05f, 0.06f, 0.84f);
            bg.SetPixels(new[] { tint, tint, tint, tint });
            bg.Apply();
            bg.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
