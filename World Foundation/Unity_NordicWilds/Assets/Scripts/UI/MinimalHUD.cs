using UnityEngine;

using NordicWilds.Combat;
using NordicWilds.Player;

public class MinimalHUD : MonoBehaviour
{
    [SerializeField] private float maxHealthWidth = 190f;
    [SerializeField] private float maxStaminaWidth = 168f;

    public float currentHealth = 1f; // 0 to 1
    public float currentStamina = 1f; // 0 to 1
    public string weaponName = "Leviathan Axe";

    private GUIStyle labelStyle;
    private GUIStyle valueStyle;
    private GUIStyle weaponStyle;
    private Texture2D healthTex;
    private Texture2D staminaTex;
    private Texture2D dashPipTex;
    private Texture2D bgTex;
    private Texture2D darkTex;
    private Texture2D hudBoardTex;
    private Texture2D noticeBoardTex;
    private Texture2D dividerTex;

    private PlayerController player;
    private Health playerHealth;
    private float currentHealthAmount = 100f;
    private float maxHealthAmount = 100f;

    void Start()
    {
        CreateTextures();
        RefreshReferences();

        UpdateBarsFromPlayer();
    }

    void Update()
    {
        if (player == null || playerHealth == null)
            RefreshReferences();

        UpdateBarsFromPlayer();
    }

    void OnDestroy()
    {
        DestroyTexture(healthTex);
        DestroyTexture(staminaTex);
        DestroyTexture(dashPipTex);
        DestroyTexture(bgTex);
        DestroyTexture(darkTex);
    }

    void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 13;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = new Color(0.22f, 0.16f, 0.10f, 0.92f);
            labelStyle.alignment = TextAnchor.MiddleLeft;

            valueStyle = new GUIStyle(labelStyle);
            valueStyle.alignment = TextAnchor.MiddleRight;
            valueStyle.fontSize = 12;

            weaponStyle = new GUIStyle(labelStyle);
            weaponStyle.fontSize = 14;
            weaponStyle.alignment = TextAnchor.MiddleCenter;
            weaponStyle.normal.textColor = new Color(0.96f, 0.88f, 0.68f, 0.95f);
        }

        currentHealth = Mathf.Clamp01(currentHealth);
        currentStamina = Mathf.Clamp01(currentStamina);

        float scale = Mathf.Clamp(Screen.height / 1080f, 0.78f, 1.05f);
        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousColor = GUI.color;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

        float screenHeight = Screen.height / scale;
        float startX = 24f;
        float startY = screenHeight - 162f;
        Rect panel = new Rect(startX, startY, 332f, 132f);

        DrawTexturedPanel(panel, hudBoardTex, new Color(0.06f, 0.04f, 0.03f, 0.82f));

        Rect hpLabel = new Rect(startX + 38f, startY + 28f, 58f, 20f);
        Rect hpBar = new Rect(startX + 104f, startY + 31f, maxHealthWidth, 14f);
        GUI.Label(hpLabel, "VITAL", labelStyle);
        DrawBar(hpBar, currentHealth, healthTex, new Color(0.48f, 0.04f, 0.08f, 0.95f));
        GUI.Label(new Rect(hpBar.x, hpBar.y - 1f, hpBar.width - 8f, 16f),
            Mathf.RoundToInt(currentHealthAmount) + " / " + Mathf.RoundToInt(maxHealthAmount), valueStyle);

        Rect divider = new Rect(startX + 42f, startY + 56f, 248f, 4f);
        if (dividerTex != null)
            GUI.DrawTexture(divider, dividerTex, ScaleMode.StretchToFill, true);

        Rect staminaLabel = new Rect(startX + 38f, startY + 66f, 72f, 20f);
        Rect staminaBar = new Rect(startX + 122f, startY + 69f, maxStaminaWidth, 12f);
        GUI.Label(staminaLabel, "STAMINA", labelStyle);
        DrawBar(staminaBar, currentStamina, staminaTex, new Color(0.05f, 0.20f, 0.36f, 0.95f));

        if (player != null && player.maxDashes > 0)
        {
            for (int i = 0; i < player.maxDashes; i++)
            {
                float pipX = startX + 122f + (i * 28f);
                float pipY = startY + 88f;
                GUI.DrawTexture(new Rect(pipX, pipY, 21f, 7f), bgTex);
                if (i < player.currentDashes)
                    GUI.DrawTexture(new Rect(pipX, pipY, 21f, 7f), dashPipTex);
            }
        }

        Rect weaponPanel = new Rect(startX + 58f, startY + 101f, 218f, 30f);
        if (noticeBoardTex != null)
            GUI.DrawTexture(weaponPanel, noticeBoardTex, ScaleMode.StretchToFill, true);
        else
            GUI.DrawTexture(weaponPanel, darkTex);
        GUI.Label(weaponPanel, weaponName, weaponStyle);

        GUI.color = previousColor;
        GUI.matrix = previousMatrix;
    }

    private void DrawTexturedPanel(Rect rect, Texture2D texture, Color fallbackColor)
    {
        Color previous = GUI.color;
        if (texture != null)
        {
            GUI.color = Color.white;
            GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
        }
        else
        {
            GUI.color = fallbackColor;
            GUI.DrawTexture(rect, darkTex);
        }
        GUI.color = previous;
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

    private void RefreshReferences()
    {
        GameObject pObj = GameObject.FindWithTag("Player");
        if (pObj != null) player = pObj.GetComponent<PlayerController>();

        if (player != null)
            playerHealth = player.GetComponent<Health>();
    }

    private void UpdateBarsFromPlayer()
    {
        if (playerHealth != null && playerHealth.MaxHealth > 0f)
        {
            currentHealthAmount = playerHealth.CurrentHealth;
            maxHealthAmount = playerHealth.MaxHealth;
            currentHealth = Mathf.Clamp01(currentHealthAmount / maxHealthAmount);
        }

        if (player != null)
        {
            if (player.maxDashes > 0)
                currentStamina = Mathf.Clamp01((float)player.currentDashes / player.maxDashes);

            if (playerHealth == null)
                currentHealth = Mathf.Clamp01(currentHealth);
        }
    }

    private void CreateTextures()
    {
        hudBoardTex = Resources.Load<Texture2D>("UI/FantasyWooden/hud_board");
        noticeBoardTex = Resources.Load<Texture2D>("UI/FantasyWooden/notice_board");
        dividerTex = Resources.Load<Texture2D>("UI/FantasyWooden/divider");
        healthTex = MakeTex(2, 2, new Color(0.8f, 0.1f, 0.2f, 0.9f));
        staminaTex = MakeTex(2, 2, new Color(0.12f, 0.55f, 0.88f, 0.9f));
        dashPipTex = MakeTex(2, 2, new Color(1f, 0.8f, 0.2f, 0.9f));
        bgTex = MakeTex(2, 2, new Color(0.05f, 0.035f, 0.025f, 0.66f));
        darkTex = MakeTex(2, 2, new Color(0.02f, 0.014f, 0.01f, 0.82f));
    }

    private void DestroyTexture(Texture2D texture)
    {
        if (texture != null)
        {
            Destroy(texture);
        }
    }

    public void UpdateWeapon(string newWeapon)
    {
        weaponName = newWeapon;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        result.hideFlags = HideFlags.HideAndDontSave;
        return result;
    }
}
