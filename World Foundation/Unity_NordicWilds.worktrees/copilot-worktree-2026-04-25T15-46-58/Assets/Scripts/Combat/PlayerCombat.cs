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
        [Header("Combat Stats")]
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackDamage = 25f;
        [SerializeField] private float attackCooldown = 0.3f;
        [SerializeField] private float dashStrikeMultiplier = 1.5f;

        [Header("Attack Setup")]
        [SerializeField] private Vector3 attackOffset = new Vector3(0f, 1f, 1.5f);
        [SerializeField] private float comboResetDelay = 0.8f;
        [SerializeField] private float inputBufferWindow = 0.3f;

        [Header("Weapon Visuals")]
        public Transform weaponTransform;
        public Material hitMaterial;

        private PlayerController controller;
        private float nextAttackTime = 0f;
        private Quaternion initialWeaponRot;
        private float bufferedAttackTime = -1f;
        private int comboStep = 0;
        private float comboDropTime = 0f;
        private Coroutine resetStateRoutine;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();

            if (weaponTransform != null)
                initialWeaponRot = weaponTransform.localRotation;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                bufferedAttackTime = Time.time;

            if (Time.time > comboDropTime)
                comboStep = 0;

            if (CanConsumeBufferedAttack())
            {
                bufferedAttackTime = -1f;
                PerformAttack();
            }

            AnimateWeapon();
        }

        private bool CanConsumeBufferedAttack()
        {
            if (controller == null)
                return false;

            if (controller.CurrentState == PlayerController.PlayerState.Dashing)
                return false;

            if (bufferedAttackTime < 0f)
                return false;

            if (Time.time - bufferedAttackTime > inputBufferWindow)
                return false;

            return Time.time >= nextAttackTime;
        }

        private void AnimateWeapon()
        {
            if (weaponTransform == null || Time.time >= nextAttackTime)
                return;

            float progress = 1f - ((nextAttackTime - Time.time) / Mathf.Max(attackCooldown, 0.0001f));
            float startAngle = (comboStep % 2 == 1) ? 90f : -90f;
            weaponTransform.localRotation = initialWeaponRot * Quaternion.Euler(0f, Mathf.Lerp(startAngle, -startAngle * 0.2f, progress), 0f);
        }

        private void PerformAttack()
        {
            comboStep++;

            float currentCooldown = attackCooldown;
            float damage = attackDamage;
            bool isComboFinisher = comboStep % 3 == 0;

            if (isComboFinisher)
            {
                currentCooldown += 0.2f;
                damage *= 1.5f;
            }

            if (Time.time - controller.LastDashTime < 0.3f && comboStep == 1)
                damage *= dashStrikeMultiplier;

            nextAttackTime = Time.time + currentCooldown;
            comboDropTime = Time.time + currentCooldown + comboResetDelay;
            controller.SetState(PlayerController.PlayerState.Attacking);

            if (resetStateRoutine != null)
                StopCoroutine(resetStateRoutine);

            resetStateRoutine = StartCoroutine(ResetStateAfterDelay(currentCooldown * 0.8f));

            Vector3 attackCenter = GetAttackOrigin();
            Collider[] hitColliders = Physics.OverlapSphere(attackCenter, attackRange, Physics.AllLayers, QueryTriggerInteraction.Ignore);

            bool hitSomething = false;
            HashSet<Component> hitTargets = new HashSet<Component>();

            foreach (Collider hitCollider in hitColliders)
            {
                if (!TryResolveDamageable(hitCollider, out IDamageable damageable, out Component ownerComponent))
                    continue;

                if (ownerComponent == null || ownerComponent.gameObject == gameObject || !hitTargets.Add(ownerComponent))
                    continue;

                Vector3 hitPoint = hitCollider.ClosestPoint(attackCenter);
                Vector3 hitDirection = (ownerComponent.transform.position - attackCenter).normalized;
                var damageInfo = new DamageInfo(
                    damage,
                    gameObject,
                    hitPoint,
                    hitDirection,
                    DamageType.Physical,
                    knockbackForce: isComboFinisher ? 35f : 15f,
                    staggerDuration: isComboFinisher ? 0.3f : 0.15f,
                    canBeBlocked: false,
                    isCritical: isComboFinisher);

                damageable.TakeDamage(damageInfo);
                hitSomething = true;

                Rigidbody enemyBody = hitCollider.attachedRigidbody != null ? hitCollider.attachedRigidbody : ownerComponent.GetComponent<Rigidbody>();
                if (enemyBody != null)
                {
                    float force = isComboFinisher ? 35f : 15f;
                    Vector3 pushDirection = (ownerComponent.transform.position - transform.position).normalized;
                    pushDirection.y = 0f;
                    enemyBody.AddForce((pushDirection.normalized * force) + Vector3.up * 5f, ForceMode.Impulse);
                }

                GameObject dmgObj = new GameObject("DamageNumber");
                dmgObj.transform.position = enemyBody != null ? enemyBody.position + Vector3.up * 1.5f : ownerComponent.transform.position + Vector3.up * 1.5f;
                var dn = dmgObj.AddComponent<DamageNumber>();
                dn.Setup(damage, isComboFinisher);
            }

            if (hitSomething && CameraJuiceManager.Instance != null)
            {
                float stopTime = isComboFinisher ? 0.08f : 0.03f;
                float shakeDur = isComboFinisher ? 0.4f : 0.1f;
                float shakeMag = isComboFinisher ? 0.6f : 0.2f;

                CameraJuiceManager.Instance.HitStop(stopTime);
                CameraJuiceManager.Instance.ShakeCamera(shakeDur, shakeMag);
            }
        }

        private Vector3 GetAttackOrigin()
        {
            if (weaponTransform != null)
                return weaponTransform.position;

            return transform.position + transform.forward * attackOffset.z + transform.up * attackOffset.y + transform.right * attackOffset.x;
        }

        private bool TryResolveDamageable(Collider hitCollider, out IDamageable damageable, out Component owningComponent)
        {
            damageable = null;
            owningComponent = null;

            MonoBehaviour[] behaviours = hitCollider.GetComponentsInParent<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null || behaviour.gameObject == gameObject)
                    continue;

                if (behaviour is IDamageable candidate)
                {
                    damageable = candidate;
                    owningComponent = behaviour;
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
            if (resetStateRoutine != null)
            {
                StopCoroutine(resetStateRoutine);
                resetStateRoutine = null;
            }

            if (controller != null && controller.CurrentState == PlayerController.PlayerState.Attacking)
                controller.SetState(PlayerController.PlayerState.Idle);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetAttackOrigin(), attackRange);
        }
    }
}
