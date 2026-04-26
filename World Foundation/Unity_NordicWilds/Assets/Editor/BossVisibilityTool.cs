using UnityEngine;
using UnityEditor;

namespace NordicWilds.EditorTools
{
    public class BossVisibilityTool : EditorWindow
    {
        [MenuItem("World Foundation/Toggle Enemy Visibility")]
        public static void ToggleEnemies()
        {
            var quest = Object.FindFirstObjectByType<NordicWilds.World.ForestQuestController>();
            if (quest == null)
            {
                Debug.LogWarning("Could not find ForestQuestController in the scene.");
                return;
            }

            bool anyHidden = false;

            // Check bosses
            if (quest.finalBoss != null && !quest.finalBoss.gameObject.activeSelf) anyHidden = true;
            
            if (quest.protectors != null)
            {
                foreach (var p in quest.protectors)
                {
                    if (p != null && !p.gameObject.activeSelf) anyHidden = true;
                }
            }

            // Toggle logic
            bool newState = anyHidden; // If any are hidden, show them all. Otherwise, hide them all.

            if (quest.finalBoss != null)
            {
                quest.finalBoss.gameObject.SetActive(newState);
                EditorUtility.SetDirty(quest.finalBoss.gameObject);
            }

            if (quest.protectors != null)
            {
                foreach (var p in quest.protectors)
                {
                    if (p != null)
                    {
                        p.gameObject.SetActive(newState);
                        EditorUtility.SetDirty(p.gameObject);
                    }
                }
            }

            Debug.Log($"[Enemy Visibility] Enemies are now {(newState ? "VISIBLE for editing" : "HIDDEN for gameplay")}.");
        }
    }
}
