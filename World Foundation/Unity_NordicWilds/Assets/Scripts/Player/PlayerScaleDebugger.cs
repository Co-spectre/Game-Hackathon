using UnityEngine;

namespace NordicWilds.Player
{
    public class PlayerScaleDebugger : MonoBehaviour
    {
        private Transform visualRoot;
        private Transform meshyChar;

        private void Start()
        {
            visualRoot = transform.Find("VisualRoot");
            if (visualRoot != null)
            {
                meshyChar = visualRoot.Find("MeshyCharacter");
            }
            else
            {
                // Fallback for old hierarchy
                meshyChar = transform.Find("PlayerVisual");
            }

            Debug.Log($"[Scale Check At START] Player: {transform.localScale} | VisualRoot: {(visualRoot ? visualRoot.localScale.ToString() : "N/A")} | Meshy Model: {(meshyChar ? meshyChar.localScale.ToString() : "N/A")}");
            
            // Check Animators
            Animator[] animators = GetComponentsInChildren<Animator>(true);
            Debug.Log($"[Animator Check] Found {animators.Length} Animators on Player.");
            foreach(var anim in animators)
            {
                Debug.Log($"   -> Animator on: {anim.gameObject.name} | Active: {anim.gameObject.activeInHierarchy} | Controller: {(anim.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "NULL")} | Avatar: {(anim.avatar != null ? anim.avatar.name : "NULL")}");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log($"[Scale Check ON DEMAND] Player: {transform.localScale} | VisualRoot: {(visualRoot ? visualRoot.localScale.ToString() : "N/A")} | Meshy Model: {(meshyChar ? meshyChar.localScale.ToString() : "N/A")}");
            }
        }
    }
}
