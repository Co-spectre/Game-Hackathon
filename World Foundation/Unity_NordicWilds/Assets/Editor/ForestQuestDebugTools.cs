using UnityEngine;
using UnityEditor;
using NordicWilds.World;

namespace NordicWilds.EditorTools
{
    public class ForestQuestDebugTools
    {
        [MenuItem("World Foundation/Debug/Skip to Goon Fight")]
        public static void SkipToGoon()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("You must be in Play Mode to use this skip tool.");
                return;
            }

            ForestQuestController controller = Object.FindFirstObjectByType<ForestQuestController>();
            if (controller == null)
            {
                Debug.LogError("ForestQuestController not found in scene.");
                return;
            }

            controller.DebugSkipToPanic();
        }

        [MenuItem("World Foundation/Debug/Win Fight & Enable Ship")]
        public static void WinFightAndEnableShip()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("You must be in Play Mode to use this skip tool.");
                return;
            }

            ForestQuestController controller = Object.FindFirstObjectByType<ForestQuestController>();
            if (controller == null)
            {
                Debug.LogError("ForestQuestController not found in scene.");
                return;
            }

            // Call the victory logic directly
            // We'll use a new public method for this or just trigger the boss death routine
            controller.StartCoroutine(ForceWinRoutine(controller));
        }

        private static System.Collections.IEnumerator ForceWinRoutine(ForestQuestController controller)
        {
            Debug.Log("[Debug] Forcing Victory State...");
            
            // Advance state to Boss if not already there, then trigger death
            // We use reflection or just assume we can call the coroutine
            
            // Just trigger the end-of-combat flow
            // Note: AfterCombatRoutine handles the camera pan and enabling the dock
            controller.StopAllCoroutines(); 
            controller.StartCoroutine("AfterCombatRoutine");
            
            yield break;
        }

        [MenuItem("World Foundation/Debug/Skip to Japan Island")]
        public static void SkipToJapan()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("You must be in Play Mode to use this skip tool.");
                return;
            }

            ForestQuestController controller = Object.FindFirstObjectByType<ForestQuestController>();
            if (controller == null)
            {
                Debug.LogError("ForestQuestController not found in scene.");
                return;
            }

            // Move player and camera immediately
            Transform player = GameObject.FindWithTag("Player")?.transform;
            if (player != null)
            {
                Vector3 japanPos = controller.yamatoBoatLandingPoint;
                player.position = japanPos;
                
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.position = japanPos;
                    rb.linearVelocity = Vector3.zero;
                }
                
                if (Camera.main != null)
                {
                    var follow = Camera.main.GetComponent<NordicWilds.CameraSystems.IsometricCameraFollow>();
                    if (follow != null)
                    {
                        follow.target = player;
                        follow.SnapToTarget();
                    }
                }
                
                Debug.Log("Teleported to Japan Island!");
            }
        }
    }
}
