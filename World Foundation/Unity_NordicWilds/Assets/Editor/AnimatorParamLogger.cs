using UnityEngine;
using UnityEditor;

namespace NordicWilds.EditorTools
{
    public class AnimatorParamLogger
    {
        [MenuItem("World Foundation/Debug - Log Leaf Animator Params")]
        public static void LogParams()
        {
            GameObject leaf = GameObject.Find("Leaf");
            if (leaf == null)
            {
                // Try finding by script
                var quest = Object.FindFirstObjectByType<NordicWilds.World.ForestQuestController>();
                if (quest != null && quest.leaf != null) leaf = quest.leaf.gameObject;
            }

            if (leaf == null)
            {
                Debug.LogError("Could not find Leaf!");
                return;
            }

            Animator anim = leaf.GetComponentInChildren<Animator>();
            if (anim == null)
            {
                Debug.LogError("Leaf has no Animator!");
                return;
            }

            Debug.Log($"[Leaf Debug] Animator Controller: {anim.runtimeAnimatorController?.name}");
            for (int i = 0; i < anim.parameterCount; i++)
            {
                var p = anim.parameters[i];
                Debug.Log($"   -> Param: {p.name} ({p.type})");
            }
        }
    }
}
