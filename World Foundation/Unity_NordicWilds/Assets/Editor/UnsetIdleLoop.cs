using UnityEngine;
using UnityEditor;

public class UnsetIdleLoop
{
    [InitializeOnLoadMethod]
    public static void Unfix()
    {
        string fbxName = "Meshy_AI_kairon_main_guy_biped_Animation_Axe_Stance_withSkin";
        string[] guids = AssetDatabase.FindAssets(fbxName + " t:Model");
        if (guids.Length == 0) return;

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer != null)
        {
            ModelImporterClipAnimation[] clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0) clips = importer.defaultClipAnimations;

            bool modified = false;
            foreach (var clip in clips)
            {
                if (clip.loopTime)
                {
                    clip.loopTime = false;
                    modified = true;
                }
            }

            if (modified)
            {
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
                Debug.Log("[UnsetIdleLoop] Disabled Loop Time for Stance.");
            }
        }
    }
}
