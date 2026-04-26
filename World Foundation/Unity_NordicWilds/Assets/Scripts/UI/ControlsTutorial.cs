using UnityEngine;
using NordicWilds.Player;

namespace NordicWilds.UI
{
    /// Brief onboarding overlay that teaches the four core controls.
    /// Shows a top-center card at game start with all four prompts. Each prompt
    /// gets a checkmark + fade as the player uses it. After ~5 seconds the card
    /// shrinks to a small bottom-corner reminder for whatever's still unused.
    public class ControlsTutorial : MonoBehaviour
    {
        public static ControlsTutorial Instance { get; private set; }

        [Header("Timing")]
        public float introDuration = 6f;
        public float fadeInDuration = 0.55f;
        public float reminderHoldDuration = 12f; // how long mini-reminders linger

        private bool moved;
        private bool sprinted;
        private bool dashed;
        private bool attacked;

        private float startTime;
        private float reminderShownAt;

        private GUIStyle titleStyle;
        private GUIStyle keyStyle;
        private GUIStyle labelStyle;
        private GUIStyle checkStyle;
        private GUIStyle hintStyle;
        private Texture2D bgTex;
        private Texture2D keyCapTex;
        private Texture2D keyCapDoneTex;

        public static ControlsTutorial GetOrCreate()
        {
            if (Instance != null) return Instance;
            Instance = FindFirstObjectByType<ControlsTutorial>();
            if (Instance != null) return Instance;
            GameObject obj = new GameObject("ControlsTutorial");
            Instance = obj.AddComponent<ControlsTutorial>();
            return Instance;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            startTime = Time.unscaledTime;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (bgTex != null) Destroy(bgTex);
            if (keyCapTex != null) Destroy(keyCapTex);
            if (keyCapDoneTex != null) Destroy(keyCapDoneTex);
        }

        private void Update()
        {
            // Detect actual input usage so checkmarks reflect real progress.
            if (!moved && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                           Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
            {
                moved = true;
                reminderShownAt = Time.unscaledTime;
            }
            if (!sprinted && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)))
            {
                sprinted = true;
                reminderShownAt = Time.unscaledTime;
            }
            if (!dashed && Input.GetKeyDown(KeyCode.Space))
            {
                dashed = true;
                reminderShownAt = Time.unscaledTime;
            }
            if (!attacked && Input.GetMouseButtonDown(0))
            {
                attacked = true;
                reminderShownAt = Time.unscaledTime;
            }
        }

        /// Force-show a reminder for a specific control (called by quest beats).
        public void NudgeReminder()
        {
            reminderShownAt = Time.unscaledTime;
        }

        public bool AllLearned => moved && sprinted && dashed && attacked;

        private void OnGUI()
        {
            EnsureStyles();

            float elapsed = Time.unscaledTime - startTime;
            bool intro = elapsed < introDuration;

            if (intro)
                DrawIntroCard(elapsed);
            else if (!AllLearned && Time.unscaledTime - reminderShownAt < reminderHoldDuration)
                DrawMiniReminder();
        }

        private void DrawIntroCard(float elapsed)
        {
            float fadeIn = Mathf.Clamp01(elapsed / fadeInDuration);
            float fadeOut = Mathf.Clamp01((introDuration - elapsed) / 0.6f);
            float alpha = fadeIn * fadeOut;

            float w = 720f;
            float h = 132f;
            float x = (Screen.width - w) * 0.5f;
            float y = 92f; // sits below the MissionTracker banner

            Color prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTexture(new Rect(x, y, w, h), bgTex);

            GUI.Label(new Rect(x, y + 8f, w, 22f), "How to Play", titleStyle);

            float colW = w / 4f;
            DrawControl(x + colW * 0f, y + 36f, colW, "WASD",      "Move",   moved);
            DrawControl(x + colW * 1f, y + 36f, colW, "SHIFT",     "Sprint", sprinted);
            DrawControl(x + colW * 2f, y + 36f, colW, "SPACE",     "Dash",   dashed);
            DrawControl(x + colW * 3f, y + 36f, colW, "LMB",       "Attack", attacked);

            GUI.color = prev;
        }

        private void DrawControl(float x, float y, float w, string key, string label, bool done)
        {
            float cap = 56f;
            float capX = x + (w - cap) * 0.5f;
            GUI.DrawTexture(new Rect(capX, y, cap, 38f), done ? keyCapDoneTex : keyCapTex);
            GUI.Label(new Rect(capX, y + 4f, cap, 30f), key, keyStyle);
            GUI.Label(new Rect(x, y + 42f, w, 22f), label, labelStyle);
            if (done)
                GUI.Label(new Rect(x + w - 22f, y - 4f, 20f, 20f), "✓", checkStyle);
        }

        private void DrawMiniReminder()
        {
            // Compact bottom-right card with just the unmastered controls.
            string remaining = "";
            if (!moved)    remaining += "WASD move   ";
            if (!sprinted) remaining += "SHIFT sprint   ";
            if (!dashed)   remaining += "SPACE dash   ";
            if (!attacked) remaining += "LMB attack";
            if (string.IsNullOrWhiteSpace(remaining)) return;

            float w = 360f;
            float h = 38f;
            float x = Screen.width - w - 24f;
            float y = Screen.height - h - 168f; // above the HUD bar
            GUI.DrawTexture(new Rect(x, y, w, h), bgTex);
            GUI.Label(new Rect(x + 12f, y + 8f, w - 24f, 24f), "Try: " + remaining, hintStyle);
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;

            bgTex = MakeTex(new Color(0.04f, 0.03f, 0.03f, 0.82f));
            keyCapTex = MakeTex(new Color(0.16f, 0.13f, 0.10f, 0.95f));
            keyCapDoneTex = MakeTex(new Color(0.18f, 0.32f, 0.18f, 0.95f));

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = new Color(0.96f, 0.82f, 0.46f, 1f);

            keyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            keyStyle.normal.textColor = new Color(0.98f, 0.96f, 0.90f, 1f);

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            labelStyle.normal.textColor = new Color(0.88f, 0.84f, 0.74f, 1f);

            checkStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            checkStyle.normal.textColor = new Color(0.62f, 1f, 0.62f, 1f);

            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft
            };
            hintStyle.normal.textColor = new Color(0.96f, 0.86f, 0.52f, 1f);
        }

        private static Texture2D MakeTex(Color c)
        {
            Texture2D t = new Texture2D(2, 2);
            t.SetPixels(new[] { c, c, c, c });
            t.Apply();
            t.hideFlags = HideFlags.HideAndDontSave;
            return t;
        }
    }
}
