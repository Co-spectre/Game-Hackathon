using UnityEngine;
using NordicWilds.Player;

namespace NordicWilds.UI
{
    public class WorldMapOverlay : MonoBehaviour
    {
        private const float YamatoOrigin = 10000f;
        private const float YamatoMinZ = -108f;
        private const float YamatoMaxZ = 88f;

        [SerializeField] private KeyCode toggleKey = KeyCode.F;

        private Texture2D parchmentTex;
        private Texture2D noticeTex;
        private Texture2D dividerTex;
        private Texture2D yamatoMapTex;
        private Texture2D frostheimMapTex;
        private Texture2D worldMapTex;
        private Texture2D darkTex;
        private Texture2D redTex;
        private Texture2D goldTex;
        private Texture2D blueTex;
        private Texture2D greenTex;
        private Texture2D blackTex;
        private Texture2D markerTex;
        private Texture2D yamatoGroundTex;
        private Texture2D frostGroundTex;
        private Texture2D waterLightTex;

        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle hintStyle;
        private PlayerController player;
        private bool isOpen;

        private void Awake()
        {
            parchmentTex = Resources.Load<Texture2D>("UI/FantasyWooden/dialogue_board")
                ?? Resources.Load<Texture2D>("UI/FantasyWooden/hud_board");
            noticeTex = Resources.Load<Texture2D>("UI/FantasyWooden/notice");
            dividerTex = Resources.Load<Texture2D>("UI/FantasyWooden/divider");
            yamatoMapTex = Resources.Load<Texture2D>("UI/Map/yamato_map");
            frostheimMapTex = Resources.Load<Texture2D>("UI/Map/frostheim_map");
            worldMapTex = Resources.Load<Texture2D>("UI/Map/world_map");

            darkTex = MakeTex(new Color(0.055f, 0.036f, 0.020f, 0.88f));
            redTex = MakeTex(new Color(0.72f, 0.10f, 0.10f, 0.90f));
            goldTex = MakeTex(new Color(0.94f, 0.68f, 0.26f, 0.92f));
            blueTex = MakeTex(new Color(0.18f, 0.46f, 0.72f, 0.90f));
            greenTex = MakeTex(new Color(0.20f, 0.44f, 0.20f, 0.88f));
            blackTex = MakeTex(new Color(0.08f, 0.055f, 0.035f, 0.92f));
            markerTex = MakeTex(new Color(1f, 0.92f, 0.35f, 1f));
            yamatoGroundTex = MakeTex(new Color(0.73f, 0.58f, 0.38f, 0.80f));
            frostGroundTex = MakeTex(new Color(0.48f, 0.56f, 0.62f, 0.82f));
            waterLightTex = MakeTex(new Color(0.34f, 0.66f, 0.82f, 0.82f));
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                isOpen = !isOpen;
                if (isOpen)
                    RefreshReferences();
            }

            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
                isOpen = false;
        }

        private void OnDestroy()
        {
            DestroyGenerated(darkTex);
            DestroyGenerated(redTex);
            DestroyGenerated(goldTex);
            DestroyGenerated(blueTex);
            DestroyGenerated(greenTex);
            DestroyGenerated(blackTex);
            DestroyGenerated(markerTex);
            DestroyGenerated(yamatoGroundTex);
            DestroyGenerated(frostGroundTex);
            DestroyGenerated(waterLightTex);
        }

        private void OnGUI()
        {
            if (!isOpen)
                return;

            EnsureStyles();

            float scale = Mathf.Clamp(Screen.height / 1080f, 0.78f, 1.04f);
            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

            float screenW = Screen.width / scale;
            float screenH = Screen.height / scale;
            Rect dim = new Rect(0f, 0f, screenW, screenH);
            GUI.color = new Color(0f, 0f, 0f, 0.42f);
            GUI.DrawTexture(dim, darkTex);
            GUI.color = Color.white;

            Rect panel = new Rect((screenW - 760f) * 0.5f, (screenH - 580f) * 0.5f, 760f, 580f);
            DrawPanel(panel);

            bool yamato = IsInYamato();
            string title = yamato ? "Yamato Garden Map" : "Frostheim Map";
            DrawHeader(panel, title);

            Rect mapRect = new Rect(panel.x + 56f, panel.y + 94f, panel.width - 112f, panel.height - 166f);
            Texture2D suppliedMap = yamato ? yamatoMapTex : frostheimMapTex;
            if (suppliedMap == null)
                suppliedMap = worldMapTex;

            if (suppliedMap != null)
                DrawSuppliedMap(mapRect, suppliedMap, yamato);
            else if (yamato)
                DrawYamatoMap(mapRect);
            else
                DrawFrostheimMap(mapRect);

            DrawPlayerMarker(mapRect, yamato);
            GUI.Label(new Rect(panel.x + 58f, panel.y + panel.height - 56f, panel.width - 116f, 28f),
                "F toggles map  |  Escape closes", hintStyle);

            GUI.color = oldColor;
            GUI.matrix = oldMatrix;
        }

        private void RefreshReferences()
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            player = playerObj != null ? playerObj.GetComponent<PlayerController>() : null;
        }

        private bool IsInYamato()
        {
            Transform t = player != null ? player.transform : GameObject.FindWithTag("Player")?.transform;
            if (t == null)
                return true;

            return t.position.x > YamatoOrigin * 0.5f || t.position.z > YamatoOrigin * 0.5f;
        }

        private void DrawPanel(Rect panel)
        {
            if (parchmentTex != null)
                GUI.DrawTexture(panel, parchmentTex, ScaleMode.StretchToFill, true);
            else
                GUI.DrawTexture(panel, darkTex);
        }

        private void DrawHeader(Rect panel, string title)
        {
            Rect plate = new Rect(panel.x + 236f, panel.y + 28f, 288f, 42f);
            if (noticeTex != null)
                GUI.DrawTexture(plate, noticeTex, ScaleMode.StretchToFill, true);
            else
                GUI.DrawTexture(plate, blackTex);

            GUI.Label(plate, title, titleStyle);

            if (dividerTex != null)
                GUI.DrawTexture(new Rect(panel.x + 70f, panel.y + 78f, panel.width - 140f, 4f), dividerTex, ScaleMode.StretchToFill, true);
        }

        private void DrawSuppliedMap(Rect mapRect, Texture2D map, bool yamato)
        {
            GUI.DrawTexture(mapRect, blackTex);
            Rect inner = new Rect(mapRect.x + 8f, mapRect.y + 8f, mapRect.width - 16f, mapRect.height - 16f);
            GUI.DrawTexture(inner, map, ScaleMode.ScaleToFit, true);
            DrawMapBorder(mapRect, yamato);
        }

        private void DrawYamatoMap(Rect r)
        {
            GUI.DrawTexture(r, yamatoGroundTex);
            DrawMapBorder(r, true);

            Rect water = new Rect(r.x + 12f, r.y + r.height - 92f, r.width - 24f, 76f);
            DrawPond(water);
            GUI.DrawTexture(new Rect(water.x, water.y, water.width, 5f), waterLightTex);

            DrawBlob(new Rect(r.x + 38f, r.y + 38f, 114f, 78f), greenTex);
            DrawBlob(new Rect(r.x + r.width - 154f, r.y + 50f, 106f, 72f), greenTex);
            DrawBlob(new Rect(r.x + 62f, r.y + 246f, 126f, 70f), greenTex);

            DrawLine(new Vector2(r.center.x, r.y + r.height - 112f), new Vector2(r.center.x, r.y + 52f), 9f, goldTex);
            DrawLine(new Vector2(r.center.x - 8f, r.y + 234f), new Vector2(r.x + 132f, r.y + 292f), 6f, goldTex);
            DrawLine(new Vector2(r.center.x + 8f, r.y + 246f), new Vector2(r.x + r.width - 132f, r.y + 302f), 6f, goldTex);

            Rect dock = new Rect(r.center.x - 16f, r.y + r.height - 142f, 32f, 74f);
            GUI.DrawTexture(dock, blackTex);
            for (int i = 0; i < 5; i++)
                GUI.DrawTexture(new Rect(dock.x - 8f, dock.y + 8f + i * 12f, dock.width + 16f, 4f), goldTex);
            DrawBoat(new Rect(r.center.x - 108f, r.y + r.height - 116f, 72f, 32f));

            DrawShrine(new Rect(r.center.x - 74f, r.y + 46f, 148f, 82f));
            DrawTorii(new Rect(r.center.x - 45f, r.y + 140f, 90f, 72f));
            DrawTorii(new Rect(r.center.x - 43f, r.y + 220f, 86f, 68f));
            DrawPond(new Rect(r.x + 72f, r.y + 242f, 142f, 86f));
            DrawPagoda(new Rect(r.x + r.width - 172f, r.y + 128f, 98f, 142f));
            DrawTorii(new Rect(r.x + r.width - 186f, r.y + 286f, 74f, 58f));
            DrawLabel(r.center.x - 50f, r.y + 18f, "Shrine");
            DrawLabel(r.center.x - 45f, r.y + 306f, "Torii Path");
            DrawLabel(r.x + 86f, r.y + 334f, "Koi Pond");
            DrawLabel(r.x + r.width - 172f, r.y + 276f, "Pagoda");
            DrawLabel(r.center.x - 34f, r.y + r.height - 162f, "Dock");
            DrawLabel(r.x + r.width - 194f, r.y + 348f, "Return Gate");
        }

        private void DrawFrostheimMap(Rect r)
        {
            GUI.DrawTexture(r, frostGroundTex);
            DrawMapBorder(r, false);

            DrawBlob(new Rect(r.x + 28f, r.y + 28f, 170f, 120f), blackTex);
            DrawBlob(new Rect(r.x + r.width - 212f, r.y + 30f, 184f, 136f), blackTex);
            DrawBlob(new Rect(r.x + 48f, r.y + r.height - 142f, 190f, 102f), blackTex);

            DrawLine(new Vector2(r.center.x, r.y + r.height - 34f), new Vector2(r.center.x - 34f, r.y + 64f), 10f, goldTex);
            DrawLine(new Vector2(r.center.x - 10f, r.y + 200f), new Vector2(r.x + 126f, r.y + 254f), 6f, goldTex);
            DrawLine(new Vector2(r.center.x + 8f, r.y + 184f), new Vector2(r.x + r.width - 130f, r.y + 238f), 6f, goldTex);

            DrawPond(new Rect(r.x + r.width - 206f, r.y + 226f, 156f, 92f));
            DrawBlockIcon(new Rect(r.center.x - 70f, r.y + 76f, 140f, 78f), blackTex, "Hall");
            DrawBlockIcon(new Rect(r.x + 88f, r.y + 220f, 112f, 70f), blackTex, "Forge");
            DrawBlockIcon(new Rect(r.center.x - 46f, r.y + r.height - 118f, 92f, 64f), redTex, "Gate");
            DrawLabel(r.x + r.width - 186f, r.y + 320f, "Fjord");
        }

        private void DrawMapBorder(Rect r, bool yamato)
        {
            Texture2D border = yamato ? redTex : blackTex;
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, 5f), border);
            GUI.DrawTexture(new Rect(r.x, r.yMax - 5f, r.width, 5f), border);
            GUI.DrawTexture(new Rect(r.x, r.y, 5f, r.height), border);
            GUI.DrawTexture(new Rect(r.xMax - 5f, r.y, 5f, r.height), border);
        }

        private void DrawPlayerMarker(Rect r, bool yamato)
        {
            Transform t = player != null ? player.transform : GameObject.FindWithTag("Player")?.transform;
            Vector2 normalized = new Vector2(0.5f, 0.5f);
            if (t != null)
            {
                Vector3 p = t.position;
                float localX = yamato ? p.x - YamatoOrigin : p.x;
                float localZ = yamato ? p.z - YamatoOrigin : p.z;
                normalized.x = Mathf.InverseLerp(-95f, 95f, localX);
                normalized.y = 1f - Mathf.InverseLerp(yamato ? YamatoMinZ : -88f, yamato ? YamatoMaxZ : 88f, localZ);
            }

            Rect marker = new Rect(r.x + normalized.x * r.width - 8f, r.y + normalized.y * r.height - 8f, 16f, 16f);
            GUI.DrawTexture(new Rect(marker.x - 4f, marker.y - 4f, 24f, 24f), blackTex);
            GUI.DrawTexture(marker, markerTex);
        }

        private void DrawTorii(Rect r)
        {
            GUI.DrawTexture(new Rect(r.x + 12f, r.y + 14f, 10f, r.height - 16f), redTex);
            GUI.DrawTexture(new Rect(r.x + r.width - 22f, r.y + 14f, 10f, r.height - 16f), redTex);
            GUI.DrawTexture(new Rect(r.x, r.y + 4f, r.width, 10f), redTex);
            GUI.DrawTexture(new Rect(r.x + 8f, r.y + 20f, r.width - 16f, 7f), redTex);
        }

        private void DrawShrine(Rect r)
        {
            GUI.DrawTexture(new Rect(r.x + 20f, r.y + 32f, r.width - 40f, r.height - 36f), redTex);
            GUI.DrawTexture(new Rect(r.x, r.y + 18f, r.width, 15f), blackTex);
            GUI.DrawTexture(new Rect(r.x + 12f, r.y + 8f, r.width - 24f, 12f), blackTex);
            GUI.DrawTexture(new Rect(r.x + 42f, r.y + 48f, r.width - 84f, r.height - 52f), goldTex);
        }

        private void DrawPagoda(Rect r)
        {
            for (int i = 0; i < 4; i++)
            {
                float y = r.y + i * 30f;
                float inset = i * 9f;
                GUI.DrawTexture(new Rect(r.x + inset, y, r.width - inset * 2f, 8f), blackTex);
                GUI.DrawTexture(new Rect(r.x + 20f + inset, y + 8f, r.width - 40f - inset * 2f, 20f), redTex);
            }
        }

        private void DrawPond(Rect r)
        {
            GUI.DrawTexture(r, blueTex);
            GUI.DrawTexture(new Rect(r.x + 8f, r.y + 8f, r.width - 16f, r.height - 16f), waterLightTex);
        }

        private void DrawBoat(Rect r)
        {
            GUI.DrawTexture(new Rect(r.x + 8f, r.y + 7f, r.width - 16f, r.height - 14f), blackTex);
            GUI.DrawTexture(new Rect(r.x + 18f, r.y + 2f, r.width - 36f, 6f), goldTex);
            GUI.DrawTexture(new Rect(r.x + 18f, r.y + r.height - 8f, r.width - 36f, 6f), goldTex);
            GUI.DrawTexture(new Rect(r.center.x - 3f, r.y, 6f, r.height), redTex);
        }

        private void DrawBlockIcon(Rect r, Texture2D texture, string label)
        {
            GUI.DrawTexture(r, texture);
            GUI.Label(r, label, labelStyle);
        }

        private void DrawLabel(float x, float y, string text)
        {
            GUI.Label(new Rect(x, y, 120f, 22f), text, labelStyle);
        }

        private void DrawBlob(Rect r, Texture2D texture)
        {
            GUI.DrawTexture(r, texture);
        }

        private void DrawLine(Vector2 a, Vector2 b, float width, Texture2D texture)
        {
            Matrix4x4 oldMatrix = GUI.matrix;
            float angle = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
            float length = Vector2.Distance(a, b);
            GUIUtility.RotateAroundPivot(angle, a);
            GUI.DrawTexture(new Rect(a.x, a.y - width * 0.5f, length, width), texture);
            GUI.matrix = oldMatrix;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null)
                return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = new Color(0.98f, 0.86f, 0.56f, 0.98f);

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            labelStyle.normal.textColor = new Color(0.16f, 0.10f, 0.06f, 0.96f);

            hintStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            hintStyle.normal.textColor = new Color(0.20f, 0.13f, 0.08f, 0.80f);
        }

        private Texture2D MakeTex(Color color)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.SetPixels(new[] { color, color, color, color });
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }

        private void DestroyGenerated(Texture2D texture)
        {
            if (texture != null)
                Destroy(texture);
        }
    }
}
