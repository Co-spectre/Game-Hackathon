using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

namespace NordicWilds.EditorTools
{
    public class LeafAnimatorFixer
    {
        [MenuItem("World Foundation/Fix Leaf Animator")]
        public static void FixLeaf()
        {
            string controllerPath = "Assets/Imported/leaf.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

            if (controller == null)
            {
                Debug.LogError("Could not find leaf.controller at " + controllerPath);
                return;
            }

            // Clear existing
            controller.parameters = new AnimatorControllerParameter[0];
            while (controller.layers.Length > 0) controller.RemoveLayer(0);
            controller.AddLayer("Base Layer");
            var rootStateMachine = controller.layers[0].stateMachine;

            // Add Speed parameter
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

            // Find Clips
            AnimationClip walkClip = FindClip("Meshy_AI_Emerald_Ranger_biped_Animation_Walking_withSkin");
            // Use the player's stance animation for Leaf's idle, as they share the same Humanoid rig type. 
            // This completely prevents the T-pose sinking issue.
            AnimationClip idleClip = FindClip("Meshy_AI_kairon_main_guy_biped_Animation_Axe_Stance_withSkin"); 
            
            if (walkClip == null) Debug.LogWarning("Walking clip not found!");
            if (idleClip == null) Debug.LogWarning("Stance clip not found!");

            // Setup States
            AnimatorState idleState = rootStateMachine.AddState("Idle");
            idleState.motion = idleClip;

            AnimatorState walkState = rootStateMachine.AddState("Walking");
            walkState.motion = walkClip;

            // Transitions
            var idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.25f;

            var walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.25f;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            // Link to Leaf in scene
            var quest = Object.FindFirstObjectByType<NordicWilds.World.ForestQuestController>();
            if (quest != null && quest.leaf != null)
            {
                Animator anim = quest.leaf.GetComponentInChildren<Animator>();
                if (anim == null) anim = quest.leaf.gameObject.AddComponent<Animator>();
                
                anim.runtimeAnimatorController = controller;
                
                // Try to find and assign Avatar
                string avatarPath = "Assets/Imported/Meshy_AI_Emerald_Ranger_biped_Character_output.fbx";
                Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(avatarPath).OfType<Avatar>().FirstOrDefault();
                if (avatar != null) anim.avatar = avatar;
                
                // Set Rig to Humanoid
                FixRig(AssetDatabase.GetAssetPath(walkClip));
                FixRig("Assets/Imported/Meshy_AI_Emerald_Ranger_biped_Character_output.fbx");
                
                Debug.Log("Leaf Animator Fixed! Make sure to set the Avatar in the Animator component if she looks like a T-pose.");
            }
        }

        private static AnimationClip FindClip(string fbxName)
        {
            string path = $"Assets/Imported/{fbxName}.fbx";
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            return assets.OfType<AnimationClip>().FirstOrDefault(c => !c.name.Contains("__preview__"));
        }

        private static void FixRig(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) return;

            bool changed = false;

            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                changed = true;
            }

            // Force looping on walking animations
            if (path.Contains("Walking"))
            {
                var animations = importer.clipAnimations;
                if (animations == null || animations.Length == 0) animations = importer.defaultClipAnimations;

                if (animations != null && animations.Length > 0)
                {
                    foreach (var clip in animations)
                    {
                        if (!clip.loopTime)
                        {
                            clip.loopTime = true;
                            changed = true;
                        }
                    }
                    importer.clipAnimations = animations;
                }
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }
    }
}
