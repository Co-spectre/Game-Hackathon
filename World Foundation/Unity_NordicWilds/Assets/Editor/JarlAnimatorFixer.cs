using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using NordicWilds.World;
using System.Linq;

namespace NordicWilds.EditorTools
{
    public class JarlAnimatorFixer
    {
        [MenuItem("World Foundation/Fix Ashen Jarl Animator")]
        public static void FixJarl()
        {
            string charPath = "Assets/Imported/Meshy_AI_Berserker_of_the_Nort_biped_Character_output.fbx";
            string attackPath = "Assets/Imported/Meshy_AI_Berserker_of_the_Nort_biped_Animation_Attack_withSkin.fbx";
            string walkPath = "Assets/Imported/Meshy_AI_Berserker_of_the_Nort_biped_Animation_Walk_Fight_Forward_withSkin.fbx";
            string controllerPath = "Assets/Imported/jarl.controller";

            // 1. Force Humanoid on all FBXs
            ForceHumanoid(charPath);
            ForceHumanoid(attackPath);
            ForceHumanoid(walkPath);

            // 2. Create/Update Controller
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(charPath); // Default stance
            AnimationClip walkClip = AssetDatabase.LoadAllAssetsAtPath(walkPath).OfType<AnimationClip>().FirstOrDefault(c => !c.name.Contains("__preview__"));
            AnimationClip attackClip = AssetDatabase.LoadAllAssetsAtPath(attackPath).OfType<AnimationClip>().FirstOrDefault(c => !c.name.Contains("__preview__"));

            if (walkClip != null) SetLoopTime(walkPath, true);

            var rootLayer = controller.layers[0];
            var stateMachine = rootLayer.stateMachine;

            var idleState = stateMachine.AddState("Idle");
            idleState.motion = idleClip;

            var runState = stateMachine.AddState("Run");
            runState.motion = walkClip;

            var attackState = stateMachine.AddState("Attack");
            attackState.motion = attackClip;

            // Transitions
            var idleToRun = idleState.AddTransition(runState);
            idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToRun.duration = 0.25f;

            var runToIdle = runState.AddTransition(idleState);
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            runToIdle.duration = 0.25f;

            var anyToAttack = stateMachine.AddAnyStateTransition(attackState);
            anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "Attack");
            anyToAttack.duration = 0.1f;

            var attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 0.85f;
            attackToIdle.duration = 0.25f;

            // 3. Find and Assign in Scene
            var quest = Object.FindFirstObjectByType<ForestQuestController>(FindObjectsInactive.Include);
            if (quest != null && quest.finalBoss != null)
            {
                GameObject jarl = quest.finalBoss.gameObject;
                // Find model child
                Transform model = jarl.transform.Find("Meshy_AI_Berserker_of_the_Nort_biped_Character_output");
                if (model == null) model = jarl.transform.Cast<Transform>().FirstOrDefault(t => t.name.Contains("Berserker"));
                if (model == null) model = jarl.transform;

                Animator anim = model.GetComponent<Animator>();
                if (anim == null) anim = model.gameObject.AddComponent<Animator>();
                
                anim.runtimeAnimatorController = controller;
                anim.applyRootMotion = false; // Prevent animations from blocking physics movement
                
                // Assign Avatar
                Avatar av = AssetDatabase.LoadAllAssetsAtPath(charPath).OfType<Avatar>().FirstOrDefault();
                if (av != null) anim.avatar = av;

                Rigidbody rb = jarl.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.linearDamping = 1f; // Add some drag so he doesn't slide forever
                    rb.angularDamping = 5f;
                    rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                }

                EditorUtility.SetDirty(jarl);
                EditorUtility.SetDirty(anim.gameObject);
                Debug.Log("Ashen Jarl Animator and Physics Fixed!");
            }
        }

        static void ForceHumanoid(string path)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null && importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.SaveAndReimport();
            }
        }

        static void SetLoopTime(string path, bool loop)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                var settings = importer.clipAnimations;
                if (settings == null || settings.Length == 0)
                {
                    // If no explicit clip animations, we can't easily set loop time via script without defining one
                    // But usually for mixamo/fbx it's in the default take
                    ModelImporterClipAnimation[] animations = importer.defaultClipAnimations;
                    foreach (var anim in animations)
                    {
                        anim.loopTime = loop;
                    }
                    importer.clipAnimations = animations;
                }
                else
                {
                    foreach (var anim in settings)
                    {
                        anim.loopTime = loop;
                    }
                    importer.clipAnimations = settings;
                }
                importer.SaveAndReimport();
            }
        }
    }
}
