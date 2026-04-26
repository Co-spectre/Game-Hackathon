using UnityEngine;
using NordicWilds.Combat;
using NordicWilds.Player;

/// <summary>
/// Bigger, premium HUD drawn via IMGUI.
/// Shows: Health bar, Stamina bar (sprint fuel), Dash pip charges, Sprint indicator.
/// </summary>
public class MinimalHUD : MonoBehaviour
{
    // ── Tuneable layout ────────────────────────────────────────────────────────
    [Header("Bar Dimensions")]
    [SerializeField] private float barWidth        = 300f;
    [SerializeField] private float healthBarHeight = 22f;
    [SerializeField] private float staminaBarHeight= 16f;
    [SerializeField] private float barSpacing      = 8f;
    [SerializeField] private float marginLeft      = 36f;
    [SerializeField] private float marginBottom    = 100f;

    [Header("Weapon Label")]
    public string weaponName = "Leviathan Axe";

    // ── Runtime state (kept public so other scripts can override) ─────────────
    public float currentHealth  = 1f;   // 0–1 fraction
    public float currentStamina = 1f;   // 0–1 fraction

    // ── Private ────────────────────────────────────────────────────────────────
    private PlayerController player;
    private Health           playerHealth;

    private GUIStyle labelStyle;
    private GUIStyle iconStyle;

    // Textures
    private Texture2D healthFillTex;
    private Texture2D healthBgTex;
    private Texture2D staminaFillTex;
    private Texture2D staminaBgTex;
    private Texture2D dashFullTex;
    private Texture2D dashEmptyTex;
    private Texture2D sprintOnTex;
    private Texture2D sprintOffTex;
    private Texture2D panelTex;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    void Start()
    {
        BuildTextures();
        RefreshReferences();
        SyncBarsFromPlayer();
    }

    void Update()
    {
        if (player == null || playerHealth == null)
            RefreshReferences();
        SyncBarsFromPlayer();
    }

    void OnDestroy()
    {
        DestroyTex(healthFillTex); DestroyTex(healthBgTex);
        DestroyTex(staminaFillTex); DestroyTex(staminaBgTex);
        DestroyTex(dashFullTex); DestroyTex(dashEmptyTex);
        DestroyTex(sprintOnTex); DestroyTex(sprintOffTex);
        DestroyTex(panelTex);
    }

    // ── IMGUI ─────────────────────────────────────────────────────────────────
    void OnGUI()
    {
        InitStyles();

        currentHealth  = Mathf.Clamp01(currentHealth);
        currentStamina = Mathf.Clamp01(currentStamina);

        float x = marginLeft;
        float y = Screen.height - marginBottom;

        // ── Dark backing panel ─────────────────────────────────────────────────
        float panelW = barWidth + 20f;
        float panelH = healthBarHeight + staminaBarHeight + barSpacing * 2f + 56f;
        GUI.DrawTexture(new Rect(x - 10f, y - 10f, panelW, panelH), panelTex);

        // ── Health bar ─────────────────────────────────────────────────────────
        // Label
        GUI.Label(new Rect(x, y, 80f, 20f), "❤  HEALTH", labelStyle);
        y += 20f;

        // Background + fill
        GUI.DrawTexture(new Rect(x, y, barWidth, healthBarHeight), healthBgTex);
        if (currentHealth > 0f)
            GUI.DrawTexture(new Rect(x, y, barWidth * currentHealth, healthBarHeight), healthFillTex);

        // Percentage text centred on bar
        GUIStyle centred = new GUIStyle(labelStyle);
        centred.alignment = TextAnchor.MiddleCenter;
        centred.fontSize  = 13;
        GUI.Label(new Rect(x, y, barWidth, healthBarHeight),
            Mathf.RoundToInt(currentHealth * 100f) + "%", centred);

        y += healthBarHeight + barSpacing;

        // ── Stamina bar ────────────────────────────────────────────────────────
        bool sprinting = player != null && player.IsSprinting;

        // Sprint toggle indicator
        string sprintLabel = sprinting ? "⚡ STAMINA  [SPRINTING]" : "⚡ STAMINA";
        GUIStyle staminaLabelStyle = new GUIStyle(labelStyle);
        staminaLabelStyle.normal.textColor = sprinting
            ? new Color(1f, 0.9f, 0.2f, 1f)   // bright yellow while sprinting
            : new Color(0.7f, 0.9f, 1f, 0.85f);
        GUI.Label(new Rect(x, y, 240f, 20f), sprintLabel, staminaLabelStyle);
        y += 20f;

        // Background
        GUI.DrawTexture(new Rect(x, y, barWidth, staminaBarHeight), staminaBgTex);

        // Fill — colour shifts from blue → orange as stamina drops
        if (currentStamina > 0f)
        {
            Color fillColour = sprinting
                ? Color.Lerp(new Color(1f, 0.5f, 0.05f), new Color(1f, 0.85f, 0.1f), currentStamina)
                : Color.Lerp(new Color(0.1f, 0.55f, 0.85f), new Color(0.3f, 0.85f, 1f), currentStamina);
            staminaFillTex.SetPixels(new Color[] { fillColour, fillColour, fillColour, fillColour });
            staminaFillTex.Apply();
            GUI.DrawTexture(new Rect(x, y, barWidth * currentStamina, staminaBarHeight), staminaFillTex);
        }

        y += staminaBarHeight + barSpacing;

        // ── Dash availability (stamina-based, no fixed pips) ──────────────────
        if (player != null)
        {
            bool depleted  = player.StaminaDepleted;
            bool canDash   = player.CanDash;
            float dashCost = 0.15f; // mirrors dashStaminaCostFraction in PlayerController

            // How many dashes worth of stamina do we currently have?
            int dashesAvailable = depleted ? 0 : Mathf.FloorToInt(currentStamina / dashCost);
            int dashesMax       = Mathf.FloorToInt(1f / dashCost); // e.g. 6 dashes at full stamina

            GUIStyle dashLabelStyle = new GUIStyle(labelStyle);
            if (depleted)
                dashLabelStyle.normal.textColor = new Color(1f, 0.25f, 0.25f, 1f); // red
            else if (!canDash)
                dashLabelStyle.normal.textColor = new Color(1f, 0.65f, 0.1f, 1f);  // orange
            else
                dashLabelStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 0.9f);

            string dashText = depleted
                ? "DASH  ✗  DEPLETED — recharging to full"
                : $"DASH  {dashesAvailable} / {dashesMax}  available  [SPACE]";

            GUI.Label(new Rect(x, y, 340f, 20f), dashText, dashLabelStyle);
            y += 22f;

            // ── Weapon label ───────────────────────────────────────────────────
            GUIStyle weapStyle = new GUIStyle(labelStyle);
            weapStyle.fontSize = 13;
            weapStyle.normal.textColor = new Color(0.85f, 0.78f, 0.55f, 0.9f);
            GUI.Label(new Rect(x, y, 300f, 22f), "⚔  " + weaponName, weapStyle);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private void RefreshReferences()
    {
        GameObject pObj = GameObject.FindWithTag("Player");
        if (pObj != null)
        {
            player       = pObj.GetComponent<PlayerController>();
            playerHealth = pObj.GetComponent<Health>();
        }
    }

    private void SyncBarsFromPlayer()
    {
        if (playerHealth != null && playerHealth.MaxHealth > 0f)
            currentHealth = playerHealth.CurrentHealth / playerHealth.MaxHealth;

        if (player != null)
            currentStamina = player.StaminaFraction;
    }

    private void InitStyles()
    {
        if (labelStyle != null) return;
        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 14,
            fontStyle = FontStyle.Bold,
        };
        labelStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f, 0.9f);
    }

    private void BuildTextures()
    {
        // Health — crimson red with a slight warm tint
        healthFillTex = MakeTex(2, 2, new Color(0.85f, 0.12f, 0.18f));
        healthBgTex   = MakeTex(2, 2, new Color(0.12f, 0.04f, 0.06f, 0.85f));

        // Stamina — starts as a 2×2; colour updated each frame in OnGUI
        staminaFillTex = MakeTex(2, 2, new Color(0.2f, 0.7f, 1f));
        staminaBgTex   = MakeTex(2, 2, new Color(0.04f, 0.08f, 0.14f, 0.85f));

        // Dash pips
        dashFullTex  = MakeTex(2, 2, new Color(1f,   0.82f, 0.18f));
        dashEmptyTex = MakeTex(2, 2, new Color(0.18f, 0.18f, 0.22f, 0.7f));

        // Panel backing
        panelTex = MakeTex(2, 2, new Color(0.04f, 0.04f, 0.08f, 0.72f));
    }

    private Texture2D MakeTex(int w, int h, Color c)
    {
        Color[] pix = new Color[w * h];
        for (int i = 0; i < pix.Length; i++) pix[i] = c;
        Texture2D t = new Texture2D(w, h);
        t.SetPixels(pix);
        t.Apply();
        t.hideFlags = HideFlags.HideAndDontSave;
        return t;
    }

    private void DestroyTex(Texture2D t) { if (t != null) Destroy(t); }

    public void UpdateWeapon(string name) => weaponName = name;
}
