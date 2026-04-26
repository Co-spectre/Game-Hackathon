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

            if (CameraJuiceManager.Instance != null)
            {
                CameraJuiceManager.Instance.ShakeCamera(0.5f, 0.5f);
            }

            yield return new WaitForSeconds(teleportDelay);

            Rigidbody pRb = player.GetComponent<Rigidbody>();
            if (pRb != null)
            {
                pRb.linearVelocity = Vector3.zero;
                pRb.position = destinationTarget;
            }

            player.position = destinationTarget;

            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.SetRespawnPoint(destinationTarget);
            }

            if (Camera.main != null)
            {
                var camScript = Camera.main.GetComponent<IsometricCameraFollow>();
                if (camScript != null)
                {
                    camScript.SnapToTarget();
                }
            }

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
                RenderSettings.ambientSkyColor = new Color(0.42f, 0.34f, 0.34f);
                RenderSettings.ambientEquatorColor = new Color(0.32f, 0.28f, 0.32f);
                RenderSettings.ambientGroundColor = new Color(0.20f, 0.22f, 0.28f);

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

                MissionTracker.Set(
                    "Chapter 8: Return to Frostheim",
                    "Defend the village. The raiders have come."
                );
            }
            else
            {
                // Yamato — warm dusk
                RenderSettings.fogDensity = 0.013f;
                RenderSettings.fogColor = new Color(0.46f, 0.30f, 0.32f);
                RenderSettings.ambientSkyColor = new Color(0.50f, 0.36f, 0.34f);
                RenderSettings.ambientEquatorColor = new Color(0.38f, 0.30f, 0.32f);
                RenderSettings.ambientGroundColor = new Color(0.22f, 0.20f, 0.28f);

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

                MissionTracker.Set(
                    "Chapter 7: Yamato",
                    "Explore the village and find the return portal beyond the shrine."
                );
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