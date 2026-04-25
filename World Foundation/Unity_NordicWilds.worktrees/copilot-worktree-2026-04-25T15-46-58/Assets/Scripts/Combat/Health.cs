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

        [Header("iFrames (Hades Mechanics)")]
        [SerializeField] private bool canIncurDamage = true;
        [SerializeField] private float invincibilityDuration = 0.2f; // Time after being hit
        private float lastHitTime = -Mathf.Infinity;

        [Header("Events")]
        public UnityEvent onTookDamage;
        public UnityEvent onHealed;
        public event System.Action<DamageInfo> OnDamageTaken;
        public Action<GameObject> OnDeath;

        private PlayerController playerController;
        private Rigidbody body;
        private Vector3 respawnPosition;
        private bool isRespawning;

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
            body = GetComponent<Rigidbody>();
            respawnPosition = transform.position;

            if (!treatAsPlayer)
                treatAsPlayer = playerController != null || CompareTag("Player");

            if (treatAsPlayer)
                destroyOnDeath = false;
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
            }

            // Check iFrames
            if (Time.time < lastHitTime + invincibilityDuration) return;

            currentHealth -= damageInfo.Amount;
            lastHitTime = Time.time;
            currentHealth = Mathf.Max(0f, currentHealth);
            
            OnDamageTaken?.Invoke(damageInfo);

            // VFX/SFX hit response
            onTookDamage?.Invoke();

            if (currentHealth <= 0)
            {
                Die();
            }
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
            OnDeath?.Invoke(gameObject); // Notify spawner or room controller
            
            if (treatAsPlayer && allowRespawn)
            {
                StartCoroutine(RespawnRoutine());
                return;
            }

            if (destroyOnDeath)
                Destroy(gameObject, destroyDelay);
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