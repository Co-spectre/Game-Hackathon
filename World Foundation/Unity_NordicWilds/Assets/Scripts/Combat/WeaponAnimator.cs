using System.Collections;
using UnityEngine;
using NordicWilds.Player;

namespace NordicWilds.Combat
{
    /// <summary>
    /// Drives all weapon and body animations procedurally (no Animator required).
    /// Auto-added by PlayerCombat.Awake — you do NOT need to add this manually.
    /// References are discovered lazily in Start (after all Awake calls), so
    /// there is no race condition with ProceduralCharacterVisual.Build().
    /// </summary>
    public class WeaponAnimator : MonoBehaviour
    {
        // ── Inspector (override auto-discovery) ───────────────────────────────────
        [Header("References (auto-found — leave blank unless overriding)")]
        public Transform weaponTransform;
        public Transform bodyVisualRoot;

        [Header("Swing Feel")]
        [SerializeField] private float windUpDuration  = 0.07f;
        [SerializeField] private float swingDuration   = 0.11f;
        [SerializeField] private float followThrough   = 0.14f;
        [SerializeField] private float swingArcAngle   = 135f;
        [SerializeField] private float bodyLeanAngle   = 10f;

        [Header("Block / Parry Feel")]
        [SerializeField] private float blockRaiseSpeed  = 10f;
        [SerializeField] private float parryRecoilAngle = 40f;
        [SerializeField] private float parryRecoilTime  = 0.07f;
        [SerializeField] private float parryRecoverTime = 0.20f;
        [SerializeField] private float parryBodyBrace   = 8f;

        [Header("Parry Sparks")]
        [SerializeField] private int   sparkCount     = 12;
        [SerializeField] private float sparkSpeed     = 5f;
        [SerializeField] private float sparkLifetime  = 0.35f;

        // ── Private state ──────────────────────────────────────────────────────────
        private PlayerController _ctrl;
        private PlayerCombat     _combat;

        // Rest pose — captured once refs are found
        private Vector3    _weaponRestPos;
        private Quaternion _weaponRestRot;
        private Vector3    _bodyRestPos;
        private Quaternion _bodyRestRot;
        private bool       _restCaptured = false;

        // Coroutines
        private Coroutine _swingC, _blockC, _parryC;
        private bool      _wasBlocking;

        // ── Lifecycle ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            _ctrl   = GetComponent<PlayerController>();
            _combat = GetComponent<PlayerCombat>();
        }

        private void Start()
        {
            // Start() runs after ALL Awake() calls, so RuntimeVisualModel is built.
            TryDiscoverReferences();
        }

        private void Update()
        {
            // If refs weren't ready in Start, keep retrying until found.
            if (!_restCaptured)
            {
                TryDiscoverReferences();
                return;
            }

            HandleBlockTransition();
        }

        // ── Reference discovery ───────────────────────────────────────────────────
        private void TryDiscoverReferences()
        {
            // 1. Try RuntimeVisualModel hierarchy (procedural character)
            if (weaponTransform == null)
            {
                Transform model = transform.Find("RuntimeVisualModel");
                if (model != null)
                    weaponTransform = model.Find("Weapon");
            }

            // 2. Fall back to PlayerCombat.weaponTransform (manually assigned)
            if (weaponTransform == null && _combat != null && _combat.weaponTransform != null)
                weaponTransform = _combat.weaponTransform;

            // 3. Deep search — any child named "Weapon"
            if (weaponTransform == null)
                weaponTransform = FindChildRecursive(transform, "Weapon");

            // Body root
            if (bodyVisualRoot == null)
                bodyVisualRoot = transform.Find("RuntimeVisualModel");

            // Only capture rest pose once BOTH refs are resolved
            if (weaponTransform == null)
                return; // still not ready — retry next frame

            _weaponRestPos = weaponTransform.localPosition;
            _weaponRestRot = weaponTransform.localRotation;

            if (bodyVisualRoot != null)
            {
                _bodyRestPos = bodyVisualRoot.localPosition;
                _bodyRestRot = bodyVisualRoot.localRotation;
            }

            // Share reference back to PlayerCombat
            if (_combat != null && _combat.weaponTransform == null)
                _combat.weaponTransform = weaponTransform;

            _restCaptured = true;
            Debug.Log($"[WeaponAnimator] Ready — weapon: {weaponTransform.name}, body: {(bodyVisualRoot != null ? bodyVisualRoot.name : "none")}");
        }

        private Transform FindChildRecursive(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName) return child;
                var found = FindChildRecursive(child, childName);
                if (found != null) return found;
            }
            return null;
        }

        // ── Public API ────────────────────────────────────────────────────────────
        public void PlaySwing(int comboStep)
        {
            if (!_restCaptured) return;
            if (_swingC != null) StopCoroutine(_swingC);
            _swingC = StartCoroutine(SwingRoutine(comboStep));
        }

        public void PlayParry(Vector3 impactPoint)
        {
            if (!_restCaptured) return;
            if (_parryC != null) StopCoroutine(_parryC);
            _parryC = StartCoroutine(ParryRoutine(impactPoint));
        }

        /// <summary>Spin-sweep finisher — full 360° body rotation with wide weapon arc.</summary>
        public void PlayFinisher()
        {
            if (!_restCaptured) return;
            if (_swingC != null) StopCoroutine(_swingC);
            _swingC = StartCoroutine(FinisherRoutine());
        }

        // ── Block transition ──────────────────────────────────────────────────────
        private void HandleBlockTransition()
        {
            bool blocking = _ctrl != null && _ctrl.IsBlocking;
            if (blocking == _wasBlocking) return;
            _wasBlocking = blocking;

            if (_blockC != null) { StopCoroutine(_blockC); _blockC = null; }
            if (_swingC != null || _parryC != null) return; // don't override active anim

            _blockC = StartCoroutine(blocking ? RaiseGuard() : LowerGuard());
        }

        // ── Swing ─────────────────────────────────────────────────────────────────
        private IEnumerator SwingRoutine(int comboStep)
        {
            float sign = (comboStep % 2 == 0) ? 1f : -1f;

            // Wind-up
            Quaternion wuRot   = _weaponRestRot * Quaternion.Euler(sign * -swingArcAngle * 0.35f, -12f, 0f);
            Vector3    wuPos   = _weaponRestPos + new Vector3(-sign * 0.08f, 0.05f, -0.06f);
            Quaternion wuBody  = _bodyRestRot   * Quaternion.Euler(0f, sign * 5f, 0f);
            yield return Tween(windUpDuration, (e) =>
            {
                weaponTransform.localRotation = Quaternion.Slerp(_weaponRestRot, wuRot, e);
                weaponTransform.localPosition = Vector3.Lerp(_weaponRestPos, wuPos, e);
                if (bodyVisualRoot != null)
                    bodyVisualRoot.localRotation = Quaternion.Slerp(_bodyRestRot, wuBody, e);
            }, EaseOut);

            // Snap swing
            Quaternion swRot  = _weaponRestRot * Quaternion.Euler(sign * swingArcAngle * 0.65f, 8f, 0f);
            Vector3    swPos  = _weaponRestPos + new Vector3(sign * 0.14f, -0.04f, 0.1f);
            Quaternion swBody = _bodyRestRot   * Quaternion.Euler(bodyLeanAngle, -sign * 8f, 0f);
            yield return Tween(swingDuration, (e) =>
            {
                weaponTransform.localRotation = Quaternion.Slerp(wuRot, swRot, e);
                weaponTransform.localPosition = Vector3.Lerp(wuPos, swPos, e);
                if (bodyVisualRoot != null)
                    bodyVisualRoot.localRotation = Quaternion.Slerp(wuBody, swBody, e);
            }, EaseOutElastic);

            // Follow-through back to rest
            Quaternion ftFrom = weaponTransform.localRotation;
            Vector3    ftPos  = weaponTransform.localPosition;
            Quaternion ftBody = bodyVisualRoot != null ? bodyVisualRoot.localRotation : _bodyRestRot;
            yield return Tween(followThrough, (e) =>
            {
                weaponTransform.localRotation = Quaternion.Slerp(ftFrom, _weaponRestRot, e);
                weaponTransform.localPosition = Vector3.Lerp(ftPos, _weaponRestPos, e);
                if (bodyVisualRoot != null)
                    bodyVisualRoot.localRotation = Quaternion.Slerp(ftBody, _bodyRestRot, e);
            }, EaseOut);

            SnapToRest();
            _swingC = null;

            if (_ctrl != null && _ctrl.IsBlocking)
                _blockC = StartCoroutine(RaiseGuard());
        }

        // ── Finisher (spin sweep) ─────────────────────────────────────────────────
        private IEnumerator FinisherRoutine()
        {
            // ── Phase 1: Coil wind-up (0.1 s) ─────────────────────────────────────
            // Pull weapon back and low, body leans back
            Quaternion coilWep  = _weaponRestRot * Quaternion.Euler(-20f, -60f, 30f);
            Vector3    coilPos  = _weaponRestPos + new Vector3(-0.15f, -0.05f, -0.08f);
            Quaternion coilBody = _bodyRestRot   * Quaternion.Euler(8f, -35f, 0f);

            yield return Tween(0.12f, (e) =>
            {
                weaponTransform.localRotation = Quaternion.Slerp(_weaponRestRot, coilWep, e);
                weaponTransform.localPosition = Vector3.Lerp(_weaponRestPos,     coilPos, e);
                if (bodyVisualRoot != null)
                    bodyVisualRoot.localRotation = Quaternion.Slerp(_bodyRestRot, coilBody, e);
            }, EaseOut);

            // ── Phase 2: Spin (0.38 s) — body rotates 360°, weapon sweeps wide ─────
            float spinDuration  = 0.38f;
            float elapsed       = 0f;
            Quaternion startBodyRot = bodyVisualRoot != null ? bodyVisualRoot.localRotation : _bodyRestRot;

            while (elapsed < spinDuration)
            {
                float t   = elapsed / spinDuration;
                float tE  = EaseOutElastic(Mathf.Clamp01(t * 1.2f)); // slightly over-drive for snap

                // Body spins 360° around local Y
                float bodyAngle  = Mathf.Lerp(0f, 360f, t);
                if (bodyVisualRoot != null)
                    bodyVisualRoot.localRotation = _bodyRestRot * Quaternion.Euler(10f, bodyAngle, 0f);

                // Weapon sweeps 540° in local space (more than a full rotation = wide arc feel)
                float weapAngle  = Mathf.Lerp(0f, 540f, tE);
                weaponTransform.localRotation = _weaponRestRot * Quaternion.Euler(0f, weapAngle, 25f * Mathf.Sin(t * Mathf.PI));
                weaponTransform.localPosition = _weaponRestPos + new Vector3(
                    Mathf.Sin(weapAngle * Mathf.Deg2Rad) * 0.12f,
                    Mathf.Abs(Mathf.Sin(t * Mathf.PI)) * 0.08f,
                    Mathf.Cos(weapAngle * Mathf.Deg2Rad) * 0.08f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // ── Phase 3: Recover to rest (0.22 s) ─────────────────────────────────
            Quaternion afterBodyRot = bodyVisualRoot != null ? bodyVisualRoot.localRotation : _bodyRestRot;
            Quaternion afterWepRot  = weaponTransform.localRotation;
            Vector3    afterWepPos  = weaponTransform.localPosition;

            yield return Tween(0.22f, (e) =>
            {
                weaponTransform.localRotation = Quaternion.Slerp(afterWepRot, _weaponRestRot, e);
                weaponTransform.localPosition = Vector3.Lerp(afterWepPos,     _weaponRestPos, e);
                if (bodyVisualRoot != null)
                    bodyVisualRoot.localRotation = Quaternion.Slerp(afterBodyRot, _bodyRestRot, e);
            }, EaseOut);

            SnapToRest();
            _swingC = null;

            if (_ctrl != null && _ctrl.IsBlocking)
                _blockC = StartCoroutine(RaiseGuard());
        }

        // ── Guard ─────────────────────────────────────────────────────────────────
        private IEnumerator RaiseGuard()
        {
            Quaternion guardRot  = _weaponRestRot * Quaternion.Euler(-50f, 20f, 45f);
            Vector3    guardPos  = _weaponRestPos + new Vector3(-0.1f, 0.25f, 0.1f);
            Quaternion bodyGuard = _bodyRestRot   * Quaternion.Euler(-5f, 0f, 0f);

            Quaternion fromRot  = weaponTransform.localRotation;
            Vector3    fromPos  = weaponTransform.localPosition;
            Quaternion fromBody = bodyVisualRoot != null ? bodyVisualRoot.localRotation : _bodyRestRot;

            yield return Tween(1f / blockRaiseSpeed, (e) =>
            {
                weaponTransform.localRotation = Quaternion.Slerp(fromRot, guardRot, e);
                weaponTransform.localPosition = Vector3.Lerp(fromPos, guardPos, e);
                if (bodyVisualRoot != null)
                    bodyVisualRoot.localRotation = Quaternion.Slerp(fromBody, bodyGuard, e);
            }, EaseOut);

            // Hold guard with subtle bob
            while (_ctrl != null && _ctrl.IsBlocking)
            {
                float bob = Mathf.Sin(Time.time * 3f) * 0.004f;
                weaponTransform.localPosition = guardPos + Vector3.up * bob;
                yield return null;
            }

            _blockC = StartCoroutine(LowerGuard());
        }

        private IEnumerator LowerGuard()
        {
            Quaternion fromRot  = weaponTransform.localRotation;
            Vector3    fromPos  = weaponTransform.localPosition;
            Quaternion fromBody = bodyVisualRoot != null ? bodyVisualRoot.localRotation : _bodyRestRot;

            yield return Tween(1f / blockRaiseSpeed, (e) =>
            {
                weaponTransform.localRotation = Quaternion.Slerp(fromRot, _weaponRestRot, e);
                weaponTransform.localPosition = Vector3.Lerp(fromPos, _weaponRestPos, e);
                if (bodyVisualRoot != null)
                    bodyVisualRoot.localRotation = Quaternion.Slerp(fromBody, _bodyRestRot, e);
            }, EaseOut);

            SnapToRest();
            _blockC = null;
        }

        // ── Parry ─────────────────────────────────────────────────────────────────
        private IEnumerator ParryRoutine(Vector3 impactPoint)
        {
            SpawnParryRing(impactPoint);

            Quaternion guardRot  = _weaponRestRot * Quaternion.Euler(-50f, 20f, 45f);
            Quaternion recoilRot = guardRot        * Quaternion.Euler(parryRecoilAngle, -10f, 0f);
            Quaternion bodyBrace = _bodyRestRot    * Quaternion.Euler(-parryBodyBrace, 0f, 0f);
            Vector3    guardPos  = _weaponRestPos  + new Vector3(-0.1f, 0.25f, 0.1f);
            Vector3    recoilPos = guardPos        + new Vector3(0f, -0.12f, -0.15f);

            Quaternion fromRot  = weaponTransform.localRotation;
            Vector3    fromPos  = weaponTransform.localPosition;
            Quaternion fromBody = bodyVisualRoot != null ? bodyVisualRoot.localRotation : _bodyRestRot;

            // Snap recoil
            yield return Tween(parryRecoilTime, (e) =>
            {
                weaponTransform.localRotation = Quaternion.Slerp(fromRot, recoilRot, e);
                weaponTransform.localPosition = Vector3.Lerp(fromPos, recoilPos, e);
                if (bodyVisualRoot != null)
                    bodyVisualRoot.localRotation = Quaternion.Slerp(fromBody, bodyBrace, e);
            }, t => t); // linear — snap is intentional

            // Recover to guard
            yield return Tween(parryRecoverTime, (e) =>
            {
                weaponTransform.localRotation = Quaternion.Slerp(recoilRot, guardRot, e);
                weaponTransform.localPosition = Vector3.Lerp(recoilPos, guardPos, e);
                if (bodyVisualRoot != null)
                    bodyVisualRoot.localRotation = Quaternion.Slerp(bodyBrace, _bodyRestRot, e);
            }, EaseOut);

            _parryC = null;

            if (_ctrl != null && _ctrl.IsBlocking && _blockC == null)
                _blockC = StartCoroutine(RaiseGuard());
        }

        // ── Parry sparks ──────────────────────────────────────────────────────────
        private void SpawnParryRing(Vector3 worldPos)
        {
            for (int i = 0; i < sparkCount; i++)
            {
                float   angle = 360f / sparkCount * i;
                Vector3 dir   = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f,
                                            Mathf.Sin(angle * Mathf.Deg2Rad));
                StartCoroutine(SparkRoutine(worldPos, dir));
            }
        }

        private IEnumerator SparkRoutine(Vector3 origin, Vector3 dir)
        {
            var go  = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "ParrySpark";
            Destroy(go.GetComponent<Collider>());
            go.transform.position   = origin;
            go.transform.localScale = Vector3.one * 0.08f;

            var mat = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"));
            mat.color = new Color(1f, 0.85f, 0.2f);
            mat.hideFlags = HideFlags.HideAndDontSave;
            go.GetComponent<MeshRenderer>().material = mat;

            Color startCol = mat.color;
            float elapsed  = 0f;
            while (elapsed < sparkLifetime && go != null)
            {
                float t = elapsed / sparkLifetime;
                go.transform.position   += dir * sparkSpeed * Time.deltaTime;
                go.transform.localScale  = Vector3.one * Mathf.Lerp(0.08f, 0.01f, t);
                mat.color = Color.Lerp(startCol, Color.clear, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (go  != null) Destroy(go);
            if (mat != null) Destroy(mat);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────
        private void SnapToRest()
        {
            if (weaponTransform != null)
            {
                weaponTransform.localRotation = _weaponRestRot;
                weaponTransform.localPosition = _weaponRestPos;
            }
            if (bodyVisualRoot != null)
                bodyVisualRoot.localRotation = _bodyRestRot;
        }

        /// Generic tween coroutine — calls setter(easedT) each frame.
        private IEnumerator Tween(float duration, System.Action<float> setter,
                                  System.Func<float, float> easing)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                setter(easing(Mathf.Clamp01(elapsed / duration)));
                elapsed += Time.deltaTime;
                yield return null;
            }
            setter(easing(1f));
        }

        private static float EaseOut(float t)           => 1f - (1f - t) * (1f - t);
        private static float EaseOutElastic(float t)
        {
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            const float c4 = (2f * Mathf.PI) / 3f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
        }
    }
}
