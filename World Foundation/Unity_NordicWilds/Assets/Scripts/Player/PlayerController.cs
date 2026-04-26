using System.Collections;
using UnityEngine;

namespace NordicWilds.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Stats")]
        [SerializeField] private float baseSpeed      = 8f;
        [SerializeField] private float sprintSpeed    = 14f;
        // Lower acceleration = gradual ramp-up; lower deceleration = smooth coast-to-stop
        [SerializeField] private float acceleration   = 28f;
        [SerializeField] private float deceleration   = 18f;

        [Header("Sprint Stamina")]
        [SerializeField] public float maxStamina         = 100f;
        [SerializeField] private float staminaDrainRate  = 20f;   // per second while sprinting
        [SerializeField] private float staminaRegenRate  = 13.8f;  // 12 × 1.15 — 15 % faster regen

        [SerializeField] private float staminaRegenDelay = 1.5f;  // seconds after sprint stops before regen begins

        [Header("Dash Mechanics (Hades Style)")]
        [SerializeField] private float dashForce        = 35f;
        [SerializeField] private float dashDuration     = 0.15f;  // active glide phase
        [SerializeField] private float dashSlideOutTime = 0.18f;  // smooth brake phase after glide
        [SerializeField] private float dashCooldown     = 0.4f;
        [SerializeField] private bool  hasInvincibilityFrames = true; // kept for legacy; iFrames are now always on


        [Header("Dash Stamina Cost")]
        [Range(0.05f, 0.5f)]
        [SerializeField] private float dashStaminaCostFraction = 0.15f; // 15 % of maxStamina per dash

        [Header("Camera & Isometric Perspective")]
        [SerializeField] private Transform isometricCameraTransform;

        // ── Sprint & stamina state ────────────────────────────────────────────────
        private bool  isSprinting        = false;
        private float currentStamina_    = 0f;
        private float lastSprintEndTime  = -999f;
        // Depletion lockout: once stamina hits 0, block all stamina use
        // until it fully recharges to 100 %.
        private bool  staminaDepleted    = false;

        // Public read-only properties for the HUD
        public bool  IsSprinting      => isSprinting;
        public float CurrentStamina   => currentStamina_;
        public float MaxStamina       => maxStamina;
        public float StaminaFraction  => maxStamina > 0f ? currentStamina_ / maxStamina : 0f;
        public bool  StaminaDepleted  => staminaDepleted;
        // Dash is available when stamina is sufficient and not in depletion lockout.
        // Dashing during sprint is allowed — stamina is deducted from the same pool.
        public bool CanDash => !staminaDepleted &&
                               currentStamina_ >= dashStaminaCostFraction * maxStamina;

        /// <summary>
        /// Called by Health.cs when a blocked hit lands.
        /// Drains <paramref name="amount"/> from stamina.
        /// Returns how much could NOT be covered by stamina (health overflow).
        /// </summary>
        public float DrainStaminaForBlock(float amount)
        {
            float available = currentStamina_;
            float drained   = Mathf.Min(available, amount);
            float overflow  = amount - drained;         // remainder that hits health

            currentStamina_ = Mathf.Max(0f, currentStamina_ - drained);

            // If stamina fully depleted by this hit, enter depletion lockout
            if (currentStamina_ <= 0f)
            {
                staminaDepleted   = true;
                lastSprintEndTime = Time.time; // reset regen delay
                if (isSprinting) StopSprint();
            }

            return overflow;
        }

        /// <summary>
        /// Unconditional stamina drain (e.g. finisher attack cost).
        /// Always executes — drains whatever is available, bottoms out at 0.
        /// </summary>
        public void DrainStaminaFlat(float amount)
        {
            currentStamina_   = Mathf.Max(0f, currentStamina_ - amount);
            lastSprintEndTime = Time.time;   // reset regen delay
            if (currentStamina_ <= 0f)
            {
                staminaDepleted = true;
                if (isSprinting) StopSprint();
            }
        }


        // ── Internal movement ─────────────────────────────────────────────────────
        private Rigidbody rb;
        private Vector3   currentInput;
        private Vector3   moveDirection;
        private bool      controlsLocked = false;

        // State Machine
        public enum PlayerState { Idle, Running, Sprinting, Dashing, Attacking, Staggered, Blocking }
        public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

        public float LastDashTime    { get; private set; } = -Mathf.Infinity;
        // Public field — written by DashRoutine (self) and Health.cs (post-hit iFrames)
        public bool  isInvincible    = false;

        // ── Block / Parry state (readable by Health.cs and PlayerCombat.cs) ────────
        [Header("Block & Parry")]
        [Tooltip("Seconds after Q is pressed where a hit counts as a perfect parry.")]
        [SerializeField] private float parryWindowDuration  = 0.25f;
        [Tooltip("Fraction of damage absorbed while holding block (0.6 = 60% blocked).")]
        [SerializeField] private float blockDamageReduction = 0.60f;
        [Tooltip("Move speed multiplier while blocking.")]
        [SerializeField] private float blockSpeedMultiplier = 0.40f;
        [Tooltip("Seconds the enemy is knocked down after a successful parry.")]
        [SerializeField] public  float parryStaggerDuration = 2.5f;
        [Tooltip("Damage multiplier applied to attacks against a parry-staggered enemy.")]
        [SerializeField] public  float parryDamageMultiplier = 2.5f;

        private bool  _isBlocking       = false;
        private bool  _parryWindowOpen  = false;
        private float _parryWindowEndTime = -1f;

        public bool  IsBlocking       => _isBlocking;
        public bool  IsParryWindowOpen => _parryWindowOpen;
        public float BlockDamageReduction => blockDamageReduction;


        // ── Lifecycle ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints   = RigidbodyConstraints.FreezeRotation;
            currentStamina_  = maxStamina;
        }

        private void Update()
        {
            // ── Block / Parry input (runs even in Attacking so you can buffer a block) ──
            HandleBlockInput();

            if (controlsLocked) return;
            if (CurrentState == PlayerState.Dashing   ||
                CurrentState == PlayerState.Attacking ||
                CurrentState == PlayerState.Staggered) return;

            HandleInput();
            HandleSprint();
            HandleDash();
            UpdateStamina();
            UpdateState();
        }

        private void FixedUpdate()
        {
            if (controlsLocked)
            {
                rb.linearVelocity = Vector3.zero;
                return;
            }

            if (CurrentState == PlayerState.Dashing || CurrentState == PlayerState.Staggered)
                return;

            ApplyRotation();

            if (CurrentState == PlayerState.Attacking)
            {
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity,
                    new Vector3(0, rb.linearVelocity.y, 0), Time.fixedDeltaTime * 10f);
                return;
            }

            // Slow movement while blocking
            if (CurrentState == PlayerState.Blocking)
            {
                ApplyMovement(speedOverride: blockSpeedMultiplier);
                return;
            }

            ApplyMovement();
        }

        // ── Input ─────────────────────────────────────────────────────────────────
        private void HandleInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical   = Input.GetAxisRaw("Vertical");

            currentInput = new Vector3(horizontal, 0f, vertical);

            // Direct world-space XZ mapping — all 8 directions work correctly.
            // (Camera-relative remapping with a 45° isometric cam causes diagonal
            //  key pairs to cancel each other's Z/X components.)
            moveDirection = currentInput.sqrMagnitude > 0.001f
                ? currentInput.normalized
                : Vector3.zero;
        }

        // ── Sprint toggle ─────────────────────────────────────────────────────────
        private void HandleSprint()
        {
            // Toggle sprint on Shift press
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                if (isSprinting)
                {
                    StopSprint();
                }
                else if (currentStamina_ > 0f)
                {
                    isSprinting = true;
                }
            }

            // Auto-cancel sprint when stamina is fully depleted
            if (isSprinting && currentStamina_ <= 0f)
                StopSprint();

            // Auto-cancel sprint when player stops moving
            if (isSprinting && moveDirection.sqrMagnitude < 0.001f)
                StopSprint();
        }

        private void StopSprint()
        {
            if (!isSprinting) return;
            isSprinting       = false;
            lastSprintEndTime = Time.time;
        }

        // ── Stamina ───────────────────────────────────────────────────────────────
        private void UpdateStamina()
        {
            if (isSprinting)
            {
                currentStamina_ = Mathf.Max(0f, currentStamina_ - staminaDrainRate * Time.deltaTime);

                // Hitting zero triggers depletion lockout
                if (currentStamina_ <= 0f)
                {
                    currentStamina_  = 0f;
                    staminaDepleted  = true;
                    StopSprint();   // auto-cancel sprint
                }
            }
            else if (Time.time >= lastSprintEndTime + staminaRegenDelay)
            {
                currentStamina_ = Mathf.Min(maxStamina, currentStamina_ + staminaRegenRate * Time.deltaTime);

                // Clear depletion lockout only when FULLY recharged
                if (staminaDepleted && currentStamina_ >= maxStamina)
                    staminaDepleted = false;
            }
        }

        // ── State ─────────────────────────────────────────────────────────────────
        private void UpdateState()
        {
            if (CurrentState != PlayerState.Idle    &&
                CurrentState != PlayerState.Running  &&
                CurrentState != PlayerState.Sprinting &&
                CurrentState != PlayerState.Blocking) return;

            if (_isBlocking) { CurrentState = PlayerState.Blocking; return; }

            if (moveDirection.sqrMagnitude > 0.01f)
                CurrentState = isSprinting ? PlayerState.Sprinting : PlayerState.Running;
            else
                CurrentState = PlayerState.Idle;
        }

        // ── Block / Parry input ───────────────────────────────────────────────────
        private void HandleBlockInput()
        {
            if (controlsLocked) return;
            if (CurrentState == PlayerState.Dashing) return;

            if (Input.GetKeyDown(KeyCode.Q))
            {
                _isBlocking         = true;
                _parryWindowOpen    = true;
                _parryWindowEndTime = Time.time + parryWindowDuration;
                if (isSprinting) StopSprint(); // can't sprint-block
            }

            if (Input.GetKeyUp(KeyCode.Q))
            {
                _isBlocking      = false;
                _parryWindowOpen = false;
                if (CurrentState == PlayerState.Blocking)
                    CurrentState = PlayerState.Idle;
            }

            // Auto-close parry window after its duration
            if (_parryWindowOpen && Time.time > _parryWindowEndTime)
                _parryWindowOpen = false;
        }



        // ── Physics ───────────────────────────────────────────────────────────────
        /// <param name="speedOverride">Optional multiplier applied on top of base/sprint speed (e.g. 0.4 while blocking).</param>
        private void ApplyMovement(float speedOverride = 1f)
        {
            float speed = isSprinting ? sprintSpeed : baseSpeed;
            speed *= speedOverride;
            Vector3 targetVelocity = moveDirection * speed;
            Vector3 velocityDiff   = targetVelocity - rb.linearVelocity;
            velocityDiff.y = 0f;   // preserve gravity

            // Use a gentler deceleration rate so stopping feels smooth, not instant
            float accelRate = targetVelocity.sqrMagnitude > 0.01f ? acceleration : deceleration;
            rb.AddForce(velocityDiff * accelRate, ForceMode.Acceleration);
        }

        private void ApplyRotation()
        {
            if (CurrentState == PlayerState.Attacking && isometricCameraTransform != null)
            {
                Camera cam = isometricCameraTransform.GetComponent<Camera>();
                if (cam != null)
                {
                    Plane groundPlane = new Plane(Vector3.up, transform.position);
                    Ray   ray         = cam.ScreenPointToRay(Input.mousePosition);
                    if (groundPlane.Raycast(ray, out float rayDistance))
                    {
                        Vector3 point   = ray.GetPoint(rayDistance);
                        Vector3 lookDir = point - transform.position;
                        lookDir.y = 0;
                        if (lookDir.sqrMagnitude > 0.1f)
                        {
                            Quaternion rot = Quaternion.LookRotation(lookDir);
                            rb.MoveRotation(Quaternion.Slerp(rb.rotation, rot, Time.fixedDeltaTime * 40f));
                        }
                    }
                }
            }
            else if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 25f);
            }
        }

        // ── Dash ──────────────────────────────────────────────────────────────────
        private void HandleDash()
        {
            if (!Input.GetKeyDown(KeyCode.Space)) return;
            if (Time.time < LastDashTime + dashCooldown) return;

            // Block if stamina is depleted or insufficient
            if (!CanDash)
            {
                Debug.Log("[Player] Dash blocked — stamina insufficient or depleted.");
                return;
            }

            // Deduct stamina cost
            currentStamina_ -= dashStaminaCostFraction * maxStamina;

            // Trigger depletion lockout if this dash drained us to zero
            if (currentStamina_ <= 0f)
            {
                currentStamina_ = 0f;
                staminaDepleted = true;
            }

            lastSprintEndTime = Time.time; // reset regen timer
            Vector3 dashDir = moveDirection.sqrMagnitude > 0.01f ? moveDirection : transform.forward;
            StartCoroutine(DashRoutine(dashDir));
        }

        private IEnumerator DashRoutine(Vector3 dashDirection)
        {
            CurrentState = PlayerState.Dashing;
            LastDashTime = Time.time;

            // Invincibility is always active for the full dash — no toggle needed.
            isInvincible = true;

            // Sharp impulse start (zero existing velocity first so it feels crisp)
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(dashDirection * dashForce, ForceMode.VelocityChange);

            if (CameraJuiceManager.Instance != null)
                CameraJuiceManager.Instance.ShakeCamera(0.1f, 0.15f);

            TrailRenderer trail = GetComponent<TrailRenderer>();
            if (trail != null) trail.emitting = true;

            // ── Active dash phase (player is airborne / gliding) ─────────────────
            yield return new WaitForSeconds(dashDuration);

            if (trail != null) trail.emitting = false;

            // ── Smooth slide-out phase ────────────────────────────────────────────
            // Instead of zeroing velocity instantly, we gently brake over slideOutTime
            // so the dash bleeds into normal movement rather than hard-stopping.
            float slideElapsed = 0f;
            Vector3 slideStartVel = rb.linearVelocity;
            // Preserve vertical component (gravity) throughout
            float yVel = slideStartVel.y;

            while (slideElapsed < dashSlideOutTime)
            {
                float t = slideElapsed / dashSlideOutTime;
                // Ease-out curve: decelerate quickly at first, then settle
                float eased = 1f - (1f - t) * (1f - t);
                Vector3 horizontal = Vector3.Lerp(
                    new Vector3(slideStartVel.x, 0f, slideStartVel.z),
                    Vector3.zero,
                    eased);
                rb.linearVelocity = new Vector3(horizontal.x, rb.linearVelocity.y, horizontal.z);
                slideElapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            isInvincible  = false;
            CurrentState  = PlayerState.Idle;
        }


        // ── Public API ────────────────────────────────────────────────────────────
        public void SetState(PlayerState newState)
        {
            CurrentState = newState;
            if (newState == PlayerState.Attacking || newState == PlayerState.Staggered)
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        public void SetControlsLocked(bool locked)
        {
            controlsLocked = locked;
            if (locked)
            {
                StopSprint();
                currentInput  = Vector3.zero;
                moveDirection = Vector3.zero;
                rb.linearVelocity = Vector3.zero;
                CurrentState  = PlayerState.Idle;
                isInvincible  = false;
            }
        }

        public void StopAllMotion()
        {
            StopSprint();
            currentInput      = Vector3.zero;
            moveDirection     = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
        }

        // ── Debug Visualisation ───────────────────────────────────────────────────
        [Header("Debug")]
        [Tooltip("Show movement direction arrow in Scene & Game view (requires Gizmos on in Game view).")]
        [SerializeField] private bool showDebugArrow = false;

        /// <summary>
        /// Draws a colour-coded arrow overlay for the player:
        ///   CYAN   — intended move direction (from input)
        ///   GREEN  — actual Rigidbody velocity (XZ projected)
        ///   YELLOW — character facing direction (transform.forward)
        /// Enable "Gizmos" in the Game view toolbar to see it during Play mode.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showDebugArrow) return;

            Vector3 origin = transform.position + Vector3.up * 0.1f; // slightly above ground

            // ── Move direction (input intent) ─────────────────────────────────
            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Gizmos.color = Color.cyan;
                DrawGizmoArrow(origin, moveDirection * 2.5f);
            }

            // ── Actual velocity (where physics is taking the player) ───────────
            if (rb != null)
            {
                Vector3 vel = rb.linearVelocity;
                vel.y = 0f;
                if (vel.sqrMagnitude > 0.1f)
                {
                    Gizmos.color = Color.green;
                    DrawGizmoArrow(origin + Vector3.up * 0.05f, vel * 0.25f);
                }
            }

            // ── Facing direction ──────────────────────────────────────────────
            Gizmos.color = Color.yellow;
            DrawGizmoArrow(origin, transform.forward * 1.5f);

            // Also write to Debug.DrawRay so the arrows appear in Game view
            // without needing Gizmos selected (visible in Scene view always).
            if (Application.isPlaying)
            {
                if (moveDirection.sqrMagnitude > 0.001f)
                    Debug.DrawRay(origin, moveDirection * 2.5f, Color.cyan);

                if (rb != null)
                {
                    Vector3 vel = rb.linearVelocity; vel.y = 0f;
                    if (vel.sqrMagnitude > 0.1f)
                        Debug.DrawRay(origin + Vector3.up * 0.05f, vel * 0.25f, Color.green);
                }

                Debug.DrawRay(origin, transform.forward * 1.5f, Color.yellow);
            }
        }

        /// <summary>Draws a Gizmo arrow (shaft + two-line arrowhead).</summary>
        private void DrawGizmoArrow(Vector3 from, Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return;

            Vector3 to = from + direction;
            Gizmos.DrawLine(from, to);

            // Arrowhead — two short lines at ~30° from the tip
            float headLen   = Mathf.Min(direction.magnitude * 0.35f, 0.55f);
            Vector3 dir     = direction.normalized;

            // Pick a perpendicular in XZ
            Vector3 perp    = Vector3.Cross(dir, Vector3.up).normalized;
            if (perp.sqrMagnitude < 0.01f) perp = Vector3.right; // fallback if dir is vertical

            Quaternion leftRot  = Quaternion.AngleAxis( 30f, Vector3.up);
            Quaternion rightRot = Quaternion.AngleAxis(-30f, Vector3.up);

            Vector3 leftWing  = leftRot  * (-dir) * headLen;
            Vector3 rightWing = rightRot * (-dir) * headLen;

            Gizmos.DrawLine(to, to + leftWing);
            Gizmos.DrawLine(to, to + rightWing);
        }
    }
}