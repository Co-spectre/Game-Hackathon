using System;
using UnityEngine;
using UnityEngine.Events;
using NordicWilds.Player;

namespace NordicWilds.Combat
{
    /// <summary>
    /// Universal health and damage system for both the Player and Enemies.
    /// Incorporates Invincibility Frames (iFrames) used heavily in Hades.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        private float currentHealth;
        private bool isDead = false;

        [Header("Death Behavior")]
        [SerializeField] private bool treatAsPlayer = false;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private float destroyDelay = 0.1f;
        [SerializeField] private bool allowRespawn = true;
        [SerializeField] private float respawnDelay = 1.25f;
        [SerializeField] [Range(0.1f, 1f)] private float respawnHealthPercent = 0.75f;

        [Header("iFrames (Post-hit Invincibility)")]
        [SerializeField] private bool  canIncurDamage = true;
        [Tooltip("Seconds of invincibility granted after taking a hit.")]
        [SerializeField] private float invincibilityDuration = 0.75f;
        [Tooltip("How fast the player blinks during iFrames (blinks per second).")]
        [SerializeField] private float blinkFrequency = 12f;
        [Tooltip("Scale applied to incoming KnockbackForce. 1 = normal, 0 = no knockback.")]
        [SerializeField] private float knockbackMultiplier = 1f;
        private float lastHitTime = -Mathf.Infinity;
        private System.Collections.IEnumerator _iFrameRoutine;

        [Header("Hit Flash")]
        [Tooltip("Color the renderer briefly turns when hit.")]
        [SerializeField] private Color  hitFlashColor    = new Color(1f, 0.15f, 0.15f, 1f); // vivid red
        [Tooltip("Seconds to lerp from flash color back to normal.")]
        [SerializeField] private float  hitFlashDuration = 0.2f;
        private Color  _originalColor  = Color.white;
        private bool   _hasMaterialColor = false;          // true if the material supports _Color
        private Material _instancedMat;                    // per-instance material (avoid shared mat mutation)
        private System.Collections.IEnumerator _flashRoutine;


        [Header("Health Regeneration")]
        [Tooltip("HP restored per second during regeneration (player only).")]
        [SerializeField] private float healthRegenRate  = 5f;
        [Tooltip("Seconds after the last hit before regeneration begins.")]
        [SerializeField] private float healthRegenDelay = 6f;
        [Tooltip("Enable automatic health regeneration for this object.")]
        [SerializeField] private bool  enableHealthRegen = true;


        [Header("Events")]
        public UnityEvent onTookDamage;
        public UnityEvent onHealed;
        public event System.Action<DamageInfo> OnDamageTaken;
        public Action<GameObject> OnDeath;

        private PlayerController playerController;
        private Rigidbody         body;
        private Renderer          _renderer;
        private Vector3           respawnPosition;
        private bool              isRespawning;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float CurrentHealthNormalized => maxHealth <= 0f ? 0f : currentHealth / maxHealth;
        public bool IsDead => isDead;

        private void Awake()
        {
            if (maxHealth <= 0f)
                maxHealth = 1f;

            currentHealth = maxHealth;

            playerController = GetComponent<PlayerController>();
            body             = GetComponent<Rigidbody>();
            respawnPosition  = transform.position;
            _renderer        = GetComponentInChildren<Renderer>();

            // Cache original material color so we can always lerp back to it.
            // Use .material (instanced) to avoid mutating a shared asset.
            if (_renderer != null)
            {
                _instancedMat = _renderer.material;   // creates a per-instance copy
                _hasMaterialColor = _instancedMat.HasProperty("_Color");
                if (_hasMaterialColor) _originalColor = _instancedMat.color;
            }

            if (!treatAsPlayer)
                treatAsPlayer = playerController != null || CompareTag("Player");

            if (treatAsPlayer)
                destroyOnDeath = false;
        }

        private void Start()
        {
            // Re-check in Start in case PlayerController wasn't ready during Awake
            if (playerController == null)
                playerController = GetComponent<PlayerController>();

            if (!treatAsPlayer)
                treatAsPlayer = playerController != null || CompareTag("Player");

            if (treatAsPlayer)
                destroyOnDeath = false;
        }

        private void Update()
        {
            // Only regen for player-type objects that have the feature enabled
            if (!enableHealthRegen || !treatAsPlayer) return;
            if (isDead || isRespawning) return;
            if (currentHealth >= maxHealth) return;

            // Wait for regenDelay seconds after the last hit before starting
            if (Time.time < lastHitTime + healthRegenDelay) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + healthRegenRate * Time.deltaTime);
            onHealed?.Invoke();
        }

        public void TakeDamage(float amount)
        {
            TakeDamage(new DamageInfo(amount, gameObject, transform.position, Vector3.zero, DamageType.Physical));
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!canIncurDamage || isDead || isRespawning) return;

            if (damageInfo.Amount <= 0f) return;

            if (playerController != null)
            {
                if (playerController.isInvincible || playerController.CurrentState == PlayerController.PlayerState.Dashing)
                    return;

                // ── PARRY (perfect timing) ────────────────────────────────────────
                if (playerController.IsParryWindowOpen)
                {
                    TriggerParry(damageInfo);
                    return; // parry fully negates the hit
                }

                // ── BLOCK (holding Q, parry window expired) ───────────────────────
                // Costs 25 % of max stamina per blocked hit.
                // If stamina can't cover the full cost, the leftover is taken from HP.
                if (playerController.IsBlocking)
                {
                    float staminaCost = playerController.maxStamina * 0.25f;
                    float overflow    = playerController.DrainStaminaForBlock(staminaCost);

                    if (overflow <= 0f)
                    {
                        // Stamina absorbed the entire hit — no health damage at all
                        lastHitTime = Time.time; // still refresh iFrame timer
                        onTookDamage?.Invoke();  // trigger any UI feedback
                        return;
                    }

                    // Stamina ran dry — only the uncovered overflow fraction reaches HP
                    // Scale it proportionally: overflow / staminaCost × original damage
                    float overflowDamage = damageInfo.Amount * (overflow / staminaCost);
                    damageInfo = new DamageInfo(overflowDamage, damageInfo.Source, damageInfo.HitPoint,
                                                damageInfo.HitDirection, damageInfo.Type,
                                                knockbackForce: damageInfo.KnockbackForce * 0.4f,
                                                staggerDuration: 0f, canBeBlocked: false, isCritical: false);
                }
            }

            // Check iFrames — block if still within invincibility window
            if (Time.time < lastHitTime + invincibilityDuration) return;

            currentHealth = Mathf.Max(0f, currentHealth - damageInfo.Amount);
            lastHitTime   = Time.time;

            // Apply knockback impulse using the direction and force from DamageInfo
            ApplyKnockback(damageInfo);

            OnDamageTaken?.Invoke(damageInfo);
            onTookDamage?.Invoke();

            // Grant post-hit invincibility (cancel any existing iFrame window first)
            if (_iFrameRoutine != null) StopCoroutine(_iFrameRoutine);
            _iFrameRoutine = PostHitInvincibilityRoutine();
            StartCoroutine(_iFrameRoutine);

            // Hit color flash (cancel and restart so rapid hits always flash)
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = HitFlashRoutine();
            StartCoroutine(_flashRoutine);


            if (currentHealth <= 0) Die();
        }

        private System.Collections.IEnumerator HitFlashRoutine()
        {
            if (!_hasMaterialColor || _instancedMat == null) yield break;

            // Snap to flash color instantly
            _instancedMat.color = hitFlashColor;

            // Lerp back to original over hitFlashDuration
            float elapsed = 0f;
            while (elapsed < hitFlashDuration)
            {
                float t = elapsed / hitFlashDuration;
                // Ease-out so the flash fades quickly at first then settles
                float eased = 1f - (1f - t) * (1f - t);
                if (_instancedMat != null)
                    _instancedMat.color = Color.Lerp(hitFlashColor, _originalColor, eased);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_instancedMat != null) _instancedMat.color = _originalColor;
            _flashRoutine = null;
        }


        private void ApplyKnockback(DamageInfo damageInfo)
        {
            if (body == null) return;
            if (damageInfo.KnockbackForce <= 0f || knockbackMultiplier <= 0f) return;

            // Determine knockback direction: use HitDirection if provided,
            // otherwise push away from the attacker's position
            Vector3 dir = damageInfo.HitDirection;
            if (dir.sqrMagnitude < 0.01f && damageInfo.Source != null)
                dir = (transform.position - damageInfo.Source.transform.position).normalized;
            if (dir.sqrMagnitude < 0.01f)
                dir = -transform.forward; // last resort: push backward

            dir.y = 0f;          // keep knockback horizontal
            dir.Normalize();

            // Zero vertical velocity so knockback doesn't launch the player upward
            body.linearVelocity = new Vector3(body.linearVelocity.x, 0f, body.linearVelocity.z);

            float force = damageInfo.KnockbackForce * knockbackMultiplier;
            body.AddForce(dir * force, ForceMode.VelocityChange);
        }

        private void TriggerParry(DamageInfo damageInfo)
        {
            // ── Weapon & body parry animation ─────────────────────────────────────
            var weaponAnimator = GetComponent<NordicWilds.Combat.WeaponAnimator>();
            if (weaponAnimator != null)
                weaponAnimator.PlayParry(damageInfo.HitPoint != Vector3.zero
                    ? damageInfo.HitPoint
                    : transform.position + transform.forward * 0.6f + Vector3.up * 1.1f);

            // ── Stagger the attacker ──────────────────────────────────────────────
            if (damageInfo.Source != null)
            {
                EnemyAI enemy = damageInfo.Source.GetComponent<EnemyAI>();
                if (enemy != null && playerController != null)
                    enemy.ApplyParryStagger(playerController.parryStaggerDuration);
            }

            // ── Camera & time-freeze juice ────────────────────────────────────────
            if (CameraJuiceManager.Instance != null)
            {
                CameraJuiceManager.Instance.HitStop(0.12f);
                CameraJuiceManager.Instance.ShakeCamera(0.25f, 0.5f);
            }

            // ── Brief body color flash (gold) ─────────────────────────────────────
            if (_hasMaterialColor && _instancedMat != null)
            {
                if (_flashRoutine != null) StopCoroutine(_flashRoutine);
                StartCoroutine(ParryFlashRoutine());
            }

            // ── Player gets brief invincibility after a parry ─────────────────────
            lastHitTime = Time.time;
            if (playerController != null) playerController.isInvincible = true;
            StartCoroutine(ClearParryInvincibility(0.3f));
        }


        private System.Collections.IEnumerator ParryFlashRoutine()
        {
            Color parryColor = new Color(1f, 0.9f, 0.2f, 1f); // gold flash
            if (_instancedMat != null) _instancedMat.color = parryColor;
            float elapsed = 0f;
            while (elapsed < 0.25f)
            {
                float eased = 1f - (1f - elapsed / 0.25f) * (1f - elapsed / 0.25f);
                if (_instancedMat != null)
                    _instancedMat.color = Color.Lerp(parryColor, _originalColor, eased);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (_instancedMat != null) _instancedMat.color = _originalColor;
        }

        private System.Collections.IEnumerator ClearParryInvincibility(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (playerController != null) playerController.isInvincible = false;
        }


        private System.Collections.IEnumerator PostHitInvincibilityRoutine()
        {
            // Signal invincibility to PlayerController so other systems see it
            if (playerController != null) playerController.isInvincible = true;

            // Visual blink feedback — toggle renderer every half-period
            float elapsed   = 0f;
            float halfPeriod = 1f / (blinkFrequency * 2f);
            bool  visible   = true;

            while (elapsed < invincibilityDuration)
            {
                visible = !visible;
                if (_renderer != null) _renderer.enabled = visible;
                yield return new WaitForSeconds(halfPeriod);
                elapsed += halfPeriod;
            }

            // Restore visibility and clear invincibility flag
            if (_renderer != null) _renderer.enabled = true;
            if (playerController != null) playerController.isInvincible = false;
            _iFrameRoutine = null;
        }

        public void Heal(float amount)
        {
            if (isDead) return;

            currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
            onHealed?.Invoke();
        }

        public void SetInvincible(bool state)
        {
            // Used by the PlayerController during the Dash mechanic to simulate Hades' dodge IFrames
            canIncurDamage = !state; 
        }

        public void SetRespawnPoint(Vector3 point)
        {
            respawnPosition = point;
        }

        private void Die()
        {
            if (isDead) return;

            isDead = true;
            OnDeath?.Invoke(gameObject);

            if (treatAsPlayer)
            {
                // Auto-create the DeathScreen if it hasn't been placed in the scene.
                // This means no manual setup step is required — it always works.
                if (NordicWilds.UI.DeathScreen.Instance == null)
                {
                    var go = new GameObject("DeathScreen");
                    go.AddComponent<NordicWilds.UI.DeathScreen>();
                    Debug.Log("[Health] DeathScreen auto-created at runtime.");
                }

                NordicWilds.UI.DeathScreen.Instance.Activate(playerController);
                return;
            }

            if (destroyOnDeath)
                Destroy(gameObject, destroyDelay);
        }


        /// <summary>
        /// Called by DeathScreen after the player clicks Respawn.
        /// Restores health and resets all death-related state.
        /// </summary>
        public void ForceRespawn(Vector3 spawnPoint)
        {
            if (_iFrameRoutine != null) { StopCoroutine(_iFrameRoutine); _iFrameRoutine = null; }
            if (_renderer != null) _renderer.enabled = true;

            // Restore to FULL health on respawn
            currentHealth   = maxHealth;
            lastHitTime     = -Mathf.Infinity; // allow regen immediately after spawn
            isDead          = false;
            isRespawning    = false;
            respawnPosition = spawnPoint;

            onHealed?.Invoke();
        }

        private System.Collections.IEnumerator RespawnRoutine()
        {
            isRespawning = true;

            if (playerController != null)
                playerController.SetControlsLocked(true);

            if (body != null)
                body.linearVelocity = Vector3.zero;

            yield return new WaitForSeconds(respawnDelay);

            Vector3 safePoint = respawnPosition;
            if (safePoint == Vector3.zero)
                safePoint = transform.position;

            if (body != null)
                body.position = safePoint;

            transform.position = safePoint;

            currentHealth = Mathf.Clamp(maxHealth * respawnHealthPercent, 1f, maxHealth);
            lastHitTime = Time.time;
            isDead = false;
            isRespawning = false;

            if (playerController != null)
            {
                playerController.StopAllMotion();
                playerController.SetControlsLocked(false);
            }

            onHealed?.Invoke();
        }
    }
}