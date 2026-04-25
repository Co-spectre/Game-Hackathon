using UnityEngine;

namespace NordicWilds.CameraSystems
{
    /// <summary>
    /// Mimics the Hades 3D Isometric / Orthographic perspective by following the player rigidly 
    /// without rotating, maintaining a specific offset and angle.
    /// </summary>
    [ExecuteInEditMode]
    public class IsometricCameraFollow : MonoBehaviour
    {
        [Header("Target Tracking")]
        [SerializeField] private Transform target; // Usually the Player
        
        [Header("Position Details")]
        [SerializeField] private Vector3 followOffset = new Vector3(-10f, 15f, -10f); // Default isometric angle
        [SerializeField] private float smoothSpeed = 10f; // Snappy tracking

        [Header("Look At Configuration")]
        [SerializeField] private bool lookAtTarget = false; // If true, camera alters rotation. Hades usually doesn't, so false by default.

        private Vector3 currentActualPosition;

        private void Start()
        {
            if (target != null)
            {
                SnapToTarget();
            }
        }

        public void SnapToTarget()
        {
            if (target == null) return;
            // Snap directly to the player right as the game starts (or portals) so we don't awkwardly pan in
            currentActualPosition = target.position + followOffset;
            transform.position = currentActualPosition;
            
            // Align rotation immediately to perfectly center the character
            if (lookAtTarget)
                transform.LookAt(target);
            else
                transform.rotation = Quaternion.LookRotation(-followOffset);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                // Attempt to find the player if target gets destroyed/disconnected (common in procedural/level reloads)
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null) 
                {
                    target = playerObj.transform;
                    currentActualPosition = target.position + followOffset;
                }
                else 
                {
                    return;
                }
            }

            // Calculate the desired position
            Vector3 desiredPosition = target.position + followOffset;

            // Lerp to the target position, tracking logic separated from physical transform to avoid shake drift
            currentActualPosition = Vector3.Lerp(currentActualPosition, desiredPosition, smoothSpeed * Time.deltaTime);

            Vector3 juiceOffset = Vector3.zero;
            // Only apply juice in play mode, preventing Editor-time null refs
            if (Application.isPlaying && CameraJuiceManager.Instance != null && CameraJuiceManager.Instance.IsShaking)
            {
                juiceOffset = CameraJuiceManager.Instance.CurrentShakeOffset;
            }

            // Apply position
            transform.position = currentActualPosition + juiceOffset;

            // Apply optional look at
            if (lookAtTarget)
            {
                transform.LookAt(target);
            }
            else
            {
                // Lock rotation to point EXACTLY opposite to the offset. This ensures the player is permanently DEAD CENTER on the screen.
                transform.rotation = Quaternion.LookRotation(-followOffset);
            }
        }
    }
}