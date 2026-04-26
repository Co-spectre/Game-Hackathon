using UnityEngine;

namespace NordicWilds.Environment
{
    // Sets sane runtime defaults so the game doesn't ship at editor defaults: uncapped vsync,
    // higher target framerate, lifted physics tickrate, and a cheap shadow distance cap that
    // costs nothing visually inside our small playable arenas.
    [DefaultExecutionOrder(-100)]
    public class PerformanceManager : MonoBehaviour
    {
        public int targetFrameRate = 144;
        public bool vsync = false;
        public float fixedDeltaTime = 1f / 60f;
        public float shadowDistance = 55f;

        void Awake()
        {
            QualitySettings.vSyncCount = vsync ? 1 : 0;
            Application.targetFrameRate = targetFrameRate;
            Time.fixedDeltaTime = fixedDeltaTime;

            // Shadows — tightest acceptable for our small arenas.
            QualitySettings.shadowDistance     = shadowDistance;
            QualitySettings.shadowCascades     = 1;          // single cascade — much cheaper
            QualitySettings.shadowResolution   = ShadowResolution.Medium;
            QualitySettings.shadows            = ShadowQuality.HardOnly;
            QualitySettings.shadowProjection   = ShadowProjection.StableFit;

            // Pixel light cap — fog hides distant fills anyway.
            QualitySettings.pixelLightCount    = 1;
            QualitySettings.globalTextureMipmapLimit = 0;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.antiAliasing       = 0;          // rely on temporal-free framerate

            // LOD aggressive bias so distant meshes drop fast.
            QualitySettings.lodBias            = 1.2f;
            QualitySettings.maximumLODLevel    = 0;

            // Particles.
            QualitySettings.softParticles         = false;
            QualitySettings.particleRaycastBudget = 32;

            // Skinned animation runs on GPU when available.
            QualitySettings.skinWeights = SkinWeights.TwoBones;

            // Physics — fewer iterations is fine for our gameplay scale.
            UnityEngine.Physics.defaultSolverIterations         = 4;
            UnityEngine.Physics.defaultSolverVelocityIterations = 1;
            UnityEngine.Physics.sleepThreshold                  = 0.01f;
        }
    }
}
