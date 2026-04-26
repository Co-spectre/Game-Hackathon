using System.Collections;
using UnityEngine;

namespace NordicWilds.UI
{
    public class YamatoArrivalDialogue : MonoBehaviour
    {
        [SerializeField] private float startDelay = 0.75f;
        [SerializeField] private float charsPerSecond = 42f;

        // Arrival cutscene reel (img3 -> img4 -> img5 -> img6). Played once before the
        // dialogue panel appears. Indices 7-10 are reserved for these in the cutscene
        // list (the quest controller uses 0-6).
        private const int YamatoArrival1Index = 7;
        private const int YamatoArrival2Index = 8;
        private const int YamatoArrival3Index = 9;
        private const int YamatoArrival4Index = 10;

        private readonly string[] lines =
        {
            "Where... am I?",
            "I remember cold water, black sky, then petals. None of this should be here.",
            "This place is beautiful, but it does not feel real. Something is holding it together."
        };

        private Texture2D dialogueBoard;
        private Texture2D noticeTex;
        private Texture2D dividerTex;
        private Texture2D darkTex;
        private Texture2D portraitTex;       // Hades-style 2D portrait of the speaker
        private Texture2D portraitFrameTex;  // soft gold rim behind the portrait
        private GUIStyle nameStyle;
        private GUIStyle lineStyle;
        private GUIStyle skipStyle;
        private int lineIndex;
        private float visibleChars;
        private float delayTimer;
        private bool open;
        private float portraitBobTimer;

        private void Awake()
        {
            dialogueBoard = Resources.Load<Texture2D>("UI/FantasyWooden/dialogue_board");
            noticeTex = Resources.Load<Texture2D>("UI/FantasyWooden/notice");
            dividerTex = Resources.Load<Texture2D>("UI/FantasyWooden/divider");
            // Drop a portrait at: Assets/Resources/Characters/protagonist.png
            // (no extension in the load path).
            portraitTex = Resources.Load<Texture2D>("Characters/protagonist");
            portraitFrameTex = MakeTex(new Color(0.92f, 0.74f, 0.30f, 0.55f));
            darkTex = MakeTex(new Color(0.04f, 0.025f, 0.016f, 0.88f));
            delayTimer = startDelay;

            StartCoroutine(PlayArrivalReel());
        }

        private IEnumerator PlayArrivalReel()
        {
            CutsceneManager manager = CutsceneManager.GetOrCreate();
            if (manager == null)
                yield break;

            manager.EnsureCutsceneImage(YamatoArrival1Index, "Cutscenes/img3", 5f,
                "A New Shore",
                "Narrator:  The longship grinds against pale wood. The wanderer steps off into air that smells of cedar, salt, and something burning slow and sweet.");
            manager.EnsureCutsceneImage(YamatoArrival2Index, "Cutscenes/img4", 5f,
                "Lanterns and Stone",
                "Narrator:  Vermillion gates stand where Frostheim's pines should be. Lanterns swing without wind. Nothing here is hostile yet — only watching.");
            manager.EnsureCutsceneImage(YamatoArrival3Index, "Cutscenes/img5", 5f,
                "Yamato",
                "Narrator:  A village without a name still knows how to greet a stranger. Bowed heads. Quiet eyes. A bell in the distance, rung by no one.");
            manager.EnsureCutsceneImage(YamatoArrival4Index, "Cutscenes/img6", 5f,
                "The Sealed Path",
                "Narrator:  Somewhere past the shrine, a portal hums with the cold of home. To reach it, the wanderer will have to walk all the way through this strange, beautiful place.");

            // Hold the dialogue panel until the reel is done.
            open = false;
            delayTimer = 999f;

            yield return manager.PlayCutsceneAndWait(YamatoArrival1Index);
            yield return manager.PlayCutsceneAndWait(YamatoArrival2Index);
            yield return manager.PlayCutsceneAndWait(YamatoArrival3Index);
            yield return manager.PlayCutsceneAndWait(YamatoArrival4Index);

            delayTimer = 0.2f;
        }

        private void Update()
        {
            if (!open)
            {
                delayTimer -= Time.unscaledDeltaTime;
                if (delayTimer <= 0f)
                    open = true;
                return;
            }

            visibleChars += charsPerSecond * Time.unscaledDeltaTime;
            portraitBobTimer += Time.unscaledDeltaTime;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
            {
                AdvanceOrSkip();
            }
        }

        private void OnDestroy()
        {
            if (darkTex != null)
                Destroy(darkTex);
            if (portraitFrameTex != null)
                Destroy(portraitFrameTex);
        }

        private void OnGUI()
        {
            if (!open)
                return;

            EnsureStyles();

            float scale = Mathf.Clamp(Screen.height / 1080f, 0.78f, 1.05f);
            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

            float width = Mathf.Min(760f, (Screen.width / scale) - 64f);
            float height = 178f;
            // Hades-style portrait sits at the bottom-left corner; nudge the box right
            // so the portrait can tuck against it without overlapping.
            float portraitSize = 220f;
            float portraitPad  = 24f;
            float panelShift   = portraitTex != null ? (portraitSize * 0.45f) : 0f;
            float x = ((Screen.width / scale) - width) * 0.5f + panelShift;
            float y = (Screen.height / scale) - height - 36f;
            Rect panel = new Rect(x, y, width, height);

            // ── Portrait (small Hades-style frame) ────────────────────────────
            if (portraitTex != null)
            {
                float bob = Mathf.Sin(portraitBobTimer * 2.4f) * 3f;
                // Anchor portrait so its base aligns with the dialogue box base; lifts above it.
                float pX = x - portraitSize - portraitPad;
                if (pX < 24f) pX = 24f;
                float pY = y + height - portraitSize + 32f + bob;
                Rect portraitRect = new Rect(pX, pY, portraitSize, portraitSize);

                // Soft gold backing frame slightly larger than the portrait.
                Rect frameRect = new Rect(pX - 6f, pY - 6f, portraitSize + 12f, portraitSize + 12f);
                Color prev = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.55f);
                GUI.DrawTexture(frameRect, portraitFrameTex, ScaleMode.StretchToFill, true);
                GUI.color = prev;

                GUI.DrawTexture(portraitRect, portraitTex, ScaleMode.ScaleToFit, true);
            }

            if (dialogueBoard != null)
                GUI.DrawTexture(panel, dialogueBoard, ScaleMode.StretchToFill, true);
            else
                GUI.DrawTexture(panel, darkTex);

            Rect namePlate = new Rect(x + 34f, y + 18f, 188f, 34f);
            if (noticeTex != null)
                GUI.DrawTexture(namePlate, noticeTex, ScaleMode.StretchToFill, true);
            GUI.Label(namePlate, "The Wanderer", nameStyle);

            if (dividerTex != null)
                GUI.DrawTexture(new Rect(x + 36f, y + 58f, width - 72f, 4f), dividerTex, ScaleMode.StretchToFill, true);

            string line = lines[Mathf.Clamp(lineIndex, 0, lines.Length - 1)];
            int charCount = Mathf.Clamp(Mathf.FloorToInt(visibleChars), 0, line.Length);
            GUI.Label(new Rect(x + 48f, y + 72f, width - 96f, 64f), line.Substring(0, charCount), lineStyle);
            GUI.Label(new Rect(x + width - 194f, y + height - 44f, 150f, 26f), "Space / E to skip", skipStyle);

            GUI.color = oldColor;
            GUI.matrix = oldMatrix;
        }

        private void EnsureStyles()
        {
            if (lineStyle != null)
                return;

            nameStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            nameStyle.normal.textColor = new Color(0.96f, 0.86f, 0.62f, 0.95f);

            lineStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 21,
                fontStyle = FontStyle.Normal,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            lineStyle.normal.textColor = new Color(0.20f, 0.13f, 0.08f, 0.96f);

            skipStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleRight
            };
            skipStyle.normal.textColor = new Color(0.24f, 0.16f, 0.10f, 0.72f);
        }

        private void AdvanceOrSkip()
        {
            string line = lines[Mathf.Clamp(lineIndex, 0, lines.Length - 1)];
            if (visibleChars < line.Length)
            {
                visibleChars = line.Length;
                return;
            }

            lineIndex++;
            visibleChars = 0f;
            if (lineIndex >= lines.Length)
                Destroy(gameObject);
        }

        private Texture2D MakeTex(Color color)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.SetPixels(new[] { color, color, color, color });
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }
    }
}
