using UnityEngine;
using System.Collections;
using NordicWilds.Player;
using NordicWilds.CameraSystems;
using NordicWilds.Combat;

namespace NordicWilds.World
{
    public class JapanPortal : MonoBehaviour
    {
        public Vector3 destinationTarget;
        public bool isReturnPortal = false; // Check this to return to Nordic Village
        [SerializeField] private float teleportDelay = 0.5f;

        private bool isTeleporting = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!isTeleporting && other.CompareTag("Player"))
            {
                StartCoroutine(TeleportRoutine(other.transform));
            }
        }

        private IEnumerator TeleportRoutine(Transform player)
        {
            isTeleporting = true;

            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetControlsLocked(true);
            }

            // Optional: Freeze player input here

            // If we have a juice manager, shake camera or add a flash
            if (CameraJuiceManager.Instance != null)
            {
                CameraJuiceManager.Instance.ShakeCamera(0.5f, 0.5f);
            }

            yield return new WaitForSeconds(teleportDelay); // Dramatize the build up!

            // Force Physics Engine to acknowledge the massive distance change
            Rigidbody pRb = player.GetComponent<Rigidbody>();
            if (pRb != null) 
            {
                pRb.linearVelocity = Vector3.zero; // cancel momentum
                pRb.position = destinationTarget;
            }
            player.position = destinationTarget;

            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
                playerHealth.SetRespawnPoint(destinationTarget);

            // Instantly Snap the Camera so it doesn't slowly pan 10,000 units across the void!
            if (Camera.main != null)
            {
                var camScript = Camera.main.GetComponent<IsometricCameraFollow>();
                if (camScript != null) camScript.SnapToTarget();
            }

            // Change Environment lighting based on destination
            if (isReturnPortal)
            {
                RenderSettings.fogColor = new Color(0.6f, 0.75f, 0.9f);
                RenderSettings.ambientSkyColor = new Color(0.4f, 0.5f, 0.7f);
                RenderSettings.ambientEquatorColor = new Color(0.3f, 0.4f, 0.6f);
                RenderSettings.ambientGroundColor = new Color(0.2f, 0.3f, 0.5f);

                var dirLight = Object.FindFirstObjectByType<Light>();
                if (dirLight != null && dirLight.type == LightType.Directional)
                {
                    dirLight.color = new Color(0.6f, 0.75f, 1f);
                    dirLight.intensity = 1.1f;
                }
            }
            else
            {
                RenderSettings.fogColor = new Color(1f, 0.8f, 0.9f); // Pinkish fog
                RenderSettings.ambientSkyColor = new Color(0.9f, 0.8f, 0.85f);
                RenderSettings.ambientEquatorColor = new Color(0.8f, 0.7f, 0.75f);
                RenderSettings.ambientGroundColor = new Color(0.6f, 0.8f, 0.6f);

                var dirLight = Object.FindFirstObjectByType<Light>();
                if (dirLight != null && dirLight.type == LightType.Directional)
                {
                    dirLight.color = new Color(1f, 0.95f, 0.85f); // Warm spring sun
                    dirLight.intensity = 1.25f;
                }
            }

            yield return new WaitForSeconds(0.2f);

            if (playerController != null)
            {
                playerController.SetControlsLocked(false);
            }

            isTeleporting = false;
        }
    }
}
