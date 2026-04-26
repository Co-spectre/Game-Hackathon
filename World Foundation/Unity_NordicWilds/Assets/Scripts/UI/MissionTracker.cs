using UnityEngine;

namespace NordicWilds.UI
{
    /// Single global objective banner pinned to the top of the screen.
    /// Quest scripts call MissionTracker.Set(...) at major beats so the player
    /// always knows what they are supposed to be doing.
    public class MissionTracker : MonoBehaviour
    {
        public static MissionTracker Instance { get; private set; }

        public string chapter = "Chapter 1: The Drowned Shore";
        public string objective = "Wake up. Find your bearings.";

        private GUIStyle chapterStyle;
        private GUIStyle objectiveStyle;
        private Texture2D bgTex;

        public static MissionTracker GetOrCreate()
        {
            if (Instance != null) return Instance;
            Instance = FindFirstObjectByType<MissionTracker>();
            if (Instance != null) return Instance;
            GameObject obj = new GameObject("MissionTracker");
            Instance = obj.AddComponent<MissionTracker>();
            return Instance;
        }

        public static void Set(string chapter, string objective)
        {
            MissionTracker tracker = GetOrCreate();
            tracker.chapter = chapter;
            tracker.objective = objective;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (bgTex != null) Destroy(bgTex);
        }

        private void OnGUI()
        {
            EnsureStyles();

            float w = 560f;
            float h = 64f;
            float x = (Screen.width - w) * 0.5f;
            float y = 14f;
            GUI.DrawTexture(new Rect(x, y, w, h), bgTex);
            GUI.Label(new Rect(x + 16f, y + 6f, w - 32f, 24f), chapter, chapterStyle);
            GUI.Label(new Rect(x + 16f, y + 30f, w - 32f, 28f), objective, objectiveStyle);
        }

        private void EnsureStyles()
        {
            if (objectiveStyle != null) return;

            bgTex = new Texture2D(2, 2);
            Color bg = new Color(0.04f, 0.03f, 0.03f, 0.78f);
            bgTex.SetPixels(new[] { bg, bg, bg, bg });
            bgTex.Apply();
            bgTex.hideFlags = HideFlags.HideAndDontSave;

            chapterStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            chapterStyle.normal.textColor = new Color(0.96f, 0.82f, 0.46f, 1f);

            objectiveStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true
            };
            objectiveStyle.normal.textColor = new Color(0.95f, 0.93f, 0.88f, 1f);
        }
    }
}
