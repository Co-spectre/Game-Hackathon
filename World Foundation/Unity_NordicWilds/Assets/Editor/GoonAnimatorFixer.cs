using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using NordicWilds.World;

namespace NordicWilds.EditorTools
{
    public class GoonAnimatorFixer
    {
        [MenuItem("World Foundation/Fix Goon Animator")]
        public static void FixGoon()
        {
            string controllerPath = "Assets/Imported/GOOn2/goon2.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

            if (controller == null)
            {
                // Create it if it doesn't exist
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            // Clear existing
            controller.parameters = new AnimatorControllerParameter[0];
            while (controller.layers.Length > 0) controller.RemoveLayer(0);
            controller.AddLayer("Base Layer");
            var rootStateMachine = controller.layers[0].stateMachine;

            // Add Parameters
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            // Find Clips
            AnimationClip runClip = FindClip("Meshy_AI_Ironwood_Marauder_biped_Animation_Running_withSkin");
            AnimationClip walkClip = FindClip("Meshy_AI_Ironwood_Marauder_biped_Animation_Walking_withSkin");
            AnimationClip attackClip = FindClip("Meshy_AI_Ironwood_Marauder_biped_Animation_Axe_Spin_Attack_withSkin");
            AnimationClip idleClip = walkClip; // Fallback to walk frame 0

            // Fix Rigs
            FixRig("Assets/Imported/GOOn2/Meshy_AI_Ironwood_Marauder_biped_Animation_Running_withSkin.fbx", true);
            FixRig("Assets/Imported/GOOn2/Meshy_AI_Ironwood_Marauder_biped_Animation_Walking_withSkin.fbx", true);
            FixRig("Assets/Imported/GOOn2/Meshy_AI_Ironwood_Marauder_biped_Animation_Axe_Spin_Attack_withSkin.fbx", false);
            FixRig("Assets/Imported/GOOn2/Meshy_AI_Ironwood_Marauder_biped_Character_output.fbx", false);
            
            // Setup States
            AnimatorState idleState = rootStateMachine.AddState("Idle");
            idleState.motion = idleClip;
            idleState.speed = 0f;

            AnimatorState runState = rootStateMachine.AddState("Running");
            runState.motion = runClip;

            AnimatorState attackState = rootStateMachine.AddState("Attack");
            attackState.motion = attackClip;

            // Transitions
            var idleToRun = idleState.AddTransition(runState);
            idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToRun.hasExitTime = false;
            idleToRun.duration = 0.25f;

            var runToIdle = runState.AddTransition(idleState);
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            runToIdle.hasExitTime = false;
            runToIdle.duration = 0.25f;

            var anyToAttack = rootStateMachine.AddAnyStateTransition(attackState);
            anyToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            anyToAttack.hasExitTime = false;

            var attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 1.0f;
            attackToIdle.duration = 0.25f;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();

            // Link to enemies in scene
            var quest = Object.FindFirstObjectByType<ForestQuestController>(FindObjectsInactive.Include);
            if (quest != null && quest.protectors != null)
            {
                Avatar avatar = AssetDatabase.LoadAllAssetsAtPath("Assets/Imported/GOOn2/Meshy_AI_Ironwood_Marauder_biped_Character_output.fbx").OfType<Avatar>().FirstOrDefault();

                foreach (var protector in quest.protectors)
                {
                    if (protector != null)
                    {
                        Animator anim = protector.GetComponentInChildren<Animator>(true);
                        if (anim != null)
                        {
                            anim.runtimeAnimatorController = controller;
                            // Avatar logic removed to preserve original mesh avatars
                            EditorUtility.SetDirty(anim.gameObject);
                        }
                    }
                }
                
                Debug.Log("Goon Animator Fixed and assigned to all Forest Foes!");
            }
        }

        private static AnimationClip FindClip(string fbxName)
        {
            string path = $"Assets/Imported/GOOn2/{fbxName}.fbx";
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            return assets.OfType<AnimationClip>().FirstOrDefault(c => !c.name.Contains("__preview__"));
        }

        private static void FixRig(string path, bool loop)
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

            if (loop)
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
