using UnityEngine;

namespace NordicWilds.World
{
    [RequireComponent(typeof(Collider))]
    public class ForestBoatInteraction : MonoBehaviour
    {
        private const string BoatPrompt = "Press E to board the boat";

        public Transform boatRoot;

        private ForestQuestController controller;
        private bool playerNearby;

        public Transform BoatRoot => boatRoot != null ? boatRoot : transform;

        public void SetController(ForestQuestController questController)
        {
            controller = questController;
        }

        private void Awake()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
        }

        private void Update()
        {
            if (playerNearby && controller != null)
            {
                if (controller.CanBoardBoat)
                    controller.SetPrompt(BoatPrompt);
                else
                    controller.ClearPrompt(BoatPrompt);
            }

            if (playerNearby && controller != null && controller.CanBoardBoat && Input.GetKeyDown(KeyCode.E))
                controller.TryStartBoatJourney(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            playerNearby = true;
            if (controller != null && controller.CanBoardBoat)
                controller.SetPrompt(BoatPrompt);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            playerNearby = false;
            if (controller != null)
                controller.ClearPrompt(BoatPrompt);
        }
    }
}
