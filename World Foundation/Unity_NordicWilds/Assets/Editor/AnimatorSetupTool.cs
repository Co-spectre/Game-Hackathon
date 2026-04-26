using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

public class AnimatorSetupTool
{
    [InitializeOnLoadMethod]
    static void AutoRunOnCompile()
    {
        Debug.Log("[AnimatorSetupTool] Auto-running Animator setup to force loop time fixes...");
        CreateAndAssignAnimator();
    }

    [MenuItem("World Foundation/Create And Assign Player Animator")]
    public static void CreateAndAssignAnimator()
    {
        FixCharacterRig();
        string controllerPath = "Assets/PlayerAnimController.controller";
        
        // 1. Create the Animator Controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        
        // 2. Add Parameters
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsSprinting", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsDashing", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("HeavyAttack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("IsDead", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);

        AnimationClip idleClip = FindClip("Meshy_AI_kairon_main_guy_biped_Animation_Axe_Stance_withSkin", false);
        AnimationClip walkClip = FindClip("Meshy_AI_kairon_main_guy_biped_Animation_Walking_withSkin", true);
        AnimationClip runClip = FindClip("Meshy_AI_kairon_main_guy_biped_Animation_Running_withSkin", true);
        AnimationClip attackClip = FindClip("Meshy_AI_kairon_main_guy_biped_Animation_Attack_withSkin", false);
        AnimationClip heavyAttackClip = FindClip("Meshy_AI_kairon_main_guy_biped_Animation_Axe_Spin_Attack_withSkin", false);

        if (idleClip == null || walkClip == null || runClip == null || attackClip == null || heavyAttackClip == null)
        {
            Debug.LogError("[AnimatorSetupTool] Could not find the animation clips! Make sure the FBX files are in Assets/Imported/");
            return;
        }

        // 4. Create States
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        AnimatorState idleState = rootStateMachine.AddState("Idle");
        idleState.motion = idleClip;

        AnimatorState walkState = rootStateMachine.AddState("Walk");
        walkState.motion = walkClip;

        AnimatorState sprintState = rootStateMachine.AddState("Sprint");
        sprintState.motion = runClip;

        AnimatorState attackState = rootStateMachine.AddState("Attack");
        attackState.motion = attackClip;

        AnimatorState heavyAttackState = rootStateMachine.AddState("HeavyAttack");
        heavyAttackState.motion = heavyAttackClip;

        // Set default state
        rootStateMachine.defaultState = idleState;

        // 5. Create Transitions
        
        // Idle -> Walk
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.hasExitTime = false;
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsSprinting");

        // Walk -> Idle
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.hasExitTime = false;
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        // Walk -> Sprint
        var walkToSprint = walkState.AddTransition(sprintState);
        walkToSprint.hasExitTime = false;
        walkToSprint.AddCondition(AnimatorConditionMode.If, 0f, "IsSprinting");

        // Sprint -> Walk
        var sprintToWalk = sprintState.AddTransition(walkState);
        sprintToWalk.hasExitTime = false;
        sprintToWalk.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsSprinting");
        sprintToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        // Any State -> Attack
        var anyToAttack = rootStateMachine.AddAnyStateTransition(attackState);
        anyToAttack.hasExitTime = false;
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

        // Attack -> Idle (Exit Time)
        var attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 0.9f;

        // Any State -> HeavyAttack
        var anyToHeavyAttack = rootStateMachine.AddAnyStateTransition(heavyAttackState);
        anyToHeavyAttack.hasExitTime = false;
        anyToHeavyAttack.AddCondition(AnimatorConditionMode.If, 0f, "HeavyAttack");

        // HeavyAttack -> Idle (Exit Time)
        var heavyAttackToIdle = heavyAttackState.AddTransition(idleState);
        heavyAttackToIdle.hasExitTime = true;
        heavyAttackToIdle.exitTime = 0.9f;

        // 6. Assign to PlayerVisual
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Animator anim = player.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.runtimeAnimatorController = controller;
                anim.applyRootMotion = false;
                Debug.Log("[AnimatorSetupTool] Successfully assigned Animator Controller to Player.");
            }
            else
            {
                Debug.LogError("[AnimatorSetupTool] Player found, but no Animator found on any child object.");
            }
        }
        else
        {
            Debug.LogError("[AnimatorSetupTool] Player object not found in the scene! Open the scene, place the player, and click again.");
        }

        Debug.Log("[AnimatorSetupTool] Setup complete! Controller saved to Assets/PlayerAnimController.controller");
        AssetDatabase.SaveAssets();
    }

    private static AnimationClip FindClip(string fbxName, bool setLoopTime)
    {
        string[] guids = AssetDatabase.FindAssets(fbxName + " t:Model");
        if (guids.Length == 0) return null;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;

        if (importer != null)
        {
            bool needsReimport = false;

            // Force Humanoid Rig
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                needsReimport = true;
            }

            // Force Avatar linkage
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                Animator playerAnim = player.GetComponentInChildren<Animator>();
                if (playerAnim != null && playerAnim.avatar != null)
                {
                    if (importer.avatarSetup != ModelImporterAvatarSetup.CopyFromOther || importer.sourceAvatar != playerAnim.avatar)
                    {
                        importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                        importer.sourceAvatar = playerAnim.avatar;
                        needsReimport = true;
                    }
                }
            }

            if (setLoopTime)
            {
                ModelImporterClipAnimation[] clips = importer.clipAnimations;
                if (clips == null || clips.Length == 0) clips = importer.defaultClipAnimations;
                
                foreach (var clip in clips)
                {
                    if (!clip.loopTime)
                    {
                        clip.loopTime = true;
                        needsReimport = true;
                    }
                }
                if (needsReimport) importer.clipAnimations = clips;
            }

            if (needsReimport)
            {
                importer.SaveAndReimport();
                Debug.Log($"[AnimatorSetupTool] Fixed Rig/Avatar/Loop for {fbxName}");
            }
        }

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (Object asset in assets)
        {
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
            {
                return clip;
            }
        }

        return null;
    }
    private static void FixCharacterRig()
    {
        string fbxName = "Meshy_AI_kairon_main_guy_biped_Character_output";
        string[] guids = AssetDatabase.FindAssets(fbxName + " t:Model");
        if (guids.Length == 0) return;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer != null && importer.animationType != ModelImporterAnimationType.Human)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            importer.SaveAndReimport();
            Debug.Log("[AnimatorSetupTool] Fixed main character rig to Humanoid.");
        }
    }
}
