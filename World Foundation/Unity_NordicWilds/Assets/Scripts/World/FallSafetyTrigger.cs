using UnityEngine;
using NordicWilds.Player;
using NordicWilds.Combat;
using NordicWilds.CameraSystems;

namespace NordicWilds.World
{
    /// <summary>
    /// Large invisible trigger placed well below the playable world. If the player
    /// ever falls into it (map hole, physics glitch, off-cliff), they are gently
    /// warped back to a safe respawn position. Prevents the "character falls off
    /// the map" class of bugs regardless of where it happens.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FallSafetyTrigger : MonoBehaviour
    {
        [SerializeField] private Vector3 respawnPosition = Vector3.zero;
        [SerializeField] private float cooldown = 1.0f;

        private float nextAllowedRespawn;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            if (Time.time < nextAllowedRespawn)
                return;

            nextAllowedRespawn = Time.time + cooldown;
            RespawnPlayer(other.transform);
        }

        private void RespawnPlayer(Transform player)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
                pc.SetControlsLocked(true);

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = respawnPosition;
            }

            player.position = respawnPosition;

            Health hp = player.GetComponent<Health>();
            if (hp != null)
                hp.SetRespawnPoint(respawnPosition);

            if (Camera.main != null)
            {
                IsometricCameraFollow cf = Camera.main.GetComponent<IsometricCameraFollow>();
                if (cf != null)
                    cf.SnapToTarget();
            }

            if (CameraJuiceManager.Instance != null)
                CameraJuiceManager.Instance.ShakeCamera(0.15f, 0.1f);

            if (pc != null)
                pc.SetControlsLocked(false);
        }
    }
}
