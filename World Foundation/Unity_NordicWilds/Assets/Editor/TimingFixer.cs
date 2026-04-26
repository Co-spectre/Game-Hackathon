using UnityEngine;
using UnityEditor;
using NordicWilds.Player;

namespace NordicWilds.Combat
{
    public class TimingFixer
    {
        [InitializeOnLoadMethod]
        public static void FixTimings()
        {
            string prefabPath = "Assets/Prefabs/Player.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                // Fix Combat Timings
                var combat = prefab.GetComponent<PlayerCombat>();
                if (combat != null)
                {
                    var so = new SerializedObject(combat);
                    so.FindProperty("hitDelayNormal").floatValue = 1.10f;
                    so.FindProperty("hitDelayFinisher").floatValue = 1.10f;
                    so.FindProperty("cooldownNormal").floatValue = 1.40f;
                    so.FindProperty("cooldownFinisher").floatValue = 1.50f;
                    so.ApplyModifiedProperties();
                }

                // Fix Controller Layers (Jump)
                var controller = prefab.GetComponent<PlayerController>();
                if (controller != null)
                {
                    var so = new SerializedObject(controller);
                    var prop = so.FindProperty("groundLayer");
                    if (prop != null) prop.intValue = 1; // Default layer
                    so.ApplyModifiedProperties();
                }

                EditorUtility.SetDirty(prefab);
                AssetDatabase.SaveAssets();
                Debug.Log("[TimingFixer] Successfully forced combat and jump settings on Prefab.");
            }

            GameObject playerInScene = GameObject.Find("Player");
            if (playerInScene != null)
            {
                var combatScene = playerInScene.GetComponent<PlayerCombat>();
                if (combatScene != null)
                {
                    var so = new SerializedObject(combatScene);
                    so.FindProperty("hitDelayNormal").floatValue = 1.10f;
                    so.FindProperty("hitDelayFinisher").floatValue = 1.10f;
                    so.FindProperty("cooldownNormal").floatValue = 1.40f;
                    so.FindProperty("cooldownFinisher").floatValue = 1.50f;
                    so.ApplyModifiedProperties();
                }

                var controllerScene = playerInScene.GetComponent<PlayerController>();
                if (controllerScene != null)
                {
                    var so = new SerializedObject(controllerScene);
                    var prop = so.FindProperty("groundLayer");
                    if (prop != null) prop.intValue = 1; // Default layer
                    so.ApplyModifiedProperties();
                }
                
                Debug.Log("[TimingFixer] Successfully forced settings on Scene Object.");
            }
        }
    }
}
