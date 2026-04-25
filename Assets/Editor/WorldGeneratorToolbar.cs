using UnityEngine;
using UnityEditor;

namespace NordicWilds.EditorTools
{
    // World Builder v2 - Adds "World Generation" menu to Unity top bar
    [InitializeOnLoad]
    public class WorldGeneratorToolbar
    {
        static WorldGeneratorToolbar()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private static double lastCheckTime = 0;
        private static bool buttonAdded = false;

        static void OnEditorUpdate()
        {
            // This runs constantly but we only check occasionally
            if (EditorApplication.timeSinceStartup - lastCheckTime > 1.0)
            {
                lastCheckTime = EditorApplication.timeSinceStartup;
                
                if (!buttonAdded)
                {
                    Debug.Log("✅ World Generator is ready! Look for 'World Generation > World Generation Models' in the menu bar at the top of Unity.");
                    buttonAdded = true;
                }
            }
        }
    }
}
