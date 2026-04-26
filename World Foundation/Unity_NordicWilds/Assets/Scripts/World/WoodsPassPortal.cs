using System.Collections;
using UnityEngine;
using NordicWilds.Player;
using NordicWilds.Combat;
using NordicWilds.CameraSystems;

namespace NordicWilds.World
{
    [RequireComponent(typeof(Collider))]
    public class WoodsPassPortal : MonoBehaviour
    {
        [SerializeField] private Vector3 destinationTarget = new Vector3(340f, 1.5f, 108f);
        [SerializeField] private float teleportDelay = 0.2f;
        [SerializeField] private bool oneShot = true;

        private bool isTeleporting;
        private bool hasTeleported;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isTeleporting)
                return;

            if (oneShot && hasTeleported)
                return;

            if (!other.CompareTag("Player"))
                return;

            StartCoroutine(TeleportRoutine(other.transform));
        }

        private IEnumerator TeleportRoutine(Transform player)
        {
            isTeleporting = true;

            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
                playerController.SetControlsLocked(true);

            if (CameraJuiceManager.Instance != null)
                CameraJuiceManager.Instance.ShakeCamera(0.18f, 0.12f);

            yield return new WaitForSeconds(teleportDelay);

            Rigidbody body = player.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.position = destinationTarget;
            }

            player.position = destinationTarget;

            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
                playerHealth.SetRespawnPoint(destinationTarget);

            if (Camera.main != null)
            {
                IsometricCameraFollow cameraFollow = Camera.main.GetComponent<IsometricCameraFollow>();
                if (cameraFollow != null)
                    cameraFollow.SnapToTarget();
            }

            yield return null;

            if (playerController != null)
                playerController.SetControlsLocked(false);

            hasTeleported = true;
            isTeleporting = false;
        }
    }
}
