using UnityEngine;

namespace NordicWilds.World
{
    [RequireComponent(typeof(Collider))]
    public class ForestArtifact : MonoBehaviour
    {
        private const string ArtifactPrompt = "Press E to inspect the glowing artifact";

        public string ArtifactName = "Ancient Artifact";
        public bool IsCollected { get; private set; }

        private ForestQuestController controller;
        private bool playerNearby;
        private Vector3 baseScale;
        private Light glowLight;

        public void SetController(ForestQuestController questController, int index)
        {
            controller = questController;
            if (string.IsNullOrEmpty(ArtifactName))
                ArtifactName = "Artifact " + (index + 1);
        }

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;

            baseScale = transform.localScale;
            glowLight = GetComponentInChildren<Light>();
        }

        private void Update()
        {
            if (IsCollected)
                return;

            float pulse = 1f + Mathf.Sin(Time.time * 3.1f) * 0.07f;
            transform.localScale = baseScale * pulse;
            transform.Rotate(0f, 32f * Time.deltaTime, 0f, Space.World);

            if (glowLight != null)
                glowLight.intensity = 0.65f + Mathf.Sin(Time.time * 4.2f) * 0.18f;

            if (playerNearby && controller != null)
            {
                if (controller.CanCollectArtifacts)
                    controller.SetPrompt(ArtifactPrompt);
                else
                    controller.ClearPrompt(ArtifactPrompt);
            }

            if (playerNearby && controller != null && controller.CanCollectArtifacts && Input.GetKeyDown(KeyCode.E))
                controller.CollectArtifact(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            playerNearby = true;
            if (controller != null && controller.CanCollectArtifacts)
                controller.SetPrompt(ArtifactPrompt);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            playerNearby = false;
            if (controller != null)
                controller.ClearPrompt(ArtifactPrompt);
        }

        public void MarkCollected()
        {
            IsCollected = true;
            if (controller != null)
                controller.ClearPrompt(ArtifactPrompt);
            gameObject.SetActive(false);
        }
    }
}
