using UnityEngine;
using UnityEditor;
using NordicWilds.World;
using NordicWilds.Combat;

namespace NordicWilds.EditorTools
{
    public class ForestFoeDuplicator
    {
        [MenuItem("World Foundation/Duplicate Forest Foe 1")]
        public static void DuplicateFoes()
        {
            var quest = Object.FindFirstObjectByType<ForestQuestController>(FindObjectsInactive.Include);
            if (quest == null)
            {
                Debug.LogError("Could not find ForestQuestController in the scene.");
                return;
            }

            GameObject foe1 = null;

            // Find Foe 1
            if (quest.protectors != null && quest.protectors.Length > 0 && quest.protectors[0] != null)
            {
                foe1 = quest.protectors[0].gameObject;
            }
            else
            {
                foe1 = GameObject.Find("Forest Foe 1");
            }

            if (foe1 == null)
            {
                Debug.LogError("Could not find 'Forest Foe 1'. Make sure it exists and is named correctly.");
                return;
            }

            // Delete old foes
            for (int i = 1; i < quest.protectors.Length; i++)
            {
                if (quest.protectors[i] != null && quest.protectors[i].gameObject != foe1)
                {
                    Object.DestroyImmediate(quest.protectors[i].gameObject);
                }
            }

            // Create new array
            Health[] newProtectors = new Health[5];
            newProtectors[0] = foe1.GetComponent<Health>();

            // Duplicate 4 times
            for (int i = 1; i < 5; i++)
            {
                GameObject copy = Object.Instantiate(foe1, foe1.transform.parent);
                copy.name = "Forest Foe " + (i + 1);
                
                // Slightly offset position so they don't overlap in the editor perfectly
                copy.transform.position = foe1.transform.position + new Vector3(i * 2f, 0, 0);

                newProtectors[i] = copy.GetComponent<Health>();
            }

            // Assign back to quest controller
            quest.protectors = newProtectors;
            EditorUtility.SetDirty(quest.gameObject);

            Debug.Log("Successfully duplicated Forest Foe 1 and updated the Quest Controller!");
        }
    }
}
