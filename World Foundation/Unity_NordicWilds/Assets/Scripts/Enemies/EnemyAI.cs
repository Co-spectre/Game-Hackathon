using UnityEngine;
using System.Collections;
using NordicWilds.Player;
using NordicWilds.Combat;
using NordicWilds.Visuals;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Health))]
public class EnemyAI : MonoBehaviour, IDamageable
{
    private enum State { Idle, Chase, Attack, Stagger, Dead }
    private State currentState = State.Idle;
    public bool IsParryStaggered { get; private set; } = false;


    [Header("Stats")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float detectRange = 20f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.8f;
    [SerializeField] private float staggerDuration = 0.25f;
    [SerializeField] private float hitFlashDuration = 0.1f;

    [Header("Fail-Safes")]
    [SerializeField] private float fallYThreshold = -8f;
    [SerializeField] private float recoveryHeightOffset = 1.25f;

    [Header("Visuals (Juice)")]
    [SerializeField] private Material hitFlashMat;
    private Material defaultMaterial;
    private MeshRenderer meshRenderer;

    private Transform playerTarget;
    private Rigidbody rb;
    private Health health;
    private float nextAttackTime;
    private Coroutine staggerRoutine;
    private Vector3 spawnAnchor;
    private bool rushOnStart;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        health = GetComponent<Health>();

        ProceduralCharacterVisual.Build(transform, CharacterVisualPreset.NordicEnemy);


        meshRenderer = FindVisibleRenderer();
        if (meshRenderer != null)
            defaultMaterial = meshRenderer.sharedMaterial;

        if (health != null)
        {
            health.OnDamageTaken += HandleDamageTaken;
            health.OnDeath += HandleDeath;
        }
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerTarget = playerObj.transform;

        spawnAnchor = transform.position;

        currentState = rushOnStart && playerTarget != null ? State.Chase : State.Idle;
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamageTaken -= HandleDamageTaken;
            health.OnDeath -= HandleDeath;
        }
    }

    private void Update()
    {
        if (transform.position.y < fallYThreshold)
        {
            RecoverFromFall();
            return;
        }

        if (currentState == State.Dead || currentState == State.Stagger || playerTarget == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (currentState == State.Idle && distance < detectRange)
            currentState = State.Chase;

        if (currentState == State.Chase)
        {
            if (distance <= attackRange)
            {
                currentState = State.Attack;
                rb.linearVelocity = Vector3.zero;
            }
        }

        if (currentState == State.Attack)
        {
            if (Time.time >= nextAttackTime)
                StartCoroutine(AttackRoutine());

            if (distance > attackRange)
                currentState = State.Chase;
        }
    }

    private void FixedUpdate()
    {
        if (currentState != State.Chase || playerTarget == null)
            return;

        Vector3 direction = playerTarget.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Vector3 moveDirection = direction.normalized;
        if (rushOnStart)
            rb.linearVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed);
        else
            rb.AddForce(moveDirection * speed, ForceMode.Acceleration);

        rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(moveDirection), Time.fixedDeltaTime * 10f);
    }

    private IEnumerator AttackRoutine()
    {
        nextAttackTime = Time.time + attackCooldown;

        transform.localScale += new Vector3(0.2f, 0.2f, 0.2f);
        yield return new WaitForSeconds(0.25f);
        transform.localScale -= new Vector3(0.2f, 0.2f, 0.2f);

        if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) <= attackRange + 0.5f)
        {
            Health playerHealth = playerTarget.GetComponent<Health>();
            if (playerHealth != null)
            {
                Vector3 hitDirection = (playerTarget.position - transform.position).normalized;
                var damageInfo = new DamageInfo(
                    attackDamage,
                    gameObject,
                    playerTarget.position,
                    hitDirection,
                    DamageType.Physical,
                    knockbackForce: 12f,
                    staggerDuration: 0.15f,
                    canBeBlocked: false,
                    isCritical: false);

                playerHealth.TakeDamage(damageInfo);
            }

            if (CameraJuiceManager.Instance != null)
                CameraJuiceManager.Instance.ShakeCamera(0.3f, 0.5f);
        }

        if (currentState != State.Dead)
            currentState = State.Chase;
    }

    public void TakeDamage(float amount)
    {
        if (health == null) return;

        health.TakeDamage(amount);
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (health == null) return;

        health.TakeDamage(damageInfo);
    }

    public void SetHitFlashMaterial(Material material)
    {
        hitFlashMat = material;
    }

    public void SetAggression(float newSpeed, float newDetectRange, float newAttackRange)
    {
        speed = Mathf.Max(0f, newSpeed);
        detectRange = Mathf.Max(0f, newDetectRange);
        attackRange = Mathf.Max(0.1f, newAttackRange);
    }

    public void RushPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerTarget = playerObj.transform;

        if (currentState != State.Dead && playerTarget != null)
        {
            rushOnStart = true;
            currentState = State.Chase;
            if (rb != null)
            {
                rb.WakeUp();
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void HandleDamageTaken(DamageInfo damageInfo)
    {
        if (currentState == State.Dead)
            return;

        if (meshRenderer != null && hitFlashMat != null)
            meshRenderer.sharedMaterial = hitFlashMat;

        if (staggerRoutine != null)
        {
            StopCoroutine(staggerRoutine);
            // If we interrupted a parry stagger, clear the flag immediately
            // so the bonus-damage window doesn't get stuck on forever.
            IsParryStaggered = false;
        }

        staggerRoutine = StartCoroutine(StaggerRoutine());
    }

    private IEnumerator StaggerRoutine()
    {
        currentState = State.Stagger;
        float flashDuration = Mathf.Clamp(hitFlashDuration, 0f, staggerDuration);

        if (flashDuration > 0f)
            yield return new WaitForSeconds(flashDuration);

        if (meshRenderer != null)
            meshRenderer.sharedMaterial = defaultMaterial;

        float remainingStagger = Mathf.Max(0f, staggerDuration - flashDuration);
        if (remainingStagger > 0f)
            yield return new WaitForSeconds(remainingStagger);

        if (currentState != State.Dead)
            currentState = State.Chase;
    }

    private void HandleDeath(GameObject deceased)
    {
        currentState = State.Dead;

        if (staggerRoutine != null)
        {
            StopCoroutine(staggerRoutine);
            staggerRoutine = null;
        }
    }

    /// <summary>
    /// Called by Health.TriggerParry when the player executes a perfect parry.
    /// Knocks the enemy to the ground and marks them as vulnerable for <paramref name="duration"/> seconds.
    /// </summary>
    public void ApplyParryStagger(float duration)
    {
        if (currentState == State.Dead) return;

        // Stop only the current stagger coroutine — don't kill unrelated routines
        if (staggerRoutine != null)
        {
            StopCoroutine(staggerRoutine);
            IsParryStaggered = false; // clean up in case we're interrupting a prior parry
        }

        staggerRoutine = StartCoroutine(ParryStaggerRoutine(duration));
    }

    /// <summary>
    /// Knockdown without opening the parry bonus-damage window.
    /// Used by the player's finisher (3rd combo hit).
    /// </summary>
    public void ApplyKnockdown(float duration)
    {
        if (currentState == State.Dead) return;
        if (staggerRoutine != null)
        {
            StopCoroutine(staggerRoutine);
            IsParryStaggered = false;
        }
        staggerRoutine = StartCoroutine(KnockdownRoutine(duration));
    }

    private IEnumerator KnockdownRoutine(float duration)
    {
        currentState = State.Stagger;
        if (rb != null) rb.linearVelocity = Vector3.zero;

        Quaternion standingRot = transform.rotation;
        Quaternion fallenRot   = standingRot * Quaternion.Euler(90f, 0f, 0f);

        // Fall
        float elapsed = 0f;
        while (elapsed < 0.25f)
        {
            float e = 1f - Mathf.Pow(1f - elapsed / 0.25f, 3f);
            transform.rotation = Quaternion.Slerp(standingRot, fallenRot, e);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = fallenRot;

        yield return new WaitForSeconds(duration);

        // Rise
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            float e = 1f - Mathf.Pow(1f - elapsed / 0.3f, 3f);
            transform.rotation = Quaternion.Slerp(fallenRot, standingRot, e);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = standingRot;

        staggerRoutine = null;
        if (currentState != State.Dead) currentState = State.Chase;
    }

    private IEnumerator ParryStaggerRoutine(float duration)
    {
        IsParryStaggered = true;
        currentState     = State.Stagger;

        if (rb != null) rb.linearVelocity = Vector3.zero;

        // ── Knockdown: rotate enemy to lie on ground ──────────────────────────
        Quaternion standingRot = transform.rotation;
        Quaternion fallenRot   = standingRot * Quaternion.Euler(90f, 0f, 0f);

        float elapsed = 0f, fallTime = 0.25f;
        while (elapsed < fallTime)
        {
            float eased = 1f - Mathf.Pow(1f - elapsed / fallTime, 3f);
            transform.rotation = Quaternion.Slerp(standingRot, fallenRot, eased);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = fallenRot;

        // Flash red while down (vulnerable visual)
        if (meshRenderer != null && hitFlashMat != null)
            meshRenderer.sharedMaterial = hitFlashMat;

        // ── Wait vulnerable duration ──────────────────────────────────────────
        yield return new WaitForSeconds(duration);

        // ── Stand back up ─────────────────────────────────────────────────────
        elapsed = 0f; float riseTime = 0.3f;
        while (elapsed < riseTime)
        {
            float eased = 1f - Mathf.Pow(1f - elapsed / riseTime, 3f);
            transform.rotation = Quaternion.Slerp(fallenRot, standingRot, eased);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = standingRot;

        // Restore material
        if (meshRenderer != null && defaultMaterial != null)
            meshRenderer.sharedMaterial = defaultMaterial;

        IsParryStaggered = false;
        staggerRoutine   = null;

        if (currentState != State.Dead)
            currentState = State.Chase;
    }

    /// <summary>
    /// Called by DeathScreen when the player respawns.
    /// Restores full health and returns the enemy to its spawn position in Idle state.
    /// </summary>
    public void ResetEnemy()
    {
        // Stop any running coroutines (attacks, staggers)
        StopAllCoroutines();
        staggerRoutine = null;
        IsParryStaggered = false;


        // Restore position to spawn anchor
        if (rb != null)
        {
            rb.linearVelocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = spawnAnchor;
        }
        transform.position = spawnAnchor;
        transform.rotation = Quaternion.identity;

        // Restore material
        if (meshRenderer != null && defaultMaterial != null)
            meshRenderer.sharedMaterial = defaultMaterial;

        // Restore state (this also resets health via Health component)
        if (health != null)
        {
            // Use reflection-free approach: call ForceRespawn if it exists,
            // otherwise just heal to full via Heal()
            var forceRespawn = health.GetType().GetMethod("ForceRespawn");
            if (forceRespawn != null)
                forceRespawn.Invoke(health, new object[] { spawnAnchor });
            else
                health.Heal(9999f); // heal to full
        }

        currentState = State.Idle;
    }


    private void RecoverFromFall()
    {
        Vector3 safePosition = spawnAnchor;
        safePosition.y = Mathf.Max(spawnAnchor.y, 0f) + recoveryHeightOffset;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = safePosition;
        transform.position = safePosition;

        if (currentState != State.Dead)
            currentState = State.Chase;
    }

    private MeshRenderer FindVisibleRenderer()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer != null && renderer.transform != transform)
                return renderer;
        }

        return GetComponent<MeshRenderer>();
    }
}
