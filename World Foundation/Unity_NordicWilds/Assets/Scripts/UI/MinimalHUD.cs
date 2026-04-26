using UnityEngine;
using NordicWilds.Combat;
using NordicWilds.Player;

/// <summary>
/// Bigger, premium HUD drawn via IMGUI.
/// Shows: Health bar, Stamina bar (sprint fuel), Dash pip charges, Sprint indicator.
/// </summary>
public class MinimalHUD : MonoBehaviour
{


    // ── Private ────────────────────────────────────────────────────────────────
    private PlayerController player;

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

    }

    // ── IMGUI ─────────────────────────────────────────────────────────────────
    void OnGUI()
    {

    }

    private void DrawBar(Rect rect, float value, Texture2D fillTexture, Color shadowColor)
    {
        GUI.DrawTexture(new Rect(rect.x - 2f, rect.y - 2f, rect.width + 4f, rect.height + 4f), darkTex);
        GUI.DrawTexture(rect, bgTex);

        Rect fillRect = rect;
        fillRect.width *= Mathf.Clamp01(value);
        GUI.DrawTexture(fillRect, fillTexture);

        Color previous = GUI.color;
        GUI.color = shadowColor;
        GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - 3f, fillRect.width, 3f), darkTex);
        GUI.color = previous;
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

        if (player != null)
            currentStamina = player.StaminaFraction;
    }

    private void InitStyles()
    {

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
