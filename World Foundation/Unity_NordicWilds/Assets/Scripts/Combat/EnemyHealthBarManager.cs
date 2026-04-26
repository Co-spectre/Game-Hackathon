using System.Collections.Generic;
using UnityEngine;
using NordicWilds.Combat;
using NordicWilds.CameraSystems;

/// <summary>
/// Draws world-space health bars for all non-player enemies via OnGUI.
/// Self-creates on scene load — no prefab or Inspector wiring needed.
/// Uses the same IMGUI coordinate system as MinimalHUD (proven to work).
/// </summary>
public class EnemyHealthBarManager : MonoBehaviour
{
    // ─── Auto-creation ────────────────────────────────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        if (FindFirstObjectByType<EnemyHealthBarManager>() != null) return;
        new GameObject("[EnemyHealthBarManager]").AddComponent<EnemyHealthBarManager>();
    }

    // ─── Inspector ────────────────────────────────────────────────────────────
    [SerializeField] float barW         = 100f;
    [SerializeField] float barH         = 13f;
    [SerializeField] float border       = 3f;
    [SerializeField] float yWorld       = 3.1f;
    [SerializeField] float showDuration = 4f;    // seconds bar stays visible after combat
    [SerializeField] float holdTime     = 0.28f;
    [SerializeField] float decayRate    = 2.2f;

    // ─── Textures ─────────────────────────────────────────────────────────────
    Texture2D _bgTex, _redTex, _flashTex;

    // ─── Enemy tracking ───────────────────────────────────────────────────────
    struct BarState { public float prevHp, flash, hold, showTimer; public bool decaying; }
    readonly Dictionary<Health, BarState> _bars = new();

    List<Health> _enemies    = new();
    float        _scanTimer  = 99f;           // trigger immediate scan on Start
    const float  SCAN_INT    = 2f;

    Camera    _cam;
    Transform _playerTr;
    Health    _playerHealth;

    // ─── Lifecycle ────────────────────────────────────────────────────────────
    void Start()
    {
        _bgTex    = Tex(new Color(0.05f, 0.05f, 0.05f, 0.93f));
        _redTex   = Tex(new Color(0.95f, 0.04f, 0.04f));
        _flashTex = Tex(new Color(1.00f, 0.88f, 0.25f));
    }

    void Update()
    {
        // Use the IsometricCameraFollow's camera — guaranteed to be the scene render camera.
        if (_cam == null || !_cam.isActiveAndEnabled)
        {
            var follow = FindFirstObjectByType<IsometricCameraFollow>();
            _cam = (follow != null) ? follow.GetComponent<Camera>() : Camera.main;
        }

        // Player — subscribe to their damage event to detect when an enemy hits them
        if (_playerTr == null)
        {
            var pg = GameObject.FindWithTag("Player");
            if (pg)
            {
                _playerTr     = pg.transform;
                _playerHealth = pg.GetComponent<Health>();
                if (_playerHealth != null)
                    _playerHealth.OnDamageTaken += OnPlayerHit;
            }
        }

        // Re-scan enemies periodically
        _scanTimer += Time.deltaTime;
        if (_scanTimer >= SCAN_INT) { _scanTimer = 0f; ScanEnemies(); }

        // Update flash for each enemy
        var dead    = new List<Health>();
        var updates = new List<(Health h, BarState s)>();

        foreach (var kv in _bars)
        {
            Health h = kv.Key;
            if (!h || h.IsDead) { dead.Add(h); continue; }

            BarState s  = kv.Value;
            float hp    = h.CurrentHealthNormalized;
            float delta = s.prevHp - hp;
            s.prevHp = hp;

            if (delta > 0.001f)
            {
                s.flash     = Mathf.Clamp01(s.flash + delta);
                s.hold      = 0f;
                s.decaying  = false;
                s.showTimer = showDuration;   // reveal bar on hit
            }

            // Count down show timer
            if (s.showTimer > 0f) s.showTimer -= Time.deltaTime;

            if (s.flash > 0f)
            {
                if (!s.decaying) { s.hold += Time.deltaTime; if (s.hold >= holdTime) s.decaying = true; }
                else             { s.flash -= decayRate * Time.deltaTime; if (s.flash < 0f) s.flash = 0f; }
            }

            updates.Add((h, s));
        }

        // Apply updates AFTER iteration to avoid "collection modified" exception
        foreach (var (h, s) in updates) _bars[h] = s;
        foreach (var h in dead) _bars.Remove(h);
    }

    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint) return;
        if (_cam == null || _bgTex == null) return;

        foreach (var kv in _bars)
        {
            Health h = kv.Key;
            if (!h || !h.gameObject.activeInHierarchy || h.IsDead) continue;

            // Only show bar if combat-triggered show timer is active
            if (kv.Value.showTimer <= 0f) continue;

            // World → screen
            Vector3 sp = _cam.WorldToScreenPoint(h.transform.position + Vector3.up * yWorld);
            if (sp.z <= 0f) continue;                    // behind camera

            // Strict frustum cull — skip bar if enemy is off-screen
            if (sp.x < 0f || sp.x > Screen.width)  continue;
            if (sp.y < 0f || sp.y > Screen.height) continue;

            // Screen → GUI  (Unity screen Y=0 bottom; GUI Y=0 top)
            float gx = sp.x - barW * 0.5f;
            float gy = Screen.height - sp.y - barH * 0.5f;

            float hf = Mathf.Clamp01(h.CurrentHealthNormalized);
            BarState s = kv.Value;
            float ff   = Mathf.Clamp01(Mathf.Min(s.flash, 1f - hf));

            // Background
            GUI.DrawTexture(new Rect(gx - border, gy - border, barW + border * 2f, barH + border * 2f), _bgTex);
            // Red HP
            if (hf > 0f)
                GUI.DrawTexture(new Rect(gx, gy, barW * hf, barH), _redTex);
            // Yellow flash (sits right of red)
            if (ff > 0.005f)
                GUI.DrawTexture(new Rect(gx + barW * hf, gy, barW * ff, barH), _flashTex);
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────
    void ScanEnemies()
    {
        _enemies.Clear();
        var playerHealth = _playerTr ? _playerTr.GetComponent<Health>() : null;

        foreach (var h in FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (h == playerHealth || h.IsDead) continue;
            _enemies.Add(h);
            if (!_bars.ContainsKey(h))
                _bars[h] = new BarState { prevHp = h.CurrentHealthNormalized };
        }
    }

    static Texture2D Tex(Color c)
    {
        var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        t.SetPixel(0, 0, c); t.Apply();
        return t;
    }

    // Called when an enemy hits the player — reveal that enemy's bar
    void OnPlayerHit(DamageInfo info)
    {
        if (info.Source == null) return;
        var srcHealth = info.Source.GetComponent<Health>();
        if (srcHealth == null)
            srcHealth = info.Source.GetComponentInParent<Health>();
        if (srcHealth == null || srcHealth == _playerHealth) return;

        if (_bars.TryGetValue(srcHealth, out BarState s))
        {
            s.showTimer = showDuration;
            _bars[srcHealth] = s;
        }
    }

    void OnDestroy()
    {
        if (_playerHealth != null)
            _playerHealth.OnDamageTaken -= OnPlayerHit;
        Destroy(_bgTex); Destroy(_redTex); Destroy(_flashTex);
    }
}
