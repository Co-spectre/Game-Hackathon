using UnityEngine;
using System.Collections;
using NordicWilds.Player;
using NordicWilds.CameraSystems;
using NordicWilds.Combat;
using NordicWilds.UI;

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

            // Change Environment lighting based on destination — both palettes are
            // sunset / low-contrast variants of the same dusk mood, just hue-shifted.
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientIntensity = 0.85f;
            RenderSettings.reflectionIntensity = 0.30f;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;

            if (isReturnPortal)
            {
                // Frostheim — cool dusk
                RenderSettings.fogDensity = 0.020f;
                RenderSettings.fogColor = new Color(0.36f, 0.28f, 0.32f);
                RenderSettings.ambientSkyColor     = new Color(0.42f, 0.34f, 0.34f);
                RenderSettings.ambientEquatorColor = new Color(0.32f, 0.28f, 0.32f);
                RenderSettings.ambientGroundColor  = new Color(0.20f, 0.22f, 0.28f);

                if (Camera.main != null)
                {
                    Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    Camera.main.backgroundColor = new Color(0.30f, 0.22f, 0.26f);
                    Camera.main.allowHDR = false;
                }

                var dirLight = Object.FindFirstObjectByType<Light>();
                if (dirLight != null && dirLight.type == LightType.Directional)
                {
                    dirLight.color = new Color(0.96f, 0.66f, 0.42f);
                    dirLight.intensity = 0.55f;
                    dirLight.transform.rotation = Quaternion.Euler(14f, -54f, 0f);
                    dirLight.shadows = LightShadows.Soft;
                    dirLight.shadowStrength = 0.45f;
                }
            }
            else
            {
                // Yamato — warm dusk
                RenderSettings.fogDensity = 0.013f;
                RenderSettings.fogColor = new Color(0.46f, 0.30f, 0.32f);
                RenderSettings.ambientSkyColor     = new Color(0.50f, 0.36f, 0.34f);
                RenderSettings.ambientEquatorColor = new Color(0.38f, 0.30f, 0.32f);
                RenderSettings.ambientGroundColor  = new Color(0.22f, 0.20f, 0.28f);

                if (Camera.main != null)
                {
                    Camera.main.clearFlags = CameraClearFlags.SolidColor;
                    Camera.main.backgroundColor = new Color(0.32f, 0.22f, 0.26f);
                    Camera.main.allowHDR = false;
                }

                var dirLight = Object.FindFirstObjectByType<Light>();
                if (dirLight != null && dirLight.type == LightType.Directional)
                {
                    dirLight.color = new Color(0.96f, 0.58f, 0.32f);
                    dirLight.intensity = 0.55f;
                    dirLight.transform.rotation = Quaternion.Euler(10f, -32f, 0f);
                    dirLight.shadows = LightShadows.Soft;
                    dirLight.shadowStrength = 0.38f;
                }

                if (Object.FindFirstObjectByType<YamatoArrivalDialogue>() == null)
                {
                    new GameObject("YamatoArrivalDialogue").AddComponent<YamatoArrivalDialogue>();
                }
                if (Object.FindFirstObjectByType<WorldMapOverlay>() == null)
                {
                    new GameObject("WorldMapOverlay").AddComponent<WorldMapOverlay>();
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
