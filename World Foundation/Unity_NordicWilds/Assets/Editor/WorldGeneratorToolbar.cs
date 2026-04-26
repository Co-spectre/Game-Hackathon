using UnityEngine;
using UnityEditor;

namespace NordicWilds.EditorTools
{
    /// <summary>
    /// Registers the "World Generation" menu item in Unity's top menu bar.
    /// Use: World Generation > Generate World
    /// </summary>
    public static class WorldGeneratorToolbar
    {
        [MenuItem("World Generation/Generate World")]
        public static void GenerateWorld()
        {
            // Find the NordicWorldBuilder in the scene and call its generation method
            var builder = GameObject.FindFirstObjectByType<MonoBehaviour>();
            if (builder == null)
            {
                Debug.LogWarning("WorldGeneratorToolbar: No scene objects found. Open a scene first.");
                return;
            }

            // Trigger via the NordicWorldBuilder editor if it exists
            var builderType = System.Type.GetType("NordicWilds.NordicWorldBuilder, Assembly-CSharp");
            if (builderType != null)
            {
                var instance = GameObject.FindFirstObjectByType(builderType) as MonoBehaviour;
                if (instance != null)
                {
                    var method = builderType.GetMethod("GenerateWorld");
                    if (method != null)
                    {
                        method.Invoke(instance, null);
                        Debug.Log("✅ World generation triggered via WorldGeneratorToolbar.");
                        
                        // Also automatically set up the player visual!
                        PlayerVisualSetup.SetupPlayerVisual();
                        
                        // Automatically save the scene so the generated world and player don't disappear when Play Mode is stopped!
                        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                        return;
                    }
                }
            }

            Debug.Log("✅ World Generation menu is active! Add a NordicWorldBuilder component to a GameObject in the scene, then use this menu to generate.");
        }

        [MenuItem("World Generation/Clear World")]
        public static void ClearWorld()
        {
            if (EditorUtility.DisplayDialog("Clear World", "This will destroy all generated world objects. Continue?", "Yes", "Cancel"))
            {
                var generated = GameObject.Find("GeneratedWorld");
                if (generated != null)
                {
                    GameObject.DestroyImmediate(generated);
                    Debug.Log("✅ Generated world cleared.");
                }
                else
                {
                    Debug.Log("No 'GeneratedWorld' object found in scene.");
                }
            }
        }

        [MenuItem("World Generation/About")]
        public static void About()
        {
            EditorUtility.DisplayDialog(
                "Nordic Wilds - World Generator",
                "World Generator Toolbar v2\n\nUse 'Generate World' to procedurally build the Nordic world.\nUse 'Clear World' to remove generated objects.\n\nEnsure a NordicWorldBuilder component exists in your scene.",
                "OK"
            );
        }
    }
}
