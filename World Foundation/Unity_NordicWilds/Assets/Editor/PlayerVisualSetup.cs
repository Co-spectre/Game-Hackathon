using UnityEngine;
using UnityEditor;
using System.IO;

namespace NordicWilds.EditorTools
{
    /// <summary>
    /// Editor utility to wire up the Meshy AI character model onto the Player GameObject.
    /// Access via: World Generation > Setup Player Visual
    /// </summary>
    public static class PlayerVisualSetup
    {
        private const string FBX_PATH    = "Assets/Imported/Meshy_AI_Character_output.fbx";
        private const string PREFAB_PATH = "Assets/Prefabs/Player.prefab";

        [MenuItem("World Generation/Setup Player Visual")]
        public static void SetupPlayerVisual()
        {
            // ── Step 1: Verify the FBX exists ────────────────────────────────────
            var meshyFBX = AssetDatabase.LoadAssetAtPath<GameObject>(FBX_PATH);
            if (meshyFBX == null)
            {
                EditorUtility.DisplayDialog(
                    "Setup Player Visual",
                    $"Could not find the Meshy FBX at:\n{FBX_PATH}\n\n" +
                    "Make sure Unity has imported it (it may need a second to refresh).",
                    "OK");
                return;
            }

            // ── Step 2: Find the Player in scene ─────────────────────────────────
            var playerGO = GameObject.FindWithTag("Player");
            if (playerGO == null)
            {
                // Try finding by component
                var pc = Object.FindFirstObjectByType<NordicWilds.Player.PlayerController>();
                if (pc != null) playerGO = pc.gameObject;
            }

            if (playerGO == null)
            {
                bool create = EditorUtility.DisplayDialog(
                    "Setup Player Visual",
                    "No Player object found in the scene (needs 'Player' tag or a PlayerController component).\n\n" +
                    "Would you like to create a new Player GameObject?",
                    "Create Player", "Cancel");

                if (!create) return;
                playerGO = CreatePlayerGameObject();
            }

            // ── Step 3: Instantiate VisualRoot and FBX as a static child ─────────
            Transform visualRoot = playerGO.transform.Find("VisualRoot");
            if (visualRoot == null)
            {
                GameObject rootGO = new GameObject("VisualRoot");
                visualRoot = rootGO.transform;
                visualRoot.SetParent(playerGO.transform, false);
                visualRoot.localPosition = Vector3.zero;
                visualRoot.localRotation = Quaternion.identity;
                visualRoot.localScale = Vector3.one;
            }

            Transform existingVisual = visualRoot.Find("MeshyCharacter");
            GameObject newVisual = null;

            if (existingVisual == null)
            {
                newVisual = (GameObject)PrefabUtility.InstantiatePrefab(meshyFBX);
                newVisual.name = "MeshyCharacter";
                newVisual.transform.SetParent(visualRoot, false);
                newVisual.transform.localPosition = new Vector3(0f, -0.9f, 0f);
                newVisual.transform.localRotation = Quaternion.identity;
                newVisual.transform.localScale = Vector3.one;
            }
            else
            {
                newVisual = existingVisual.gameObject;
            }

            // ── Step 3.5: Add / update PlayerVisualLinker and PlayerAnimationDriver ───────────────────────────
            var linker = playerGO.GetComponent<NordicWilds.Player.PlayerVisualLinker>();
            if (linker == null)
                linker = playerGO.AddComponent<NordicWilds.Player.PlayerVisualLinker>();

            var animDriver = playerGO.GetComponent<NordicWilds.Player.PlayerAnimationDriver>();
            if (animDriver == null)
                animDriver = playerGO.AddComponent<NordicWilds.Player.PlayerAnimationDriver>();

            var scaleDebugger = playerGO.GetComponent<NordicWilds.Player.PlayerScaleDebugger>();
            if (scaleDebugger == null)
                scaleDebugger = playerGO.AddComponent<NordicWilds.Player.PlayerScaleDebugger>();

            // Set the visual instance via SerializedObject so it shows in Undo history
            var so = new SerializedObject(linker);
            var visualProp = so.FindProperty("visualInstance");
            if (visualProp != null) visualProp.objectReferenceValue = newVisual;
            so.ApplyModifiedProperties();
            
            var soAnim = new SerializedObject(animDriver);
            var animProp = soAnim.FindProperty("animator");
            var animatorComp = newVisual.GetComponentInChildren<Animator>();
            if (animProp != null && animatorComp != null) animProp.objectReferenceValue = animatorComp;
            soAnim.ApplyModifiedProperties();

            // Make absolutely sure the Animator has the generated controller attached
            var runtimeController = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>("Assets/PlayerAnimController.controller");
            if (animatorComp != null && runtimeController != null)
            {
                animatorComp.runtimeAnimatorController = runtimeController;
                animatorComp.applyRootMotion = false;
            }

            // ── Step 4: Remove any old capsule/mesh renderer on the root ─────────
            // (keep Collider + Rigidbody, but disable any old MeshRenderer so only
            //  the Meshy model renders)
            var rootMR = playerGO.GetComponent<MeshRenderer>();
            if (rootMR != null) rootMR.enabled = false;
            
            var legacyVisual = playerGO.transform.Find("RuntimeVisualModel");
            if (legacyVisual != null) legacyVisual.gameObject.SetActive(false);

            var rootMF = playerGO.GetComponent<MeshFilter>();

            // ── Step 5: Mark dirty & save scene ──────────────────────────────────
            EditorUtility.SetDirty(playerGO);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                playerGO.scene);

            // ── Step 6: Save as prefab ────────────────────────────────────────────
            string dir = Path.GetDirectoryName(PREFAB_PATH);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            bool prefabSuccess;
            PrefabUtility.SaveAsPrefabAssetAndConnect(playerGO, PREFAB_PATH,
                InteractionMode.UserAction, out prefabSuccess);

            if (prefabSuccess)
                Debug.Log($"[PlayerVisualSetup] ✅ Player prefab saved to {PREFAB_PATH}");

            EditorUtility.DisplayDialog(
                "Setup Player Visual ✅",
                $"Done!\n\n" +
                $"• Meshy model is now a direct child of '{playerGO.name}'\n" +
                $"• Prefab saved to: {PREFAB_PATH}\n\n" +
                "You can now see the model in the editor without hitting Play! " +
                "You can adjust its transform using standard Unity tools.",
                "OK");
        }

        private static GameObject CreatePlayerGameObject()
        {
            var go = new GameObject("Player");
            go.tag = "Player";

            // Physics
            var rb            = go.AddComponent<Rigidbody>();
            rb.interpolation  = RigidbodyInterpolation.Interpolate;
            rb.constraints    = RigidbodyConstraints.FreezeRotation;

            // Capsule collider sized for a humanoid
            var col           = go.AddComponent<CapsuleCollider>();
            col.height        = 1.8f;
            col.radius        = 0.35f;
            col.center        = new Vector3(0f, 0.9f, 0f);

            // Player scripts
            go.AddComponent<NordicWilds.Player.PlayerController>();

            go.transform.position = Vector3.zero;

            Undo.RegisterCreatedObjectUndo(go, "Create Player");
            return go;
        }
    }
}
