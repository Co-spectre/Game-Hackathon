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

        currentState = State.Idle;
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

    private void HandleDamageTaken(DamageInfo damageInfo)
    {
        if (currentState == State.Dead)
            return;

        if (meshRenderer != null && hitFlashMat != null)
            meshRenderer.sharedMaterial = hitFlashMat;

        if (staggerRoutine != null)
            StopCoroutine(staggerRoutine);

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
