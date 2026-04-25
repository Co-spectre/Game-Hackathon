using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using NordicWilds.CameraSystems;
using NordicWilds.Player;

namespace NordicWilds.UI
{
    public class MainMenuController : MonoBehaviour
    {
        public CanvasGroup faderGroup;
        public GameObject boat;
        public Transform player;
        public Camera mainCamera;
        
        public Vector3 gameStartPos = new Vector3(10000f, 1f, 9980f); // Yamato
        
        private bool isStarting = false;

        private void Start()
        {
            var btn = transform.Find("StartButton")?.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(StartGame);
            }
        }

        public void StartGame()
        {
            if (isStarting) return;
            isStarting = true;
            StartCoroutine(StartGameSequence());
        }

        private IEnumerator StartGameSequence()
        {
            // 1. Boat moves away
            float elapsedTime = 0f;
            Vector3 startBoatPos = boat.transform.position;
            Vector3 targetBoatPos = startBoatPos + new Vector3(0, 0, 20f); // move forward

            // Hide UI except fader
            foreach (Transform child in transform)
            {
                if (child.gameObject.name != "FaderPanel")
                    child.gameObject.SetActive(false);
            }

            // Disable player control while in cutscene
            var pController = player.GetComponent<PlayerController>();
            if (pController != null) pController.enabled = false;

            while (elapsedTime < 3f)
            {
                boat.transform.position = Vector3.Lerp(startBoatPos, targetBoatPos, elapsedTime / 3f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // 2. Fade to black
            elapsedTime = 0f;
            while (elapsedTime < 2f)
            {
                faderGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / 2f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            faderGroup.alpha = 1f;

            // 3. Teleport Player & update Camera
            player.SetParent(null); // Unparent from boat
            player.position = gameStartPos;
            boat.SetActive(false); // Hide boat
            
            var isoCam = mainCamera.GetComponentInParent<IsometricCameraFollow>();
            if (isoCam != null)
            {
                isoCam.target = player;
                // Snap camera to new pos instantly
                mainCamera.transform.parent.position = player.position + new Vector3(-15f, 20f, -15f);
            }

            // Small delay
            yield return new WaitForSeconds(1f);

            // 4. Fade back in
            elapsedTime = 0f;
            while (elapsedTime < 2f)
            {
                faderGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / 2f);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            faderGroup.alpha = 0f;

            if (pController != null) pController.enabled = true;
            
            // Cleanup UI
            Destroy(gameObject);
        }
    }
}
