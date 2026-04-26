using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NordicWilds.Player;
using NordicWilds.UI;

namespace NordicWilds.Combat
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerCombat : MonoBehaviour
    {
        // ── Stats ─────────────────────────────────────────────────────────────────
        [Header("Base Stats")]
        [SerializeField] private float attackRange         = 2f;
        [SerializeField] private float dashStrikeMultiplier = 1.5f;

        [Header("Combo Damage")]
        [Tooltip("Damage for hit 1 (right overhead chop).")]
        [SerializeField] private float damageHit1 = 20f;
        [Tooltip("Damage for hit 2 (left horizontal slash).")]
        [SerializeField] private float damageHit2 = 22f;
        [Tooltip("Damage for hit 3 — spin sweep finisher.")]
        [SerializeField] private float damageHit3 = 50f;

        [Header("Combo Timing")]
        [Tooltip("Seconds after swing starts before the hitbox fires (swing-to-hit delay).")]
        [SerializeField] private float hitDelayNormal   = 0.18f;  // swing travels → hit registers
        [SerializeField] private float hitDelayFinisher = 0.28f;  // spin is slower → longer delay
        [Tooltip("Total cooldown between attacks (includes wind-up + follow-through).")]
        [SerializeField] private float cooldownNormal    = 0.55f;
        [SerializeField] private float cooldownFinisher  = 0.80f;
        [Tooltip("Window in seconds where a new combo hit is still valid after last attack.")]
        [SerializeField] private float comboResetDelay  = 0.85f;
        [Tooltip("Input buffer — how early you can queue the next click.")]
        [SerializeField] private float inputBufferWindow = 0.35f;
        [Tooltip("Fraction of maxStamina drained by the finisher.")]
        [SerializeField] private float finisherStaminaCost = 0.30f;
        [Tooltip("Seconds the enemy is knocked down by the finisher.")]
        [SerializeField] private float finisherKnockdownDuration = 2f;

        [Header("Attack Setup")]
        [SerializeField] private Vector3 attackOffset = new Vector3(0f, 1f, 1.5f);
        [Tooltip("Finisher sweep range (multiplied on top of attackRange).")]
        [SerializeField] private float finisherRangeMultiplier = 1.4f;

        [Header("Weapon Visuals")]
        public Transform weaponTransform;
        public Material  hitMaterial;

        [Header("Blood Effect")]
        [SerializeField] private int   bloodParticlesNormal   = 7;
        [SerializeField] private int   bloodParticlesHeavy    = 16;
        [SerializeField] private float bloodParticleSpeed     = 4f;
        [SerializeField] private float bloodParticleLifetime  = 0.4f;
        [SerializeField] private Color bloodColor             = new Color(0.55f, 0.04f, 0.04f);

        // ── Runtime ───────────────────────────────────────────────────────────────
        private PlayerController controller;
        private WeaponAnimator   _weaponAnimator;
        private float            nextAttackTime  = 0f;
        private Quaternion       initialWeaponRot;
        private float            bufferedAttackTime = -1f;
        private int              comboStep = 0;
        private float            comboDropTime = 0f;
        private Coroutine        resetStateRoutine;

        // ── Lifecycle ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            controller      = GetComponent<PlayerController>();

            _weaponAnimator = GetComponent<WeaponAnimator>();
            if (_weaponAnimator == null)
                _weaponAnimator = gameObject.AddComponent<WeaponAnimator>();

            if (weaponTransform != null)
                initialWeaponRot = weaponTransform.localRotation;
        }

        // ── Update ────────────────────────────────────────────────────────────────
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                bufferedAttackTime = Time.time;

            if (Time.time > comboDropTime)
                comboStep = 0;

            if (CanConsumeBufferedAttack())
            {
                bufferedAttackTime = -1f;
                BeginAttack();
            }

            AnimateWeapon();
        }

        // ── Input guard ───────────────────────────────────────────────────────────
        private bool CanConsumeBufferedAttack()
        {
            if (controller == null)                          return false;
            if (controller.CurrentState == PlayerController.PlayerState.Dashing) return false;
            if (controller.CurrentState == PlayerController.PlayerState.Blocking) return false;
            if (bufferedAttackTime < 0f)                     return false;
            if (Time.time - bufferedAttackTime > inputBufferWindow) return false;
            return Time.time >= nextAttackTime;
        }

        // ── Fallback weapon animation (WeaponAnimator not present) ─────────────
        private void AnimateWeapon()
        {
            if (_weaponAnimator != null) return;
            if (weaponTransform == null || Time.time >= nextAttackTime) return;

            float progress   = 1f - ((nextAttackTime - Time.time) / Mathf.Max(cooldownNormal, 0.001f));
            float startAngle = (comboStep % 2 == 1) ? 90f : -90f;
            weaponTransform.localRotation = initialWeaponRot *
                Quaternion.Euler(0f, Mathf.Lerp(startAngle, -startAngle * 0.2f, progress), 0f);
        }

        // ── Attack entry point ────────────────────────────────────────────────────
        private void BeginAttack()
        {
            comboStep++;
            bool isFinisher = (comboStep % 3 == 0);

            float cooldown = isFinisher ? cooldownFinisher : cooldownNormal;
            float hitDelay = isFinisher ? hitDelayFinisher : hitDelayNormal;

            // Dash strike bonus on first hit
            float rawDamage = GetComboDamage(comboStep);
            if (Time.time - controller.LastDashTime < 0.3f && comboStep == 1)
                rawDamage *= dashStrikeMultiplier;

            nextAttackTime = Time.time + cooldown;
            comboDropTime  = Time.time + cooldown + comboResetDelay;
            controller.SetState(PlayerController.PlayerState.Attacking);

            // Trigger visual swing animation
            if (_weaponAnimator != null)
            {
                if (isFinisher) _weaponAnimator.PlayFinisher();
                else            _weaponAnimator.PlaySwing(comboStep);
            }

            // Schedule state reset
            if (resetStateRoutine != null) StopCoroutine(resetStateRoutine);
            resetStateRoutine = StartCoroutine(ResetStateAfterDelay(cooldown * 0.75f));

            // Launch delayed hitbox coroutine
            StartCoroutine(HitboxRoutine(hitDelay, rawDamage, isFinisher));
        }

        private float GetComboDamage(int step)
        {
            int s = ((step - 1) % 3) + 1;   // normalise: 1, 2, 3, 1, 2, 3 …
            return s switch
            {
                1 => damageHit1,
                2 => damageHit2,
                _ => damageHit3,
            };
        }

        // ── Delayed hitbox ────────────────────────────────────────────────────────
        /// Waits for the swing animation to reach impact point, then fires.
        private IEnumerator HitboxRoutine(float delay, float damage, bool isFinisher)
        {
            yield return new WaitForSeconds(delay);

            float range        = isFinisher ? attackRange * finisherRangeMultiplier : attackRange;
            Vector3 origin     = GetAttackOrigin();
            Collider[] cols    = Physics.OverlapSphere(origin, range,
                                    Physics.AllLayers, QueryTriggerInteraction.Ignore);

            bool             hitSomething = false;
            HashSet<Component> hitTargets = new HashSet<Component>();

            foreach (Collider col in cols)
            {
                if (!TryResolveDamageable(col, out IDamageable damageable, out Component owner))
                    continue;
                if (owner == null || owner.gameObject == gameObject || !hitTargets.Add(owner))
                    continue;

                Vector3 hitPoint = col.ClosestPoint(origin);
                Vector3 hitDir   = (owner.transform.position - origin).normalized;

                // Parry-execution bonus
                float finalDamage       = damage;
                bool  isParryExecution  = false;
                EnemyAI targetEnemy     = owner.GetComponent<EnemyAI>();
                if (targetEnemy != null && targetEnemy.IsParryStaggered)
                {
                    finalDamage    *= controller.parryDamageMultiplier;
                    isParryExecution = true;
                }

                var info = new DamageInfo(
                    finalDamage,
                    gameObject,
                    hitPoint,
                    hitDir,
                    DamageType.Physical,
                    knockbackForce:   0f,
                    staggerDuration:  isFinisher ? 0.4f : 0.15f,
                    canBeBlocked:     false,
                    isCritical:       isFinisher || isParryExecution);

                damageable.TakeDamage(info);
                hitSomething = true;

                // ── Enemy knockdown / physics ─────────────────────────────────────
                Rigidbody enemyRb = col.attachedRigidbody ?? owner.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 push = (owner.transform.position - transform.position).normalized;
                    push.y = 0f;
                    float force = isFinisher ? 18f : (isParryExecution ? 20f : 8f);
                    enemyRb.AddForce(push * force, ForceMode.Impulse);
                }

                if (isFinisher && targetEnemy != null)
                    targetEnemy.ApplyKnockdown(finisherKnockdownDuration);

                // ── Blood burst ───────────────────────────────────────────────────
                StartCoroutine(BloodBurstRoutine(hitPoint, isFinisher || isParryExecution));

                // ── Damage number ─────────────────────────────────────────────────
                var dmgGo = new GameObject("DamageNumber");
                dmgGo.transform.position = enemyRb != null
                    ? enemyRb.position + Vector3.up * 1.5f
                    : owner.transform.position + Vector3.up * 1.5f;
                dmgGo.AddComponent<DamageNumber>().Setup(finalDamage, isFinisher || isParryExecution);
            }

            // Finisher stamina cost (always, hit or miss — commitment matters)
            if (isFinisher)
                controller.DrainStaminaFlat(controller.maxStamina * finisherStaminaCost);

            // ── Camera juice ──────────────────────────────────────────────────────
            if (hitSomething && CameraJuiceManager.Instance != null)
            {
                if (isFinisher)
                {
                    CameraJuiceManager.Instance.HitStop(0.10f);
                    CameraJuiceManager.Instance.ShakeCamera(0.45f, 0.65f);
                }
                else
                {
                    CameraJuiceManager.Instance.HitStop(0.03f);
                    CameraJuiceManager.Instance.ShakeCamera(0.12f, 0.22f);
                }
            }
        }

        // ── Blood burst ───────────────────────────────────────────────────────────
        private IEnumerator BloodBurstRoutine(Vector3 worldPos, bool isHeavy)
        {
            int count = isHeavy ? bloodParticlesHeavy : bloodParticlesNormal;

            for (int i = 0; i < count; i++)
            {
                // Random hemisphere biased slightly upward and forward
                Vector3 dir = Random.insideUnitSphere;
                dir.y = Mathf.Abs(dir.y) * 0.6f + 0.15f;
                dir   = dir.normalized;
                float speed = bloodParticleSpeed * Random.Range(0.6f, 1.4f);
                float size  = isHeavy ? Random.Range(0.04f, 0.10f) : Random.Range(0.03f, 0.07f);
                StartCoroutine(BloodParticle(worldPos, dir, size, speed, bloodParticleLifetime));
            }

            yield return null; // fire-and-forget
        }

        private IEnumerator BloodParticle(Vector3 origin, Vector3 dir, float size, float speed, float life)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "BloodDrop";
            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);

            go.transform.position   = origin;
            go.transform.localScale = Vector3.one * size;

            var mat = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            mat.color = bloodColor;
            mat.hideFlags = HideFlags.HideAndDontSave;
            go.GetComponent<MeshRenderer>().material = mat;

            float   elapsed  = 0f;
            Vector3 velocity = dir * speed;
            const float gravity = 12f;

            while (elapsed < life && go != null)
            {
                float t = elapsed / life;
                velocity.y -= gravity * Time.deltaTime;
                go.transform.position   += velocity * Time.deltaTime;
                go.transform.localScale  = Vector3.one * Mathf.Lerp(size, 0.002f, t * t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (go  != null) Destroy(go);
            if (mat != null) Destroy(mat);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        private Vector3 GetAttackOrigin()
        {
            if (weaponTransform != null) return weaponTransform.position;
            return transform.position
                + transform.forward * attackOffset.z
                + transform.up      * attackOffset.y
                + transform.right   * attackOffset.x;
        }

        private bool TryResolveDamageable(Collider hitCollider,
            out IDamageable damageable, out Component owningComponent)
        {
            damageable      = null;
            owningComponent = null;

            foreach (MonoBehaviour mb in hitCollider.GetComponentsInParent<MonoBehaviour>(true))
            {
                if (mb == null || mb.gameObject == gameObject) continue;
                if (mb is IDamageable candidate)
                {
                    damageable      = candidate;
                    owningComponent = mb;
                    return true;
                }
            }
            return false;
        }

        private IEnumerator ResetStateAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (controller != null && controller.CurrentState == PlayerController.PlayerState.Attacking)
                controller.SetState(PlayerController.PlayerState.Idle);
        }

        private void OnDisable()
        {
            if (resetStateRoutine != null) { StopCoroutine(resetStateRoutine); resetStateRoutine = null; }
            if (controller != null && controller.CurrentState == PlayerController.PlayerState.Attacking)
                controller.SetState(PlayerController.PlayerState.Idle);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetAttackOrigin(), attackRange);
            Gizmos.color = new Color(1f, 0.4f, 0f, 0.5f);
            Gizmos.DrawWireSphere(GetAttackOrigin(), attackRange * finisherRangeMultiplier);
        }
    }
}
