using UnityEngine;
using UnityEditor;
using NordicWilds.Player;

namespace NordicWilds.EditorTools
{
    public class FixEverythingPlayer
    {
        [MenuItem("World Foundation/CRITICAL - Rebuild and Fix Player")]
        public static void FixEverything()
        {
            Debug.Log("[FixEverything] Starting deep fix...");

            // 1. Find the Player
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("[FixEverything] No 'Player' object found in scene!");
                return;
            }

            // 2. Find the Animator (should be on a child)
            Animator animator = player.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("[FixEverything] No Animator found. Checking for model...");
                // If no animator, find the model child and add one
                Transform visualRoot = player.transform.Find("VisualRoot");
                if (visualRoot != null && visualRoot.childCount > 0)
                {
                    animator = visualRoot.GetChild(0).gameObject.AddComponent<Animator>();
                }
                else
                {
                    Debug.LogError("[FixEverything] Could not find VisualRoot or model to add Animator to!");
                    return;
                }
            }

            // 3. Find the Controller
            string controllerPath = "Assets/PlayerAnimController.controller";
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (controller == null)
            {
                Debug.Log("[FixEverything] Controller missing. Rebuilding via Tool...");
                AnimatorSetupTool.CreateAndAssignAnimator();
                controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            }
            animator.runtimeAnimatorController = controller;

            // 4. Find the Avatar
            // We look for any Avatar asset in the project that matches our character
            string[] avatarGuids = AssetDatabase.FindAssets("Character_output t:Avatar");
            if (avatarGuids.Length > 0)
            {
                string avatarPath = AssetDatabase.GUIDToAssetPath(avatarGuids[0]);
                Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath);
                animator.avatar = avatar;
                Debug.Log("[FixEverything] Linked Avatar: " + avatar.name);
            }
            else
            {
                Debug.LogError("[FixEverything] Could not find a Humanoid Avatar! Make sure the Character FBX is set to Humanoid.");
            }

            // 5. Setup scale and physics
            Transform vr = player.transform.Find("VisualRoot");
            if (vr != null) vr.localScale = Vector3.one;
            animator.transform.localScale = Vector3.one;
            
            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            Debug.Log("✅ [FixEverything] Player structure, Animator, Controller, and Avatar are now FIXED.");
            Selection.activeGameObject = animator.gameObject;
        }
    }
}
