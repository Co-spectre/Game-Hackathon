using System.Collections;
using UnityEngine;
using NordicWilds.Player;
using NordicWilds.CameraSystems;

namespace NordicWilds.UI
{
    /// <summary>
    /// Elden Ring-style death screen.
    /// Activated by Health.cs when the player dies.
    /// Shows: red vignette → "You Died" → "Respawn" button.
    /// On respawn: resets ALL enemies and teleports player to start.
    /// </summary>
    public class DeathScreen : MonoBehaviour
    {
        // ── Singleton so Health.cs can call it without a scene reference ──────────
        public static DeathScreen Instance { get; private set; }

        [Header("Timing")]
        [SerializeField] private float vignetteRiseTime  = 0.6f;  // red fills in
        [SerializeField] private float youDiedDelay      = 0.8f;  // pause before text
        [SerializeField] private float youDiedFadeTime   = 1.4f;  // text fade-in
        [SerializeField] private float buttonDelay       = 1.2f;  // pause after text before button
        [SerializeField] private float buttonFadeTime    = 1.0f;  // button fade-in

        [Header("Respawn")]
        [SerializeField] private Vector3 respawnPosition = new Vector3(10000f, 2f, 9980f);

        // ── Runtime state ─────────────────────────────────────────────────────────
        private bool  _active        = false;
        private float _vignetteAlpha = 0f;
        private float _textAlpha     = 0f;
        private float _buttonAlpha   = 0f;
        private bool  _buttonVisible = false;

        // ── Textures & styles ─────────────────────────────────────────────────────
        private Texture2D _redTex;
        private GUIStyle  _youDiedStyle;
        private GUIStyle  _buttonStyle;
        private GUIStyle  _buttonHoverStyle;
        private bool      _stylesInitialised = false;

        // ── Player ref (set by Activate) ──────────────────────────────────────────
        private PlayerController _player;
        private Transform        _playerTransform;
        private Rigidbody        _playerRb;

        // ── Lifecycle ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _redTex = MakeTex(4, 4, new Color(0.55f, 0f, 0f, 1f));
        }

        private void OnDestroy()
        {
            if (_redTex != null) Destroy(_redTex);
            if (Instance == this) Instance = null;
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>Called by Health.cs when the player's health reaches zero.</summary>
        public void Activate(PlayerController player)
        {
            if (_active) return;

            // If player ref is null, try finding it by tag as a fallback
            if (player == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                    player = playerObj.GetComponent<PlayerController>();
            }

            _active            = true;
            _vignetteAlpha     = 0f;
            _textAlpha         = 0f;
            _buttonAlpha       = 0f;
            _buttonVisible     = false;
            _stylesInitialised = false;

            _player          = player;
            _playerTransform = player != null ? player.transform : null;
            _playerRb        = player != null ? player.GetComponent<Rigidbody>() : null;

            // Last resort: grab transform from tag even if no PlayerController component
            if (_playerTransform == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    _playerTransform = playerObj.transform;
                    _playerRb        = playerObj.GetComponent<Rigidbody>();
                }
            }

            StartCoroutine(DeathSequence());
        }


        // ── Death sequence ────────────────────────────────────────────────────────
        private IEnumerator DeathSequence()
        {
            // 1. Lock controls & collapse player (rotate as if falling)
            if (_player != null)
            {
                _player.SetControlsLocked(true);
                _player.isInvincible = true; // no more hits while dead
            }
            if (_playerRb != null) _playerRb.linearVelocity = Vector3.zero;

            StartCoroutine(CollapsePlayer());

            // 2. Rise red vignette
            float elapsed = 0f;
            while (elapsed < vignetteRiseTime)
            {
                _vignetteAlpha = Mathf.Lerp(0f, 0.55f, elapsed / vignetteRiseTime);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            _vignetteAlpha = 0.55f;

            // 3. Pause, then fade in "You Died"
            yield return new WaitForSecondsRealtime(youDiedDelay);

            elapsed = 0f;
            while (elapsed < youDiedFadeTime)
            {
                _textAlpha = Mathf.Lerp(0f, 1f, elapsed / youDiedFadeTime);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            _textAlpha = 1f;

            // 4. Pause, then fade in Respawn button
            yield return new WaitForSecondsRealtime(buttonDelay);
            _buttonVisible = true;

            elapsed = 0f;
            while (elapsed < buttonFadeTime)
            {
                _buttonAlpha = Mathf.Lerp(0f, 1f, elapsed / buttonFadeTime);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            _buttonAlpha = 1f;
        }

        private IEnumerator CollapsePlayer()
        {
            if (_playerTransform == null) yield break;

            // Smoothly rotate the player to lie on their side (simulate falling over)
            Quaternion startRot = _playerTransform.rotation;
            Quaternion deadRot  = startRot * Quaternion.Euler(90f, 0f, 0f);
            float elapsed = 0f;
            float duration = 0.6f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float eased = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic
                if (_playerTransform != null)
                    _playerTransform.rotation = Quaternion.Slerp(startRot, deadRot, eased);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_playerTransform != null)
                _playerTransform.rotation = deadRot;
        }

        // ── IMGUI rendering ───────────────────────────────────────────────────────
        private void OnGUI()
        {
            if (!_active) return;

            InitStyles();

            float sw = Screen.width;
            float sh = Screen.height;

            // ── Red vignette overlay ──────────────────────────────────────────────
            if (_vignetteAlpha > 0f)
            {
                Color prev = GUI.color;

                // Four edge gradients (top, bottom, left, right) for vignette feel
                DrawVignette(sw, sh, _vignetteAlpha);

                GUI.color = prev;
            }

            // ── "You Died" text ───────────────────────────────────────────────────
            if (_textAlpha > 0f)
            {
                _youDiedStyle.normal.textColor = new Color(0.88f, 0.06f, 0.06f, _textAlpha);

                // Subtle shadow
                Color shadowCol = new Color(0f, 0f, 0f, _textAlpha * 0.6f);
                GUIStyle shadow = new GUIStyle(_youDiedStyle);
                shadow.normal.textColor = shadowCol;
                float offX = sw * 0.003f, offY = sh * 0.004f;
                GUI.Label(new Rect(offX, sh * 0.35f + offY, sw, 120f), "You Died", shadow);
                GUI.Label(new Rect(0,    sh * 0.35f,        sw, 120f), "You Died", _youDiedStyle);
            }

            // ── Respawn button ────────────────────────────────────────────────────
            if (_buttonVisible && _buttonAlpha > 0f)
            {
                float btnW = 260f, btnH = 54f;
                float btnX = (sw - btnW) * 0.5f;
                float btnY = sh * 0.60f;

                _buttonStyle.normal.textColor  = new Color(0.92f, 0.85f, 0.75f, _buttonAlpha);
                _buttonStyle.hover.textColor   = new Color(1f,    1f,    1f,    _buttonAlpha);

                GUI.color = new Color(1f, 1f, 1f, _buttonAlpha);
                if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "Respawn", _buttonStyle))
                {
                    GUI.color = Color.white;
                    StartCoroutine(DoRespawn());
                }
                GUI.color = Color.white;
            }
        }

        // ── Respawn logic ─────────────────────────────────────────────────────────
        private IEnumerator DoRespawn()
        {
            // Fade everything out first
            float elapsed = 0f, fadeOut = 0.5f;
            float startVig = _vignetteAlpha, startTxt = _textAlpha, startBtn = _buttonAlpha;
            while (elapsed < fadeOut)
            {
                float t = 1f - elapsed / fadeOut;
                _vignetteAlpha = startVig * t;
                _textAlpha     = startTxt * t;
                _buttonAlpha   = startBtn * t;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // ── Reset all enemies ─────────────────────────────────────────────────
            foreach (EnemyAI enemy in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
                enemy.ResetEnemy();

            // ── Teleport player to respawn point ──────────────────────────────────
            if (_playerTransform != null)
            {
                // Find safe ground at respawn position
                Vector3 safePos = FindSafeSpawn(respawnPosition);

                if (_playerRb != null)
                {
                    _playerRb.linearVelocity  = Vector3.zero;
                    _playerRb.angularVelocity = Vector3.zero;
                    _playerRb.position = safePos;
                }

                _playerTransform.position = safePos;
                _playerTransform.rotation = Quaternion.identity; // stand back up
            }

            // Restore camera to player
            var camFollow = FindFirstObjectByType<IsometricCameraFollow>();
            if (camFollow != null && _playerTransform != null)
            {
                camFollow.target = _playerTransform;
                camFollow.SnapToTarget();
            }

            // ── Restore player health & controls ──────────────────────────────────
            var health = _player != null ? _player.GetComponent<NordicWilds.Combat.Health>() : null;
            if (health != null) health.ForceRespawn(respawnPosition);

            if (_player != null)
            {
                _player.isInvincible = false;
                _player.SetControlsLocked(false);
            }

            // ── Reset state ───────────────────────────────────────────────────────
            _active        = false;
            _vignetteAlpha = 0f;
            _textAlpha     = 0f;
            _buttonAlpha   = 0f;
            _buttonVisible = false;
        }

        private Vector3 FindSafeSpawn(Vector3 desired)
        {
            if (Physics.Raycast(desired + Vector3.up * 120f, Vector3.down, out RaycastHit hit, 250f))
                return new Vector3(desired.x, hit.point.y + 1f, desired.z);
            return desired;
        }

        // ── Vignette helper ───────────────────────────────────────────────────────
        private void DrawVignette(float sw, float sh, float alpha)
        {
            // Semi-transparent dark-red overlay across the whole screen
            Color full = new Color(0.45f, 0f, 0f, alpha * 0.7f);
            GUI.color = full;
            GUI.DrawTexture(new Rect(0, 0, sw, sh), _redTex);
            GUI.color = Color.white;
        }

        // ── Style initialisation ──────────────────────────────────────────────────
        private void InitStyles()
        {
            if (_stylesInitialised) return;
            _stylesInitialised = true;

            // "You Died" — large centred serif-feel bold text
            _youDiedStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = Mathf.RoundToInt(Screen.height * 0.11f),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap  = false,
            };

            // Respawn button — dark bordered, parchment text
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize  = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            _buttonStyle.normal.background  = MakeTex(2, 2, new Color(0.08f, 0.04f, 0.04f, 0.88f));
            _buttonStyle.hover.background   = MakeTex(2, 2, new Color(0.22f, 0.06f, 0.06f, 0.95f));
            _buttonStyle.active.background  = MakeTex(2, 2, new Color(0.35f, 0.08f, 0.08f, 1f));
            _buttonStyle.border             = new RectOffset(4, 4, 4, 4);
            _buttonStyle.padding            = new RectOffset(20, 20, 10, 10);
        }

        private Texture2D MakeTex(int w, int h, Color c)
        {
            Color[] px = new Color[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = c;
            var t = new Texture2D(w, h);
            t.SetPixels(px);
            t.Apply();
            t.hideFlags = HideFlags.HideAndDontSave;
            return t;
        }
    }
}
