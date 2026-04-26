using UnityEngine;

namespace NordicWilds.UI
{
    /// Drop this on a trigger collider near the great hall / mead hall. While the
    /// player is inside the trigger, pressing E cycles through tavern-patron lines.
    [RequireComponent(typeof(Collider))]
    public class TavernDialogue : MonoBehaviour
    {
        [System.Serializable]
        public struct Line
        {
            public string speaker;
            [TextArea(2, 4)] public string text;
        }

        public Line[] lines = new Line[]
        {
            new Line { speaker = "Old Skald", text = "Sit, stranger. The mead is warm and the songs are older than the stones." },
            new Line { speaker = "Old Skald", text = "There was a fire in the eastern sea. They say a longship sailed through it and was never seen again." },
            new Line { speaker = "Skadi the Forge-Hand", text = "If you find Leaf in the southern forest, tell her her brother still keeps her seat by the hearth." },
            new Line { speaker = "Skadi the Forge-Hand", text = "Iron we have. Faith we are running out of." },
            new Line { speaker = "Bjorn the One-Eyed", text = "The portal at the shrine? I would not stand near it after sundown. It hums when no one is watching." },
            new Line { speaker = "Bjorn the One-Eyed", text = "Three relics, the runes say. Find them and the world bends. I have not been brave enough to test it." },
            new Line { speaker = "A Quiet Stranger", text = "You have the look of someone who has crossed water that is not on any map." },
            new Line { speaker = "A Quiet Stranger", text = "There are two shores in this world, and only one of them remembers you." },
            new Line { speaker = "Innkeeper Ulfa", text = "Drink, eat, sleep if you must — but do not stay so long that the raiders find this hall before you do." },
            new Line { speaker = "Innkeeper Ulfa", text = "If you make it back from the other shore, the first horn of mead is on the house. The second you pay double." }
        };

        [Header("Tuning")]
        public float panelWidth = 720f;
        public float panelHeight = 168f;
        public KeyCode advanceKey = KeyCode.E;

        private bool playerNearby;
        private int index = -1;       // -1 means panel hidden, 0+ means showing that line
        private GUIStyle nameStyle;
        private GUIStyle lineStyle;
        private GUIStyle hintStyle;
        private Texture2D bgTex;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                playerNearby = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerNearby = false;
                index = -1;
            }
        }

        private void Update()
        {
            if (!playerNearby) return;
            if (Input.GetKeyDown(advanceKey))
            {
                if (lines == null || lines.Length == 0) return;
                index = (index + 1) % lines.Length;
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            if (playerNearby && index < 0)
            {
                GUI.Label(new Rect((Screen.width - 480f) * 0.5f, Screen.height - 96f, 480f, 28f),
                    "Press E to listen to the tavern", hintStyle);
                return;
            }

            if (index < 0 || lines == null || lines.Length == 0) return;

            Line line = lines[Mathf.Clamp(index, 0, lines.Length - 1)];

            float x = (Screen.width - panelWidth) * 0.5f;
            float y = Screen.height - panelHeight - 36f;
            Rect panel = new Rect(x, y, panelWidth, panelHeight);
            GUI.DrawTexture(panel, bgTex);

            GUI.Label(new Rect(x + 24f, y + 14f, panelWidth - 48f, 30f), line.speaker, nameStyle);
            GUI.Label(new Rect(x + 24f, y + 50f, panelWidth - 48f, panelHeight - 70f), line.text, lineStyle);
            GUI.Label(new Rect(x + panelWidth - 220f, y + panelHeight - 28f, 200f, 22f),
                "Press E for next  (" + (index + 1) + "/" + lines.Length + ")", hintStyle);
        }

        private void EnsureStyles()
        {
            if (lineStyle != null) return;

            bgTex = new Texture2D(2, 2);
            Color bg = new Color(0.05f, 0.04f, 0.03f, 0.9f);
            bgTex.SetPixels(new[] { bg, bg, bg, bg });
            bgTex.Apply();
            bgTex.hideFlags = HideFlags.HideAndDontSave;

            nameStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            nameStyle.normal.textColor = new Color(0.96f, 0.82f, 0.46f, 1f);

            lineStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            lineStyle.normal.textColor = new Color(0.94f, 0.92f, 0.88f, 1f);

            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            hintStyle.normal.textColor = new Color(0.85f, 0.78f, 0.62f, 0.85f);
        }

        private void OnDestroy()
        {
            if (bgTex != null) Destroy(bgTex);
        }
    }
}
