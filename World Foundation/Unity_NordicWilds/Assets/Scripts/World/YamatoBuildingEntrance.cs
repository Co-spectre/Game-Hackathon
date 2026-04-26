using System.Collections;
using NordicWilds.CameraSystems;
using NordicWilds.Combat;
using NordicWilds.Player;
using UnityEngine;

namespace NordicWilds.World
{
    [RequireComponent(typeof(Collider))]
    public class YamatoBuildingEntrance : MonoBehaviour
    {
        public GameObject interiorRoot;
        public Vector3 destinationTarget = new Vector3(10092f, 1.05f, 9920f);
        public string prompt = "Press E to enter the bamboo dojo";

        private bool playerNearby;
        private bool entering;
        private float fadeAlpha;
        private string message;
        private float messageUntil;
        private GUIStyle promptStyle;
        private GUIStyle messageStyle;

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;

            if (interiorRoot != null)
                interiorRoot.SetActive(false);
        }

        private void Update()
        {
            if (playerNearby && !entering && Input.GetKeyDown(KeyCode.E))
                StartCoroutine(EnterRoutine());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                playerNearby = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                playerNearby = false;
        }

        private IEnumerator EnterRoutine()
        {
            entering = true;
            GameObject playerObj = GameObject.FindWithTag("Player");
            Transform player = playerObj != null ? playerObj.transform : null;
            PlayerController controller = playerObj != null ? playerObj.GetComponent<PlayerController>() : null;
            Rigidbody body = playerObj != null ? playerObj.GetComponent<Rigidbody>() : null;

            if (controller != null)
                controller.SetControlsLocked(true);

            message = "The bamboo opens into a hidden hall.";
            messageUntil = Time.time + 2.0f;

            float t = 0f;
            while (t < 0.75f)
            {
                t += Time.deltaTime;
                fadeAlpha = Mathf.Clamp01(t / 0.75f);
                yield return null;
            }

            if (interiorRoot != null)
                interiorRoot.SetActive(true);

            if (player != null)
            {
                if (body != null)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                    body.position = destinationTarget;
                }

                player.position = destinationTarget;
                Health health = player.GetComponent<Health>();
                if (health != null)
                    health.SetRespawnPoint(destinationTarget);
            }

            if (Camera.main != null)
            {
                IsometricCameraFollow follow = Camera.main.GetComponent<IsometricCameraFollow>();
                if (follow != null)
                    follow.SnapToTarget();
            }

            yield return new WaitForSeconds(0.2f);

            t = 0f;
            while (t < 0.9f)
            {
                t += Time.deltaTime;
                fadeAlpha = Mathf.Lerp(1f, 0f, t / 0.9f);
                yield return null;
            }

            fadeAlpha = 0f;
            message = "Final trial: defeat the warlord inside the dojo.";
            messageUntil = Time.time + 4.0f;

            if (controller != null)
                controller.SetControlsLocked(false);
        }

        private void OnGUI()
        {
            EnsureStyles();

            if (playerNearby && !entering)
                GUI.Label(new Rect((Screen.width - 520f) * 0.5f, Screen.height - 104f, 520f, 34f), prompt, promptStyle);

            if (!string.IsNullOrEmpty(message) && Time.time < messageUntil)
                GUI.Label(new Rect((Screen.width - 680f) * 0.5f, 78f, 680f, 42f), message, messageStyle);

            if (fadeAlpha > 0.001f)
            {
                Color previous = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, fadeAlpha);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = previous;
            }
        }

        private void EnsureStyles()
        {
            if (promptStyle != null)
                return;

            promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            promptStyle.normal.textColor = new Color(0.95f, 0.88f, 0.68f, 0.96f);

            messageStyle = new GUIStyle(promptStyle)
            {
                fontSize = 22
            };
            messageStyle.normal.textColor = new Color(0.98f, 0.88f, 0.58f, 1f);
        }
    }
}
