using UnityEngine;
using UnityEditor;
using NordicWilds.Player;
using NordicWilds.Combat;
using NordicWilds.World;
using NordicWilds.CameraSystems;
using NordicWilds.Environment;
using System.Collections.Generic;

namespace NordicWilds.EditorTools
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  NORDIC WORLD BUILDER  |  Dual-Realm Edition
    //  ┌─────────────────────────────────────────────────────────────────────┐
    //  │  REALM 1 — FROSTHEIM   (Norse Viking)   coords: origin  (0,0,0)    │
    //  │  REALM 2 — YAMATO      (Feudal Japan)   coords: offset (10000,0,10000)│
    //  │                                                                     │
    //  │  Props added vs. original:                                          │
    //  │   FROSTHEIM: standing-stone circle, burial mounds, blacksmith       │
    //  │              forge, stone well, Viking longboat, wolves,            │
    //  │              berserkers, aurora borealis, frozen lake, more huts    │
    //  │   YAMATO   : bamboo groves, koi pond, zen garden, stone lanterns,  │
    //  │              procession of Torii gates, main shrine, bell tower,   │
    //  │              tea house, bamboo fence perimeter, deer & cranes,      │
    //  │              samurai & ninja enemies, cherry-blossom weather,       │
    //  │              misty mountain backdrop                                │
    //  └─────────────────────────────────────────────────────────────────────┘
    // ═══════════════════════════════════════════════════════════════════════════

    public class NordicWorldBuilder : EditorWindow
    {
        [MenuItem("World Foundation/Generate Masterpiece Environment")]
        [MenuItem("Nordic Wilds/Generate Masterpiece Environment")]
        public static void BuildMasterpieceWorld()
        {
            Debug.Log("══════════════════════════════════════════════════════════");
            Debug.Log("  NORDIC WORLD BUILDER — Dual-Realm Edition");
            Debug.Log("  Building FROSTHEIM (Norse) ←→ YAMATO (Feudal Japan)");
            Debug.Log("══════════════════════════════════════════════════════════");

            // ── 1. Tags ────────────────────────────────────────────────────────
            EnsureTagExists("Player");
            EnsureTagExists("Campfire");
            EnsureTagExists("Enemy");
            EnsureTagExists("Portal");
            EnsureTagExists("Interactable");

            // ── 2. Clear old scene ────────────────────────────────────────────
            DestroyImmediate(GameObject.Find("Nordic World Root"));
            DestroyImmediate(GameObject.Find("Player"));
            DestroyImmediate(GameObject.Find("Isometric Camera Rig"));

            GameObject worldRoot = new GameObject("Nordic World Root");

            // ── 3. FROSTHEIM Materials ────────────────────────────────────────
            Material snowMat      = MakeMat(GenNoise(new Color(0.85f,0.90f,0.95f), new Color(0.70f,0.82f,0.90f), 12f), 0.08f);
            Material stoneMat     = MakeMat(GenNoise(new Color(0.32f,0.36f,0.42f), new Color(0.18f,0.22f,0.28f), 28f), 0.30f);
            Material darkStoneMat = MakeMat(GenNoise(new Color(0.18f,0.20f,0.24f), new Color(0.10f,0.12f,0.16f), 25f), 0.25f);
            Material pineMat      = MakeMat(GenNoise(new Color(0.12f,0.32f,0.18f), new Color(0.08f,0.22f,0.12f), 10f), 0.05f);
            Material woodMat      = MakeMat(GenNoise(new Color(0.25f,0.15f,0.05f), new Color(0.15f,0.10f,0.03f), 45f), 0.20f);
            Material iceMat       = MakeMat(GenNoise(new Color(0.75f,0.88f,0.98f), new Color(0.60f,0.80f,0.95f), 18f), 0.85f);
            Material pathMat      = MakeMat(GenNoise(new Color(0.55f,0.58f,0.62f), new Color(0.42f,0.45f,0.48f), 30f), 0.10f);
            Material metalMat     = MakeMat(null, 0.75f); metalMat.color  = new Color(0.65f,0.70f,0.75f);
            Material coalMat      = MakeMat(null, 0.05f); coalMat.color   = new Color(0.08f,0.08f,0.10f);
            Material bannerRedMat = MakeMat(null, 0.15f); bannerRedMat.color = new Color(0.72f,0.10f,0.10f);

            // ── 4. FROSTHEIM Lighting ─────────────────────────────────────────
#if UNITY_2023_1_OR_NEWER
            Light dirLight = Object.FindFirstObjectByType<Light>();
#else
            Light dirLight = Object.FindObjectOfType<Light>();
#endif
            if (dirLight != null && dirLight.type == LightType.Directional)
            {
                dirLight.color          = new Color(0.55f, 0.70f, 1.00f);
                dirLight.intensity      = 1.05f;
                dirLight.transform.rotation = Quaternion.Euler(32f, -55f, 0f);
                dirLight.shadows        = LightShadows.Soft;
                dirLight.shadowStrength = 0.80f;
            }

            RenderSettings.fog               = true;
            RenderSettings.fogColor          = new Color(0.58f, 0.72f, 0.88f);
            RenderSettings.fogMode           = FogMode.ExponentialSquared;
            RenderSettings.fogDensity        = 0.012f;
            RenderSettings.ambientMode       = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor   = new Color(0.38f, 0.48f, 0.68f);
            RenderSettings.ambientEquatorColor = new Color(0.28f, 0.38f, 0.55f);
            RenderSettings.ambientGroundColor  = new Color(0.18f, 0.28f, 0.45f);

            // ── 5. FROSTHEIM Terrain ──────────────────────────────────────────
            CreateBlock("Arena Floor",  new Vector3(  0, -0.5f,   0), new Vector3(400,  1, 400), snowMat,  worldRoot.transform);
            CreateBlock("North Cliff",  new Vector3(  0,  1.5f,  95), new Vector3(400,  3, 210), snowMat,  worldRoot.transform);
            CreateBlock("West Cliff",   new Vector3(-200, 5f,    0),  new Vector3( 10, 25, 400), stoneMat, worldRoot.transform);
            CreateBlock("East Cliff",   new Vector3( 200, 5f,    0),  new Vector3( 10, 25, 400), stoneMat, worldRoot.transform);
            CreateBlock("South Wall",   new Vector3(  0,  5f, -200), new Vector3(400, 25,  10), stoneMat, worldRoot.transform);
            CreateBlock("North Wall",   new Vector3(  0,  5f,  200), new Vector3(400, 25,  10), stoneMat, worldRoot.transform);

            // Frozen lake — east side of village
            CreateBlock("Frozen Lake",  new Vector3( 55f, -0.45f,  20f), new Vector3(30,  0.10f, 25),  iceMat,   worldRoot.transform);
            CreateBlock("Lake Shore W", new Vector3( 42f, -0.25f,  20f), new Vector3( 4,  0.30f, 25),  stoneMat, worldRoot.transform);
            CreateBlock("Lake Shore E", new Vector3( 68f, -0.25f,  20f), new Vector3( 4,  0.30f, 25),  stoneMat, worldRoot.transform);

            // Invisible play bounds
            GameObject bounds = new GameObject("Invisible Boundaries");
            bounds.transform.SetParent(worldRoot.transform);
            CreateInvisibleWall("N Bound", new Vector3(   0, 5f,  75f), new Vector3(160, 20,  2), bounds.transform);
            CreateInvisibleWall("S Bound", new Vector3(   0, 5f, -75f), new Vector3(160, 20,  2), bounds.transform);
            CreateInvisibleWall("W Bound", new Vector3( -80f, 5f,   0), new Vector3(  2, 20, 160), bounds.transform);
            CreateInvisibleWall("E Bound", new Vector3(  80f, 5f,   0), new Vector3(  2, 20, 160), bounds.transform);

            // Entrance corridor
            CreateBlock("Entrance Path", new Vector3(0, -0.5f, -40), new Vector3(8, 1, 40), snowMat, worldRoot.transform);

            // Grand staircase up to North Altar
            GameObject grandStairs = new GameObject("Grand Stairs");
            grandStairs.transform.SetParent(worldRoot.transform);
            for (int i = 0; i < 12; i++)
                CreateBlock("Step_" + i,
                    new Vector3(0, (i * 0.30f) - 0.5f, 40f + (i * 0.50f)),
                    new Vector3(14f, 0.30f, 0.55f), stoneMat, grandStairs.transform);
            GameObject stairRamp = CreateInvisibleWall("Stair Ramp",
                new Vector3(0, 1.4f, 42.5f), new Vector3(14f, 0.5f, 7f), grandStairs.transform);
            stairRamp.transform.rotation = Quaternion.Euler(-32f, 0, 0);

            // Great Runestone on the North Altar
            GameObject runestone = CreateBlock("Great Runestone",
                new Vector3(0, 4.5f, 52), new Vector3(3, 9, 2.5f), darkStoneMat, worldRoot.transform);
            runestone.transform.rotation = Quaternion.Euler(0, 12, 8);
            AddPointLight(worldRoot.transform, new Vector3(0, 6f, 52),
                new Color(0.4f, 0.6f, 1f), 2.5f, 12f, "Rune Glow");

            // Standing stone circle surrounding the runestone
            CreateStoneCircle(new Vector3(0, 0, 52), 14f, 12, stoneMat, worldRoot.transform);

            // Burial mounds (Viking age barrows)
            CreateBurialMound(new Vector3(-30f, 0, 45f), worldRoot.transform, stoneMat, snowMat);
            CreateBurialMound(new Vector3( 35f, 0, 50f), worldRoot.transform, stoneMat, snowMat);

            // Dirt/snow paths toward the altar
            for (int i = 0; i < 22; i++)
            {
                var p = CreateBlock("Path Node",
                    new Vector3(Random.Range(-2f, 2f), 0.05f, -25 + (i * 4)),
                    new Vector3(6f, 0.1f, 6f), pathMat, worldRoot.transform);
                p.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }

            // ── 6. FROSTHEIM Village ──────────────────────────────────────────
            CreateGreatHall(new Vector3(  0f, 0f, 25f), worldRoot.transform, woodMat, snowMat, pathMat);
            CreateHut(new Vector3(-25f, 0f,  15f), worldRoot.transform, woodMat, snowMat);
            CreateHut(new Vector3( 30f, 0f,   5f), worldRoot.transform, woodMat, snowMat);
            CreateHut(new Vector3(-15f, 0f, -10f), worldRoot.transform, woodMat, snowMat);
            CreateHut(new Vector3( 25f, 0f,  38f), worldRoot.transform, woodMat, snowMat);
            CreateHut(new Vector3(-38f, 0f,  38f), worldRoot.transform, woodMat, snowMat);
            CreateHut(new Vector3( 42f, 0f,  55f), worldRoot.transform, woodMat, snowMat);
            CreateHut(new Vector3(-42f, 0f,  55f), worldRoot.transform, woodMat, snowMat);

            // Blacksmith forge
            CreateForge(new Vector3(18f, 0f, -5f), worldRoot.transform, stoneMat, woodMat, metalMat, coalMat);

            // Stone well in the village centre
            CreateWell(new Vector3(-8f, 0f, 5f), worldRoot.transform, stoneMat, woodMat);

            // Viking longboat docked by the frozen lake
            CreateLongboat(new Vector3(55f, 0.1f, -5f), worldRoot.transform, woodMat, metalMat);

            // Campfire safe zone
            GameObject campfireZone = new GameObject("Campfire Safe Zone");
            campfireZone.tag = "Campfire";
            campfireZone.transform.position = new Vector3(-12f, 0.5f, 8f);
            campfireZone.transform.SetParent(worldRoot.transform);
            CreateBlock("Fire Pit Base", Vector3.zero, new Vector3(2.5f, 0.3f, 2.5f), stoneMat, campfireZone.transform);
            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f * Mathf.Deg2Rad;
                CreateBlock("Ring Stone " + i,
                    new Vector3(Mathf.Sin(a) * 1.5f, 0, Mathf.Cos(a) * 1.5f),
                    new Vector3(0.4f, 0.6f, 0.4f), stoneMat, campfireZone.transform);
            }
            AddPointLight(campfireZone.transform, new Vector3(0, 1.2f, 0),
                new Color(1f, 0.45f, 0f), 6f, 10f, "Fire Glow");
            BoxCollider heatZone = campfireZone.AddComponent<BoxCollider>();
            heatZone.isTrigger = true;
            heatZone.size = new Vector3(8, 5, 8);

            // Village lights & dressing
            CreateTorch(new Vector3(-22f, 2f, 22f), worldRoot.transform);
            CreateTorch(new Vector3( 27f, 2f, 17f), worldRoot.transform);
            CreateTorch(new Vector3(  -8f, 2f, 28f), worldRoot.transform);
            CreateTorch(new Vector3(   8f, 2f, 28f), worldRoot.transform);
            CreateTorch(new Vector3( -28f, 2f, -5f), worldRoot.transform);
            CreateBanner(new Vector3( -5f, 5f, 10f), worldRoot.transform, bannerRedMat, woodMat);
            CreateBanner(new Vector3(  5f, 5f, 10f), worldRoot.transform, bannerRedMat, woodMat);
            CreateBanner(new Vector3(-20f, 5f, 30f), worldRoot.transform, bannerRedMat, woodMat);
            CreateBanner(new Vector3( 20f, 5f, 30f), worldRoot.transform, bannerRedMat, woodMat);

            // ── 7. FROSTHEIM Foliage & Ruins ──────────────────────────────────
            for (int i = 0; i < 700; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-190f, 190f), 0.5f, Random.Range(-190f, 190f));
                if (pos.magnitude > 28f) CreatePineTree(pos, worldRoot.transform, pineMat, woodMat);
            }
            for (int i = 0; i < 220; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-180f, 180f), 1f, Random.Range(-180f, 180f));
                var s = CreateBlock("Ruin Stone", pos,
                    new Vector3(Random.Range(1f, 5f), Random.Range(2f, 9f), Random.Range(1f, 4f)),
                    stoneMat, worldRoot.transform);
                s.transform.rotation = Quaternion.Euler(
                    Random.Range(-18, 18), Random.Range(0, 360), Random.Range(-18, 18));
            }
            for (int i = 0; i < 70; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-150f, 150f), 0.5f, Random.Range(-150f, 150f));
                var l = CreateBlock("Fallen Log", pos,
                    new Vector3(1f, 1f, Random.Range(4f, 9f)), woodMat, worldRoot.transform);
                l.transform.rotation = Quaternion.Euler(
                    Random.Range(-10, 10), Random.Range(0, 360), Random.Range(-10, 10));
            }

            // ── 8. FROSTHEIM → YAMATO Portal ─────────────────────────────────
            CreatePortalGateway(
                "Portal Gateway (To Yamato)",
                new Vector3(6f, 0f, -15f),
                worldRoot.transform,
                stoneMat,
                new Color(1f, 0.50f, 0.80f, 0.60f),  // glow plane colour
                new Color(1f, 0.40f, 0.80f),           // point-light colour
                new Color(1f, 0.70f, 0.80f),           // particle colour
                new Vector3(10000, 2f, 10000),
                isReturn: false);

            // ── 9. FROSTHEIM Weather ──────────────────────────────────────────
            CreateBlizzard(worldRoot.transform);
            CreateAuroraBorealis(worldRoot.transform);

            // ── 10. FROSTHEIM Wildlife ────────────────────────────────────────
            Material squirrelMat = MakeMat(null, 0.20f); squirrelMat.color = new Color(0.40f, 0.25f, 0.10f);
            Material foxMat      = MakeMat(null, 0.10f); foxMat.color      = new Color(0.88f, 0.92f, 0.96f);
            Material wolfMat     = MakeMat(null, 0.20f); wolfMat.color     = new Color(0.22f, 0.22f, 0.26f);

            for (int i = 0; i < 40; i++)
                CreateAnimal("Squirrel", new Vector3(Random.Range(-70f, 70f), 0.25f, Random.Range(-70f, 70f)),
                    new Vector3(0.5f, 0.5f, 0.8f), squirrelMat, 6f, worldRoot.transform);
            for (int i = 0; i < 15; i++)
                CreateAnimal("Snow Fox", new Vector3(Random.Range(-80f, 80f), 0.5f, Random.Range(-80f, 80f)),
                    new Vector3(0.8f, 1f, 1.6f), foxMat, 4f, worldRoot.transform);
            for (int i = 0; i < 8; i++)
                CreateAnimal("Wolf", new Vector3(Random.Range(-90f, 90f), 0.6f, Random.Range(-90f, 90f)),
                    new Vector3(1f, 0.9f, 1.8f), wolfMat, 5f, worldRoot.transform);

            // ── 11. FROSTHEIM Enemies ─────────────────────────────────────────
            Material enemyMat   = MakeMat(null, 0.10f); enemyMat.color   = new Color(0.80f, 0.10f, 0.10f);
            Material berserkMat = MakeMat(null, 0.10f); berserkMat.color = new Color(0.60f, 0.05f, 0.05f);
            Material hitFlashMat = MakeMat(null, 0.00f); hitFlashMat.color = Color.white;

            for (int i = 0; i < 15; i++)
                CreateEnemy("Draugr Thrall",
                    new Vector3(Random.Range(-40f, 40f), 1f, Random.Range(10f, 65f)),
                    enemyMat, hitFlashMat, worldRoot.transform);
            for (int i = 0; i < 5; i++)
                CreateEnemy("Berserker",
                    new Vector3(Random.Range(-35f, 35f), 1f, Random.Range(20f, 65f)),
                    berserkMat, hitFlashMat, worldRoot.transform);

            // ── 12. YAMATO Realm ──────────────────────────────────────────────
            CreateYamatoRealm(worldRoot.transform);

            // ── 13. Player, Camera, Encounter ─────────────────────────────────
            GameObject player = SetupPlayer("Player");
            player.transform.position = new Vector3(0, 1f, -20);

            Camera cam = SetupCamera(player.transform);
            SetupHadesEncounter(worldRoot.transform);

            var pController = player.GetComponent<PlayerController>();
            SerializedObject pSo = new SerializedObject(pController);
            pSo.FindProperty("isometricCameraTransform").objectReferenceValue = cam.transform;
            pSo.ApplyModifiedProperties();

            CreateMinimalUI();

            Debug.Log("═══ BUILD COMPLETE ═══  FROSTHEIM ←→ YAMATO  ═══");
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  YAMATO — FEUDAL JAPAN REALM
        //  All coordinates are offset by +10 000 on X and Z to sit cleanly
        //  in the same Unity scene without any visual bleed-through.
        // ═══════════════════════════════════════════════════════════════════════

        private static void CreateYamatoRealm(Transform worldRoot)
        {
            Vector3 O = new Vector3(10000f, 0f, 10000f); // realm origin offset

            // ── Yamato Materials ──────────────────────────────────────────────
            Material grassMat  = MakeMat(GenNoise(new Color(0.38f,0.62f,0.28f), new Color(0.28f,0.52f,0.20f), 18f), 0.05f);
            Material stoneMat  = MakeMat(GenNoise(new Color(0.55f,0.55f,0.58f), new Color(0.38f,0.38f,0.42f), 26f), 0.30f);
            Material woodMat   = MakeMat(null, 0.20f); woodMat.color   = new Color(0.18f, 0.10f, 0.04f);
            Material redMat    = MakeMat(null, 0.18f); redMat.color    = new Color(0.82f, 0.12f, 0.12f);
            Material sakuraMat = MakeMat(null, 0.10f); sakuraMat.color = new Color(1f, 0.72f, 0.85f);
            Material pathMat   = MakeMat(GenNoise(new Color(0.62f,0.58f,0.50f), new Color(0.50f,0.46f,0.38f), 28f), 0.15f);
            Material paperMat  = MakeMat(null, 0.05f); paperMat.color  = new Color(0.98f, 0.95f, 0.85f);
            Material bambooMat = MakeMat(null, 0.15f); bambooMat.color = new Color(0.42f, 0.68f, 0.22f);
            Material sandMat   = MakeMat(GenNoise(new Color(0.82f,0.78f,0.65f), new Color(0.70f,0.66f,0.55f), 14f), 0.05f);
            Material waterMat  = MakeMat(null, 0.80f); waterMat.color  = new Color(0.25f, 0.55f, 0.75f, 0.8f);
            Material goldMat   = MakeMat(null, 0.85f); goldMat.color   = new Color(1f, 0.82f, 0.25f);

            // ── Yamato Terrain ────────────────────────────────────────────────
            CreateBlock("Yamato Ground", O + new Vector3(0, -0.5f, 0), new Vector3(220, 1, 220), grassMat, worldRoot);

            // Gently raised shrine hill to the north
            CreateBlock("Shrine Hill", O + new Vector3(0, 1.5f, 75f), new Vector3(50, 3, 60), grassMat, worldRoot);

            // Distant misty mountains (visual backdrop, outside play area)
            for (int i = 0; i < 6; i++)
            {
                float mx = -120f + i * 50f;
                var mt = CreateBlock("Mountain " + i,
                    O + new Vector3(mx, 12f, 108f),
                    new Vector3(32, 30, 28), stoneMat, worldRoot);
                mt.transform.rotation = Quaternion.Euler(0, Random.Range(-12, 12), 0);
            }

            // Realm borders
            CreateBlock("Y West Cliff",  O + new Vector3(-110f,  5f,   0), new Vector3( 10, 20, 220), stoneMat, worldRoot);
            CreateBlock("Y East Cliff",  O + new Vector3( 110f,  5f,   0), new Vector3( 10, 20, 220), stoneMat, worldRoot);
            CreateBlock("Y South Wall",  O + new Vector3(   0f,  5f,-110f), new Vector3(220, 20,  10), stoneMat, worldRoot);
            CreateBlock("Y North Wall",  O + new Vector3(   0f,  5f, 110f), new Vector3(220, 20,  10), stoneMat, worldRoot);

            // Invisible play bounds
            GameObject yBounds = new GameObject("Yamato Boundaries");
            yBounds.transform.SetParent(worldRoot);
            CreateInvisibleWall("YN", O + new Vector3(   0, 5f,  82f), new Vector3(180, 20,  2), yBounds.transform);
            CreateInvisibleWall("YS", O + new Vector3(   0, 5f, -82f), new Vector3(180, 20,  2), yBounds.transform);
            CreateInvisibleWall("YW", O + new Vector3(-92f, 5f,    0), new Vector3(  2, 20, 180), yBounds.transform);
            CreateInvisibleWall("YE", O + new Vector3( 92f, 5f,    0), new Vector3(  2, 20, 180), yBounds.transform);

            // ── Torii Gate Procession (Fushimi Inari-style) ───────────────────
            for (int i = 0; i < 8; i++)
                CreateToriiGate("Torii " + i, O + new Vector3(0, 0, (i * 8.5f) - 10f), redMat, worldRoot);

            // ── Stone-paved main path ──────────────────────────────────────────
            for (int i = 0; i < 20; i++)
            {
                var slab = CreateBlock("Stone Slab",
                    O + new Vector3(0, 0.06f, (i * 4.5f) - 5f),
                    new Vector3(4.5f, 0.12f, 3.8f), pathMat, worldRoot);
                slab.transform.rotation = Quaternion.Euler(0, Random.Range(-5, 5), 0);
            }

            // Stone lanterns (toro) flanking the path
            for (int i = 0; i < 16; i++)
            {
                float z = (i * 5.5f) - 10f;
                CreateStoneLantern(O + new Vector3(-4.5f, 0, z), worldRoot, stoneMat);
                CreateStoneLantern(O + new Vector3( 4.5f, 0, z), worldRoot, stoneMat);
            }

            // ── Shrine Approach Steps ─────────────────────────────────────────
            for (int i = 0; i < 10; i++)
                CreateBlock("Shrine Step " + i,
                    O + new Vector3(0, (i * 0.28f) + 0.5f, 52f + (i * 0.50f)),
                    new Vector3(10f, 0.28f, 0.55f), stoneMat, worldRoot);

            // ── Shrine Complex on the Hill ─────────────────────────────────────
            CreateMainShrine( O + new Vector3(  0f, 3f, 72f), worldRoot, redMat, woodMat, goldMat, stoneMat, pathMat);
            CreatePagoda(     O + new Vector3( 22f, 3f, 72f), worldRoot, redMat, woodMat);
            CreateBellTower(  O + new Vector3(-22f, 3f, 72f), worldRoot, woodMat, goldMat);

            // ── Koi Pond — west of path ────────────────────────────────────────
            CreateKoiPond(O + new Vector3(-28f, 0, 18f), worldRoot, stoneMat, waterMat, goldMat);

            // ── Zen Garden — east of path ──────────────────────────────────────
            CreateZenGarden(O + new Vector3(28f, 0, 15f), worldRoot, stoneMat, sandMat);

            // ── Bamboo Groves (east & west flanks) ────────────────────────────
            for (int i = 0; i < 65; i++)
            {
                Vector3 bp = O + new Vector3(Random.Range(20f, 85f), 0, Random.Range(-72f, 72f));
                if (Mathf.Abs(bp.z - O.z) > 5f) CreateBambooClump(bp, worldRoot, bambooMat);
            }
            for (int i = 0; i < 65; i++)
            {
                Vector3 bp = O + new Vector3(Random.Range(-85f, -20f), 0, Random.Range(-72f, 72f));
                if (Mathf.Abs(bp.z - O.z) > 5f) CreateBambooClump(bp, worldRoot, bambooMat);
            }

            // ── Sakura Trees ──────────────────────────────────────────────────
            for (int i = 0; i < 180; i++)
            {
                Vector3 tp = O + new Vector3(Random.Range(-100f, 100f), 0.5f, Random.Range(-100f, 100f));
                if (Mathf.Abs(tp.x - O.x) > 6f) CreateSakuraTree(tp, worldRoot, sakuraMat, woodMat);
            }

            // ── Tea House ─────────────────────────────────────────────────────
            CreateTeaHouse(O + new Vector3(-20f, 0, 30f), worldRoot, woodMat, paperMat, redMat, stoneMat);

            // ── Bamboo Perimeter Fence ─────────────────────────────────────────
            for (int i = 0; i < 22; i++) CreateBambooFenceSection(O + new Vector3(-55f + i * 5f, 0, -55f),  worldRoot, bambooMat, woodMat, false);
            for (int i = 0; i < 22; i++) CreateBambooFenceSection(O + new Vector3(-55f + i * 5f, 0,  55f),  worldRoot, bambooMat, woodMat, false);
            for (int i = 0; i < 22; i++) CreateBambooFenceSection(O + new Vector3(-55f, 0, -55f + i * 5f),  worldRoot, bambooMat, woodMat, true);
            for (int i = 0; i < 22; i++) CreateBambooFenceSection(O + new Vector3( 55f, 0, -55f + i * 5f),  worldRoot, bambooMat, woodMat, true);

            // ── Cherry Blossom Weather ────────────────────────────────────────
            CreateCherryBlossomWeather(O, worldRoot);

            // ── Yamato Wildlife ───────────────────────────────────────────────
            Material deerMat  = MakeMat(null, 0.15f); deerMat.color  = new Color(0.62f, 0.45f, 0.28f);
            Material craneMat = MakeMat(null, 0.15f); craneMat.color = Color.white;
            for (int i = 0; i < 10; i++)
                CreateAnimal("Deer", O + new Vector3(Random.Range(-60f, 60f), 0.5f, Random.Range(-60f, 60f)),
                    new Vector3(0.7f, 1.4f, 1.6f), deerMat, 3f, worldRoot);
            for (int i = 0; i < 8; i++)
                CreateAnimal("Crane", O + new Vector3(Random.Range(-50f, 50f), 0.4f, Random.Range(-50f, 50f)),
                    new Vector3(0.6f, 1f, 0.4f), craneMat, 2.5f, worldRoot);

            // ── Yamato Enemies ────────────────────────────────────────────────
            Material samuraiMat  = MakeMat(null, 0.25f); samuraiMat.color  = new Color(0.12f, 0.18f, 0.30f);
            Material ninjaMat    = MakeMat(null, 0.05f); ninjaMat.color    = new Color(0.05f, 0.05f, 0.05f);
            Material yHitFlash   = MakeMat(null, 0.00f); yHitFlash.color   = Color.white;
            for (int i = 0; i < 12; i++)
                CreateEnemy("Samurai",
                    O + new Vector3(Random.Range(-45f, 45f), 1f, Random.Range(5f, 65f)),
                    samuraiMat, yHitFlash, worldRoot);
            for (int i = 0; i < 8; i++)
                CreateEnemy("Ninja",
                    O + new Vector3(Random.Range(-40f, 40f), 1f, Random.Range(5f, 60f)),
                    ninjaMat, yHitFlash, worldRoot);

            // ── Return Portal → Frostheim ──────────────────────────────────────
            CreatePortalGateway(
                "Return Portal (To Frostheim)",
                O + new Vector3(10f, 0f, 5f),
                worldRoot,
                redMat,
                new Color(0.50f, 0.80f, 1.00f, 0.60f),
                new Color(0.40f, 0.80f, 1.00f),
                new Color(0.80f, 0.90f, 1.00f),
                new Vector3(0, 2f, -15f),
                isReturn: true);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  FROSTHEIM PROP BUILDERS
        // ═══════════════════════════════════════════════════════════════════════

        private static void CreateStoneCircle(Vector3 centre, float radius, int count,
            Material mat, Transform parent)
        {
            GameObject circle = new GameObject("Standing Stone Circle");
            circle.transform.position = centre;
            circle.transform.SetParent(parent);
            for (int i = 0; i < count; i++)
            {
                float a = (i / (float)count) * Mathf.PI * 2f;
                Vector3 p = centre + new Vector3(Mathf.Sin(a) * radius, 0, Mathf.Cos(a) * radius);
                float h = Random.Range(3f, 6.5f);
                var s = CreateBlock("Stone " + i, p,
                    new Vector3(Random.Range(0.8f, 1.4f), h, Random.Range(0.8f, 1.2f)),
                    mat, circle.transform);
                s.transform.rotation = Quaternion.Euler(
                    Random.Range(-6, 6), Random.Range(0, 360), Random.Range(-8, 8));
            }
        }

        private static void CreateBurialMound(Vector3 pos, Transform parent,
            Material stone, Material snow)
        {
            GameObject mound = new GameObject("Burial Mound");
            mound.transform.position = pos;
            mound.transform.SetParent(parent);

            // Earth mound — sphere scaled into a low dome
            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Mound Body";
            body.transform.SetParent(mound.transform);
            body.transform.position  = pos + new Vector3(0, 1.8f, 0);
            body.transform.localScale = new Vector3(9f, 3.8f, 9f);
            body.GetComponent<Renderer>().sharedMaterial = snow;
            body.isStatic = true;

            // Stone-lintel entrance passage
            CreateBlock("Post L",   pos + new Vector3(-1.2f, 1.0f, 4.2f), new Vector3(0.5f, 2.5f, 0.5f), stone, mound.transform);
            CreateBlock("Post R",   pos + new Vector3( 1.2f, 1.0f, 4.2f), new Vector3(0.5f, 2.5f, 0.5f), stone, mound.transform);
            CreateBlock("Lintel",   pos + new Vector3(    0, 2.4f, 4.2f), new Vector3(3.2f, 0.4f, 0.5f), stone, mound.transform);
        }

        private static void CreateForge(Vector3 pos, Transform parent,
            Material stone, Material wood, Material metal, Material coal)
        {
            GameObject forge = new GameObject("Blacksmith Forge");
            forge.transform.position = pos;
            forge.transform.SetParent(parent);

            CreateBlock("Forge Base",    pos + new Vector3(  0, 0.8f,    0), new Vector3(4.0f, 1.6f, 3.0f), stone,  forge.transform);
            CreateBlock("Forge Chimney", pos + new Vector3(  0, 3.5f,  0.5f), new Vector3(1.2f, 5.0f, 1.2f), stone,  forge.transform);
            CreateBlock("Forge Anvil",   pos + new Vector3(2.0f,1.65f,  0),  new Vector3(1.0f, 0.5f, 0.6f), metal,  forge.transform);
            CreateBlock("Anvil Stand",   pos + new Vector3(2.0f,1.15f,  0),  new Vector3(0.5f, 0.8f, 0.5f), wood,   forge.transform);
            CreateBlock("Fuel Bin",      pos + new Vector3(-2f, 0.8f,   0),  new Vector3(1.5f, 1.0f, 1.2f), wood,   forge.transform);
            CreateBlock("Coal",          pos + new Vector3(-2f, 1.42f,  0),  new Vector3(1.3f, 0.3f, 1.0f), coal,   forge.transform);
            // Weapon rack + three swords
            CreateBlock("Weapon Rack",  pos + new Vector3(-0.5f,1.5f,1.8f), new Vector3(2.5f,1.5f,0.3f), wood, forge.transform);
            for (int i = 0; i < 3; i++)
                CreateBlock("Sword " + i,
                    pos + new Vector3(-0.8f + i * 0.8f, 2f, 1.85f),
                    new Vector3(0.1f, 1.4f, 0.1f), metal, forge.transform);
            // Fire glow inside the forge
            AddPointLight(forge.transform, pos + new Vector3(0, 2.5f, 0.5f) - pos,
                new Color(1f, 0.35f, 0f), 4f, 8f, "Forge Fire Glow");
        }

        private static void CreateWell(Vector3 pos, Transform parent,
            Material stone, Material wood)
        {
            GameObject well = new GameObject("Stone Well");
            well.transform.position = pos;
            well.transform.SetParent(parent);

            // Circular stone ring approximated with 8 blocks
            for (int i = 0; i < 8; i++)
            {
                float a = (i / 8f) * Mathf.PI * 2f;
                CreateBlock("Ring " + i,
                    pos + new Vector3(Mathf.Sin(a) * 1.4f, 1f, Mathf.Cos(a) * 1.4f),
                    new Vector3(0.7f, 2f, 0.7f), stone, well.transform);
            }
            CreateBlock("Well Floor", pos + new Vector3(0, 0.1f, 0),   new Vector3(3f, 0.2f, 3f), stone, well.transform);
            CreateBlock("Post L",     pos + new Vector3(-1.5f,2.5f,0), new Vector3(0.25f,3f,0.25f), wood, well.transform);
            CreateBlock("Post R",     pos + new Vector3( 1.5f,2.5f,0), new Vector3(0.25f,3f,0.25f), wood, well.transform);
            CreateBlock("Beam",       pos + new Vector3(0, 4.2f, 0),   new Vector3(3.5f,0.25f,0.25f), wood, well.transform);
        }

        private static void CreateLongboat(Vector3 pos, Transform parent,
            Material wood, Material metal)
        {
            GameObject boat = new GameObject("Viking Longboat");
            boat.transform.position = pos;
            boat.transform.rotation = Quaternion.Euler(0, 15, 0);
            boat.transform.SetParent(parent);

            // Hull
            CreateBlock("Hull Bottom", pos + new Vector3(  0, 0.5f,   0), new Vector3(3.5f,  1f, 14f), wood,  boat.transform);
            CreateBlock("Hull L",      pos + new Vector3(-2.2f,1.5f,  0), new Vector3(0.5f,1.5f, 12f), wood,  boat.transform);
            CreateBlock("Hull R",      pos + new Vector3( 2.2f,1.5f,  0), new Vector3(0.5f,1.5f, 12f), wood,  boat.transform);
            // Prow & stern dragon-head posts
            var prow  = CreateBlock("Prow",  pos + new Vector3(0,2.5f, 7f), new Vector3(0.5f,4f,0.5f), wood, boat.transform);
            prow.transform.rotation  = Quaternion.Euler(-25, 0, 0);
            var stern = CreateBlock("Stern", pos + new Vector3(0,2.5f,-7f), new Vector3(0.5f,4f,0.5f), wood, boat.transform);
            stern.transform.rotation = Quaternion.Euler( 25, 0, 0);
            // Oars
            for (int i = 0; i < 5; i++)
            {
                CreateBlock("Oar L " + i, pos + new Vector3(-3f, 2f, -5f + i * 2.5f), new Vector3(0.15f,0.15f,4f), wood, boat.transform).transform.rotation = Quaternion.Euler(0,0,20);
                CreateBlock("Oar R " + i, pos + new Vector3( 3f, 2f, -5f + i * 2.5f), new Vector3(0.15f,0.15f,4f), wood, boat.transform).transform.rotation = Quaternion.Euler(0,0,-20);
            }
            // Round shields along the gunwale
            for (int i = 0; i < 4; i++)
            {
                CreateBlock("Shield L " + i, pos + new Vector3(-2.6f, 2f, -4f + i * 2.5f), new Vector3(0.1f,1.2f,1.2f), metal, boat.transform);
                CreateBlock("Shield R " + i, pos + new Vector3( 2.6f, 2f, -4f + i * 2.5f), new Vector3(0.1f,1.2f,1.2f), metal, boat.transform);
            }
            // Mast & crossbeam
            CreateBlock("Mast",      pos + new Vector3(0, 5f, 0),   new Vector3(0.3f,10f,0.3f), wood, boat.transform);
            CreateBlock("Crossbeam", pos + new Vector3(0, 8.5f, 0), new Vector3(5f,0.3f,0.3f),  wood, boat.transform);
        }

        private static void CreateBlizzard(Transform parent)
        {
            GameObject weather = new GameObject("Blizzard Weather");
            weather.transform.position = new Vector3(0, 30, 0);
            weather.transform.SetParent(parent);

            ParticleSystem ps = weather.AddComponent<ParticleSystem>();
            var rend = weather.GetComponent<ParticleSystemRenderer>();
            if (rend)
            {
                rend.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                rend.sharedMaterial.color = new Color(1f, 1f, 1f, 0.8f);
            }

            var m = ps.main;
            m.startLifetime = 14f;
            m.startSpeed    = 9f;
            m.startSize     = new ParticleSystem.MinMaxCurve(0.08f, 0.28f);
            m.maxParticles  = 6000;

            var em = ps.emission; em.rateOverTime = 2200f;
            var sh = ps.shape;   sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(400, 1, 400);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-2.5f);
            vel.y = new ParticleSystem.MinMaxCurve(-4.5f);
            vel.z = new ParticleSystem.MinMaxCurve( 0.5f);

            var col = ps.collision;
            col.enabled      = true;
            col.type         = ParticleSystemCollisionType.World;
            col.bounce        = 0.08f;
            col.lifetimeLoss  = 0.85f;
            col.quality      = ParticleSystemCollisionQuality.High;
        }

        private static void CreateAuroraBorealis(Transform parent)
        {
            GameObject aurora = new GameObject("Aurora Borealis");
            aurora.transform.position = new Vector3(0, 60, 0);
            aurora.transform.SetParent(parent);

            ParticleSystem ps = aurora.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startLifetime   = new ParticleSystem.MinMaxCurve(9f, 15f);
            m.startSpeed      = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
            m.startSize       = new ParticleSystem.MinMaxCurve(5f, 14f);
            m.maxParticles    = 250;
            m.simulationSpace = ParticleSystemSimulationSpace.World;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(
                new[] {
                    new GradientColorKey(new Color(0.20f, 1.0f, 0.6f), 0.0f),
                    new GradientColorKey(new Color(0.30f, 0.5f, 1.0f), 0.5f),
                    new GradientColorKey(new Color(0.60f, 0.2f, 1.0f), 1.0f)
                },
                new[] {
                    new GradientAlphaKey(0.00f, 0.0f),
                    new GradientAlphaKey(0.38f, 0.3f),
                    new GradientAlphaKey(0.38f, 0.7f),
                    new GradientAlphaKey(0.00f, 1.0f)
                });
            col.color = new ParticleSystem.MinMaxGradient(g);

            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(400, 1, 400);
            var em = ps.emission; em.rateOverTime = 8f;

            var rend = aurora.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            rend.renderMode     = ParticleSystemRenderMode.Stretch;
            rend.velocityScale  = 6f;
            rend.lengthScale    = 1f;
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  YAMATO PROP BUILDERS
        // ═══════════════════════════════════════════════════════════════════════

        private static void CreateStoneLantern(Vector3 pos, Transform parent, Material stone)
        {
            GameObject lantern = new GameObject("Stone Lantern (Toro)");
            lantern.transform.position = pos;
            lantern.transform.SetParent(parent);

            CreateBlock("Base",    pos + new Vector3(0, 0.20f, 0), new Vector3(0.90f, 0.40f, 0.90f), stone, lantern.transform);
            CreateBlock("Post",    pos + new Vector3(0, 1.10f, 0), new Vector3(0.35f, 1.80f, 0.35f), stone, lantern.transform);
            CreateBlock("Cap",     pos + new Vector3(0, 2.20f, 0), new Vector3(0.80f, 0.35f, 0.80f), stone, lantern.transform);
            CreateBlock("Housing", pos + new Vector3(0, 2.80f, 0), new Vector3(0.70f, 0.70f, 0.70f), stone, lantern.transform);
            var roof = CreateBlock("Roof", pos + new Vector3(0, 3.35f, 0), new Vector3(1.10f, 0.25f, 1.10f), stone, lantern.transform);
            roof.transform.rotation = Quaternion.Euler(0, 45, 0);
            // Warm inner glow
            AddPointLight(lantern.transform, new Vector3(0, 2.8f, 0),
                new Color(1f, 0.75f, 0.30f), 1.8f, 6f, "Lantern Glow");
        }

        private static void CreateKoiPond(Vector3 pos, Transform parent,
            Material stone, Material water, Material gold)
        {
            GameObject pond = new GameObject("Koi Pond");
            pond.transform.position = pos;
            pond.transform.SetParent(parent);

            CreateBlock("Pond Basin",   pos + new Vector3(   0,-0.30f,  0), new Vector3(12f,0.60f,9f), stone, pond.transform);
            CreateBlock("Water Surface",pos + new Vector3(   0, 0.05f,  0), new Vector3(11f,0.06f,8f), water, pond.transform);
            CreateBlock("Edge N",       pos + new Vector3(   0, 0.20f,4.5f),new Vector3(12f,0.40f,0.6f),stone,pond.transform);
            CreateBlock("Edge S",       pos + new Vector3(   0, 0.20f,-4.5f),new Vector3(12f,0.40f,0.6f),stone,pond.transform);
            CreateBlock("Edge W",       pos + new Vector3(-6f,  0.20f,  0), new Vector3(0.6f,0.40f,9f), stone, pond.transform);
            CreateBlock("Edge E",       pos + new Vector3( 6f,  0.20f,  0), new Vector3(0.6f,0.40f,9f), stone, pond.transform);

            // Stepping stones across the pond
            foreach (float sx in new float[]{ -3f, 0f, 3f })
                CreateBlock("Stepping Stone",
                    pos + new Vector3(sx, 0.12f, 0),
                    new Vector3(1.2f, 0.2f, 1.2f), stone, pond.transform);

            // Koi fish (small golden roamers beneath the water surface)
            for (int i = 0; i < 8; i++)
            {
                var fish = CreateBlock("Koi " + i,
                    pos + new Vector3(Random.Range(-4f, 4f), -0.1f, Random.Range(-3f, 3f)),
                    new Vector3(0.3f, 0.15f, 0.7f), gold, pond.transform);
                fish.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                fish.AddComponent<RoamingAnimal>().moveSpeed = 1.2f;
            }

            // Decorative feature rock
            var bigRock = CreateBlock("Pond Rock",
                pos + new Vector3(-4.5f, 0.6f, -2f), new Vector3(2f, 1.2f, 1.5f), stone, pond.transform);
            bigRock.transform.rotation = Quaternion.Euler(8, 35, 5);
        }

        private static void CreateZenGarden(Vector3 pos, Transform parent,
            Material stone, Material sand)
        {
            GameObject zen = new GameObject("Zen Garden");
            zen.transform.position = pos;
            zen.transform.SetParent(parent);

            // Raked sand bed
            CreateBlock("Sand Bed",   pos + new Vector3(    0,-0.05f,   0), new Vector3(16f,0.2f,12f),    sand, zen.transform);
            CreateBlock("Border N",   pos + new Vector3(    0,0.30f, 6.2f), new Vector3(16.8f,0.5f,0.5f), stone,zen.transform);
            CreateBlock("Border S",   pos + new Vector3(    0,0.30f,-6.2f), new Vector3(16.8f,0.5f,0.5f), stone,zen.transform);
            CreateBlock("Border W",   pos + new Vector3(-8.2f,0.30f,   0), new Vector3(0.5f,0.5f,12.8f),  stone,zen.transform);
            CreateBlock("Border E",   pos + new Vector3( 8.2f,0.30f,   0), new Vector3(0.5f,0.5f,12.8f),  stone,zen.transform);

            // Rake lines (thin stone-coloured strips pressed into the sand)
            for (int i = 0; i < 10; i++)
                CreateBlock("Rake " + i,
                    pos + new Vector3(0, 0.06f, -5f + i * 1.2f),
                    new Vector3(14f, 0.04f, 0.2f), stone, zen.transform);

            // Feature rocks — three grouped stones
            float[,] r = { {-4f,1.0f,2f,1.4f,1.8f,1.3f}, {1f,0.8f,-1.5f,2.2f,1.6f,1f}, {4f,0.6f,2.5f,1f,1.2f,0.9f} };
            for (int i = 0; i < 3; i++)
            {
                var rock = CreateBlock("Zen Rock " + i,
                    pos + new Vector3(r[i,0], r[i,1], r[i,2]),
                    new Vector3(r[i,3], r[i,4], r[i,5]), stone, zen.transform);
                rock.transform.rotation = Quaternion.Euler(
                    Random.Range(-8, 8), Random.Range(0, 360), Random.Range(-8, 8));
            }

            // Moss-ball bushes
            Material moss = MakeMat(null, 0.05f); moss.color = new Color(0.18f, 0.42f, 0.15f);
            CreateBlock("Moss L", pos + new Vector3(-3f, 0.5f,-1f), new Vector3(1f,1f,1f),         moss, zen.transform);
            CreateBlock("Moss R", pos + new Vector3( 3f, 0.5f, 2f), new Vector3(0.8f,0.8f,0.8f),   moss, zen.transform);
        }

        private static void CreateBambooClump(Vector3 pos, Transform parent, Material bamboo)
        {
            GameObject clump = new GameObject("Bamboo Clump");
            clump.transform.position = pos;
            clump.transform.SetParent(parent);

            int count = Random.Range(4, 9);
            for (int i = 0; i < count; i++)
            {
                float ox = Random.Range(-1.2f, 1.2f);
                float oz = Random.Range(-1.2f, 1.2f);
                float h  = Random.Range(6f, 10f);
                CreateBlock("Stalk", pos + new Vector3(ox, h * 0.5f, oz),
                    new Vector3(0.18f, h, 0.18f), bamboo, clump.transform);
                // Leaf tufts at the top
                for (int j = 0; j < 3; j++)
                {
                    var leaf = CreateBlock("Leaf",
                        pos + new Vector3(ox + Random.Range(-0.8f, 0.8f),
                                          h  - Random.Range(0f, 1.5f),
                                          oz + Random.Range(-0.8f, 0.8f)),
                        new Vector3(1.2f, 0.15f, 0.8f), bamboo, clump.transform);
                    leaf.transform.rotation = Quaternion.Euler(
                        Random.Range(-20, 20), Random.Range(0, 360), Random.Range(-20, 20));
                }
            }
        }

        private static void CreateMainShrine(Vector3 pos, Transform parent,
            Material red, Material wood, Material gold, Material stone, Material path)
        {
            GameObject shrine = new GameObject("Main Shrine (Haiden)");
            shrine.transform.position = pos;
            shrine.transform.SetParent(parent);

            // Stone platform
            CreateBlock("Platform",    pos + new Vector3(  0, 0.8f,   0), new Vector3(22f, 1.8f, 18f), stone, shrine.transform);
            // Shrine body
            CreateBlock("Walls",       pos + new Vector3(  0, 4.0f,   0), new Vector3(14f, 5.0f, 10f), wood,  shrine.transform);
            // Front porch (hisashi)
            CreateBlock("Porch Floor", pos + new Vector3(  0, 2.0f,-7.0f), new Vector3(14f, 0.4f, 4f), wood,  shrine.transform);
            CreateBlock("Porch Post L",pos + new Vector3(-5.5f,4f,-7f),   new Vector3(0.5f, 4f, 0.5f), red,   shrine.transform);
            CreateBlock("Porch Post R",pos + new Vector3( 5.5f,4f,-7f),   new Vector3(0.5f, 4f, 0.5f), red,   shrine.transform);
            // Two-tier irimoya (hip-and-gable) roof
            CreateBlock("Roof Lower",  pos + new Vector3(  0, 7.5f,  0),  new Vector3(18f, 0.6f, 14f), red,  shrine.transform);
            CreateBlock("Roof Upper",  pos + new Vector3(  0, 9.5f,  0),  new Vector3(12f, 0.6f, 10f), red,  shrine.transform);
            CreateBlock("Ridge Beam",  pos + new Vector3(  0,10.4f,  0),  new Vector3(0.5f,0.5f, 10f), wood, shrine.transform);
            // Gold finials (chigi)
            CreateBlock("Finial L",    pos + new Vector3(-6f,10.6f,  0),  new Vector3(0.3f,1.5f,0.3f), gold, shrine.transform);
            CreateBlock("Finial R",    pos + new Vector3( 6f,10.6f,  0),  new Vector3(0.3f,1.5f,0.3f), gold, shrine.transform);
            // Offering box (saisen-bako)
            CreateBlock("Offering Box",pos + new Vector3(  0, 2.5f,-5.0f),new Vector3(2.5f,1f,1.5f),  wood, shrine.transform);
            // Shimenawa paper streamers
            for (int i = 0; i < 5; i++)
                CreateBlock("Streamer " + i,
                    pos + new Vector3(-5f + i * 2.5f, 7.5f, -5.5f),
                    new Vector3(0.1f, Random.Range(0.8f, 1.4f), 0.1f), gold, shrine.transform);
            // Flanking stone lanterns
            CreateStoneLantern(pos + new Vector3(-3.5f, -2f, -7.5f), shrine.transform, stone);
            CreateStoneLantern(pos + new Vector3( 3.5f, -2f, -7.5f), shrine.transform, stone);
        }

        private static void CreateBellTower(Vector3 pos, Transform parent,
            Material wood, Material gold)
        {
            GameObject tower = new GameObject("Bell Tower (Shoro)");
            tower.transform.position = pos;
            tower.transform.SetParent(parent);

            // Four corner posts
            foreach (Vector3 c in new[]{
                new Vector3(-2,3, 2), new Vector3(2,3, 2),
                new Vector3(-2,3,-2), new Vector3(2,3,-2)})
                CreateBlock("Post", pos + c, new Vector3(0.4f, 6f, 0.4f), wood, tower.transform);

            // Horizontal cross-braces
            CreateBlock("Brace N", pos + new Vector3(0, 4.5f,  2f), new Vector3(4.5f,0.35f,0.35f), wood, tower.transform);
            CreateBlock("Brace S", pos + new Vector3(0, 4.5f, -2f), new Vector3(4.5f,0.35f,0.35f), wood, tower.transform);
            CreateBlock("Brace W", pos + new Vector3(-2f,4.5f, 0),  new Vector3(0.35f,0.35f,4.5f), wood, tower.transform);
            CreateBlock("Brace E", pos + new Vector3( 2f,4.5f, 0),  new Vector3(0.35f,0.35f,4.5f), wood, tower.transform);

            // Bonsho (large cast bell) — sphere primitive
            var bell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bell.name = "Bonsho Bell";
            bell.transform.SetParent(tower.transform);
            bell.transform.position   = pos + new Vector3(0, 6.5f, 0);
            bell.transform.localScale = new Vector3(1.4f, 1.8f, 1.4f);
            bell.GetComponent<Renderer>().sharedMaterial = gold;
            bell.isStatic = true;

            // Roof cap
            CreateBlock("Bell Roof", pos + new Vector3(0, 8.5f, 0), new Vector3(5.5f,0.5f,5.5f), wood, tower.transform);
            CreateBlock("Peak",      pos + new Vector3(0, 9.2f, 0), new Vector3(0.3f,1.2f,0.3f), gold, tower.transform);
        }

        private static void CreateTeaHouse(Vector3 pos, Transform parent,
            Material wood, Material paper, Material red, Material stone)
        {
            GameObject teaHouse = new GameObject("Tea House (Chashitsu)");
            teaHouse.transform.position = pos;
            teaHouse.transform.rotation = Quaternion.Euler(0, 30, 0);
            teaHouse.transform.SetParent(parent);

            // Stone foundation
            CreateBlock("Foundation",  pos + new Vector3(  0, 0.4f,    0), new Vector3(9f,  0.8f, 8f),   stone, teaHouse.transform);
            // Walls
            CreateBlock("Wall Back",   pos + new Vector3(  0, 3.0f,  3.5f), new Vector3(8f,  5f, 0.4f),  wood,  teaHouse.transform);
            CreateBlock("Wall L",      pos + new Vector3(-3.8f,3f,   0),   new Vector3(0.4f,5f, 7f),     wood,  teaHouse.transform);
            CreateBlock("Wall R",      pos + new Vector3( 3.8f,3f,   0),   new Vector3(0.4f,5f, 7f),     wood,  teaHouse.transform);
            // Shoji screens (paper panels) on the open front
            CreateBlock("Shoji L",     pos + new Vector3(-1.8f,3f, -3.2f), new Vector3(3.5f,4f,0.15f),  paper, teaHouse.transform);
            CreateBlock("Shoji R",     pos + new Vector3( 1.8f,3f, -3.2f), new Vector3(3.5f,4f,0.15f),  paper, teaHouse.transform);
            // Engawa (timber veranda)
            CreateBlock("Veranda",     pos + new Vector3(  0, 1.0f, -5.5f), new Vector3(8.5f,0.3f,2.5f), wood,  teaHouse.transform);
            // Hip roof
            var roofL = CreateBlock("Roof L", pos + new Vector3(-3f,6.5f,0), new Vector3(4.5f,0.5f,9.5f), red, teaHouse.transform);
            roofL.transform.rotation = Quaternion.Euler(0, 0,  20);
            var roofR = CreateBlock("Roof R", pos + new Vector3( 3f,6.5f,0), new Vector3(4.5f,0.5f,9.5f), red, teaHouse.transform);
            roofR.transform.rotation = Quaternion.Euler(0, 0, -20);
            CreateBlock("Ridge",       pos + new Vector3(  0, 7.2f,  0),  new Vector3(0.4f,0.4f,9.5f),  wood,  teaHouse.transform);
            // Interior: low table + cushions
            Material cushion = MakeMat(null, 0.1f); cushion.color = new Color(0.18f, 0.38f, 0.22f);
            CreateBlock("Tea Table",   pos + new Vector3(  0, 1.3f,   1f), new Vector3(3f,0.12f,2f),    wood,   teaHouse.transform);
            CreateBlock("Cushion L",   pos + new Vector3(-1.2f,1.22f, 0.8f),new Vector3(0.8f,0.12f,0.8f),cushion,teaHouse.transform);
            CreateBlock("Cushion R",   pos + new Vector3( 1.2f,1.22f, 0.8f),new Vector3(0.8f,0.12f,0.8f),cushion,teaHouse.transform);
        }

        private static void CreateBambooFenceSection(Vector3 pos, Transform parent,
            Material bamboo, Material wood, bool rotate90 = false)
        {
            GameObject section = new GameObject("Bamboo Fence Section");
            section.transform.position = pos;
            section.transform.SetParent(parent);
            if (rotate90) section.transform.rotation = Quaternion.Euler(0, 90, 0);

            CreateBlock("Rail Top",    pos + new Vector3(0, 2f, 0), new Vector3(5f, 0.2f, 0.2f), wood,   section.transform);
            CreateBlock("Rail Bottom", pos + new Vector3(0, 1f, 0), new Vector3(5f, 0.2f, 0.2f), wood,   section.transform);
            for (int i = 0; i < 8; i++)
                CreateBlock("Cane " + i,
                    pos + new Vector3(-2.2f + i * 0.62f, 1.5f, 0),
                    new Vector3(0.16f, 3.2f, 0.16f), bamboo, section.transform);
        }

        private static void CreateCherryBlossomWeather(Vector3 origin, Transform parent)
        {
            GameObject weather = new GameObject("Cherry Blossom Weather");
            weather.transform.position = origin + new Vector3(0, 25, 0);
            weather.transform.SetParent(parent);

            ParticleSystem ps = weather.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startColor    = new Color(1f, 0.75f, 0.85f);
            m.startLifetime = new ParticleSystem.MinMaxCurve(10f, 16f);
            m.startSpeed    = new ParticleSystem.MinMaxCurve(0.5f,  2f);
            m.startSize     = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            m.maxParticles  = 2000;

            var rend = weather.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = new Material(Shader.Find("Sprites/Default"));

            var em  = ps.emission; em.rateOverTime  = 120f;
            var sh  = ps.shape;    sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(220, 1, 220);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-0.8f,  0.8f);
            vel.y = new ParticleSystem.MinMaxCurve(-1.2f);
            vel.z = new ParticleSystem.MinMaxCurve(-0.4f,  0.8f);

            var noise = ps.noise;
            noise.enabled   = true;
            noise.strength  = 0.5f;
            noise.frequency = 0.3f;
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  SHARED BUILDERS — used by both realms
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Unified portal gateway factory.
        /// Builds the pillar arch, glowing portal plane, trigger collider,
        /// JapanPortal script, and particle vortex — for both directions.
        /// </summary>
        private static void CreatePortalGateway(
            string   name,
            Vector3  pos,
            Transform parent,
            Material pillarMat,
            Color    glowColor,
            Color    lightColor,
            Color    particleColor,
            Vector3  destination,
            bool     isReturn)
        {
            GameObject group = new GameObject(name);
            group.transform.position = pos;
            group.transform.SetParent(parent);
            if (!isReturn) group.transform.rotation = Quaternion.Euler(0, -25f, 0);

            // Arch structure
            CreateBlock("Pillar L",  pos + new Vector3(-2.8f, 3.5f, 0), new Vector3(0.8f, 7f,  0.8f),  pillarMat, group.transform);
            CreateBlock("Pillar R",  pos + new Vector3( 2.8f, 3.5f, 0), new Vector3(0.8f, 7f,  0.8f),  pillarMat, group.transform);
            CreateBlock("Arch",      pos + new Vector3(    0, 7.0f, 0), new Vector3(8f,   1f,  1f),    pillarMat, group.transform);
            CreateBlock("Sub Arch",  pos + new Vector3(    0, 6.2f, 0), new Vector3(6.5f, 0.5f,0.6f), pillarMat, group.transform);

            // Glowing portal plane
            Material glowMat   = new Material(Shader.Find("Unlit/Color")); glowMat.color = glowColor;
            GameObject glowObj = CreateBlock("Portal Glow",
                pos + new Vector3(0, 3.2f, 0), new Vector3(4.8f, 6.2f, 0.4f), glowMat, group.transform);
            if (isReturn) glowObj.transform.rotation = Quaternion.Euler(0, -90, 0);

            // Point light
            Light pl = glowObj.AddComponent<Light>();
            pl.type = LightType.Point; pl.color = lightColor;
            pl.intensity = 5.5f; pl.range = 18f;

            // Trigger collider
            BoxCollider bc = glowObj.GetComponent<BoxCollider>() ?? glowObj.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(1.1f, 1.1f, 6f);

            // Portal teleport script
            var script = glowObj.AddComponent<JapanPortal>();
            script.destinationTarget = destination;
            script.isReturnPortal    = isReturn;

            // Particle vortex (sakura petals or snow flakes depending on colour)
            ParticleSystem ps     = glowObj.AddComponent<ParticleSystem>();
            var pMain              = ps.main;
            pMain.startColor       = particleColor;
            pMain.startSize        = new ParticleSystem.MinMaxCurve(0.08f, 0.32f);
            pMain.startLifetime    = 4.5f;
            pMain.startSpeed       = 2.2f;
            var pEm  = ps.emission; pEm.rateOverTime  = 40;
            var pSh  = ps.shape;    pSh.shapeType = ParticleSystemShapeType.Box; pSh.scale = new Vector3(4f, 6f, 1f);
            var pRnd = glowObj.GetComponent<ParticleSystemRenderer>();
            pRnd.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            var pVel = ps.velocityOverLifetime;
            pVel.enabled = true;
            pVel.x = new ParticleSystem.MinMaxCurve(-0.5f,  0.5f);
            pVel.y = new ParticleSystem.MinMaxCurve(-0.5f,  0.5f);
            pVel.z = new ParticleSystem.MinMaxCurve(-3.0f, -1.0f);
        }

        private static void CreateSakuraTree(Vector3 pos, Transform parent,
            Material petals, Material wood)
        {
            GameObject tree = new GameObject("Sakura Tree");
            tree.transform.position = pos;
            tree.transform.SetParent(parent);

            CreateBlock("Trunk", pos + new Vector3(0, 2.2f, 0), new Vector3(0.5f, 4.5f, 0.5f), wood, tree.transform);
            for (int i = 0; i < 5; i++)
            {
                var p = CreateBlock("Canopy_" + i,
                    pos + new Vector3(Random.Range(-2f, 2f), Random.Range(3.5f, 5.5f), Random.Range(-2f, 2f)),
                    new Vector3(Random.Range(3f, 5f), Random.Range(2f, 3.5f), Random.Range(3f, 5f)),
                    petals, tree.transform);
                p.transform.rotation = Quaternion.Euler(
                    Random.Range(-15, 15), Random.Range(0, 360), Random.Range(-15, 15));
            }
            // Per-tree petal emitter — fills the grove with drifting blossoms
            var ps = tree.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startColor    = new Color(1f, 0.70f, 0.85f);
            m.startSize     = 0.12f;
            m.startLifetime = 5f;
            m.startSpeed    = 0.5f;
            m.maxParticles  = 60;
            ps.emission.rateOverTime = 4f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 2.5f;
            tree.GetComponent<ParticleSystemRenderer>().sharedMaterial =
                new Material(Shader.Find("Sprites/Default"));
        }

        private static void CreateToriiGate(string name, Vector3 pos, Material red, Transform parent)
        {
            GameObject gate = new GameObject(name);
            gate.transform.position = pos;
            gate.transform.SetParent(parent);

            // Authentic proportions: two round pillars, kasagi, shimagi, nuki, center fuda
            CreateBlock("Pillar L",     pos + new Vector3(-3,  3f,  0), new Vector3(0.75f, 6.5f, 0.75f), red, gate.transform);
            CreateBlock("Pillar R",     pos + new Vector3( 3,  3f,  0), new Vector3(0.75f, 6.5f, 0.75f), red, gate.transform);
            CreateBlock("Kasagi",       pos + new Vector3( 0,  6.4f,0), new Vector3(8.5f,  0.75f, 0.9f), red, gate.transform);
            CreateBlock("Nuki",         pos + new Vector3( 0,  5.2f,0), new Vector3(7.0f,  0.45f, 0.5f), red, gate.transform);
            CreateBlock("Shimagi",      pos + new Vector3( 0,  6.8f,0), new Vector3(9.2f,  0.35f, 0.4f), red, gate.transform);
            CreateBlock("Center Fuda",  pos + new Vector3( 0,  5.8f,0), new Vector3(0.5f,  1.8f,  1.0f), red, gate.transform);
        }

        private static void CreatePagoda(Vector3 pos, Transform parent, Material red, Material wood)
        {
            GameObject pagoda = new GameObject("Pagoda");
            pagoda.transform.position = pos;
            pagoda.transform.SetParent(parent);

            Material grayMat = MakeMat(null, 0.3f); grayMat.color = Color.gray;
            CreateBlock("Stone Base", pos + new Vector3(0, 1, 0), new Vector3(20, 2, 20), grayMat, pagoda.transform);

            // Four tiers with curving eaves
            for (int i = 0; i < 4; i++)
            {
                float h = 3f + i * 4.5f;
                float w = 14f - i * 2.8f;
                CreateBlock("Tier " + i, pos + new Vector3(0, h, 0), new Vector3(w, 4.5f, w), red, pagoda.transform);
                var eL = CreateBlock("Eave L " + i, pos + new Vector3(-w / 2.1f, h + 2.5f, 0), new Vector3(w * 0.9f, 0.5f, w + 1.8f), wood, pagoda.transform);
                eL.transform.rotation = Quaternion.Euler(0, 0,  14);
                var eR = CreateBlock("Eave R " + i, pos + new Vector3( w / 2.1f, h + 2.5f, 0), new Vector3(w * 0.9f, 0.5f, w + 1.8f), wood, pagoda.transform);
                eR.transform.rotation = Quaternion.Euler(0, 0, -14);
            }
            CreateBlock("Finial Spire", pos + new Vector3(0, 22f, 0), new Vector3(0.5f, 10f, 0.5f), wood, pagoda.transform);
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  FROSTHEIM VILLAGE HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        private static void CreateGreatHall(Vector3 pos, Transform parent,
            Material wood, Material snow, Material pathMat)
        {
            GameObject hall = new GameObject("Great Mead Hall");
            hall.transform.position = pos;
            hall.transform.SetParent(parent);

            // Walls
            GameObject walls = new GameObject("Walls");
            walls.transform.SetParent(hall.transform);
            CreateBlock("Wall Back",       pos + new Vector3(    0, 3f,  8f), new Vector3(22, 6,  1),  wood, walls.transform);
            CreateBlock("Wall Left",       pos + new Vector3(-10.5f,3f,  0),  new Vector3( 1, 6, 18),  wood, walls.transform);
            CreateBlock("Wall Right",      pos + new Vector3( 10.5f,3f,  0),  new Vector3( 1, 6, 18),  wood, walls.transform);
            CreateBlock("Wall FrontLeft",  pos + new Vector3( -7.5f,3f, -9f), new Vector3( 8, 6,  1),  wood, walls.transform);
            CreateBlock("Wall FrontRight", pos + new Vector3(  7.5f,3f, -9f), new Vector3( 8, 6,  1),  wood, walls.transform);

            // Roof — two large slanted snow-covered panels with a ridgeline
            GameObject roofHolder = new GameObject("Roof Fader");
            roofHolder.transform.position = pos;
            roofHolder.transform.SetParent(hall.transform);
            var r1 = CreateBlock("Roof L", pos + new Vector3(-5.5f, 8f, 0), new Vector3(13, 1f, 20), snow, roofHolder.transform);
            r1.transform.rotation = Quaternion.Euler(0, 0,  24);
            var r2 = CreateBlock("Roof R", pos + new Vector3( 5.5f, 8f, 0), new Vector3(13, 1f, 20), snow, roofHolder.transform);
            r2.transform.rotation = Quaternion.Euler(0, 0, -24);
            CreateBlock("Roof Peak", pos + new Vector3(0, 10.5f, 0), new Vector3(2, 1f, 20), wood, roofHolder.transform);

            // Dragon-head carved gable ends — painted red
            Material dragonMat = MakeMat(null, 0.5f); dragonMat.color = new Color(0.7f, 0.2f, 0.1f);
            CreateBlock("Gable End N", pos + new Vector3(0, 10f,  10f), new Vector3(3, 3, 1.5f), dragonMat, hall.transform);
            CreateBlock("Gable End S", pos + new Vector3(0, 10f, -10f), new Vector3(3, 3, 1.5f), dragonMat, hall.transform);

            // Interior trigger & roof-fader component
            BoxCollider trigger = walls.AddComponent<BoxCollider>();
            trigger.isTrigger = true; trigger.size = new Vector3(20, 6, 16); trigger.center = new Vector3(0, 3, 0);
            var bInterior = walls.AddComponent<BuildingInterior>(); bInterior.roofObject = roofHolder;

            // Interior furnishings
            CreateBlock("Jarl Table",  pos + new Vector3( 0, 1f,   4f), new Vector3(14, 0.5f, 2),  wood, hall.transform);
            CreateBlock("Left Bench",  pos + new Vector3( 0, 0.5f, 2.5f),new Vector3(14, 0.5f, 1), wood, hall.transform);
            CreateBlock("Right Bench", pos + new Vector3( 0, 0.5f, 6f),  new Vector3(14, 0.5f, 1), wood, hall.transform);
            CreateBlock("Throne",      pos + new Vector3( 0, 1f,   7.5f),new Vector3( 2, 2.5f,1.5f),wood,hall.transform);

            // Central long-fire pit
            var pitMat = MakeMat(null, 0f); pitMat.color = Color.black;
            CreateBlock("Hall Fire Pit", pos + new Vector3(0, 0.2f, -1f), new Vector3(4, 0.4f, 4), pitMat, hall.transform);
            AddPointLight(hall.transform, pos + new Vector3(0, 1.5f, -1f) - hall.transform.position,
                new Color(1f, 0.4f, 0f), 6f, 14f, "Hall Fire Glow");

            // Lead-up path
            CreateBlock("Hall Path", pos + new Vector3(0, 0.05f, -14f), new Vector3(6, 0.1f, 10), pathMat, hall.transform);
        }

        private static void CreateHut(Vector3 pos, Transform parent, Material wood, Material snow)
        {
            GameObject hut = new GameObject("Nordic Hut");
            hut.transform.position = pos;
            hut.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            hut.transform.SetParent(parent);

            CreateBlock("Cabin Walls", pos + new Vector3(0, 1.5f, 0), new Vector3(5, 3, 5), wood, hut.transform);
            var r1 = CreateBlock("Roof L", pos + new Vector3(-1.8f, 3.5f, 0), new Vector3(6f, 0.5f, 6f), snow, hut.transform);
            r1.transform.rotation = hut.transform.rotation * Quaternion.Euler(0, 0,  35);
            var r2 = CreateBlock("Roof R", pos + new Vector3( 1.8f, 3.5f, 0), new Vector3(6f, 0.5f, 6f), snow, hut.transform);
            r2.transform.rotation = hut.transform.rotation * Quaternion.Euler(0, 0, -35);
        }

        private static void CreateBanner(Vector3 pos, Transform parent, Material cloth, Material pole)
        {
            CreateBlock("Flag Pole", pos, new Vector3(0.2f, 6f, 0.2f), pole, parent);
            GameObject banner = CreateBlock("Banner Cloth",
                pos + new Vector3(1.5f, 2f, 0), new Vector3(3f, 4f, 0.1f), cloth, parent, false);
            var sway = banner.AddComponent<SwayingObject>();
            sway.swayAxis = Vector3.forward; sway.swayAmount = 15f; sway.swaySpeed = 2.8f;
        }

        private static void CreateTorch(Vector3 pos, Transform parent)
        {
            Material poleMat = MakeMat(null, 0.1f); poleMat.color = new Color(0.18f, 0.10f, 0.04f);
            CreateBlock("Torch Pole", pos, new Vector3(0.3f, 4f, 0.3f), poleMat, parent);
            AddPointLight(parent, pos + new Vector3(0, 2.2f, 0) - parent.position,
                new Color(1f, 0.55f, 0.08f), 3.2f, 10f, "Torch Flame");
        }

        private static void CreatePineTree(Vector3 pos, Transform parent, Material foliage, Material wood)
        {
            GameObject tree = new GameObject("Nordic Pine");
            tree.transform.position = pos;
            tree.transform.SetParent(parent);
            CreateBlock("Trunk", pos + new Vector3(0, 1.2f, 0), new Vector3(0.65f, 2.5f, 0.65f), wood, tree.transform);
            for (int i = 0; i < 4; i++)
            {
                float w = 3.2f - i * 0.7f;
                var layer = CreateBlock("Layer_" + i,
                    pos + new Vector3(0, 2.8f + i * 1.6f, 0),
                    new Vector3(w, 1.6f, w), foliage, tree.transform);
                layer.transform.rotation = Quaternion.Euler(0, i * 22f, 45);
            }
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  PLAYER / CAMERA / COMBAT / UI SETUP
        // ═══════════════════════════════════════════════════════════════════════

        private static GameObject SetupPlayer(string name)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = name;
            player.tag  = "Player";

            var rb = player.AddComponent<Rigidbody>();
            rb.constraints   = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            player.AddComponent<PlayerController>();
            player.AddComponent<Health>();
            player.AddComponent<TemperatureSystem>();

            // Dash trail
            var trail = player.AddComponent<TrailRenderer>();
            trail.time       = 0.22f;
            trail.startWidth = 0.55f;
            trail.endWidth   = 0f;
            trail.emitting   = false;
            trail.material   = new Material(Shader.Find("Sprites/Default"))
                               { color = new Color(0.2f, 0.8f, 1f, 0.5f) };

            // Leviathan Axe — handle
            Material weaponMat = MakeMat(null, 0.6f); weaponMat.color = Color.gray;
            Material bladeMat  = MakeMat(null, 0.9f); bladeMat.color  = new Color(0.78f, 0.9f, 1f);

            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Axe Handle";
            handle.transform.SetParent(player.transform);
            handle.transform.localPosition = new Vector3(0.6f, 0, 0.5f);
            handle.transform.localScale    = new Vector3(0.1f, 1.2f, 0.1f);
            DestroyImmediate(handle.GetComponent<BoxCollider>());
            handle.GetComponent<Renderer>().sharedMaterial = weaponMat;

            // Axe blade
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Axe Blade";
            blade.transform.SetParent(handle.transform);
            blade.transform.localPosition = new Vector3(0, 0.4f, 2f);
            blade.transform.localScale    = new Vector3(6f, 0.3f, 4f);
            DestroyImmediate(blade.GetComponent<BoxCollider>());
            blade.GetComponent<Renderer>().sharedMaterial = bladeMat;

            var combat = player.AddComponent<PlayerCombat>();
            combat.weaponTransform = handle.transform;
            return player;
        }

        private static Camera SetupCamera(Transform target)
        {
            GameObject rig = new GameObject("Isometric Camera Rig");
            Camera cam = rig.AddComponent<Camera>();
            cam.tag             = "MainCamera";
            cam.orthographic    = true;
            cam.orthographicSize = 12f;
            rig.AddComponent<CameraJuiceManager>();

            var iso = rig.AddComponent<IsometricCameraFollow>();
            SerializedObject so = new SerializedObject(iso);
            so.FindProperty("target").objectReferenceValue         = target;
            so.FindProperty("followOffset").vector3Value           = new Vector3(-15, 22, -15);
            so.ApplyModifiedProperties();
            return cam;
        }

        private static void SetupHadesEncounter(Transform worldRoot)
        {
            GameObject room = new GameObject("Main Arena Encounter Trigger");
            room.transform.position = Vector3.zero;
            room.transform.SetParent(worldRoot);

            var trig = room.AddComponent<BoxCollider>();
            trig.isTrigger = true;
            trig.size = new Vector3(25, 10, 25);

            var roomCtrl   = room.AddComponent<RoomController>();
            var waveSpawner = room.AddComponent<WaveSpawner>();

            GameObject gate = CreateBlock("Arena Locking Gate",
                new Vector3(0, -5, -16), new Vector3(8, 8, 2),
                new Material(Shader.Find("Standard")) { color = Color.red }, room.transform);
            gate.SetActive(false);

            SerializedObject rSo = new SerializedObject(roomCtrl);
            rSo.FindProperty("waveSpawner").objectReferenceValue = waveSpawner;
            SerializedProperty eDoors = rSo.FindProperty("entranceDoors");
            eDoors.arraySize = 1;
            eDoors.GetArrayElementAtIndex(0).objectReferenceValue = gate;
            rSo.ApplyModifiedProperties();
        }

        private static void CreateMinimalUI()
        {
            new GameObject("MinimalHUDOverlay").AddComponent<MinimalHUD>();
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  ENTITY HELPERS — Animal, Enemy
        // ═══════════════════════════════════════════════════════════════════════

        private static void CreateAnimal(string name, Vector3 pos, Vector3 scale,
            Material mat, float speed, Transform parent)
        {
            GameObject animal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            animal.name = name;
            animal.transform.position   = pos;
            animal.transform.localScale = scale;
            animal.transform.SetParent(parent);
            animal.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var rb = animal.AddComponent<Rigidbody>();
            rb.mass        = 2f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            animal.AddComponent<RoamingAnimal>().moveSpeed = speed;

            animal.GetComponent<BoxCollider>().material = new PhysicsMaterial
                { bounciness = 0, staticFriction = 0, dynamicFriction = 0 };
        }

        private static void CreateEnemy(string name, Vector3 pos,
            Material mat, Material hitFlash, Transform parent)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = name;
            enemy.transform.position = pos;
            enemy.transform.SetParent(parent);
            enemy.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var rb = enemy.AddComponent<Rigidbody>();
            rb.mass        = 5f;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            enemy.AddComponent<EnemyAI>().SetHitFlashMaterial(hitFlash);

            // Crude weapon prop
            GameObject weapon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            weapon.name = "Weapon";
            weapon.transform.SetParent(enemy.transform);
            weapon.transform.localPosition = new Vector3(0.6f, 0, 0.5f);
            weapon.transform.localScale    = new Vector3(0.2f, 1.5f, 0.2f);
            Object.DestroyImmediate(weapon.GetComponent<BoxCollider>());
            weapon.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Standard")) { color = Color.black };
        }


        // ═══════════════════════════════════════════════════════════════════════
        //  PRIMITIVE HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        private static void AddPointLight(Transform parent, Vector3 localOffset,
            Color color, float intensity, float range, string lightName = "Point Light")
        {
            GameObject obj = new GameObject(lightName);
            obj.transform.SetParent(parent);
            obj.transform.localPosition = localOffset;
            Light l = obj.AddComponent<Light>();
            l.type = LightType.Point; l.color = color;
            l.intensity = intensity;  l.range  = range;
        }

        private static Material MakeMat(Texture2D tex, float glossiness)
        {
            Material m = new Material(Shader.Find("Standard"));
            m.enableInstancing = true;
            if (tex != null) m.mainTexture = tex;
            m.SetFloat("_Glossiness", glossiness);
            m.color = Color.white;
            return m;
        }

        private static GameObject CreateInvisibleWall(string name, Vector3 pos, Vector3 size, Transform parent)
        {
            GameObject w = new GameObject(name);
            w.transform.position = pos;
            w.transform.SetParent(parent);
            w.AddComponent<BoxCollider>().size = size;
            return w;
        }

        private static GameObject CreateBlock(string name, Vector3 pos, Vector3 scale,
            Material mat, Transform parent, bool isStatic = true)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position   = pos;
            cube.transform.localScale = scale;
            cube.transform.SetParent(parent);
            var rend = cube.GetComponent<Renderer>();
            if (rend) rend.sharedMaterial = mat;
            cube.isStatic = isStatic;
            return cube;
        }

        private static void EnsureTagExists(string tag)
        {
            UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (asset == null || asset.Length == 0) return;
            SerializedObject so     = new SerializedObject(asset[0]);
            SerializedProperty tags = so.FindProperty("tags");
            for (int i = 0; i < tags.arraySize; ++i)
                if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            so.ApplyModifiedProperties();
            so.Update();
        }

        private static Texture2D GenNoise(Color a, Color b, float scale)
        {
            int sz = 256;
            Texture2D tex = new Texture2D(sz, sz);
            float ox = Random.Range(0f, 9999f);
            float oy = Random.Range(0f, 9999f);
            for (int x = 0; x < sz; x++)
                for (int y = 0; y < sz; y++)
                {
                    float s = Mathf.Pow(Mathf.PerlinNoise(
                        (float)x / sz * scale + ox,
                        (float)y / sz * scale + oy), 1.5f);
                    tex.SetPixel(x, y, Color.Lerp(a, b, s));
                }
            tex.Apply();
            return tex;
        }
    }
}