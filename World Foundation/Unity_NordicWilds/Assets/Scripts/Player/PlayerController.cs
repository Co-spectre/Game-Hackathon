using System.Collections;
using UnityEngine;

namespace NordicWilds.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Stats")]
        [SerializeField] private float baseSpeed = 8f;
        [SerializeField] private float sprintSpeed = 13f;
        [SerializeField] private float acceleration = 60f;
        [SerializeField] private float deceleration = 60f;
        public bool IsSprinting { get; private set; }

        [Header("Dash Mechanics (Hades Style)")]
        [SerializeField] private float dashForce = 35f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 0.4f;
        [SerializeField] private bool hasInvincibilityFrames = true;
        
        [Header("Dash Stamina")]
        public int maxDashes = 3;
        public int currentDashes { get; private set; }
        public float dashRechargeRate = 1.5f;
        private float lastDashRechargeTime = 0f;

        [Header("Camera & Isometric Perspective")]
        [SerializeField] private Transform isometricCameraTransform;

        private Rigidbody rb;
        private Vector3 currentInput;
        private Vector3 moveDirection;
        private bool controlsLocked = false;

        // State Machine
        public enum PlayerState { Idle, Running, Dashing, Attacking, Staggered }
        public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

        public float LastDashTime { get; private set; } = -Mathf.Infinity;
        public bool isInvincible { get; private set; } = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth 3D physics rendering
            rb.constraints = RigidbodyConstraints.FreezeRotation; // Lock rotation, handle via code
            currentDashes = maxDashes;
        }

        private void Update()
        {
            if (controlsLocked)
                return;

            if (CurrentState == PlayerState.Dashing || CurrentState == PlayerState.Attacking || CurrentState == PlayerState.Staggered)
                return; // Lock input during these states

            HandleInput();
            HandleDash();
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
                return; // Disable physics entirely while taking damage or rolling

            ApplyRotation(); // Let player rotate/aim during attacks!

            if (CurrentState == PlayerState.Attacking)
            {
                // Brake movement instantly so combat swings feel weighty
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(0, rb.linearVelocity.y, 0), Time.fixedDeltaTime * 10f);
                return; 
            }

            ApplyMovement();
        }

        private void HandleInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            currentInput = new Vector3(horizontal, 0f, vertical).normalized;
            IsSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            // Translate input relative to Isometric camera view
            if (isometricCameraTransform != null)
            {
                Vector3 camForward = isometricCameraTransform.forward;
                Vector3 camRight = isometricCameraTransform.right;
                
                camForward.y = 0f;
                camRight.y = 0f;

                moveDirection = (camForward.normalized * currentInput.z + camRight.normalized * currentInput.x).normalized;
            }
            else
            {
                moveDirection = currentInput;
            }
        }

        private void UpdateState()
        {
            if (CurrentState != PlayerState.Idle && CurrentState != PlayerState.Running) return;

            CurrentState = currentInput.sqrMagnitude > 0.01f ? PlayerState.Running : PlayerState.Idle;
        }

        private void ApplyMovement()
        {
            float speed = IsSprinting && currentInput.sqrMagnitude > 0.01f ? sprintSpeed : baseSpeed;
            Vector3 targetVelocity = moveDirection * speed;
            Vector3 velocityDiff = targetVelocity - rb.linearVelocity;
            
            // Retain vertical velocity for gravity
            velocityDiff.y = 0f;

            float accelRate = (targetVelocity.sqrMagnitude > 0.01f) ? acceleration : deceleration;
            Vector3 force = velocityDiff * accelRate;

            rb.AddForce(force, ForceMode.Acceleration);
        }

        private void ApplyRotation()
        {
            // Smoothly rotate character to mouse cursor when attacking so combat feels precise
            if (CurrentState == PlayerState.Attacking && isometricCameraTransform != null)
            {
                Camera cam = isometricCameraTransform.GetComponent<Camera>();
                if (cam != null)
                {
                    Plane groundPlane = new Plane(Vector3.up, transform.position);
                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    if (groundPlane.Raycast(ray, out float rayDistance))
                    {
                        Vector3 point = ray.GetPoint(rayDistance);
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
                // Snappy rotation rotation typical of responsive action RPGs
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 25f);
            }
        }

        private void HandleDash()
        {
            if (currentDashes < maxDashes && Time.time >= lastDashRechargeTime)
            {
                currentDashes++;
                lastDashRechargeTime = Time.time + dashRechargeRate;
            }

            if (Input.GetKeyDown(KeyCode.Space) && currentDashes > 0 && Time.time >= LastDashTime + dashCooldown)
            {
                currentDashes--;
                lastDashRechargeTime = Time.time + dashRechargeRate; // Reset recharge timer on use
                Vector3 dashDir = moveDirection.sqrMagnitude > 0.01f ? moveDirection : transform.forward;
                StartCoroutine(DashRoutine(dashDir));
            }
        }

        private IEnumerator DashRoutine(Vector3 dashDirection)
        {
            CurrentState = PlayerState.Dashing;
            LastDashTime = Time.time;

            if (hasInvincibilityFrames) isInvincible = true;

            // Zero out current velocity for a sharp dash 
            rb.linearVelocity = Vector3.zero;
            
            // Apply massive impulse 
            rb.AddForce(dashDirection * dashForce, ForceMode.VelocityChange);

            // Dash VFX and Camera Shake for "Juice"
            if (CameraJuiceManager.Instance != null)
                CameraJuiceManager.Instance.ShakeCamera(0.1f, 0.15f);

            TrailRenderer trail = GetComponent<TrailRenderer>();
            if (trail != null) trail.emitting = true;

            yield return new WaitForSeconds(dashDuration);

            if (trail != null) trail.emitting = false;

            // Instantly stop the dash glide to make it feel extremely crisp (Hades style)
            rb.linearVelocity = Vector3.zero;

            if (hasInvincibilityFrames) isInvincible = false;
            CurrentState = PlayerState.Idle;
        }

        // Called by other systems (like Combat manager)
        public void SetState(PlayerState newState)
        {
            CurrentState = newState;
            if (newState == PlayerState.Attacking || newState == PlayerState.Staggered)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); // Halt momentum
            }
        }

        public void SetControlsLocked(bool locked)
        {
            controlsLocked = locked;

            if (locked)
            {
                currentInput = Vector3.zero;
                moveDirection = Vector3.zero;
                rb.linearVelocity = Vector3.zero;
                CurrentState = PlayerState.Idle;
                isInvincible = false;
            }
        }

        public void StopAllMotion()
        {
            currentInput = Vector3.zero;
            moveDirection = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
        }
    }
}