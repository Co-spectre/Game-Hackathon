using UnityEngine;

namespace NordicWilds.CameraSystems
{
    /// <summary>
    /// Isometric camera that smoothly follows the player using SmoothDamp.
    /// Eases in when the player starts moving, eases out when they stop.
    /// </summary>
    [ExecuteInEditMode]
    public class IsometricCameraFollow : MonoBehaviour
    {
        [Header("Target Tracking")]
        [SerializeField] public Transform target;

        [Header("Position Details")]
        [SerializeField] private Vector3 followOffset = new Vector3(-10f, 15f, -10f);

        [Header("Smoothing")]
        [Tooltip("Seconds to reach the target while the player is moving. Lower = snappier.")]
        [SerializeField] private float followSmoothTime = 0.18f;  // ease-in speed
        [Tooltip("Seconds to settle after the player stops. Higher = floatier stop.")]
        [SerializeField] private float stopSmoothTime   = 0.32f;  // ease-out speed
        [Tooltip("Maximum camera travel speed in units/second.")]
        [SerializeField] private float maxSpeed         = 40f;

        [Header("Look At Configuration")]
        [SerializeField] private bool lookAtTarget = false;

        // SmoothDamp requires a persistent velocity reference
        private Vector3 _smoothVelocity = Vector3.zero;
        private Vector3 currentActualPosition;

        // Track whether the player was moving last frame so we can switch smooth times
        private bool _playerWasMoving = false;

        private void Start()
        {
            if (target != null) SnapToTarget();
        }

        public void SnapToTarget()
        {
            if (target == null) return;
            currentActualPosition = target.position + followOffset;
            transform.position    = currentActualPosition;
            _smoothVelocity       = Vector3.zero;

            if (lookAtTarget) transform.LookAt(target);
            else              transform.rotation = Quaternion.LookRotation(-followOffset);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    target = playerObj.transform;
                    currentActualPosition = target.position + followOffset;
                }
                else return;
            }

            Vector3 desiredPosition = target.position + followOffset;

            // Detect whether the player is currently moving
            bool playerMoving = (desiredPosition - currentActualPosition).sqrMagnitude > 0.0001f;

            // Use a tighter smooth time while catching up, looser while settling
            float smoothTime = playerMoving ? followSmoothTime : stopSmoothTime;

            // SmoothDamp gives natural ease-in / ease-out automatically
            // (unlike Lerp, it remembers velocity so it can't stutter or snap)
            currentActualPosition = Vector3.SmoothDamp(
                currentActualPosition,
                desiredPosition,
                ref _smoothVelocity,
                smoothTime,
                maxSpeed,
                Time.deltaTime);

            // Camera shake juice offset
            Vector3 juiceOffset = Vector3.zero;
            if (Application.isPlaying &&
                CameraJuiceManager.Instance != null &&
                CameraJuiceManager.Instance.IsShaking)
            {
                juiceOffset = CameraJuiceManager.Instance.CurrentShakeOffset;
            }

            transform.position = currentActualPosition + juiceOffset;

            if (lookAtTarget) transform.LookAt(target);
            else              transform.rotation = Quaternion.LookRotation(-followOffset);

            _playerWasMoving = playerMoving;
        }
    }
}