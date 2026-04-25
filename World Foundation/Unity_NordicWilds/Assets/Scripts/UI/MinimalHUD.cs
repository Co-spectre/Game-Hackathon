using UnityEngine;

using NordicWilds.Combat;
using NordicWilds.Player;

public class MinimalHUD : MonoBehaviour
{
    [SerializeField] private float maxHealthWidth = 180f;
    [SerializeField] private float maxStaminaWidth = 140f;

    public float currentHealth = 1f; // 0 to 1
    public float currentStamina = 1f; // 0 to 1
    public string weaponName = "Leviathan Axe";

    private GUIStyle textStyle;
    private Texture2D healthTex;
    private Texture2D staminaTex;
    private Texture2D dashPipTex;
    private Texture2D bgTex;

    private PlayerController player;
    private Health playerHealth;

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
    }

    void OnGUI()
    {
        if (textStyle == null)
        {
            textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = 16;
            textStyle.fontStyle = FontStyle.Bold;
            textStyle.normal.textColor = new Color(1, 1, 1, 0.8f);
        }

        currentHealth = Mathf.Clamp01(currentHealth);
        currentStamina = Mathf.Clamp01(currentStamina);

        float startX = 30f;
        float startY = Screen.height - 80f;

        GUI.DrawTexture(new Rect(startX, startY, maxHealthWidth, 8), bgTex);
        GUI.DrawTexture(new Rect(startX, startY, maxHealthWidth * currentHealth, 8), healthTex);

        GUI.DrawTexture(new Rect(startX, startY + 12, maxStaminaWidth, 6), bgTex);
        GUI.DrawTexture(new Rect(startX, startY + 12, maxStaminaWidth * currentStamina, 6), staminaTex);

        if (player != null)
        {
            for (int i = 0; i < player.maxDashes; i++)
            {
                float pipX = startX + (i * 20f);
                float pipY = startY + 22;
                GUI.DrawTexture(new Rect(pipX, pipY, 15, 6), bgTex);
                if (i < player.currentDashes)
                    GUI.DrawTexture(new Rect(pipX, pipY, 15, 6), dashPipTex);
            }

            GUI.Label(new Rect(startX, startY + 32, 220, 30), weaponName, textStyle);
        }
        else
        {
            GUI.Label(new Rect(startX, startY + 20, 220, 30), weaponName, textStyle);
        }
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
            currentHealth = Mathf.Clamp01(playerHealth.CurrentHealth / playerHealth.MaxHealth);
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
        healthTex = MakeTex(2, 2, new Color(0.8f, 0.1f, 0.2f, 0.9f));
        staminaTex = MakeTex(2, 2, new Color(0.2f, 0.7f, 0.9f, 0.9f));
        dashPipTex = MakeTex(2, 2, new Color(1f, 0.8f, 0.2f, 0.9f));
        bgTex = MakeTex(2, 2, new Color(0, 0, 0, 0.5f));
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
