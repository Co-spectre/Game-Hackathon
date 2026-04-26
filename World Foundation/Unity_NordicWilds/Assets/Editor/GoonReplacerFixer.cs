using UnityEngine;
using UnityEditor;
using NordicWilds.World;
using NordicWilds.Combat;
using System.Linq;

namespace NordicWilds.EditorTools
{
    public class GoonReplacerFixer
    {
        [MenuItem("World Foundation/CRITICAL - Replace and Fix Goons")]
        public static void ReplaceAndFixGoons()
        {
            var quest = Object.FindFirstObjectByType<ForestQuestController>(FindObjectsInactive.Include);
            if (quest == null)
            {
                Debug.LogError("Could not find ForestQuestController in the scene.");
                return;
            }

            GameObject foe1 = null;
            if (quest.protectors != null && quest.protectors.Length > 0 && quest.protectors[0] != null)
            {
                foe1 = quest.protectors[0].gameObject;
            }
            else
            {
                foe1 = GameObject.Find("Forest Foe 1");
            }

            if (foe1 == null) return;

            // Decrease Speed on Foe 1
            EnemyAI ai1 = foe1.GetComponent<EnemyAI>();
            if (ai1 != null) ai1.SetAggression(2.5f, 20f, 2f); // Slower speed

            // Fix the Animator on Foe 1 (don't override Avatar if already set, but ensure it exists)
            string controllerPath = "Assets/Imported/GOOn2/goon2.controller";
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (controller != null)
            {
                // Find the actual model object (usually named "goon" or has the mesh)
                Transform goonTransform = foe1.transform.Find("goon");
                if (goonTransform == null) goonTransform = foe1.transform; // Fallback to root

                Animator anim1 = goonTransform.GetComponent<Animator>();
                if (anim1 == null) anim1 = goonTransform.gameObject.AddComponent<Animator>();
                
                anim1.runtimeAnimatorController = controller;
                
                // Try to find the correct Avatar for this specific model
                if (anim1.avatar == null)
                {
                    string avatarPath = "Assets/Imported/GOOn2/Meshy_AI_Ironwood_Marauder_biped_Character_output.fbx";
                    Avatar av = AssetDatabase.LoadAllAssetsAtPath(avatarPath).OfType<Avatar>().FirstOrDefault();
                    if (av != null) anim1.avatar = av;
                }
                
                EditorUtility.SetDirty(anim1.gameObject);
            }

            // Delete old foes
            for (int i = 1; i < quest.protectors.Length; i++)
            {
                if (quest.protectors[i] != null && quest.protectors[i].gameObject != foe1)
                {
                    Object.DestroyImmediate(quest.protectors[i].gameObject);
                }
            }
            // Also clean up any extra duplicates named "Forest Foe X"
            for (int i = 2; i <= 10; i++)
            {
                GameObject extra = GameObject.Find("Forest Foe " + i);
                if (extra != null) Object.DestroyImmediate(extra);
            }

            // Create new array
            Health[] newProtectors = new Health[5];
            newProtectors[0] = foe1.GetComponent<Health>();

            // Duplicate 4 times
            for (int i = 1; i < 5; i++)
            {
                // Instantiate makes a perfect exact clone of Foe 1
                GameObject copy = Object.Instantiate(foe1, foe1.transform.parent);
                copy.name = "Forest Foe " + (i + 1);
                
                // Offset position
                copy.transform.position = foe1.transform.position + new Vector3(i * 1.5f, 0, 0);

                newProtectors[i] = copy.GetComponent<Health>();
            }

            // Assign back to quest controller
            quest.protectors = newProtectors;
            EditorUtility.SetDirty(quest.gameObject);

            Debug.Log("Goons replaced, speed decreased, and animator fixed!");
        }
    }
}
