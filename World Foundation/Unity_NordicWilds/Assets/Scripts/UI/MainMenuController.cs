using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using NordicWilds.CameraSystems;
using NordicWilds.Player;
using NordicWilds.Environment;

namespace NordicWilds.UI
{
    // Cinematic start-menu controller. Hides the player visual, runs the title intro, then
    // on Start: kicks the camera director into a launch dolly while the boat sails forward,
    // crossfades to black, teleports the player to the gameplay spawn, and crossfades back.
    public class MainMenuController : MonoBehaviour
    {
        [Header("Refs")]
        public CanvasGroup faderGroup;
        public GameObject boat;
        public Transform player;
        public Camera mainCamera;
        public MenuCameraDirector cameraDirector;
        public MenuTitleAnimator titleAnimator;
        public CanvasGroup menuRootGroup;
        public Button startButton;
        public Button quitButton;
        public BoatBobber boatBobber;
        public GameObject[] menuOnlyObjects;     // Sun, lights, particles, ocean — destroyed on launch

        [Header("Sequence")]
        // Yamato ground top surface sits at y=0 (block at -0.5, thickness 1). Capsule
        // half-height is ~1, so center at y=1.05 = bottom resting at y=0.05 (just
        // above ground). The 5cm gap collapses on the first physics tick and never
        // looks like a fall.
        public Vector3 gameStartPos = new Vector3(10000f, 1.05f, 9980f);
        public float boatSailDuration = 3.0f;
        public float boatSailDistance = 26f;
        public float fadeOutDuration  = 1.6f;
        public float menuUiFadeDuration = 0.35f;
        public float blackHold        = 0.7f;
        public float fadeInDuration   = 1.4f;
        public bool autoWalkFromBoatOnStart = false;
        public Vector3 autoWalkTargetPos = new Vector3(-620f, 1.05f, -628f);
        public float autoWalkDuration = 2.65f;
        public int startGameCutsceneIndex = 0;

        [Header("Player Visual Hiding")]
        public bool hidePlayerOnMenu = true;
        public bool startsInYamato = true;

        bool isStarting;
        bool buttonsBound;
        Renderer[] hiddenPlayerRenderers;
        Collider[] hiddenPlayerColliders;
        bool[] hiddenPlayerColliderStates;

        void Start()
        {
            EnsureMenuInfrastructure();

            if (faderGroup != null)
            {
                faderGroup.alpha = 0f;
                faderGroup.blocksRaycasts = false;
            }

            if (cameraDirector != null)
            {
                cameraDirector.SnapToIdle();
            }

            HidePlayerVisual();
            BindButtons();
        }

        void OnDestroy()
        {
            UnbindButtons();
        }

        void HidePlayerVisual()
        {
            if (!hidePlayerOnMenu || player == null) return;

            // Disable renderers so the capsule doesn't appear on the boat
            hiddenPlayerRenderers = player.GetComponentsInChildren<Renderer>(true);
            foreach (var r in hiddenPlayerRenderers) r.enabled = false;

            // Note: CreateOceanMenuUI already disables colliders before this runs, so we
            // can't read their *current* enabled state to know what they should restore to.
            // Assume collider-on is the gameplay default and remember which are real colliders.
            hiddenPlayerColliders = player.GetComponentsInChildren<Collider>(true);
            hiddenPlayerColliderStates = new bool[hiddenPlayerColliders.Length];
            for (int i = 0; i < hiddenPlayerColliders.Length; i++)
            {
                hiddenPlayerColliderStates[i] = true; // gameplay default — colliders on
                hiddenPlayerColliders[i].enabled = false;
            }

            // Disable any Light children (axe rune glow, etc.) for the menu
            foreach (var l in player.GetComponentsInChildren<Light>(true)) l.enabled = false;

            var pCtrl = player.GetComponent<PlayerController>();
            if (pCtrl != null) pCtrl.SetControlsLocked(true);

            var rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                rb.isKinematic = true;
            }
        }

        void RestorePlayerVisual()
        {
            if (hiddenPlayerRenderers != null)
                foreach (var r in hiddenPlayerRenderers) if (r != null) r.enabled = true;

            if (hiddenPlayerColliders != null)
            {
                for (int i = 0; i < hiddenPlayerColliders.Length; i++)
                {
                    if (hiddenPlayerColliders[i] != null)
                        hiddenPlayerColliders[i].enabled = hiddenPlayerColliderStates == null || i >= hiddenPlayerColliderStates.Length || hiddenPlayerColliderStates[i];
                }
            }

            if (player != null)
            {
                foreach (var l in player.GetComponentsInChildren<Light>(true)) l.enabled = true;
                var rb = player.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false;
            }
        }

        void BindButtons()
        {
            if (buttonsBound) return;

            if (menuRootGroup == null)
            {
                Transform menuRoot = FindDeepChild(transform, "MenuRoot");
                if (menuRoot != null)
                    menuRootGroup = menuRoot.GetComponent<CanvasGroup>();
            }

            if (startButton == null)
                startButton = FindButton("StartButton");

            if (quitButton == null)
                quitButton = FindButton("QuitButton");

            if (startButton != null)
            {
                startButton.onClick.RemoveListener(StartGame);
                startButton.onClick.AddListener(StartGame);
                startButton.interactable = true;

                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(startButton.gameObject);
            }
            else
            {
                Debug.LogWarning("MainMenuController could not find StartButton. The menu will not be able to start the game.");
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(QuitGame);
                quitButton.onClick.AddListener(QuitGame);
                quitButton.interactable = true;
            }

            buttonsBound = true;
        }

        void UnbindButtons()
        {
            if (!buttonsBound) return;

            if (startButton != null)
                startButton.onClick.RemoveListener(StartGame);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(QuitGame);

            buttonsBound = false;
        }

        Button FindButton(string buttonName)
        {
            Transform found = FindDeepChild(transform, buttonName);
            return found != null ? found.GetComponent<Button>() : null;
        }

        static Transform FindDeepChild(Transform root, string childName)
        {
            if (root == null) return null;

            foreach (Transform child in root)
            {
                if (child.name == childName)
                    return child;

                Transform nested = FindDeepChild(child, childName);
                if (nested != null)
                    return nested;
            }

            return null;
        }

        void EnsureMenuInfrastructure()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas != null && GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            if (EventSystem.current == null && Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            if (menuRootGroup != null)
            {
                menuRootGroup.alpha = 1f;
                menuRootGroup.interactable = true;
                menuRootGroup.blocksRaycasts = true;
            }
        }

        public void StartGame()
        {
            if (isStarting) return;
            isStarting = true;
            if (startButton != null) startButton.interactable = false;
            if (quitButton != null) quitButton.interactable = false;
            StartCoroutine(StartGameFlow());
        }

        IEnumerator StartGameFlow()
        {
            CutsceneManager manager = CutsceneManager.GetOrCreate();
            manager.EnsureStartGameCutscene("Cutscenes/img12", 4f, string.Empty);

            if (manager != null && manager.HasCutscene(startGameCutsceneIndex))
                yield return manager.PlayCutsceneAndWait(startGameCutsceneIndex);

            yield return StartGameSequence();
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        IEnumerator StartGameSequence()
        {
            // Disable menu interaction immediately
            if (menuRootGroup != null)
            {
                menuRootGroup.interactable = false;
                menuRootGroup.blocksRaycasts = false;
                menuRootGroup.alpha = 1f;
            }
            if (titleAnimator != null) titleAnimator.enabled = false;

            // Kick the camera into launch dolly
            if (cameraDirector != null) cameraDirector.StartLaunch();

            // Sail the boat forward along its own heading
            Vector3 startPos = boat != null ? boat.transform.position : Vector3.zero;
            Vector3 fwd = boat != null ? boat.transform.forward : Vector3.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward; else fwd.Normalize();

            float t = 0f;
            while (t < boatSailDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / boatSailDuration);
                float ease = 1f - Mathf.Pow(1f - k, 2.0f);
                if (boat != null)
                {
                    Vector3 newXZ = startPos + fwd * (boatSailDistance * ease);
                    // Let the bobber keep handling Y; just write XZ
                    boat.transform.position = new Vector3(newXZ.x, boat.transform.position.y, newXZ.z);
                }

                if (menuRootGroup != null && menuRootGroup.gameObject.activeSelf)
                {
                    float uiK = Mathf.Clamp01(t / Mathf.Max(0.01f, menuUiFadeDuration));
                    float uiEase = uiK * uiK * (3f - 2f * uiK);
                    menuRootGroup.alpha = Mathf.Lerp(1f, 0f, uiEase);
                    if (uiK >= 1f)
                        menuRootGroup.gameObject.SetActive(false);
                }

                // Begin the black fade in the second half of the sail
                if (faderGroup != null && k > 0.28f)
                {
                    float fk = Mathf.InverseLerp(0.28f, 1f, k);
                    faderGroup.alpha = Mathf.Lerp(0f, 1f, fk);
                }
                yield return null;
            }
            if (faderGroup != null) faderGroup.alpha = 1f;

            yield return new WaitForSeconds(blackHold);

            // Hand the world back to gameplay
            if (boatBobber != null) boatBobber.enabled = false;
            if (cameraDirector != null) cameraDirector.enabled = false;

            if (player != null)
            {
                player.SetParent(null, worldPositionStays: false);
                player.position = gameStartPos;
                player.rotation = Quaternion.identity;

                // Sync the rigidbody to the new transform BEFORE re-enabling physics
                // so gravity doesn't kick in mid-teleport and produce a visible fall.
                var rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.position = gameStartPos;
                    rb.rotation = Quaternion.identity;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                Physics.SyncTransforms();

                RestorePlayerVisual();
            }
            if (boat != null) boat.SetActive(false);

            // Tear down menu-only scene props (sun, ocean, mist) so they don't bleed into gameplay
            if (menuOnlyObjects != null)
            {
                foreach (var go in menuOnlyObjects)
                    if (go != null) Destroy(go);
            }

            if (startsInYamato)
                ApplyYamatoLighting();
            else
                ApplyForestLandingLighting();

            if (mainCamera != null)
            {
                // Restore orthographic gameplay projection — the menu uses perspective
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 12f;
                mainCamera.farClipPlane = 500f;

                var iso = mainCamera.GetComponent<IsometricCameraFollow>();
                if (iso != null)
                {
                    iso.enabled = true;
                    iso.target = player;
                    iso.SnapToTarget();
                }
                else if (player != null)
                {
                    mainCamera.transform.position = player.position + new Vector3(-15f, 22f, -15f);
                    mainCamera.transform.LookAt(player);
                }

                // Drop the menu's perspective director once we're done
                var dir = mainCamera.GetComponent<MenuCameraDirector>();
                if (dir != null) Destroy(dir);
            }

            // Fade back into gameplay
            float ft = 0f;
            while (ft < fadeInDuration)
            {
                ft += Time.deltaTime;
                if (faderGroup != null) faderGroup.alpha = Mathf.Lerp(1f, 0f, ft / fadeInDuration);
                yield return null;
            }
            if (faderGroup != null) faderGroup.alpha = 0f;

            if (autoWalkFromBoatOnStart && !startsInYamato && player != null)
                yield return AutoWalkPlayerFromBoat();

            // Unlock player + spawn HUD if missing
            if (player != null)
            {
                var pCtrl = player.GetComponent<PlayerController>();
                if (pCtrl != null) pCtrl.SetControlsLocked(false);
            }
            if (Object.FindFirstObjectByType<MinimalHUD>() == null)
            {
                new GameObject("MinimalHUDOverlay").AddComponent<MinimalHUD>();
            }
            if (startsInYamato && Object.FindFirstObjectByType<WorldMapOverlay>() == null)
            {
                new GameObject("WorldMapOverlay").AddComponent<WorldMapOverlay>();
            }
            if (startsInYamato && Object.FindFirstObjectByType<YamatoArrivalDialogue>() == null)
            {
                new GameObject("YamatoArrivalDialogue").AddComponent<YamatoArrivalDialogue>();
            }

            Destroy(gameObject);
        }

        IEnumerator AutoWalkPlayerFromBoat()
        {
            var pCtrl = player.GetComponent<PlayerController>();
            if (pCtrl != null)
                pCtrl.SetControlsLocked(true);

            Rigidbody rb = player.GetComponent<Rigidbody>();
            Vector3 start = player.position;
            Vector3 end = autoWalkTargetPos;
            Vector3 direction = end - start;
            direction.y = 0f;
            Quaternion targetRotation = direction.sqrMagnitude > 0.01f
                ? Quaternion.LookRotation(direction.normalized)
                : player.rotation;

            float t = 0f;
            while (t < autoWalkDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / Mathf.Max(0.01f, autoWalkDuration));
                float ease = k * k * (3f - 2f * k);
                Vector3 pos = Vector3.Lerp(start, end, ease);

                player.position = pos;
                player.rotation = Quaternion.Slerp(player.rotation, targetRotation, Time.deltaTime * 8f);

                if (rb != null)
                {
                    rb.position = pos;
                    rb.rotation = player.rotation;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                yield return null;
            }

            player.position = end;
            player.rotation = targetRotation;
            if (rb != null)
            {
                rb.position = end;
                rb.rotation = targetRotation;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            Physics.SyncTransforms();
        }

        void ApplyForestLandingLighting()
        {
            RenderSettings.skybox = null;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.016f;
            RenderSettings.fogColor = new Color(0.34f, 0.40f, 0.42f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.42f, 0.50f, 0.56f);
            RenderSettings.ambientEquatorColor = new Color(0.30f, 0.36f, 0.34f);
            RenderSettings.ambientGroundColor = new Color(0.16f, 0.20f, 0.20f);
            RenderSettings.ambientIntensity = 0.80f;
            RenderSettings.reflectionIntensity = 0.28f;

            if (mainCamera != null)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = new Color(0.22f, 0.28f, 0.30f);
                mainCamera.allowHDR = false;
            }

            var dirLight = Object.FindFirstObjectByType<Light>();
            if (dirLight != null && dirLight.type == LightType.Directional)
            {
                dirLight.color = new Color(0.74f, 0.86f, 0.96f);
                dirLight.intensity = 0.62f;
                dirLight.transform.rotation = Quaternion.Euler(24f, -48f, 0f);
                dirLight.shadows = LightShadows.Soft;
                dirLight.shadowStrength = 0.48f;
            }
        }

        void ApplyYamatoLighting()
        {
            // Yamato sunset: warm dusk haze, low orange sun, lifted ambient for soft contrast.
            RenderSettings.skybox = null;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.013f;
            RenderSettings.fogColor = new Color(0.46f, 0.30f, 0.32f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor     = new Color(0.50f, 0.36f, 0.34f);
            RenderSettings.ambientEquatorColor = new Color(0.38f, 0.30f, 0.32f);
            RenderSettings.ambientGroundColor  = new Color(0.22f, 0.20f, 0.28f);
            RenderSettings.ambientIntensity    = 0.85f;
            RenderSettings.reflectionIntensity = 0.30f;

            if (mainCamera != null)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = new Color(0.30f, 0.22f, 0.26f);
                mainCamera.allowHDR = false;
            }

            var dirLight = Object.FindFirstObjectByType<Light>();
            if (dirLight != null && dirLight.type == LightType.Directional)
            {
                dirLight.color = new Color(0.96f, 0.58f, 0.32f);   // sunset orange
                dirLight.intensity = 0.55f;                          // dim
                dirLight.transform.rotation = Quaternion.Euler(10f, -32f, 0f); // very low
                dirLight.shadows = LightShadows.Soft;
                dirLight.shadowStrength = 0.38f;
            }
        }
    }
}
