using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NordicWilds.UI
{
    public class CutsceneManager : MonoBehaviour
    {
        public static CutsceneManager Instance;

        [System.Serializable]
        public struct CutsceneData
        {
            public string sceneName;
            public Sprite image;
            [TextArea(3, 5)]
            public string dialogueText;
            public float duration;
        }

        [Header("Cutscene Data")]
        public List<CutsceneData> cutscenes = new List<CutsceneData>();

        [Header("UI References")]
        public GameObject cutsceneOverlay;
        public Image displayImage;
        public Image titleCardPanel;
        public TextMeshProUGUI displayTitle;
        public TextMeshProUGUI displayText;

        [Header("Title Card Box")]
        public Color titleCardBackgroundColor = new Color(0.06f, 0.05f, 0.05f, 0.86f);
        public Color titleCardBorderColor = new Color(0.93f, 0.78f, 0.43f, 0.95f);

        [Header("Ken Burns")]
        public bool useKenBurns = true;
        public float zoomStart = 1.0f;
        public float zoomEnd = 1.08f;

        private Coroutine activeCutsceneRoutine;
        private RectTransform imageRect;
        private readonly List<Sprite> runtimeCreatedSprites = new List<Sprite>();
        
        [Header("Skip Settings")]
        public float skipHoldTime = 1.0f;
        private float currentSkipHold = 0f;
        private Image skipProgressBar;

        public bool IsPlaying { get; private set; }

        public static CutsceneManager GetOrCreate()
        {
            if (Instance != null)
                return Instance;

            Instance = FindFirstObjectByType<CutsceneManager>();
            if (Instance != null)
                return Instance;

            GameObject root = new GameObject("CutsceneManager");
            Instance = root.AddComponent<CutsceneManager>();
            return Instance;
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            EnsureRuntimeUi();

            if (displayImage != null)
                imageRect = displayImage.rectTransform;

            if (cutsceneOverlay != null)
                cutsceneOverlay.SetActive(false);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < runtimeCreatedSprites.Count; i++)
            {
                if (runtimeCreatedSprites[i] != null)
                    Destroy(runtimeCreatedSprites[i]);
            }
            runtimeCreatedSprites.Clear();
        }

        public void EnsureStartGameCutscene(string resourcesSpritePath, float duration, string text = "")
        {
            EnsureRuntimeUi();

            if (cutscenes == null)
                cutscenes = new List<CutsceneData>();

            while (cutscenes.Count <= 0)
                cutscenes.Add(new CutsceneData());

            CutsceneData entry = cutscenes[0];
            if (entry.image == null)
                entry.image = LoadSpriteOrTextureAsSprite(resourcesSpritePath);

            if (string.IsNullOrWhiteSpace(entry.sceneName))
                entry.sceneName = string.Empty; // No default text like "Start Game"

            entry.dialogueText = text ?? string.Empty;
            entry.duration = Mathf.Max(0.05f, duration);
            cutscenes[0] = entry;
        }

        public void EnsureCutsceneImage(int index, string resourcesSpritePath, float duration, string sceneName = "", string text = "")
        {
            if (index < 0)
                return;

            EnsureRuntimeUi();

            if (cutscenes == null)
                cutscenes = new List<CutsceneData>();

            while (cutscenes.Count <= index)
                cutscenes.Add(new CutsceneData());

            CutsceneData entry = cutscenes[index];
            if (entry.image == null)
                entry.image = LoadSpriteOrTextureAsSprite(resourcesSpritePath);

            if (!string.IsNullOrWhiteSpace(sceneName))
                entry.sceneName = sceneName;

            entry.dialogueText = text ?? string.Empty;
            entry.duration = Mathf.Max(0.05f, duration);
            cutscenes[index] = entry;
        }

        private Sprite LoadSpriteOrTextureAsSprite(string resourcesPath)
        {
            Sprite sprite = Resources.Load<Sprite>(resourcesPath);
            if (sprite != null)
                return sprite;

            Texture2D tex = Resources.Load<Texture2D>(resourcesPath);
            if (tex == null)
                return null;

            Sprite generated = Sprite.Create(
                tex,
                new Rect(0f, 0f, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect
            );

            if (generated != null)
                runtimeCreatedSprites.Add(generated);

            return generated;
        }

        public bool HasCutscene(int index)
        {
            return index >= 0 && index < cutscenes.Count;
        }

        public void PlayCutscene(int index)
        {
            if (!HasCutscene(index))
                return;

            if (activeCutsceneRoutine != null)
                StopCoroutine(activeCutsceneRoutine);

            activeCutsceneRoutine = StartCoroutine(ExecuteCutscene(cutscenes[index]));
        }

        public IEnumerator PlayCutsceneAndWait(int index)
        {
            if (!HasCutscene(index))
                yield break;

            if (activeCutsceneRoutine != null)
                StopCoroutine(activeCutsceneRoutine);

            yield return ExecuteCutscene(cutscenes[index]);
        }

        private IEnumerator ExecuteCutscene(CutsceneData data)
        {
            EnsureRuntimeUi();

            IsPlaying = true;

            if (cutsceneOverlay != null)
                cutsceneOverlay.SetActive(true);

            if (displayImage != null)
                displayImage.sprite = data.image;

            if (displayText != null)
                displayText.text = data.dialogueText;

            bool hasBody = displayText != null && !string.IsNullOrWhiteSpace(displayText.text);

            if (displayTitle != null)
            {
                bool hasTitle = !string.IsNullOrWhiteSpace(data.sceneName);
                displayTitle.text = hasTitle ? data.sceneName : string.Empty;
                displayTitle.gameObject.SetActive(hasTitle);

                if (titleCardPanel != null)
                    titleCardPanel.gameObject.SetActive(hasTitle || hasBody);
            }
            else if (titleCardPanel != null)
            {
                titleCardPanel.gameObject.SetActive(hasBody);
            }

            if (imageRect != null)
                imageRect.localScale = Vector3.one * zoomStart;

            float duration = Mathf.Max(0.05f, data.duration);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smooth = t * t * (3f - 2f * t);

                if (useKenBurns && imageRect != null)
                    imageRect.localScale = Vector3.one * Mathf.Lerp(zoomStart, zoomEnd, smooth);

                // Skip Logic
                if (Input.GetKey(KeyCode.Space))
                {
                    currentSkipHold += Time.unscaledDeltaTime;
                    if (currentSkipHold >= skipHoldTime)
                    {
                        elapsed = duration; // Skip to end
                    }
                }
                else
                {
                    currentSkipHold = Mathf.MoveTowards(currentSkipHold, 0f, Time.unscaledDeltaTime * 2f);
                }

                if (skipProgressBar != null)
                {
                    skipProgressBar.fillAmount = currentSkipHold / skipHoldTime;
                    skipProgressBar.transform.parent.gameObject.SetActive(currentSkipHold > 0.01f);
                }

                yield return null;
            }

            currentSkipHold = 0f;
            if (skipProgressBar != null) skipProgressBar.transform.parent.gameObject.SetActive(false);

            if (cutsceneOverlay != null)
                cutsceneOverlay.SetActive(false);

            if (imageRect != null)
                imageRect.localScale = Vector3.one;

            IsPlaying = false;
            activeCutsceneRoutine = null;
        }

        private void EnsureRuntimeUi()
        {
            if (cutsceneOverlay != null && displayImage != null && displayText != null)
            {
                EnsureTitleCardBoxVisuals();
                return;
            }

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("CutsceneCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            if (cutsceneOverlay == null)
            {
                GameObject overlay = new GameObject("CutsceneOverlay");
                overlay.transform.SetParent(canvas.transform, false);
                RectTransform overlayRect = overlay.AddComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;

                Image bg = overlay.AddComponent<Image>();
                bg.color = Color.black;
                cutsceneOverlay = overlay;
            }

            if (displayImage == null)
            {
                GameObject imageObj = new GameObject("CutsceneImage");
                imageObj.transform.SetParent(cutsceneOverlay.transform, false);
                RectTransform rect = imageObj.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                displayImage = imageObj.AddComponent<Image>();
                displayImage.preserveAspect = false;
                imageRect = rect;
            }

            if (displayText == null)
            {
                GameObject textObj = new GameObject("CutsceneText");
                textObj.transform.SetParent(cutsceneOverlay.transform, false);
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0.08f, 0.04f);
                textRect.anchorMax = new Vector2(0.92f, 0.14f);
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                displayText = textObj.AddComponent<TextMeshProUGUI>();
                displayText.alignment = TextAlignmentOptions.Center;
                displayText.fontSize = 36f;
                displayText.color = Color.white;
            }

            if (displayImage != null && imageRect == null)
                imageRect = displayImage.rectTransform;

            if (displayImage != null)
            {
                RectTransform rect = displayImage.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                displayImage.type = Image.Type.Simple;
                displayImage.preserveAspect = false;
            }

            EnsureTitleCardBoxVisuals();

            if (cutsceneOverlay != null)
                cutsceneOverlay.SetActive(false);
        }

        private void EnsureTitleCardBoxVisuals()
        {
            if (cutsceneOverlay == null || displayText == null)
                return;

            if (titleCardPanel == null)
            {
                GameObject panelObj = new GameObject("CutsceneTitleCard");
                panelObj.transform.SetParent(cutsceneOverlay.transform, false);
                RectTransform panelRect = panelObj.AddComponent<RectTransform>();
                // Slightly taller so 2-3 line narrator passages fit without clipping.
                panelRect.anchorMin = new Vector2(0.06f, 0.03f);
                panelRect.anchorMax = new Vector2(0.94f, 0.30f);
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;

                titleCardPanel = panelObj.AddComponent<Image>();
                titleCardPanel.color = titleCardBackgroundColor;

                Outline outline = panelObj.AddComponent<Outline>();
                outline.effectColor = titleCardBorderColor;
                outline.effectDistance = new Vector2(2f, -2f);
            }
            else
            {
                titleCardPanel.color = titleCardBackgroundColor;
                Outline outline = titleCardPanel.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = titleCardPanel.gameObject.AddComponent<Outline>();
                    outline.effectDistance = new Vector2(2f, -2f);
                }
                outline.effectColor = titleCardBorderColor;
            }

            if (displayTitle == null)
            {
                GameObject titleObj = new GameObject("CutsceneTitle");
                titleObj.transform.SetParent(titleCardPanel.transform, false);
                RectTransform titleRect = titleObj.AddComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0.03f, 0.56f);
                titleRect.anchorMax = new Vector2(0.97f, 0.96f);
                titleRect.offsetMin = Vector2.zero;
                titleRect.offsetMax = Vector2.zero;

                displayTitle = titleObj.AddComponent<TextMeshProUGUI>();
                displayTitle.alignment = TextAlignmentOptions.Center;
                displayTitle.fontSize = 42f;
                displayTitle.color = new Color(0.98f, 0.91f, 0.72f, 1f);
                displayTitle.fontStyle = FontStyles.Bold;
            }

            if (displayText.transform.parent != titleCardPanel.transform)
                displayText.transform.SetParent(titleCardPanel.transform, false);

            RectTransform bodyRect = displayText.rectTransform;
            bodyRect.anchorMin = new Vector2(0.04f, 0.10f);
            bodyRect.anchorMax = new Vector2(0.96f, 0.58f);
            bodyRect.offsetMin = Vector2.zero;
            bodyRect.offsetMax = Vector2.zero;

            displayText.alignment = TextAlignmentOptions.Center;
            displayText.fontSize = 36f;
            displayText.color = Color.white;
            displayText.fontStyle = FontStyles.Bold; // Thicker, bolder text
            displayText.enableWordWrapping = true;

            EnsureSkipUI();
        }

        private void EnsureSkipUI()
        {
            if (skipProgressBar != null) return;

            GameObject skipRoot = new GameObject("SkipUI");
            skipRoot.transform.SetParent(cutsceneOverlay.transform, false);
            RectTransform rootRect = skipRoot.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.05f);
            rootRect.anchorMax = new Vector2(0.5f, 0.05f);
            rootRect.sizeDelta = new Vector2(200f, 10f);
            rootRect.anchoredPosition = new Vector2(0f, 30f);

            Image bg = skipRoot.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.5f);

            GameObject barObj = new GameObject("ProgressBar");
            barObj.transform.SetParent(skipRoot.transform, false);
            RectTransform barRect = barObj.AddComponent<RectTransform>();
            barRect.anchorMin = Vector2.zero;
            barRect.anchorMax = Vector2.one;
            barRect.sizeDelta = Vector2.zero;

            skipProgressBar = barObj.AddComponent<Image>();
            skipProgressBar.color = new Color(0.93f, 0.78f, 0.43f, 1f);
            skipProgressBar.type = Image.Type.Filled;
            skipProgressBar.fillMethod = Image.FillMethod.Horizontal;
            skipProgressBar.fillAmount = 0f;

            skipRoot.SetActive(false);
        }
    }
}