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
    // ╔══════════════════════════════════════════════════════════════════════════════╗
    //  NORDIC WORLD BUILDER  |  Dual-Realm Prestige Edition  v3.0
    // ╠══════════════════════════════════════════════════════════════════════════════╣
    //
    //  DESIGN PHILOSOPHY
    //  ─────────────────
    //  Each realm is built around a distinct MOOD PALETTE, not just geometry.
    //  We follow the "Rule of Thirds" for landmark placement, the "Five-Layer
    //  Depth" model (sky / far-bg / mid / near / floor), and the "Silhouette Test"
    //  (every major prop must read clearly in silhouette).
    //
    //  FROSTHEIM  ─ Norse Viking  (origin 0,0,0)
    //  ──────────────────────────────────────────
    //  Mood:    Iron-grey dawn, survival, ancient power.
    //  Palette: Cold steel blues, ash whites, ember oranges, blood reds.
    //  Shapes:  Heavy, earthbound, asymmetric ruin.
    //  Sound:   Howling wind, fire crackle, distant wolves.
    //  New:     Cliffside waterfall (frozen), mass-grave pit, chieftain's
    //           burial ship on hill, menhir rune-gate, meadery cellar
    //           entrance hatch, watch-towers, skull-post ward markers,
    //           collapsed bridge, fjord valley vista, GODRAYS through
    //           overcast sky.
    //
    //  YAMATO  ─ Feudal Japan  (offset 10000,0,10000)
    //  ────────────────────────────────────────────────
    //  Mood:    Golden afternoon light, disciplined beauty, hidden danger.
    //  Palette: Vermillion reds, warm gold, deep indigo, moss greens,
    //           cream paper, gunmetal black.
    //  Shapes:  Precise geometry, layered rooflines, negative space.
    //  Sound:   Wind through bamboo, distant temple bells, water.
    //  New:     Dry-moat castle approach, Noh theatre stage, ceremonial
    //           archery range (kyudojo), ornamental bridge over stream,
    //           watermill, midnight-black armoury, stone garden of the
    //           dead (gorintō stupa field), lantern festival pennants,
    //           hidden ninja rooftop paths, blossoming wisteria arcade,
    //           koi waterfall cascade, Butsudan altar alcove, VOLUMETRIC
    //           morning-mist layer.
    //
    // ╚══════════════════════════════════════════════════════════════════════════════╝

    public class NordicWorldBuilder : EditorWindow
    {
        [MenuItem("World Foundation/Generate Masterpiece Environment")]
        [MenuItem("Nordic Wilds/Generate Masterpiece Environment")]
        public static void BuildMasterpieceWorld()
        {
            Debug.Log("══════════════════════════════════════════════════════════════════");
            Debug.Log("  NORDIC WORLD BUILDER v3.0 — Dual-Realm Prestige Edition");
            Debug.Log("  FROSTHEIM (Norse) ←→ YAMATO (Feudal Japan)");
            Debug.Log("══════════════════════════════════════════════════════════════════");

            // ── Tags ─────────────────────────────────────────────────────────────
            foreach (string t in new[]{ "Player","Campfire","Enemy","Portal",
                                         "Interactable","Climbable","Secret" })
                EnsureTagExists(t);

            // ── Clear ─────────────────────────────────────────────────────────────
            foreach (string n in new[]{ "Nordic World Root","Player","Isometric Camera Rig" })
                DestroyImmediate(GameObject.Find(n));

            GameObject worldRoot = new GameObject("Nordic World Root");

            // ══════════════════════════════════════════════════════════════════════
            //  FROSTHEIM MATERIALS  ─ five-tone palette + noise textures
            // ══════════════════════════════════════════════════════════════════════
            // Primary surfaces
            Material snowMat        = MakeMat(GenNoise(new Color(0.87f,0.91f,0.96f), new Color(0.72f,0.83f,0.91f), 10f), 0.06f);
            Material snowDirtyMat   = MakeMat(GenNoise(new Color(0.70f,0.74f,0.80f), new Color(0.55f,0.60f,0.68f), 14f), 0.04f);
            Material stoneMat       = MakeMat(GenNoise(new Color(0.34f,0.37f,0.43f), new Color(0.20f,0.23f,0.29f), 26f), 0.28f);
            Material darkStoneMat   = MakeMat(GenNoise(new Color(0.19f,0.21f,0.25f), new Color(0.11f,0.13f,0.17f), 22f), 0.22f);
            Material runeStoneVein  = MakeMat(GenNoise(new Color(0.22f,0.28f,0.40f), new Color(0.14f,0.20f,0.32f), 18f), 0.35f);
            Material iceMat         = MakeMat(GenNoise(new Color(0.76f,0.89f,0.99f), new Color(0.62f,0.81f,0.96f), 15f), 0.90f);
            Material iceDeepMat     = MakeMat(GenNoise(new Color(0.45f,0.68f,0.88f), new Color(0.30f,0.55f,0.80f), 12f), 0.95f);
            Material woodMat        = MakeMat(GenNoise(new Color(0.26f,0.16f,0.06f), new Color(0.16f,0.11f,0.03f), 42f), 0.18f);
            Material woodDarkMat    = MakeMat(GenNoise(new Color(0.14f,0.09f,0.03f), new Color(0.08f,0.05f,0.01f), 38f), 0.12f);
            Material pineMat        = MakeMat(GenNoise(new Color(0.13f,0.33f,0.19f), new Color(0.08f,0.22f,0.12f), 10f), 0.04f);
            Material mossRockMat    = MakeMat(GenNoise(new Color(0.25f,0.35f,0.22f), new Color(0.16f,0.26f,0.14f), 20f), 0.08f);
            Material pathMat        = MakeMat(GenNoise(new Color(0.54f,0.57f,0.61f), new Color(0.41f,0.44f,0.47f), 28f), 0.08f);
            Material mudMat         = MakeMat(GenNoise(new Color(0.28f,0.22f,0.14f), new Color(0.18f,0.14f,0.09f), 20f), 0.03f);
            Material metalMat       = MakeMat(null, 0.78f);  metalMat.color   = new Color(0.66f,0.71f,0.76f);
            Material ironRustMat    = MakeMat(GenNoise(new Color(0.55f,0.38f,0.22f), new Color(0.40f,0.26f,0.14f), 18f), 0.12f);
            Material coalMat        = MakeMat(null, 0.04f);  coalMat.color    = new Color(0.07f,0.07f,0.09f);
            Material ashMat         = MakeMat(null, 0.03f);  ashMat.color     = new Color(0.52f,0.50f,0.48f);
            Material bannerRedMat   = MakeMat(null, 0.14f);  bannerRedMat.color  = new Color(0.75f,0.10f,0.10f);
            Material bannerBlackMat = MakeMat(null, 0.08f);  bannerBlackMat.color = new Color(0.12f,0.12f,0.14f);
            Material bonesMat       = MakeMat(null, 0.05f);  bonesMat.color   = new Color(0.84f,0.80f,0.72f);
            Material bloodDecalMat  = MakeMat(null, 0.02f);  bloodDecalMat.color = new Color(0.55f,0.05f,0.05f);
            Material thatchMat      = MakeMat(GenNoise(new Color(0.52f,0.44f,0.28f), new Color(0.38f,0.32f,0.18f), 16f), 0.06f);
            Material copperMat      = MakeMat(null, 0.60f);  copperMat.color  = new Color(0.38f,0.62f,0.42f); // patinated
            Material gildedMat      = MakeMat(null, 0.88f);  gildedMat.color  = new Color(0.88f,0.74f,0.28f);
            Material charMat        = MakeMat(null, 0.02f);  charMat.color    = new Color(0.09f,0.08f,0.07f);

            // ── Frostheim Lighting ────────────────────────────────────────────────
            // Overcast iron-dawn: low, blue-grey directional from north-west
#if UNITY_2023_1_OR_NEWER
            Light dirLight = Object.FindFirstObjectByType<Light>();
#else
            Light dirLight = Object.FindObjectOfType<Light>();
#endif
            if (dirLight != null && dirLight.type == LightType.Directional)
            {
                dirLight.color          = new Color(0.52f, 0.66f, 0.96f);
                dirLight.intensity      = 0.92f;
                dirLight.transform.rotation = Quaternion.Euler(28f, -62f, 0f);
                dirLight.shadows        = LightShadows.Soft;
                dirLight.shadowStrength = 0.88f;
                dirLight.shadowBias     = 0.04f;
            }

            // Trilight ambient: cold sky / neutral equator / cool-shadow ground
            RenderSettings.ambientMode         = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor     = new Color(0.35f, 0.44f, 0.65f);
            RenderSettings.ambientEquatorColor = new Color(0.26f, 0.34f, 0.52f);
            RenderSettings.ambientGroundColor  = new Color(0.14f, 0.22f, 0.38f);

            // Fog: exponential, cold mist
            RenderSettings.fog        = true;
            RenderSettings.fogColor   = new Color(0.55f, 0.68f, 0.85f);
            RenderSettings.fogMode    = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.009f;

            // ── Frostheim Terrain & Geology ───────────────────────────────────────
            //  FIVE-LAYER DEPTH:
            //   1. Sky / god-rays (particles)
            //   2. Distant mountain silhouettes  ← new
            //   3. Mid cliff faces
            //   4. Village & encounter arena
            //   5. Foreground path & detail props

            // Base snow field — slightly undulating via scale trick
            CreateBlock("Arena Floor",     new Vector3(  0,-0.50f,   0), new Vector3(420,1.0f,420), snowMat,      worldRoot.transform);
            CreateBlock("Under Floor",     new Vector3(  0,-1.20f,   0), new Vector3(420,1.0f,420), snowDirtyMat, worldRoot.transform);

            // --- Layer 2: Distant mountain silhouettes (silhouette-readable skyboxers) ---
            CreateDistantMountains(worldRoot.transform, stoneMat, snowMat, darkStoneMat);

            // --- Layer 3: Cliffs & natural boundaries ---
            // North cliff mass
            CreateBlock("North Cliff",  new Vector3(  0,  2.0f, 100f), new Vector3(420,  5f, 210), stoneMat,     worldRoot.transform);
            // West & East sheer cliff faces (strata layering — two-tone)
            CreateStratifiedCliff("West Cliff",  new Vector3(-205f, 10f,   0), new Vector3(8f, 28f, 420), stoneMat, darkStoneMat, worldRoot.transform, false);
            CreateStratifiedCliff("East Cliff",  new Vector3( 205f, 10f,   0), new Vector3(8f, 28f, 420), stoneMat, darkStoneMat, worldRoot.transform, false);
            CreateBlock("South Wall",   new Vector3(  0,  8f, -205f), new Vector3(420, 28f,  8), stoneMat,      worldRoot.transform);

            // Frozen waterfall — north-west cliff face (design landmark #1)
            CreateFrozenWaterfall(new Vector3(-65f, 0f, 68f), worldRoot.transform, iceMat, iceDeepMat, stoneMat);

            // Frozen lake — east of village, with cracked ice detail
            CreateFrozenLake(new Vector3(58f, 0f, 22f), worldRoot.transform, iceMat, iceDeepMat, stoneMat, snowMat);

            // Fjord valley glimpse cut into the south wall
            CreateFjordValleyVista(new Vector3(0f, 0f, -200f), worldRoot.transform, iceDeepMat, stoneMat, snowMat);

            // Invisible play bounds
            GameObject bounds = new GameObject("Invisible Boundaries");
            bounds.transform.SetParent(worldRoot.transform);
            CreateInvisibleWall("N Bound", new Vector3(   0, 5f,  80f), new Vector3(165, 22,  2), bounds.transform);
            CreateInvisibleWall("S Bound", new Vector3(   0, 5f, -80f), new Vector3(165, 22,  2), bounds.transform);
            CreateInvisibleWall("W Bound", new Vector3( -82f,5f,    0), new Vector3(  2, 22, 165), bounds.transform);
            CreateInvisibleWall("E Bound", new Vector3(  82f,5f,    0), new Vector3(  2, 22, 165), bounds.transform);

            // ── Terrain Surface Detail ────────────────────────────────────────────
            // Scattered snow/mud patches for visual noise (Rule: vary scale & rotation)
            for (int i = 0; i < 80; i++)
            {
                var patch = CreateBlock("Snow Patch",
                    new Vector3(Random.Range(-160f,160f), 0.02f, Random.Range(-160f,160f)),
                    new Vector3(Random.Range(1.5f,6f), 0.04f, Random.Range(1.5f,5f)),
                    i % 3 == 0 ? mudMat : snowDirtyMat, worldRoot.transform);
                patch.transform.rotation = Quaternion.Euler(0, Random.Range(0,360), 0);
            }

            // ── Grand Approach: Entrance Corridor & Causeway ──────────────────────
            // Stone-paved causeway from south entry
            CreateBlock("Entrance Causeway", new Vector3(0,-0.5f,-42f), new Vector3(10,1,44), snowMat, worldRoot.transform);
            for (int i = 0; i < 11; i++)
            {
                var slab = CreateBlock("Causeway Slab",
                    new Vector3(Random.Range(-0.5f,0.5f), 0.08f, -58f + i*5f),
                    new Vector3(9.5f,0.18f,4.5f), pathMat, worldRoot.transform);
                slab.transform.rotation = Quaternion.Euler(0, Random.Range(-3,3), 0);
            }

            // Skull-post ward markers flanking the approach (every 10 units)
            for (int i = 0; i < 5; i++)
            {
                float z = -55f + i * 12f;
                CreateSkullPost(new Vector3(-6.5f, 0f, z), worldRoot.transform, woodDarkMat, bonesMat);
                CreateSkullPost(new Vector3( 6.5f, 0f, z), worldRoot.transform, woodDarkMat, bonesMat);
            }

            // Watch-towers flanking the approach (design landmark #2)
            CreateWatchTower(new Vector3(-14f, 0f, -45f), worldRoot.transform, woodMat, stoneMat, metalMat, bannerRedMat);
            CreateWatchTower(new Vector3( 14f, 0f, -45f), worldRoot.transform, woodMat, stoneMat, metalMat, bannerBlackMat);

            // Grand staircase up to the Altar (12 steps, invisible ramp overlay)
            GameObject grandStairs = new GameObject("Grand Stairs to Altar");
            grandStairs.transform.SetParent(worldRoot.transform);
            for (int i = 0; i < 14; i++)
                CreateBlock("Step_" + i,
                    new Vector3(0, i * 0.32f - 0.5f, 40f + i * 0.55f),
                    new Vector3(16f, 0.32f, 0.60f), stoneMat, grandStairs.transform);
            var stairRamp = CreateInvisibleWall("Stair Ramp",
                new Vector3(0, 1.6f, 43.5f), new Vector3(16f, 0.5f, 8f), grandStairs.transform);
            stairRamp.transform.rotation = Quaternion.Euler(-30f, 0, 0);

            // ── North Altar: Sacred Heart of Frostheim ────────────────────────────
            //  Design goal: Overwhelming sense of age and power.
            //  Uses offset rotation on stones to avoid "manufactured" look.

            // Altar platform — tiered
            CreateBlock("Altar Platform 1", new Vector3(0, -0.5f, 55f), new Vector3(38f,1.0f,30f), stoneMat,     worldRoot.transform);
            CreateBlock("Altar Platform 2", new Vector3(0,  0.5f, 55f), new Vector3(28f,1.0f,22f), darkStoneMat, worldRoot.transform);
            CreateBlock("Altar Platform 3", new Vector3(0,  1.5f, 55f), new Vector3(18f,0.8f,14f), darkStoneMat, worldRoot.transform);

            // The Great Runestone — veined rune-stone material, slightly tilted
            GameObject runestone = CreateBlock("Great Runestone",
                new Vector3(0, 6.2f, 55f), new Vector3(3.2f, 11f, 2.8f), runeStoneVein, worldRoot.transform);
            runestone.transform.rotation = Quaternion.Euler(0, 14, 5);

            // Rune glow — blue-white cold fire
            AddPointLight(worldRoot.transform, new Vector3(0, 8f, 55),   new Color(0.35f, 0.58f, 1.0f), 3.0f, 16f, "Rune Core Glow");
            AddPointLight(worldRoot.transform, new Vector3(0, 12f, 55),  new Color(0.50f, 0.70f, 1.0f), 1.5f, 8f,  "Rune Top Glow");

            // Standing stone circle (outer) + inner sanctum ring
            CreateStoneCircle(new Vector3(0, 2f, 55f), 18f, 13, stoneMat,     worldRoot.transform);
            CreateStoneCircle(new Vector3(0, 2f, 55f), 10f,  8, runeStoneVein, worldRoot.transform);

            // Sacrificial altar block with blood decal (design detail)
            CreateBlock("Sacrifice Stone", new Vector3(0, 2.8f, 49f), new Vector3(4f,1.2f,2.5f), darkStoneMat, worldRoot.transform);
            CreateBlock("Blood Pool",      new Vector3(0, 2.6f, 50f), new Vector3(3.5f,0.1f,4f),  bloodDecalMat, worldRoot.transform);

            // Burial mounds (three, asymmetric placement for natural feel)
            CreateBurialMound(new Vector3(-32f, 0, 47f), worldRoot.transform, stoneMat, snowMat);
            CreateBurialMound(new Vector3( 38f, 0, 52f), worldRoot.transform, stoneMat, snowMat);
            CreateBurialMound(new Vector3(-18f, 0, 62f), worldRoot.transform, mossRockMat, snowDirtyMat);

            // Burial ship on the hill behind the runestone — chieftain's send-off
            CreateBurialShipOnHill(new Vector3(-5f, 4f, 66f), worldRoot.transform, woodDarkMat, metalMat, ashMat);

            // ── Frostheim Village ─────────────────────────────────────────────────
            //  Layout: great hall anchors north, forge + well form the east quad,
            //  meadery and huts fill west. Paths connect everything.

            // Central dirt path network
            CreateVillagePaths(worldRoot.transform, mudMat, pathMat);

            // Great Mead Hall (village centrepiece)
            CreateGreatHall(new Vector3(0f, 0f, 22f), worldRoot.transform, woodMat, woodDarkMat, thatchMat, stoneMat, pathMat, bannerRedMat);

            // Meadery — underground cellar entrance (hatch + steps going down)
            CreateMeadery(new Vector3(-30f, 0f, 18f), worldRoot.transform, stoneMat, woodMat, woodDarkMat, gildedMat);

            // Blacksmith forge (design landmark #3 — always smoking)
            CreateForge(new Vector3(22f, 0f, 0f), worldRoot.transform, stoneMat, woodMat, metalMat, coalMat, ironRustMat, gildedMat);

            // Stone well with detailed rim and bucket
            CreateWell(new Vector3(-9f, 0f, 4f), worldRoot.transform, stoneMat, woodMat, ironRustMat);

            // Viking longboat docked at the frozen lake
            CreateLongboat(new Vector3(58f, 0.15f, -4f), worldRoot.transform, woodMat, woodDarkMat, metalMat, bannerRedMat);

            // Huts — varied sizes, organic placement
            CreateHut(new Vector3(-26f, 0f,  14f), worldRoot.transform, woodMat, thatchMat, stoneMat, 0f);
            CreateHut(new Vector3( 32f, 0f,   4f), worldRoot.transform, woodMat, thatchMat, stoneMat, 25f);
            CreateHut(new Vector3(-16f, 0f, -12f), worldRoot.transform, woodMat, thatchMat, stoneMat, -15f);
            CreateHut(new Vector3( 26f, 0f,  36f), worldRoot.transform, woodMat, thatchMat, stoneMat, 10f);
            CreateHut(new Vector3(-40f, 0f,  36f), worldRoot.transform, woodMat, thatchMat, stoneMat, -5f);
            CreateHut(new Vector3( 44f, 0f,  52f), worldRoot.transform, woodMat, thatchMat, stoneMat, 180f);
            CreateHut(new Vector3(-44f, 0f,  52f), worldRoot.transform, woodMat, thatchMat, stoneMat, 170f);
            CreateHut(new Vector3( 12f, 0f, -18f), worldRoot.transform, woodMat, thatchMat, stoneMat, 40f);
            // Small storage shed
            CreateStorageShed(new Vector3(-32f, 0f, 5f), worldRoot.transform, woodDarkMat, thatchMat);

            // Collapsed / burned hut (battle scar / storytelling)
            CreateCollapsedHut(new Vector3(38f, 0f, 18f), worldRoot.transform, charMat, ashMat, woodDarkMat);

            // Campfire safe zone — with full log seating
            CreateCampfireSafeZone(new Vector3(-14f, 0f, 7f), worldRoot.transform, stoneMat, woodMat);

            // Village torches, banners, and dressing
            float[,] torchPos = {
                {-24f,2f,20f},{28f,2f,16f},{-10f,2f,30f},{10f,2f,30f},{-30f,2f,-7f},
                { 30f,2f,-7f},{  0f,2f,-5f},{-18f,2f,45f},{ 18f,2f,45f},{0f,2f,18f}
            };
            for (int i = 0; i < 10; i++)
                CreateTorch(new Vector3(torchPos[i,0], torchPos[i,1], torchPos[i,2]), worldRoot.transform);

            CreateBanner(new Vector3(-5f, 5f,  9f), worldRoot.transform, bannerRedMat,   woodDarkMat);
            CreateBanner(new Vector3( 5f, 5f,  9f), worldRoot.transform, bannerBlackMat, woodDarkMat);
            CreateBanner(new Vector3(-22f,5f, 28f), worldRoot.transform, bannerRedMat,   woodDarkMat);
            CreateBanner(new Vector3( 22f,5f, 28f), worldRoot.transform, bannerRedMat,   woodDarkMat);
            CreateBanner(new Vector3( -5f,5f, 42f), worldRoot.transform, bannerBlackMat, woodDarkMat);
            CreateBanner(new Vector3(  5f,5f, 42f), worldRoot.transform, bannerBlackMat, woodDarkMat);

            // Mass-grave pit (dark lore, design contrast — keeps players unsettled)
            CreateMassGravePit(new Vector3(55f, 0f, 45f), worldRoot.transform, mudMat, bonesMat, stoneMat);

            // Collapsed / ruined bridge (over a dry gully — storytelling prop)
            CreateCollapsedBridge(new Vector3(-50f, 0f, -10f), worldRoot.transform, stoneMat, woodDarkMat, ironRustMat);

            // ── Frostheim Foliage & Environmental Scatter ──────────────────────────
            // Pine forest — density gradient (thicker at edges, open near village)
            for (int i = 0; i < 200; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-195f, 195f), 0.5f, Random.Range(-195f, 195f));
                float distFromCentre = new Vector2(pos.x, pos.z).magnitude;
                // Reject trees too close to village core
                if (distFromCentre < 30f) continue;
                // Density bias: more trees towards edges
                if (distFromCentre < 70f && Random.value < 0.4f) continue;
                CreatePineTree(pos, worldRoot.transform, pineMat, woodMat, woodDarkMat);
            }

            // Moss-covered ruin stones (ancient, pre-village)
            for (int i = 0; i < 60; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-185f, 185f), 1f, Random.Range(-185f, 185f));
                float h = Random.Range(1.5f, 10f);
                Material rm = Random.value > 0.35f ? stoneMat : mossRockMat;
                var s = CreateBlock("Ruin Stone",  pos,
                    new Vector3(Random.Range(0.8f,5f), h, Random.Range(0.8f,4f)), rm, worldRoot.transform);
                s.transform.rotation = Quaternion.Euler(
                    Random.Range(-22,22), Random.Range(0,360), Random.Range(-22,22));
            }

            // Fallen logs with snow caps
            for (int i = 0; i < 20; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-155f,155f), 0.5f, Random.Range(-155f,155f));
                var l = CreateBlock("Fallen Log", pos,
                    new Vector3(1f, 1f, Random.Range(4f,10f)), woodDarkMat, worldRoot.transform);
                l.transform.rotation = Quaternion.Euler(Random.Range(-12,12), Random.Range(0,360), Random.Range(-12,12));
                // Snow cap on top of log
                CreateBlock("Log Snow", pos + new Vector3(0,0.7f,0),
                    new Vector3(0.95f,0.3f,Random.Range(3.5f,9f)), snowMat, worldRoot.transform);
            }

            // Large scattered boulders (glacial erratics)
            for (int i = 0; i < 15; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-185f,185f), Random.Range(0.5f,2f), Random.Range(-185f,185f));
                var b = CreateBlock("Glacial Boulder", pos,
                    new Vector3(Random.Range(2f,7f), Random.Range(1.5f,5f), Random.Range(2f,6f)),
                    Random.value > 0.5f ? stoneMat : mossRockMat, worldRoot.transform);
                b.transform.rotation = Quaternion.Euler(
                    Random.Range(-20,20), Random.Range(0,360), Random.Range(-20,20));
            }

            // ── Frostheim → Yamato Portal ──────────────────────────────────────────
            CreatePortalGateway(
                "Portal Gateway (To Yamato)",
                new Vector3(6f, 0f, -16f),
                worldRoot.transform,
                runeStoneVein,
                new Color(0.90f, 0.40f, 0.75f, 0.55f),
                new Color(1.00f, 0.35f, 0.80f),
                new Color(1.00f, 0.72f, 0.88f),
                new Vector3(10000, 2f, 10000),
                isReturn: false);

            // ── Frostheim Weather System ───────────────────────────────────────────
            CreateBlizzard(worldRoot.transform);
            CreateAuroraBorealis(worldRoot.transform);
            CreateGodraysOvercast(worldRoot.transform, new Color(0.65f,0.75f,0.95f));

            // ── Frostheim Wildlife ─────────────────────────────────────────────────
            Material squirrelMat = MakeMat(null,0.18f); squirrelMat.color = new Color(0.40f,0.25f,0.10f);
            Material foxMat      = MakeMat(null,0.10f); foxMat.color      = new Color(0.90f,0.93f,0.97f);
            Material wolfMat     = MakeMat(null,0.18f); wolfMat.color     = new Color(0.22f,0.22f,0.26f);
            Material ravenMat    = MakeMat(null,0.12f); ravenMat.color    = new Color(0.08f,0.08f,0.10f);
            Material elkMat      = MakeMat(null,0.12f); elkMat.color      = new Color(0.50f,0.38f,0.22f);

            for (int i = 0; i < 10; i++)
                CreateAnimal("Squirrel",  new Vector3(Random.Range(-72f,72f),0.25f,Random.Range(-72f,72f)), new Vector3(0.5f,0.5f,0.8f),  squirrelMat,6.0f,worldRoot.transform);
            for (int i = 0; i < 5; i++)
                CreateAnimal("Snow Fox",  new Vector3(Random.Range(-82f,82f),0.5f, Random.Range(-82f,82f)), new Vector3(0.8f,1.0f,1.6f),  foxMat,      4.0f,worldRoot.transform);
            for (int i = 0; i < 3; i++)
                CreateAnimal("Wolf",      new Vector3(Random.Range(-92f,92f),0.6f, Random.Range(-92f,92f)), new Vector3(1.0f,0.9f,1.8f),  wolfMat,     5.5f,worldRoot.transform);
            for (int i = 0; i < 3; i++)
                CreateAnimal("Raven",     new Vector3(Random.Range(-60f,60f),4.0f, Random.Range(-60f,60f)), new Vector3(0.6f,0.4f,0.5f),  ravenMat,    3.5f,worldRoot.transform);
            for (int i = 0; i < 2; i++)
                CreateAnimal("Elk",       new Vector3(Random.Range(-80f,80f),0.8f, Random.Range(20f,80f)),  new Vector3(1.2f,2.2f,2.2f),  elkMat,      2.5f,worldRoot.transform);

            // ── Frostheim Enemies ──────────────────────────────────────────────────
            Material draugrMat    = MakeMat(null,0.08f); draugrMat.color   = new Color(0.78f,0.10f,0.10f);
            Material berserkMat   = MakeMat(null,0.10f); berserkMat.color  = new Color(0.62f,0.04f,0.04f);
            Material jarlMat      = MakeMat(null,0.35f); jarlMat.color     = new Color(0.82f,0.65f,0.22f); // gilded jarl
            Material hitFlashMat  = MakeMat(null,0.00f); hitFlashMat.color = Color.white;

            for (int i = 0; i < 18; i++)
                CreateEnemy("Draugr Thrall",
                    new Vector3(Random.Range(-44f,44f),1f,Random.Range(10f,70f)),
                    draugrMat, hitFlashMat, worldRoot.transform);
            for (int i = 0; i < 6; i++)
                CreateEnemy("Berserker",
                    new Vector3(Random.Range(-36f,36f),1f,Random.Range(22f,68f)),
                    berserkMat, hitFlashMat, worldRoot.transform);
            // Elite Jarl mini-boss
            CreateEnemy("Jarl Champion",
                new Vector3(4f, 1f, 62f), jarlMat, hitFlashMat, worldRoot.transform);

            // ── YAMATO Realm ────────────────────────────────────────────────────────
            CreateYamatoRealm(worldRoot.transform);

            // ── Player, Camera, Encounter ───────────────────────────────────────────
            GameObject player = SetupPlayer("Player");

            Camera cam = SetupCamera(player.transform);
            SetupHadesEncounter(worldRoot.transform);

            var pController = player.GetComponent<PlayerController>();
            SerializedObject pSo = new SerializedObject(pController);
            pSo.FindProperty("isometricCameraTransform").objectReferenceValue = cam.transform;
            pSo.ApplyModifiedProperties();

            CreateMinimalUI();

            // Setup Ocean Main Menu Start
            CreateOceanMenuUI(worldRoot.transform, player, cam);

            Debug.Log("═══ BUILD COMPLETE — FROSTHEIM ←→ YAMATO  ═══");
        }


        // ╔══════════════════════════════════════════════════════════════════════════╗
        //  YAMATO  —  FEUDAL JAPAN REALM
        //  All coordinates offset by +10 000 on X and Z.
        //  Design language: disciplined geometry, layered rooflines,
        //  controlled negative space, warm gold / vermillion / indigo.
        // ╚══════════════════════════════════════════════════════════════════════════╝

        private static void CreateYamatoRealm(Transform worldRoot)
        {
            Vector3 O = new Vector3(10000f, 0f, 10000f);

            // ── Yamato Materials (warm, deliberate palette) ────────────────────────
            Material grassMat    = MakeMat(GenNoise(new Color(0.40f,0.64f,0.30f), new Color(0.30f,0.54f,0.22f), 16f), 0.04f);
            Material mossMat     = MakeMat(GenNoise(new Color(0.28f,0.48f,0.20f), new Color(0.18f,0.38f,0.13f), 20f), 0.06f);
            Material stoneMat    = MakeMat(GenNoise(new Color(0.56f,0.56f,0.60f), new Color(0.40f,0.40f,0.44f), 24f), 0.28f);
            Material darkStone   = MakeMat(GenNoise(new Color(0.22f,0.22f,0.26f), new Color(0.14f,0.14f,0.18f), 20f), 0.22f);
            Material woodMat     = MakeMat(null,0.18f); woodMat.color    = new Color(0.19f,0.11f,0.04f);
            Material woodLightMat= MakeMat(null,0.22f); woodLightMat.color = new Color(0.48f,0.35f,0.18f);
            Material redMat      = MakeMat(null,0.16f); redMat.color     = new Color(0.84f,0.12f,0.12f);
            Material darkRedMat  = MakeMat(null,0.12f); darkRedMat.color = new Color(0.56f,0.08f,0.08f);
            Material sakuraMat   = MakeMat(null,0.08f); sakuraMat.color  = new Color(1.00f,0.74f,0.86f);
            Material wisteriaM   = MakeMat(null,0.08f); wisteriaM.color  = new Color(0.62f,0.40f,0.82f);
            Material pathMat     = MakeMat(GenNoise(new Color(0.64f,0.60f,0.52f), new Color(0.52f,0.48f,0.40f), 26f), 0.12f);
            Material paperMat    = MakeMat(null,0.04f); paperMat.color   = new Color(0.98f,0.95f,0.84f);
            Material bambooMat   = MakeMat(null,0.14f); bambooMat.color  = new Color(0.44f,0.70f,0.24f);
            Material sandMat     = MakeMat(GenNoise(new Color(0.84f,0.80f,0.66f), new Color(0.72f,0.68f,0.56f), 12f), 0.04f);
            Material waterMat    = MakeMat(null,0.88f); waterMat.color   = new Color(0.26f,0.56f,0.76f,0.82f);
            Material goldMat     = MakeMat(null,0.88f); goldMat.color    = new Color(1.00f,0.84f,0.26f);
            Material copperAgedM = MakeMat(null,0.55f); copperAgedM.color= new Color(0.36f,0.60f,0.40f);
            Material indigoMat   = MakeMat(null,0.12f); indigoMat.color  = new Color(0.12f,0.16f,0.38f);
            Material whiteMat    = MakeMat(null,0.08f); whiteMat.color   = Color.white;
            Material charcoalMat = MakeMat(null,0.05f); charcoalMat.color= new Color(0.10f,0.10f,0.12f);
            Material tileGrayM   = MakeMat(GenNoise(new Color(0.58f,0.58f,0.62f), new Color(0.44f,0.44f,0.48f), 22f), 0.35f);
            Material metalMat    = MakeMat(null,0.78f); metalMat.color   = new Color(0.66f,0.71f,0.76f);

            // ── Yamato Lighting ────────────────────────────────────────────────────
            //  We don't change the global directional here (player can only be in
            //  one realm at a time; the JapanPortal script handles the crossfade).
            //  Instead we seed many warm point-lights throughout the realm.

            // ── Yamato Terrain ─────────────────────────────────────────────────────
            //  FIVE-LAYER DEPTH for Yamato:
            //   1. Soft sky haze (particles)
            //   2. Misty mountain backdrop
            //   3. Bamboo grove walls / forested hillsides
            //   4. Shrine complex and village on gentle hill
            //   5. Stone-paved paths, koi pond, gardens at ground level

            // Ground plane — warm grass
            CreateBlock("Yamato Ground",   O + new Vector3(0,-0.50f,0), new Vector3(240,1.0f,240), grassMat, worldRoot);
            CreateBlock("Yamato Subsoil",  O + new Vector3(0,-1.20f,0), new Vector3(240,1.0f,240), mossMat,  worldRoot);

            // Shrine hill — gentle asymmetric rise
            CreateBlock("Shrine Hill Base",  O + new Vector3( 0, 1.2f, 72f), new Vector3(58,2.5f,70), grassMat, worldRoot);
            CreateBlock("Shrine Hill Crest", O + new Vector3( 0, 2.8f, 75f), new Vector3(38,2.0f,48), grassMat, worldRoot);

            // Distant misty mountains (Layer 2) — six peaks, varied height
            float[] mxArr = { -130f,-80f,-30f,30f,80f,130f };
            float[] mhArr = {   22f, 35f, 28f,32f,26f, 30f };
            for (int i = 0; i < 6; i++)
            {
                var mt = CreateBlock("Mountain "+i,
                    O + new Vector3(mxArr[i], mhArr[i], 115f),
                    new Vector3(38,mhArr[i]*2,32), stoneMat, worldRoot);
                mt.transform.rotation = Quaternion.Euler(0,Random.Range(-10,10),0);
                // Snow cap
                CreateBlock("Mtn Snow "+i,
                    O + new Vector3(mxArr[i], mhArr[i]*2+mhArr[i]*0.5f, 115f),
                    new Vector3(20,mhArr[i]*0.6f,18), whiteMat, worldRoot);
            }

            // Realm borders
            CreateBlock("Y West Cliff",  O+new Vector3(-115f, 6f,   0), new Vector3(10,24,240), stoneMat, worldRoot);
            CreateBlock("Y East Cliff",  O+new Vector3( 115f, 6f,   0), new Vector3(10,24,240), stoneMat, worldRoot);
            CreateBlock("Y South Wall",  O+new Vector3(    0, 6f,-115f), new Vector3(240,24,10),stoneMat, worldRoot);
            CreateBlock("Y North Wall",  O+new Vector3(    0, 6f, 115f), new Vector3(240,24,10),stoneMat, worldRoot);

            // Invisible play bounds
            GameObject yBounds = new GameObject("Yamato Boundaries"); yBounds.transform.SetParent(worldRoot);
            CreateInvisibleWall("YN", O+new Vector3(   0,5f, 88f),new Vector3(190,22,2), yBounds.transform);
            CreateInvisibleWall("YS", O+new Vector3(   0,5f,-88f),new Vector3(190,22,2), yBounds.transform);
            CreateInvisibleWall("YW", O+new Vector3(-98f,5f,   0),new Vector3(2,22,190), yBounds.transform);
            CreateInvisibleWall("YE", O+new Vector3( 98f,5f,   0),new Vector3(2,22,190), yBounds.transform);

            // Ground surface variety
            for (int i = 0; i < 60; i++)
            {
                var patch = CreateBlock("Grass Patch",
                    O+new Vector3(Random.Range(-100f,100f),0.02f,Random.Range(-100f,100f)),
                    new Vector3(Random.Range(2f,8f),0.04f,Random.Range(2f,7f)), mossMat, worldRoot);
                patch.transform.rotation = Quaternion.Euler(0,Random.Range(0,360),0);
            }

            // ── Dry Moat Castle Approach (south entry) ────────────────────────────
            CreateDryMoatApproach(O, worldRoot, stoneMat, darkStone, woodMat, redMat, pathMat);

            // ── Torii Gate Procession (Fushimi Inari-style) ───────────────────────
            //  Rule of Thirds: procession occupies the central third of the Z axis
            for (int i = 0; i < 10; i++)
                CreateToriiGate("Torii "+i, O+new Vector3(0,0f,(i*9f)-14f), redMat, woodMat, worldRoot);

            // ── Stone-paved main path ──────────────────────────────────────────────
            for (int i = 0; i < 24; i++)
            {
                var slab = CreateBlock("Stone Slab",
                    O+new Vector3(0,0.08f,(i*4.8f)-8f),
                    new Vector3(5f,0.15f,4.2f), pathMat, worldRoot);
                slab.transform.rotation = Quaternion.Euler(0,Random.Range(-4,4),0);
            }

            // ── Stone Lanterns flanking path ──────────────────────────────────────
            for (int i = 0; i < 20; i++)
            {
                float z = (i*5.2f)-12f;
                CreateStoneLantern(O+new Vector3(-5.2f,0,z), worldRoot, stoneMat);
                CreateStoneLantern(O+new Vector3( 5.2f,0,z), worldRoot, stoneMat);
            }

            // ── Shrine Approach: Wide Stone Steps ─────────────────────────────────
            for (int i = 0; i < 12; i++)
                CreateBlock("Shrine Step "+i,
                    O+new Vector3(0,(i*0.30f)+0.6f,56f+(i*0.55f)),
                    new Vector3(12f,0.30f,0.60f), stoneMat, worldRoot);

            // ── Shrine Complex on the Hill ─────────────────────────────────────────
            CreateMainShrine(    O+new Vector3(  0f,5f,76f), worldRoot, redMat,darkRedMat,woodMat,goldMat,stoneMat,pathMat);
            CreatePagoda(        O+new Vector3( 26f,5f,76f), worldRoot, redMat,darkRedMat,woodMat,tileGrayM);
            CreateBellTower(     O+new Vector3(-26f,5f,76f), worldRoot, woodMat,goldMat,stoneMat);
            CreateNohTheatre(    O+new Vector3( -8f,5f,92f), worldRoot, woodMat,woodLightMat,stoneMat,redMat);
            CreateButsudanAltar( O+new Vector3(  8f,5f,92f), worldRoot, woodMat,goldMat,redMat,paperMat);

            // ── Armoury (midnight black, north-east of shrine hill) ────────────────
            CreateArmoury(O+new Vector3(44f,5f,78f), worldRoot, charcoalMat,darkStone,metalMat,redMat);

            // ── Kyudojo — Ceremonial Archery Range ────────────────────────────────
            CreateKyudojo(O+new Vector3(52f,0f,20f), worldRoot, woodMat,stoneMat,pathMat,indigoMat);

            // ── Koi Pond with waterfall cascade ───────────────────────────────────
            CreateKoiPond(O+new Vector3(-30f,0,18f), worldRoot, stoneMat,waterMat,goldMat);
            CreateWaterfallCascade(O+new Vector3(-42f,0,22f), worldRoot, waterMat,stoneMat,mossMat);

            // ── Zen Garden ────────────────────────────────────────────────────────
            CreateZenGarden(O+new Vector3(30f,0,15f), worldRoot, stoneMat,sandMat);

            // ── Gorintō Stupa Field (stone memorial towers, layer of dread) ────────
            CreateGorintoStupaField(O+new Vector3(-55f,0,55f), worldRoot, stoneMat, darkStone, mossMat);

            // ── Ornamental Bridge over the stream ─────────────────────────────────
            CreateOrnamentalBridge(O+new Vector3(-10f,0,-20f), worldRoot, redMat,woodMat,stoneMat,waterMat);

            // ── Watermill on the stream ────────────────────────────────────────────
            CreateWatermill(O+new Vector3(-30f,0,-25f), worldRoot, woodMat,stoneMat,waterMat);

            // ── Wisteria Arcade (visual corridor, Layer 3 depth cue) ──────────────
            CreateWisteriaArcade(O+new Vector3(20f,0,40f), worldRoot, woodMat,wisteriaM,stoneMat);

            // ── Lantern Festival Pennants across the path ──────────────────────────
            CreateFestivalPennants(O, worldRoot, redMat,goldMat,woodMat,paperMat);

            // ── Tea House ─────────────────────────────────────────────────────────
            CreateTeaHouse(O+new Vector3(-22f,0,32f), worldRoot, woodMat,paperMat,redMat,stoneMat);

            // ── Bamboo Groves ─────────────────────────────────────────────────────
            for (int i = 0; i < 20; i++)
            {
                Vector3 bp = O+new Vector3(Random.Range(22f,92f),0,Random.Range(-80f,80f));
                if (Mathf.Abs(bp.z-O.z) > 4f) CreateBambooClump(bp, worldRoot, bambooMat);
            }
            for (int i = 0; i < 20; i++)
            {
                Vector3 bp = O+new Vector3(Random.Range(-92f,-22f),0,Random.Range(-80f,80f));
                if (Mathf.Abs(bp.z-O.z) > 4f) CreateBambooClump(bp, worldRoot, bambooMat);
            }

            // ── Sakura Trees ──────────────────────────────────────────────────────
            for (int i = 0; i < 50; i++)
            {
                Vector3 tp = O+new Vector3(Random.Range(-108f,108f),0.5f,Random.Range(-108f,108f));
                if (Mathf.Abs(tp.x-O.x) > 5f) CreateSakuraTree(tp, worldRoot, sakuraMat, woodMat);
            }

            // ── Bamboo Perimeter Fence ─────────────────────────────────────────────
            for (int i=0;i<26;i++) CreateBambooFenceSection(O+new Vector3(-65f+i*5f,0,-65f), worldRoot,bambooMat,woodMat,false);
            for (int i=0;i<26;i++) CreateBambooFenceSection(O+new Vector3(-65f+i*5f,0, 65f), worldRoot,bambooMat,woodMat,false);
            for (int i=0;i<26;i++) CreateBambooFenceSection(O+new Vector3(-65f,0,-65f+i*5f), worldRoot,bambooMat,woodMat,true);
            for (int i=0;i<26;i++) CreateBambooFenceSection(O+new Vector3( 65f,0,-65f+i*5f), worldRoot,bambooMat,woodMat,true);

            // Ninja rooftop path cables (rope-climb shortcuts above key buildings)
            CreateNinjaRooftopPaths(O, worldRoot, charcoalMat, woodMat);

            // ── Cherry Blossom & Morning Mist Weather ─────────────────────────────
            CreateCherryBlossomWeather(O, worldRoot);
            CreateMorningMist(O, worldRoot);

            // ── Yamato Wildlife ───────────────────────────────────────────────────
            Material deerMat   = MakeMat(null,0.12f); deerMat.color  = new Color(0.64f,0.46f,0.28f);
            Material craneMat  = MakeMat(null,0.12f); craneMat.color = Color.white;
            Material foxMatJ   = MakeMat(null,0.10f); foxMatJ.color  = new Color(0.90f,0.55f,0.20f); // red fox
            for (int i=0;i<4;i++) CreateAnimal("Deer",  O+new Vector3(Random.Range(-65f,65f),0.5f,Random.Range(-65f,65f)),new Vector3(0.7f,1.4f,1.6f),deerMat, 3.0f,worldRoot);
            for (int i=0;i<3;i++) CreateAnimal("Crane", O+new Vector3(Random.Range(-55f,55f),0.4f,Random.Range(-55f,55f)),new Vector3(0.6f,1.0f,0.4f),craneMat,2.5f,worldRoot);
            for (int i=0;i<2; i++) CreateAnimal("Red Fox",O+new Vector3(Random.Range(-50f,50f),0.4f,Random.Range(-50f,50f)),new Vector3(0.8f,1.0f,1.5f),foxMatJ, 4.0f,worldRoot);

            // ── Yamato Enemies ─────────────────────────────────────────────────────
            Material samuraiMat = MakeMat(null,0.28f); samuraiMat.color = new Color(0.12f,0.18f,0.32f);
            Material ninjaMat   = MakeMat(null,0.04f); ninjaMat.color   = new Color(0.04f,0.04f,0.05f);
            Material ronin      = MakeMat(null,0.15f); ronin.color      = new Color(0.38f,0.32f,0.24f); // wandering ronin
            Material oniMat     = MakeMat(null,0.08f); oniMat.color     = new Color(0.70f,0.08f,0.12f); // Oni demon
            Material yHitFlash  = MakeMat(null,0.00f); yHitFlash.color  = Color.white;

            for (int i=0;i<14;i++) CreateEnemy("Samurai", O+new Vector3(Random.Range(-48f,48f),1f,Random.Range(5f,68f)), samuraiMat,yHitFlash,worldRoot);
            for (int i=0;i<10;i++) CreateEnemy("Ninja",   O+new Vector3(Random.Range(-42f,42f),1f,Random.Range(5f,62f)), ninjaMat,  yHitFlash,worldRoot);
            for (int i=0;i< 6;i++) CreateEnemy("Ronin",   O+new Vector3(Random.Range(-55f,55f),1f,Random.Range(-20f,40f)),ronin,    yHitFlash,worldRoot);
            // Oni mini-boss at the shrine gate
            CreateEnemy("Oni Guardian", O+new Vector3(0f,1f,55f), oniMat, yHitFlash, worldRoot);

            // ── Return Portal → Frostheim ──────────────────────────────────────────
            CreatePortalGateway(
                "Return Portal (To Frostheim)",
                O+new Vector3(12f,0f,6f),
                worldRoot,
                darkRedMat,
                new Color(0.45f,0.78f,1.00f,0.58f),
                new Color(0.38f,0.80f,1.00f),
                new Color(0.78f,0.90f,1.00f),
                new Vector3(0,2f,-18f),
                isReturn: true);
        }


        // ╔══════════════════════════════════════════════════════════════════════════╗
        //  FROSTHEIM — NEW LANDMARK BUILDERS
        // ╚══════════════════════════════════════════════════════════════════════════╝

        private static void CreateDistantMountains(Transform parent,
            Material stone, Material snow, Material dark)
        {
            GameObject mRange = new GameObject("Distant Mountain Range");
            mRange.transform.SetParent(parent);

            // North-west arc of peaks (silhouette layer, outside play area)
            float[] mx = { -180f,-130f,-80f,-30f,20f,70f,120f,170f };
            float[] mz = {  180f, 200f,195f,188f,190f,185f,200f,178f };
            float[] mh = {   45f,  60f, 52f, 38f, 55f, 48f, 65f, 42f };
            for (int i = 0; i < 8; i++)
            {
                CreateBlock("Peak "+i, new Vector3(mx[i],mh[i],mz[i]),
                    new Vector3(50,mh[i]*2,45), stone, mRange.transform);
                CreateBlock("Snow Cap "+i, new Vector3(mx[i],mh[i]*2+mh[i]*0.35f,mz[i]),
                    new Vector3(26,mh[i]*0.55f,22), snow, mRange.transform);
            }
            // South wall mountain glimpse through the fjord cut
            CreateBlock("South Peak", new Vector3(0,-5f,-240f), new Vector3(80,60,40), dark, mRange.transform);
        }

        private static void CreateStratifiedCliff(string name, Vector3 pos, Vector3 size,
            Material a, Material b, Transform parent, bool horizontal)
        {
            // Simulate geological strata with alternating material blocks
            int layers = 6;
            for (int i = 0; i < layers; i++)
            {
                float t = (float)i / layers;
                Vector3 layerSize = horizontal
                    ? new Vector3(size.x, size.y / layers, size.z)
                    : new Vector3(size.x, size.y, size.z / layers);
                Vector3 layerPos = horizontal
                    ? pos + new Vector3(0, (i * size.y / layers) - size.y * 0.5f, 0)
                    : pos + new Vector3(0, 0, (i * size.z / layers) - size.z * 0.5f);
                CreateBlock(name + "_L" + i, layerPos, layerSize,
                    i % 2 == 0 ? a : b, parent);
            }
        }

        private static void CreateFrozenWaterfall(Vector3 pos, Transform parent,
            Material ice, Material iceDeep, Material stone)
        {
            GameObject wf = new GameObject("Frozen Waterfall");
            wf.transform.SetParent(parent);
            wf.transform.position = pos;

            // Cliff face behind the waterfall
            CreateBlock("Cliff Back",  pos+new Vector3(0,10f,3f),  new Vector3(12f,24f,4f),  stone,   wf.transform);
            CreateBlock("Cliff Left",  pos+new Vector3(-8f,8f,0),  new Vector3(6f,20f,8f),   stone,   wf.transform);
            CreateBlock("Cliff Right", pos+new Vector3( 8f,8f,0),  new Vector3(6f,20f,8f),   stone,   wf.transform);

            // Frozen ice columns — main fall
            for (int i = 0; i < 5; i++)
            {
                float ox = -2.5f + i * 1.2f;
                float h  = Random.Range(12f, 20f);
                var ic = CreateBlock("Ice Column "+i,
                    pos+new Vector3(ox, h*0.5f, 0),
                    new Vector3(Random.Range(0.6f,1.4f), h, Random.Range(0.6f,1.1f)),
                    i%2==0 ? ice : iceDeep, wf.transform);
                ic.transform.rotation = Quaternion.Euler(Random.Range(-3,3),0,Random.Range(-4,4));
            }

            // Splash pool at the base — cracked ice
            CreateBlock("Splash Pool",  pos+new Vector3(0,-0.35f,-2f), new Vector3(10f,0.15f,5f), ice,     wf.transform);
            CreateBlock("Pool Crack 1", pos+new Vector3(2f,-0.28f,-2f),new Vector3(0.12f,0.1f,4f),iceDeep, wf.transform);
            CreateBlock("Pool Crack 2", pos+new Vector3(-1f,-0.28f,-1f),new Vector3(0.1f,0.1f,3f),iceDeep, wf.transform);

            // Mist particle at base
            GameObject mist = new GameObject("Waterfall Mist");
            mist.transform.SetParent(wf.transform);
            mist.transform.position = pos+new Vector3(0,1f,-1f);
            var ps = mist.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startColor   = new Color(0.85f,0.90f,0.98f,0.4f);
            m.startSize    = new ParticleSystem.MinMaxCurve(0.4f,1.2f);
            m.startLifetime= 3.5f;
            m.startSpeed   = new ParticleSystem.MinMaxCurve(0.2f,0.8f);
            m.maxParticles = 80;
            var em = ps.emission; em.rateOverTime = 18f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(8f,0.5f,1f);
            mist.GetComponent<ParticleSystemRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));

            // Cold glow light
            AddPointLight(wf.transform, new Vector3(0,4f,0),
                new Color(0.55f,0.80f,1.0f), 2.5f, 10f, "Waterfall Glow");
        }

        private static void CreateFrozenLake(Vector3 pos, Transform parent,
            Material ice, Material iceDeep, Material stone, Material snow)
        {
            GameObject lake = new GameObject("Frozen Lake");
            lake.transform.SetParent(parent);

            // Main surface
            CreateBlock("Lake Surface",    pos+new Vector3(0,-0.40f,0),  new Vector3(34f,0.15f,28f), ice,     lake.transform);
            // Deep-water visible sections under thin ice
            CreateBlock("Deep Ice 1",      pos+new Vector3(-8f,-0.45f,5f),new Vector3(10f,0.05f,8f), iceDeep,lake.transform);
            CreateBlock("Deep Ice 2",      pos+new Vector3( 7f,-0.45f,-3f),new Vector3(7f,0.05f,6f), iceDeep,lake.transform);
            // Crack lines
            for (int i=0; i<6; i++)
            {
                var crack = CreateBlock("Ice Crack "+i,
                    pos+new Vector3(Random.Range(-14f,14f),-0.36f,Random.Range(-11f,11f)),
                    new Vector3(Random.Range(3f,10f),0.05f,0.08f), iceDeep, lake.transform);
                crack.transform.rotation = Quaternion.Euler(0,Random.Range(0,180),0);
            }
            // Stone shore edges
            CreateBlock("Shore W",  pos+new Vector3(-18f,-0.20f,0),  new Vector3(4f,0.40f,28f), stone, lake.transform);
            CreateBlock("Shore E",  pos+new Vector3( 18f,-0.20f,0),  new Vector3(4f,0.40f,28f), stone, lake.transform);
            CreateBlock("Shore N",  pos+new Vector3(0,-0.20f,  15f), new Vector3(34f,0.40f,4f), stone, lake.transform);
            CreateBlock("Shore S",  pos+new Vector3(0,-0.20f, -15f), new Vector3(34f,0.40f,4f), stone, lake.transform);
            // Snow rim on shores
            CreateBlock("Snow W",   pos+new Vector3(-20f,0.10f,0), new Vector3(2f,0.3f,30f), snow, lake.transform);
            CreateBlock("Snow E",   pos+new Vector3( 20f,0.10f,0), new Vector3(2f,0.3f,30f), snow, lake.transform);
        }

        private static void CreateFjordValleyVista(Vector3 pos, Transform parent,
            Material water, Material stone, Material snow)
        {
            // A gap cut into the south wall with a glimpse of a fjord below
            GameObject fjord = new GameObject("Fjord Vista");
            fjord.transform.SetParent(parent);

            CreateBlock("Fjord Water",  pos+new Vector3(0,-8f,0),   new Vector3(40f,1f,20f), water, fjord.transform);
            CreateBlock("Fjord L Wall", pos+new Vector3(-20f,4f,0), new Vector3(6f,18f,20f), stone, fjord.transform);
            CreateBlock("Fjord R Wall", pos+new Vector3( 20f,4f,0), new Vector3(6f,18f,20f), stone, fjord.transform);
            CreateBlock("Fjord Snow L", pos+new Vector3(-20f,14f,0),new Vector3(6f,4f,20f),  snow,  fjord.transform);
            CreateBlock("Fjord Snow R", pos+new Vector3( 20f,14f,0),new Vector3(6f,4f,20f),  snow,  fjord.transform);
        }

        private static void CreateWatchTower(Vector3 pos, Transform parent,
            Material wood, Material stone, Material metal, Material banner)
        {
            GameObject tower = new GameObject("Watch Tower");
            tower.transform.SetParent(parent);

            // Stone base
            CreateBlock("Tower Base",    pos+new Vector3(0,1.5f,0),  new Vector3(5f,3f,5f),   stone, tower.transform);
            // Wooden upper section
            CreateBlock("Tower Upper",   pos+new Vector3(0,6f,0),    new Vector3(4.5f,6f,4.5f), wood, tower.transform);
            // Battlements (4 corner merlons)
            foreach (Vector3 c in new[]{new Vector3(-1.8f,10f,-1.8f),new Vector3(1.8f,10f,-1.8f),
                                         new Vector3(-1.8f,10f, 1.8f),new Vector3(1.8f,10f, 1.8f)})
                CreateBlock("Merlon", pos+c, new Vector3(1.2f,2f,1.2f), stone, tower.transform);
            // Walkway floor
            CreateBlock("Walkway",       pos+new Vector3(0,9.2f,0),  new Vector3(5f,0.4f,5f), wood, tower.transform);
            // Arrow slit windows
            CreateBlock("Slit N",        pos+new Vector3(0,7f,2.28f),new Vector3(0.4f,1f,0.1f), stone, tower.transform);
            CreateBlock("Slit E",        pos+new Vector3(2.28f,7f,0),new Vector3(0.1f,1f,0.4f), stone, tower.transform);
            // Roof — conical approximation (4 slanted panels)
            CreateBlock("Roof N",  pos+new Vector3(0f,11.5f, 1.5f), new Vector3(5f,0.5f,4f), wood, tower.transform).transform.rotation = Quaternion.Euler(-30,0,0);
            CreateBlock("Roof S",  pos+new Vector3(0f,11.5f,-1.5f), new Vector3(5f,0.5f,4f), wood, tower.transform).transform.rotation = Quaternion.Euler( 30,0,0);
            CreateBlock("Roof E",  pos+new Vector3( 1.5f,11.5f,0), new Vector3(4f,0.5f,5f), wood, tower.transform).transform.rotation = Quaternion.Euler(0,0, 30);
            CreateBlock("Roof W",  pos+new Vector3(-1.5f,11.5f,0), new Vector3(4f,0.5f,5f), wood, tower.transform).transform.rotation = Quaternion.Euler(0,0,-30);
            // Torch at the top
            AddPointLight(tower.transform, new Vector3(0,12f,0), new Color(1f,0.5f,0.1f), 4f,14f,"Tower Torch");
            // Banner
            CreateBlock("Flag Pole",    pos+new Vector3(0,13f,0), new Vector3(0.2f,4f,0.2f), wood,   tower.transform);
            CreateBlock("Banner",       pos+new Vector3(1.2f,14.5f,0),new Vector3(2.5f,3.5f,0.1f), banner, tower.transform);
            // Access ladder
            for (int i=0; i<8; i++)
                CreateBlock("Rung "+i, pos+new Vector3(2.6f,i*1.1f+1f,0), new Vector3(0.6f,0.1f,0.1f), wood, tower.transform);
        }

        private static void CreateSkullPost(Vector3 pos, Transform parent,
            Material wood, Material bones)
        {
            CreateBlock("Skull Post", pos+new Vector3(0,1.5f,0), new Vector3(0.22f,3f,0.22f), wood, parent);
            // Skull approximation — stacked cubes
            CreateBlock("Skull Body", pos+new Vector3(0,3.2f,0), new Vector3(0.5f,0.45f,0.5f), bones, parent);
            CreateBlock("Skull Jaw",  pos+new Vector3(0,2.85f,0), new Vector3(0.4f,0.20f,0.4f), bones, parent);
            // Crossbar with two more skulls
            CreateBlock("Crossbar",   pos+new Vector3(0,2.5f,0), new Vector3(2.5f,0.15f,0.15f), wood, parent);
            CreateBlock("Skull L",    pos+new Vector3(-1.15f,2.7f,0), new Vector3(0.4f,0.4f,0.4f), bones, parent);
            CreateBlock("Skull R",    pos+new Vector3( 1.15f,2.7f,0), new Vector3(0.4f,0.4f,0.4f), bones, parent);
        }

        private static void CreateBurialShipOnHill(Vector3 pos, Transform parent,
            Material wood, Material metal, Material ash)
        {
            // The chieftain's send-off: a longship set alight on a burial hill
            GameObject ship = new GameObject("Chieftain Burial Ship");
            ship.transform.SetParent(parent);
            ship.transform.rotation = Quaternion.Euler(0, 30, 0);

            CreateBlock("Hull",       pos+new Vector3(0,0.6f, 0),  new Vector3(3f, 1f, 12f), wood,   ship.transform);
            CreateBlock("Hull L",     pos+new Vector3(-2f,1.4f, 0),new Vector3(0.5f,1.2f,10f),wood,  ship.transform);
            CreateBlock("Hull R",     pos+new Vector3( 2f,1.4f, 0),new Vector3(0.5f,1.2f,10f),wood,  ship.transform);
            CreateBlock("Char Deck",  pos+new Vector3(0,1.1f, 0),  new Vector3(3f,0.2f,10f),  ash,   ship.transform);
            // Dragon-head prow (torched, charred)
            var prow = CreateBlock("Prow",pos+new Vector3(0,2.2f,6f),new Vector3(0.5f,3.5f,0.5f),wood,ship.transform);
            prow.transform.rotation = Quaternion.Euler(-20,30,0);
            // Broken mast
            CreateBlock("Mast Stub",  pos+new Vector3(0,3.5f,0),   new Vector3(0.3f,3f,0.3f), wood,  ship.transform);
            CreateBlock("Fallen Mast",pos+new Vector3(2f,2.2f,1f), new Vector3(0.3f,0.3f,7f), wood,  ship.transform);
            // Cremation fire particles
            GameObject fire = new GameObject("Burial Fire");
            fire.transform.SetParent(ship.transform);
            fire.transform.position = pos+new Vector3(0,2f,0);
            var ps = fire.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startColor   = new Color(1f,0.35f,0.05f);
            m.startSize    = new ParticleSystem.MinMaxCurve(0.3f,0.9f);
            m.startLifetime= 3f;
            m.startSpeed   = new ParticleSystem.MinMaxCurve(1f,3f);
            m.maxParticles = 80;
            var emission = ps.emission;
            emission.rateOverTime = 22f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(4f,0.5f,10f);
            fire.GetComponent<ParticleSystemRenderer>().sharedMaterial =
                new Material(Shader.Find("Sprites/Default")) { color = new Color(1f,0.4f,0.1f,0.6f) };
            AddPointLight(ship.transform, new Vector3(0,3f,0), new Color(1f,0.42f,0.08f), 5f,18f,"Burial Fire Glow");
        }

        private static void CreateMeadery(Vector3 pos, Transform parent,
            Material stone, Material wood, Material woodDark, Material gold)
        {
            GameObject mead = new GameObject("Meadery (Cellar Entrance)");
            mead.transform.SetParent(parent);

            // Small above-ground building
            CreateBlock("Mead Building",   pos+new Vector3(0,2f,0),   new Vector3(9f,4f,7f),  woodDark, mead.transform);
            CreateBlock("Mead Roof L",     pos+new Vector3(-2.2f,5f,0),new Vector3(5f,0.4f,8f),stone,   mead.transform).transform.rotation = Quaternion.Euler(0,0,35);
            CreateBlock("Mead Roof R",     pos+new Vector3( 2.2f,5f,0),new Vector3(5f,0.4f,8f),stone,   mead.transform).transform.rotation = Quaternion.Euler(0,0,-35);
            // Sign board
            CreateBlock("Mead Sign",       pos+new Vector3(0,4.5f,-4f),new Vector3(3f,1f,0.2f),wood,   mead.transform);
            // Ale barrels outside
            for (int i=0;i<4;i++)
                CreateAleBarel(pos+new Vector3(-3.5f+i*2.2f, 0.6f, -4.5f), mead.transform, wood, stone);
            // Underground hatch in floor
            CreateBlock("Cellar Hatch",    pos+new Vector3(0,0.2f,-2f),new Vector3(2f,0.15f,1.5f),woodDark,mead.transform);
            // Warm golden glow from inside
            AddPointLight(mead.transform, new Vector3(0,1.5f,0), new Color(1f,0.72f,0.28f), 4f,10f,"Meadery Glow");
        }

        private static void CreateAleBarel(Vector3 pos, Transform parent, Material wood, Material metal)
        {
            // Barrel — cylinder approximated with cube + ring bands
            CreateBlock("Barrel Body", pos, new Vector3(1.0f,1.2f,1.0f), wood, parent);
            CreateBlock("Band Top",    pos+new Vector3(0,0.45f,0), new Vector3(1.12f,0.12f,1.12f), metal, parent);
            CreateBlock("Band Mid",    pos+new Vector3(0,0,0),     new Vector3(1.15f,0.12f,1.15f), metal, parent);
            CreateBlock("Band Bot",    pos+new Vector3(0,-0.45f,0),new Vector3(1.12f,0.12f,1.12f), metal, parent);
        }

        private static void CreateMassGravePit(Vector3 pos, Transform parent,
            Material mud, Material bones, Material stone)
        {
            GameObject pit = new GameObject("Mass Grave Pit");
            pit.transform.SetParent(parent);

            CreateBlock("Pit Floor",  pos+new Vector3(0,-1.5f,0), new Vector3(12f,0.5f,9f), mud, pit.transform);
            CreateBlock("Pit Wall N", pos+new Vector3(0,-0.5f,4.5f),new Vector3(12f,2f,0.5f),stone,pit.transform);
            CreateBlock("Pit Wall S", pos+new Vector3(0,-0.5f,-4.5f),new Vector3(12f,2f,0.5f),stone,pit.transform);
            CreateBlock("Pit Wall W", pos+new Vector3(-6f,-0.5f,0),new Vector3(0.5f,2f,9f),  stone,pit.transform);
            CreateBlock("Pit Wall E", pos+new Vector3( 6f,-0.5f,0),new Vector3(0.5f,2f,9f),  stone,pit.transform);
            // Scattered bones
            for (int i=0;i<20;i++)
            {
                var b = CreateBlock("Bone "+i,
                    pos+new Vector3(Random.Range(-5f,5f),-1.2f,Random.Range(-3.5f,3.5f)),
                    new Vector3(Random.Range(0.15f,0.8f),0.1f,Random.Range(0.5f,2f)),
                    bones, pit.transform);
                b.transform.rotation = Quaternion.Euler(Random.Range(-10,10),Random.Range(0,360),Random.Range(-10,10));
            }
            // Skull pile
            for (int i=0;i<8;i++)
                CreateBlock("Skull",
                    pos+new Vector3(Random.Range(-4f,4f),-0.9f+i*0.15f,Random.Range(-3f,3f)),
                    new Vector3(0.5f,0.4f,0.5f), bones, pit.transform);
        }

        private static void CreateCollapsedBridge(Vector3 pos, Transform parent,
            Material stone, Material wood, Material rust)
        {
            GameObject bridge = new GameObject("Collapsed Bridge");
            bridge.transform.SetParent(parent);

            // Surviving west pier
            CreateBlock("West Pier",   pos+new Vector3(-6f,2f,0),  new Vector3(3f,5f,2.5f), stone, bridge.transform);
            CreateBlock("West Arch",   pos+new Vector3(-6f,5f,0),  new Vector3(3f,1f,2.5f), stone, bridge.transform);
            // Collapsed east section
            var fallen = CreateBlock("Fallen Span",pos+new Vector3(1f,-0.5f,0),new Vector3(10f,0.6f,2.5f),wood,bridge.transform);
            fallen.transform.rotation = Quaternion.Euler(-22f,0,8f);
            // Rubble
            for (int i=0;i<10;i++)
            {
                var r = CreateBlock("Rubble "+i,
                    pos+new Vector3(Random.Range(-4f,6f),Random.Range(-0.5f,0.5f),Random.Range(-1f,1f)),
                    new Vector3(Random.Range(0.5f,2f),Random.Range(0.3f,1f),Random.Range(0.5f,1.5f)),
                    i%2==0?stone:wood, bridge.transform);
                r.transform.rotation = Quaternion.Euler(Random.Range(-40f,40f),Random.Range(0,360),Random.Range(-40f,40f));
            }
            // Rusted iron chains dangling
            for (int i=0;i<3;i++)
                CreateBlock("Chain "+i,
                    pos+new Vector3(-5f+i*1f,3f+i*-0.3f,0),
                    new Vector3(0.12f,2.5f,0.12f), rust, bridge.transform).transform.rotation = Quaternion.Euler(0,0,Random.Range(-25f,25f));
        }

        private static void CreateCollapsedHut(Vector3 pos, Transform parent,
            Material charred, Material ash, Material wood)
        {
            GameObject hut = new GameObject("Collapsed Burned Hut");
            hut.transform.SetParent(parent);

            // Partial walls
            CreateBlock("Wall Stub N", pos+new Vector3(0,1.2f,2.5f),  new Vector3(5f,2.5f,0.5f), charred, hut.transform);
            CreateBlock("Wall Stub W", pos+new Vector3(-2.5f,0.8f,0), new Vector3(0.5f,1.8f,5f), charred, hut.transform);
            // Collapsed roof debris
            var r1 = CreateBlock("Roof Debris",pos+new Vector3(0.5f,0.8f,0), new Vector3(5f,0.4f,5f), charred, hut.transform);
            r1.transform.rotation = Quaternion.Euler(-28f,15f,0);
            // Ash bed on floor
            CreateBlock("Ash Floor",   pos+new Vector3(0,0.05f,0),    new Vector3(5.5f,0.1f,5.5f),ash,    hut.transform);
            // Smoldering embers
            AddPointLight(hut.transform, new Vector3(0.5f,0.8f,0), new Color(1f,0.35f,0.05f), 1.5f,4f,"Smoldering Ember");
        }

        private static void CreateStorageShed(Vector3 pos, Transform parent,
            Material wood, Material thatch)
        {
            GameObject shed = new GameObject("Storage Shed");
            shed.transform.SetParent(parent);
            shed.transform.rotation = Quaternion.Euler(0,18,0);
            CreateBlock("Shed Walls",  pos+new Vector3(0,1f,0),   new Vector3(4f,2f,3f),   wood,   shed.transform);
            CreateBlock("Shed Roof L", pos+new Vector3(-1.2f,2.6f,0),new Vector3(3f,0.4f,3.5f),thatch,shed.transform).transform.rotation = Quaternion.Euler(0,0, 30);
            CreateBlock("Shed Roof R", pos+new Vector3( 1.2f,2.6f,0),new Vector3(3f,0.4f,3.5f),thatch,shed.transform).transform.rotation = Quaternion.Euler(0,0,-30);
        }

        private static void CreateVillagePaths(Transform parent, Material mud, Material path)
        {
            // Spoke paths radiating from the great hall to key locations
            // Each path: a series of offset slabs
            float[,] spokes = {
                {0,0,-12f,  0,0,-40f},     // South to entrance
                {0,0, 12f,  0,0, 38f},     // North to altar
                {-5f,0,0,  -28f,0,0},      // West to meadery
                { 5f,0,0,   28f,0,0},      // East to forge
                {0,0,0,    52f,0,12f},      // NE to lake
            };
            for (int s=0; s<5; s++)
            {
                Vector3 start = new Vector3(spokes[s,0],spokes[s,1],spokes[s,2]);
                Vector3 end   = new Vector3(spokes[s,3],spokes[s,4],spokes[s,5]);
                int steps = Mathf.RoundToInt(Vector3.Distance(start,end)/3.5f);
                for (int i=0; i<steps; i++)
                {
                    Vector3 p = Vector3.Lerp(start,end,(float)i/steps);
                    var slab = CreateBlock("Path Slab",
                        new Vector3(p.x+Random.Range(-0.3f,0.3f), 0.06f, p.z+Random.Range(-0.3f,0.3f)),
                        new Vector3(3.5f,0.12f,3.2f), i%4==0?mud:path, parent);
                    slab.transform.rotation = Quaternion.Euler(0,Random.Range(-8f,8f),0);
                }
            }
        }

        private static void CreateCampfireSafeZone(Vector3 pos, Transform parent,
            Material stone, Material wood)
        {
            GameObject zone = new GameObject("Campfire Safe Zone");
            zone.tag  = "Campfire";
            zone.transform.position = pos;
            zone.transform.SetParent(parent);

            // Fire pit stone ring
            CreateBlock("Fire Pit Base", Vector3.zero, new Vector3(2.8f,0.35f,2.8f), stone, zone.transform);
            for (int i=0;i<8;i++)
            {
                float a = (i/8f)*Mathf.PI*2f;
                CreateBlock("Ring Stone "+i,
                    new Vector3(Mathf.Sin(a)*1.65f,0,Mathf.Cos(a)*1.65f),
                    new Vector3(0.45f,0.65f,0.45f), stone, zone.transform);
            }
            // Log seating around the fire (4 benches)
            for (int i=0;i<4;i++)
            {
                float a = i * 90f * Mathf.Deg2Rad;
                var log = CreateBlock("Seat Log "+i,
                    new Vector3(Mathf.Sin(a)*3.5f,0.3f,Mathf.Cos(a)*3.5f),
                    new Vector3(0.5f,0.5f,2.8f), wood, zone.transform);
                log.transform.rotation = Quaternion.Euler(0,i*90f,0);
            }
            // Fire glow
            AddPointLight(zone.transform, new Vector3(0,1.4f,0),
                new Color(1f,0.44f,0f), 7f, 12f, "Campfire Glow");
            // Heat zone trigger
            var heatZone = zone.AddComponent<BoxCollider>();
            heatZone.isTrigger = true;
            heatZone.size = new Vector3(9,5,9);
            // Flame particles
            GameObject flame = new GameObject("Campfire Flame");
            flame.transform.SetParent(zone.transform);
            flame.transform.position = pos + new Vector3(0,0.5f,0);
            var ps = flame.AddComponent<ParticleSystem>();
            var pm = ps.main;
            pm.startColor   = new Color(1f,0.45f,0.05f);
            pm.startSize    = new ParticleSystem.MinMaxCurve(0.3f,0.8f);
            pm.startLifetime= 1.5f;
            pm.startSpeed   = new ParticleSystem.MinMaxCurve(1.2f,2.5f);
            pm.maxParticles = 50;
            var emission = ps.emission;
            emission.rateOverTime = 18f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(1.5f,0.2f,1.5f);
            flame.GetComponent<ParticleSystemRenderer>().sharedMaterial =
                new Material(Shader.Find("Sprites/Default")){ color = new Color(1f,0.5f,0.1f,0.7f) };
        }

        private static void CreateGodraysOvercast(Transform parent, Color col)
        {
            // Volumetric "god-ray" shafts through overcast sky — point lights high up
            // angled downward to simulate scattering
            for (int i=0; i<6; i++)
            {
                var shaft = new GameObject("Godray Shaft "+i);
                shaft.transform.SetParent(parent);
                shaft.transform.position = new Vector3(Random.Range(-60f,60f), 28f, Random.Range(-60f,60f));
                var l = shaft.AddComponent<Light>();
                l.type       = LightType.Spot;
                l.color      = col;
                l.intensity  = 1.8f;
                l.range      = 40f;
                l.spotAngle  = 22f;
                l.shadows    = LightShadows.None;
                shaft.transform.rotation = Quaternion.Euler(Random.Range(80f,90f),Random.Range(0f,360f),0f);
                // Shaft visual — thin tall box
                CreateBlock("Ray Volume "+i, shaft.transform.position+new Vector3(0,-10f,0),
                    new Vector3(1.5f,20f,1.5f), MakeMat(null,0f) , parent).GetComponent<Renderer>().sharedMaterial.color =
                    new Color(col.r,col.g,col.b,0.04f);
            }
        }


        // ╔══════════════════════════════════════════════════════════════════════════╗
        //  FROSTHEIM — EXISTING LANDMARK BUILDERS (enhanced)
        // ╚══════════════════════════════════════════════════════════════════════════╝

        private static void CreateStoneCircle(Vector3 centre, float radius, int count,
            Material mat, Transform parent)
        {
            GameObject circle = new GameObject("Standing Stone Circle");
            circle.transform.position = centre;
            circle.transform.SetParent(parent);
            for (int i=0;i<count;i++)
            {
                float a = (i/(float)count)*Mathf.PI*2f;
                Vector3 p = centre+new Vector3(Mathf.Sin(a)*radius,0,Mathf.Cos(a)*radius);
                float h = Random.Range(3.5f,7.5f);
                var s = CreateBlock("Stone "+i, p,
                    new Vector3(Random.Range(0.9f,1.6f),h,Random.Range(0.9f,1.4f)),
                    mat, circle.transform);
                s.transform.rotation = Quaternion.Euler(
                    Random.Range(-8,8),Random.Range(0,360),Random.Range(-10,10));
            }
        }

        private static void CreateBurialMound(Vector3 pos, Transform parent,
            Material stone, Material snow)
        {
            GameObject mound = new GameObject("Burial Mound");
            mound.transform.position = pos;
            mound.transform.SetParent(parent);

            var body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Mound Body";
            body.transform.SetParent(mound.transform);
            body.transform.position   = pos+new Vector3(0,2.0f,0);
            body.transform.localScale = new Vector3(10f,4.2f,10f);
            body.GetComponent<Renderer>().sharedMaterial = snow;
            body.isStatic = true;

            // Stone lintel entrance
            CreateBlock("Post L",  pos+new Vector3(-1.3f,1.0f,4.8f),new Vector3(0.5f,2.8f,0.5f),stone,mound.transform);
            CreateBlock("Post R",  pos+new Vector3( 1.3f,1.0f,4.8f),new Vector3(0.5f,2.8f,0.5f),stone,mound.transform);
            CreateBlock("Lintel",  pos+new Vector3(    0,2.6f,4.8f),new Vector3(3.5f,0.5f,0.5f),stone,mound.transform);
            // Two small guard stones flanking
            CreateBlock("Guard L", pos+new Vector3(-2.5f,0.8f,5.5f),new Vector3(0.6f,1.8f,0.6f),stone,mound.transform);
            CreateBlock("Guard R", pos+new Vector3( 2.5f,0.8f,5.5f),new Vector3(0.6f,1.8f,0.6f),stone,mound.transform);
        }

        private static void CreateForge(Vector3 pos, Transform parent,
            Material stone, Material wood, Material metal, Material coal,
            Material rust, Material gold)
        {
            GameObject forge = new GameObject("Blacksmith Forge");
            forge.transform.position = pos;
            forge.transform.SetParent(parent);

            // Main forge structure
            CreateBlock("Forge Base",      pos+new Vector3(  0,0.90f,   0), new Vector3(4.5f,1.8f,3.5f), stone,  forge.transform);
            CreateBlock("Forge Chimney",   pos+new Vector3(  0,4.20f, 0.6f), new Vector3(1.4f,6.0f,1.4f), stone,  forge.transform);
            CreateBlock("Chimney Cap",     pos+new Vector3(  0,7.80f, 0.6f), new Vector3(1.8f,0.4f,1.8f), stone,  forge.transform);
            // Anvil + stand
            CreateBlock("Forge Anvil",     pos+new Vector3( 2.2f,1.80f,  0), new Vector3(1.1f,0.55f,0.65f),metal, forge.transform);
            CreateBlock("Anvil Stand",     pos+new Vector3( 2.2f,1.22f,  0), new Vector3(0.55f,0.88f,0.55f),wood, forge.transform);
            // Fuel bin + coal
            CreateBlock("Fuel Bin",        pos+new Vector3(-2.2f,0.90f,  0), new Vector3(1.6f,1.1f,1.3f),  wood,  forge.transform);
            CreateBlock("Coal Pile",       pos+new Vector3(-2.2f,1.55f,  0), new Vector3(1.4f,0.35f,1.1f),  coal,  forge.transform);
            // Weapon rack (3 swords + 2 axes)
            CreateBlock("Weapon Rack",     pos+new Vector3(-0.6f,1.6f,2.0f), new Vector3(3.0f,1.6f,0.35f), wood,  forge.transform);
            for (int i=0;i<3;i++)
                CreateBlock("Sword "+i,    pos+new Vector3(-0.9f+i*0.9f,2.2f,2.08f),new Vector3(0.12f,1.5f,0.12f),metal,forge.transform);
            for (int i=0;i<2;i++)
            {
                CreateBlock("Axe Haft "+i, pos+new Vector3(-1.5f+i*3.0f,2.2f,2.08f),new Vector3(0.1f,1.3f,0.1f),wood,forge.transform);
                CreateBlock("Axe Head "+i, pos+new Vector3(-1.5f+i*3.0f,2.7f,2.15f),new Vector3(0.6f,0.5f,0.15f),metal,forge.transform);
            }
            // Quench trough
            CreateBlock("Quench Trough",   pos+new Vector3( 2.2f,1.1f,2.0f), new Vector3(2.4f,0.6f,1.0f), rust,  forge.transform);
            // Gold-trimmed bellows
            CreateBlock("Bellows Body",    pos+new Vector3(-0.8f,1.6f,-1.8f),new Vector3(1.5f,0.8f,0.8f), wood,  forge.transform);
            CreateBlock("Bellows Gold",    pos+new Vector3(-0.8f,2.05f,-1.8f),new Vector3(1.6f,0.12f,0.9f),gold, forge.transform);
            // Scattered tools on workbench
            CreateBlock("Bench",           pos+new Vector3(-0.8f,1.4f,-2.2f),new Vector3(3f,0.5f,1f),      wood,  forge.transform);
            for (int i=0;i<5;i++)
                CreateBlock("Tool "+i,     pos+new Vector3(-1.2f+i*0.6f,1.72f,-2.22f),new Vector3(0.1f,0.1f,0.6f),metal,forge.transform).transform.rotation = Quaternion.Euler(0,Random.Range(-30,30),0);
            // Fire glow
            AddPointLight(forge.transform, pos+new Vector3(0,3f,0.6f)-pos,
                new Color(1f,0.32f,0f),5f,10f,"Forge Fire Glow");
            // Smoke particles
            GameObject smoke = new GameObject("Forge Smoke");
            smoke.transform.SetParent(forge.transform);
            smoke.transform.position = pos+new Vector3(0,8.5f,0.6f);
            var ps = smoke.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startColor   = new Color(0.3f,0.3f,0.3f,0.6f);
            m.startSize    = new ParticleSystem.MinMaxCurve(0.5f,1.5f);
            m.startLifetime= 5f;
            m.startSpeed   = new ParticleSystem.MinMaxCurve(0.3f,1f);
            m.maxParticles = 40;
            var emission = ps.emission;
            emission.rateOverTime = 6f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(1f,0.3f,1f);
            smoke.GetComponent<ParticleSystemRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default")){ color = new Color(0.3f,0.3f,0.3f,0.5f) };
        }

        private static void CreateWell(Vector3 pos, Transform parent,
            Material stone, Material wood, Material rust)
        {
            GameObject well = new GameObject("Stone Well");
            well.transform.position = pos;
            well.transform.SetParent(parent);

            // 10-segment stone ring
            for (int i=0;i<10;i++)
            {
                float a = (i/10f)*Mathf.PI*2f;
                CreateBlock("Ring "+i,
                    pos+new Vector3(Mathf.Sin(a)*1.5f,1.1f,Mathf.Cos(a)*1.5f),
                    new Vector3(0.7f,2.2f,0.7f), stone, well.transform);
            }
            CreateBlock("Well Floor", pos+new Vector3(0,0.1f,0), new Vector3(3.2f,0.25f,3.2f), stone, well.transform);
            // Dark water inside
            Material darkWater = MakeMat(null,0.95f); darkWater.color = new Color(0.05f,0.08f,0.12f,0.9f);
            CreateBlock("Well Water", pos+new Vector3(0,0.55f,0), new Vector3(2.4f,0.1f,2.4f), darkWater, well.transform);
            // Crossbeam support posts
            CreateBlock("Post L",   pos+new Vector3(-1.6f,2.8f,0), new Vector3(0.28f,3.5f,0.28f), wood, well.transform);
            CreateBlock("Post R",   pos+new Vector3( 1.6f,2.8f,0), new Vector3(0.28f,3.5f,0.28f), wood, well.transform);
            CreateBlock("Beam",     pos+new Vector3(0,4.6f,0),     new Vector3(4f,0.28f,0.28f),   wood, well.transform);
            // Windlass (rope reel)
            CreateBlock("Windlass", pos+new Vector3(0,4.3f,0),     new Vector3(3f,0.5f,0.5f),     rust, well.transform);
            // Rope (thin dark strip)
            CreateBlock("Rope",     pos+new Vector3(0,3f,0),       new Vector3(0.08f,3f,0.08f),   wood, well.transform);
            // Bucket
            CreateBlock("Bucket",   pos+new Vector3(0,2.2f,0),     new Vector3(0.45f,0.5f,0.45f), wood, well.transform);
        }

        private static void CreateLongboat(Vector3 pos, Transform parent,
            Material wood, Material woodDark, Material metal, Material banner)
        {
            GameObject boat = new GameObject("Viking Longboat");
            boat.transform.position = pos;
            boat.transform.rotation = Quaternion.Euler(0,18,0);
            boat.transform.SetParent(parent);

            // Hull — three-piece for convex side profile
            CreateBlock("Hull Bottom",  pos+new Vector3(0,0.6f,  0), new Vector3(3.8f,1.0f,16f), wood,     boat.transform);
            CreateBlock("Hull L",       pos+new Vector3(-2.4f,1.7f,0),new Vector3(0.55f,1.8f,14f),wood,    boat.transform);
            CreateBlock("Hull R",       pos+new Vector3( 2.4f,1.7f,0),new Vector3(0.55f,1.8f,14f),wood,    boat.transform);
            CreateBlock("Keel",         pos+new Vector3(0,0f,   0),  new Vector3(0.4f,0.4f,16f),  woodDark, boat.transform);
            // Dragon-head prow & stern
            var prow  = CreateBlock("Prow",  pos+new Vector3(0,3f, 8.5f),new Vector3(0.6f,4.5f,0.6f),woodDark,boat.transform);
            prow.transform.rotation  = Quaternion.Euler(-28,0,0);
            var stern = CreateBlock("Stern", pos+new Vector3(0,3f,-8.5f),new Vector3(0.6f,4.5f,0.6f),woodDark,boat.transform);
            stern.transform.rotation = Quaternion.Euler( 28,0,0);
            // Decking
            for (int i=0;i<7;i++)
                CreateBlock("Plank "+i, pos+new Vector3(0,1.5f,-6f+i*2f),new Vector3(3.5f,0.15f,1.8f),wood,boat.transform);
            // 6 oars each side
            for (int i=0;i<6;i++)
            {
                CreateBlock("Oar L "+i, pos+new Vector3(-3.2f,2.2f,-6f+i*2.2f),new Vector3(0.16f,0.16f,4.5f),wood,boat.transform).transform.rotation = Quaternion.Euler(0,0,22);
                CreateBlock("Oar R "+i, pos+new Vector3( 3.2f,2.2f,-6f+i*2.2f),new Vector3(0.16f,0.16f,4.5f),wood,boat.transform).transform.rotation = Quaternion.Euler(0,0,-22);
            }
            // Round shields — 5 each side
            for (int i=0;i<5;i++)
            {
                CreateBlock("Shield L "+i, pos+new Vector3(-2.85f,2.15f,-5f+i*2.5f),new Vector3(0.12f,1.3f,1.3f),metal,boat.transform);
                CreateBlock("Shield R "+i, pos+new Vector3( 2.85f,2.15f,-5f+i*2.5f),new Vector3(0.12f,1.3f,1.3f),metal,boat.transform);
            }
            // Mast, crossbeam, furled sail (banner material)
            CreateBlock("Mast",       pos+new Vector3(0,6f,0),    new Vector3(0.35f,12f,0.35f),wood,    boat.transform);
            CreateBlock("Crossbeam",  pos+new Vector3(0,10.5f,0), new Vector3(6f,0.35f,0.35f),  wood,    boat.transform);
            CreateBlock("Furled Sail",pos+new Vector3(0,8.5f,0),  new Vector3(5.8f,4f,0.15f),   banner,  boat.transform);
            // Cargo — two chests amidship
            for (int i=0;i<2;i++)
                CreateAleBarel(pos+new Vector3(-0.8f+i*1.6f,1.8f,0), boat.transform, woodDark, metal);
        }

        private static void CreateBlizzard(Transform parent)
        {
            GameObject weather = new GameObject("Blizzard Weather");
            weather.transform.position = new Vector3(0,32f,0);
            weather.transform.SetParent(parent);

            ParticleSystem ps = weather.AddComponent<ParticleSystem>();
            var rend = weather.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = new Material(Shader.Find("Sprites/Default")) { color = new Color(1f,1f,1f,0.75f) };

            var m = ps.main;
            m.startLifetime = 16f;
            m.startSpeed    = 10f;
            m.startSize     = new ParticleSystem.MinMaxCurve(0.06f,0.30f);
            m.maxParticles  = 7000;

            var em = ps.emission; em.rateOverTime = 2600f;
            var sh = ps.shape;   sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(420,1,420);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-3.0f,-1.5f);
            vel.y = new ParticleSystem.MinMaxCurve(-5.0f, -4.9f);
            vel.z = new ParticleSystem.MinMaxCurve( 0.5f,  0.6f);

            var col = ps.collision;
            col.enabled     = true;
            col.type        = ParticleSystemCollisionType.World;
            col.bounce       = 0.06f;
            col.lifetimeLoss = 0.90f;
            col.quality     = ParticleSystemCollisionQuality.High;
        }

        private static void CreateAuroraBorealis(Transform parent)
        {
            GameObject aurora = new GameObject("Aurora Borealis");
            aurora.transform.position = new Vector3(0,65f,0);
            aurora.transform.SetParent(parent);

            ParticleSystem ps = aurora.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startLifetime   = new ParticleSystem.MinMaxCurve(10f,18f);
            m.startSpeed      = new ParticleSystem.MinMaxCurve(0.08f,0.45f);
            m.startSize       = new ParticleSystem.MinMaxCurve(6f,16f);
            m.maxParticles    = 300;
            m.simulationSpace = ParticleSystemSimulationSpace.World;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(
                new[]{ new GradientColorKey(new Color(0.18f,1.0f,0.55f),0.0f),
                       new GradientColorKey(new Color(0.28f,0.5f,1.0f),0.4f),
                       new GradientColorKey(new Color(0.65f,0.18f,1.0f),0.8f),
                       new GradientColorKey(new Color(0.18f,0.85f,0.6f),1.0f) },
                new[]{ new GradientAlphaKey(0f,0f), new GradientAlphaKey(0.42f,0.25f),
                       new GradientAlphaKey(0.42f,0.75f), new GradientAlphaKey(0f,1f) });
            col.color = new ParticleSystem.MinMaxGradient(g);

            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(420,1,420);
            var emission = ps.emission;
            emission.rateOverTime = 10f;
            aurora.GetComponent<ParticleSystemRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        private static void CreateArmoury(Vector3 pos, Transform parent,
            Material charcoal, Material stone, Material metal, Material red)
        {
            GameObject arm = new GameObject("Midnight Armoury");
            arm.transform.SetParent(parent);

            CreateBlock("Arm Foundation",  pos+new Vector3(0,0.6f,0),  new Vector3(14f,1.2f,10f), stone,    arm.transform);
            CreateBlock("Arm Walls",       pos+new Vector3(0,4f,0),    new Vector3(12f,7f,9f),    charcoal, arm.transform);
            CreateBlock("Arm Roof L",      pos+new Vector3(-3.5f,8f,0),new Vector3(7f,0.6f,11f),  charcoal, arm.transform).transform.rotation=Quaternion.Euler(0,0,22);
            CreateBlock("Arm Roof R",      pos+new Vector3( 3.5f,8f,0),new Vector3(7f,0.6f,11f),  charcoal, arm.transform).transform.rotation=Quaternion.Euler(0,0,-22);
            CreateBlock("Arm Ridge",       pos+new Vector3(0,9f,0),    new Vector3(0.5f,0.5f,11f), charcoal, arm.transform);
            // Red accents on corner beams
            foreach (Vector3 c in new[]{new Vector3(-5.5f,4f,-4f),new Vector3(5.5f,4f,-4f),
                                         new Vector3(-5.5f,4f, 4f),new Vector3(5.5f,4f, 4f)})
                CreateBlock("Corner Beam",pos+c,new Vector3(0.45f,7.2f,0.45f),red,arm.transform);
            // Weapon racks inside (swords, spears, bows)
            for (int i=0;i<4;i++)
            {
                CreateBlock("Rack "+i,     pos+new Vector3(-4f+i*2.8f,2.5f,3.5f),new Vector3(2f,3f,0.3f),charcoal,arm.transform);
                for (int j=0;j<3;j++)
                    CreateBlock("Weapon ",  pos+new Vector3(-4f+i*2.8f-0.5f+j*0.5f,3f,3.55f),
                        new Vector3(0.1f,2.5f,0.1f),metal,arm.transform);
            }
            // Dim interior light
            AddPointLight(arm.transform, new Vector3(0,4f,0), new Color(0.8f,0.6f,0.2f),2f,8f,"Armoury Lamp");
        }

        private static void CreateKyudojo(Vector3 pos, Transform parent,
            Material wood, Material stone, Material path, Material indigo)
        {
            GameObject dojo = new GameObject("Kyudojo (Archery Range)");
            dojo.transform.SetParent(parent);

            // Shooting lane floor
            CreateBlock("Lane Floor",   pos+new Vector3(0,-0.05f,0),  new Vector3(8f,0.2f,40f), path,  dojo.transform);
            // Covered shooting shelter
            CreateBlock("Shelter Post L",pos+new Vector3(-3.5f,2.5f,-18f),new Vector3(0.5f,5f,0.5f), wood, dojo.transform);
            CreateBlock("Shelter Post R",pos+new Vector3( 3.5f,2.5f,-18f),new Vector3(0.5f,5f,0.5f), wood, dojo.transform);
            CreateBlock("Shelter Roof",  pos+new Vector3(0,5.5f,-18f),  new Vector3(9f,0.5f,4f),    wood, dojo.transform);
            // Targets (makiwara) — indigo-circled boards
            for (int i=0;i<3;i++)
            {
                CreateBlock("Target Board "+i,
                    pos+new Vector3(-3f+i*3f,2f,19f), new Vector3(1.2f,1.8f,0.3f), wood, dojo.transform);
                CreateBlock("Target Ring "+i,
                    pos+new Vector3(-3f+i*3f,2.2f,19.2f),new Vector3(1.0f,1.0f,0.1f), indigo, dojo.transform);
                CreateBlock("Target Bull "+i,
                    pos+new Vector3(-3f+i*3f,2.2f,19.25f),new Vector3(0.4f,0.4f,0.08f),
                    MakeMat(null,0.1f) ,dojo.transform).GetComponent<Renderer>().sharedMaterial.color = Color.white;
            }
            // Sand lane
            CreateBlock("Sand Lane",    pos+new Vector3(0,-0.02f,0),   new Vector3(7.5f,0.12f,38f),
                MakeMat(GenNoise(new Color(0.80f,0.76f,0.62f),new Color(0.68f,0.64f,0.52f),14f),0.04f), dojo.transform);
            // Fencing
            for (int i=0;i<8;i++)
            {
                CreateBlock("Fence Post L "+i, pos+new Vector3(-4.5f,1f,-18f+i*5f),new Vector3(0.3f,2.5f,0.3f),wood,dojo.transform);
                CreateBlock("Fence Post R "+i, pos+new Vector3( 4.5f,1f,-18f+i*5f),new Vector3(0.3f,2.5f,0.3f),wood,dojo.transform);
                CreateBlock("Fence Rail L "+i, pos+new Vector3(-4.5f,2f,-18f+i*5f),new Vector3(0.2f,0.2f,5f),  wood,dojo.transform);
                CreateBlock("Fence Rail R "+i, pos+new Vector3( 4.5f,2f,-18f+i*5f),new Vector3(0.2f,0.2f,5f),  wood,dojo.transform);
            }
        }

        // FIX: Removed the space from "CreateGorinto StupaField" → "CreateGorintoStupaField"
        private static void CreateGorintoStupaField(Vector3 pos, Transform parent,
            Material stone, Material dark, Material moss)
        {
            // Gorintō = five-element stone stupa (memorial tower)
            // Field of 12, arranged in irregular rows — ancient graveyard feel
            GameObject field = new GameObject("Gorinto Stupa Field");
            field.transform.SetParent(parent);
            field.transform.position = pos;

            float[,] spots = {
                {0,0},{4,2},{-3,5},{8,-2},{-6,1},{2,-6},
                {-8,4},{5,8},{-2,10},{9,5},{-9,-3},{3,-10}
            };
            for (int i=0;i<12;i++)
                CreateGorinto(pos+new Vector3(spots[i,0],0,spots[i,1]),
                    field.transform, i%3==0?dark:stone, i%4==0?moss:stone);
        }

        private static void CreateGorinto(Vector3 pos, Transform parent, Material a, Material b)
        {
            // Five elements stacked: cube (earth) + sphere (water) + pyramid (fire) + hemisphere (wind) + jewel (void)
            CreateBlock("Base (Cube)",  pos+new Vector3(0,0.4f,0), new Vector3(1.0f,0.8f,1.0f), a, parent);
            CreateBlock("Water",        pos+new Vector3(0,1.1f,0), new Vector3(0.85f,0.8f,0.85f),b, parent);
            CreateBlock("Fire",         pos+new Vector3(0,1.7f,0), new Vector3(0.70f,0.7f,0.70f),a, parent).transform.rotation=Quaternion.Euler(0,45,0);
            var wind = CreateBlock("Wind",pos+new Vector3(0,2.3f,0),new Vector3(0.60f,0.4f,0.60f),b,parent);
            var void_ = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            void_.name = "Void"; void_.transform.SetParent(parent);
            void_.transform.position = pos+new Vector3(0,2.65f,0);
            void_.transform.localScale = new Vector3(0.45f,0.45f,0.45f);
            void_.GetComponent<Renderer>().sharedMaterial = a;
            void_.isStatic = true;
        }

        private static void CreateOrnamentalBridge(Vector3 pos, Transform parent,
            Material red, Material wood, Material stone, Material water)
        {
            GameObject bridge = new GameObject("Ornamental Bridge");
            bridge.transform.SetParent(parent);

            // Stream under the bridge
            CreateBlock("Stream",     pos+new Vector3(0,-0.8f,0), new Vector3(22f,0.4f,6f),  water, bridge.transform);
            // Approach stones
            CreateBlock("Stone N",    pos+new Vector3(0, 0.1f,-4f),new Vector3(5f,0.2f,4f),  stone, bridge.transform);
            CreateBlock("Stone S",    pos+new Vector3(0, 0.1f, 4f),new Vector3(5f,0.2f,4f),  stone, bridge.transform);
            // Arched bridge deck (arched via rotation trick — two sloped halves)
            var deckL = CreateBlock("Deck L",pos+new Vector3(0,0.6f,-1.2f),new Vector3(5.5f,0.3f,3.5f),red,bridge.transform);
            deckL.transform.rotation = Quaternion.Euler(18,0,0);
            var deckR = CreateBlock("Deck R",pos+new Vector3(0,0.6f, 1.2f),new Vector3(5.5f,0.3f,3.5f),red,bridge.transform);
            deckR.transform.rotation = Quaternion.Euler(-18,0,0);
            CreateBlock("Deck Mid",   pos+new Vector3(0,1.0f,0),   new Vector3(5.5f,0.3f,2f), red,  bridge.transform);
            // Railings
            CreateBlock("Rail N",     pos+new Vector3(-2.6f,1.5f,0),new Vector3(0.2f,1f,7f), wood,bridge.transform);
            CreateBlock("Rail S",     pos+new Vector3( 2.6f,1.5f,0),new Vector3(0.2f,1f,7f), wood,bridge.transform);
            // Decorative posts
            foreach (float fz in new[]{-2.5f,0f,2.5f})
            {
                CreateBlock("Post N",pos+new Vector3(-2.6f,2.1f,fz),new Vector3(0.3f,1.4f,0.3f),red,bridge.transform);
                CreateBlock("Post S",pos+new Vector3( 2.6f,2.1f,fz),new Vector3(0.3f,1.4f,0.3f),red,bridge.transform);
            }
            // Water ripple glow
            AddPointLight(bridge.transform, new Vector3(0,-0.2f,0), new Color(0.28f,0.58f,0.80f),1.5f,5f,"Stream Glow");
        }

        private static void CreateWatermill(Vector3 pos, Transform parent,
            Material wood, Material stone, Material water)
        {
            GameObject mill = new GameObject("Watermill");
            mill.transform.SetParent(parent);

            // Mill building
            CreateBlock("Mill Base",    pos+new Vector3(0,1.5f,0),   new Vector3(8f,3f,7f),    stone, mill.transform);
            CreateBlock("Mill Upper",   pos+new Vector3(0,4.5f,0),   new Vector3(7f,3.5f,6f),  wood,  mill.transform);
            CreateBlock("Mill Roof L",  pos+new Vector3(-2f,7.2f,0), new Vector3(4.5f,0.5f,8f),wood,  mill.transform).transform.rotation=Quaternion.Euler(0,0, 28);
            CreateBlock("Mill Roof R",  pos+new Vector3( 2f,7.2f,0), new Vector3(4.5f,0.5f,8f),wood,  mill.transform).transform.rotation=Quaternion.Euler(0,0,-28);
            // Mill wheel (cross of two beams + paddles)
            CreateBlock("Wheel Hub",    pos+new Vector3(-4.5f,2f,-1f),new Vector3(0.5f,0.5f,0.5f),wood,mill.transform);
            CreateBlock("Wheel Spoke V",pos+new Vector3(-4.5f,2f,-1f),new Vector3(0.3f,4.5f,0.3f),wood,mill.transform);
            CreateBlock("Wheel Spoke H",pos+new Vector3(-4.5f,2f,-1f),new Vector3(0.3f,0.3f,4.5f),wood,mill.transform);
            for (int i=0;i<6;i++)
            {
                float a = i*60f*Mathf.Deg2Rad;
                CreateBlock("Paddle "+i,
                    pos+new Vector3(-4.5f, 2f+Mathf.Sin(a)*2f,-1f+Mathf.Cos(a)*2f),
                    new Vector3(0.3f,0.6f,0.8f), wood, mill.transform);
            }
            // Mill stream
            CreateBlock("Mill Stream",  pos+new Vector3(-8f,-0.5f,-1f),new Vector3(6f,0.4f,3f),water,mill.transform);
            // Water sound glow
            AddPointLight(mill.transform,new Vector3(-5f,1f,-1f),new Color(0.3f,0.6f,0.85f),1.5f,5f,"Millstream Glow");
        }

        private static void CreateWisteriaArcade(Vector3 pos, Transform parent,
            Material wood, Material wisteria, Material stone)
        {
            // Pergola-style arcade with hanging wisteria chains
            GameObject arcade = new GameObject("Wisteria Arcade");
            arcade.transform.SetParent(parent);
            arcade.transform.position = pos;

            int bays = 8;
            for (int i=0;i<bays;i++)
            {
                float z = i*4.5f;
                // Left & right posts
                CreateBlock("Post L "+i,pos+new Vector3(-2.5f,2.5f,z),new Vector3(0.4f,5f,0.4f),wood,arcade.transform);
                CreateBlock("Post R "+i,pos+new Vector3( 2.5f,2.5f,z),new Vector3(0.4f,5f,0.4f),wood,arcade.transform);
                // Crossbeam
                CreateBlock("Beam "+i,  pos+new Vector3(0,5.2f,z),    new Vector3(6f,0.35f,0.35f),wood,arcade.transform);
                // Wisteria chains (cascading purple blobs)
                for (int j=0;j<4;j++)
                {
                    float bx = -2f+j*1.4f;
                    for (int k=0;k<3;k++)
                        CreateBlock("Wist "+i+"_"+j+"_"+k,
                            pos+new Vector3(bx,4.5f-k*0.5f,z+Random.Range(-0.2f,0.2f)),
                            new Vector3(0.5f,0.6f,0.5f), wisteria, arcade.transform);
                }
            }
            // Stone bases for posts
            for (int i=0;i<bays;i++)
            {
                CreateBlock("Base L "+i,pos+new Vector3(-2.5f,0.25f,i*4.5f),new Vector3(0.8f,0.5f,0.8f),stone,arcade.transform);
                CreateBlock("Base R "+i,pos+new Vector3( 2.5f,0.25f,i*4.5f),new Vector3(0.8f,0.5f,0.8f),stone,arcade.transform);
            }
        }

        private static void CreateFestivalPennants(Vector3 O, Transform parent,
            Material red, Material gold, Material wood, Material paper)
        {
            // Zigzag rope-lines with hanging paper lanterns / pennant flags
            // Between the torii gates, crossing the main path
            for (int i=0;i<5;i++)
            {
                float z = -8f+i*18f;
                // Two anchor poles
                CreateBlock("Pennant Pole L",O+new Vector3(-8f,4f,z),new Vector3(0.25f,8f,0.25f),wood,parent);
                CreateBlock("Pennant Pole R",O+new Vector3( 8f,4f,z),new Vector3(0.25f,8f,0.25f),wood,parent);
                // Hanging lanterns
                for (int j=0;j<5;j++)
                {
                    float lx = -6f+j*3f;
                    CreateBlock("Lantern "+i+"_"+j,
                        O+new Vector3(lx,7.5f-Mathf.Abs(lx)*0.08f,z),
                        new Vector3(0.55f,0.8f,0.55f), j%2==0?red:paper, parent);
                    AddPointLight(parent, O+new Vector3(lx,7.2f,z),
                        new Color(1f,0.65f,0.18f),1.0f,4f,"Festival Lantern");
                }
                // Pennant flags on the poles
                CreateBlock("Flag L",O+new Vector3(-8.8f,8.5f,z),new Vector3(0.1f,1.8f,1.2f),red,  parent);
                CreateBlock("Flag R",O+new Vector3( 8.8f,8.5f,z),new Vector3(0.1f,1.8f,1.2f),gold, parent);
            }
        }

        private static void CreateWaterfallCascade(Vector3 pos, Transform parent,
            Material water, Material stone, Material moss)
        {
            GameObject wf = new GameObject("Koi Waterfall Cascade");
            wf.transform.SetParent(parent);

            CreateBlock("Cliff A",  pos+new Vector3(0,4f,2f),  new Vector3(6f,10f,3f),  stone, wf.transform);
            CreateBlock("Pool A",   pos+new Vector3(0,-0.4f,0),new Vector3(6f,0.3f,4f),  water, wf.transform);
            // Cascade jets
            for (int i=0;i<3;i++)
            {
                var jet = CreateBlock("Jet "+i,pos+new Vector3(-1.5f+i*1.5f,2f,0.5f),
                    new Vector3(0.3f,5f,0.3f),water,wf.transform);
                jet.transform.rotation = Quaternion.Euler(8,0,0);
            }
            // Spray particles
            GameObject spray = new GameObject("Spray");
            spray.transform.SetParent(wf.transform);
            spray.transform.position = pos+new Vector3(0,0.5f,-0.5f);
            var ps = spray.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startColor   = new Color(0.7f,0.85f,0.95f,0.5f);
            m.startSize    = new ParticleSystem.MinMaxCurve(0.1f,0.4f);
            m.startLifetime= 2f;
            m.startSpeed   = new ParticleSystem.MinMaxCurve(0.5f,1.5f);
            m.maxParticles = 60;
            var emission = ps.emission;
            emission.rateOverTime = 20f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(5f,0.2f,1f);
            spray.GetComponent<ParticleSystemRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            AddPointLight(wf.transform, new Vector3(0,1f,0), new Color(0.4f,0.7f,0.9f),2f,6f,"Cascade Glow");
        }

        private static void CreateNinjaRooftopPaths(Vector3 O, Transform parent,
            Material rope, Material wood)
        {
            // Thin rope-bridge / cable segments connecting rooftops at height
            // Invisible movement platform + visible cable
            GameObject paths = new GameObject("Ninja Rooftop Paths");
            paths.transform.SetParent(parent);

            float[,] routes = {
                {-22f,9f,32f, -26f,6f,76f},  // tea house → shrine hill
                { 26f,5f,76f,  44f,8f,78f},  // pagoda → armoury
                { 44f,8f,78f,  52f,5f,20f},  // armoury → kyudojo
            };
            for (int i=0;i<3;i++)
            {
                Vector3 a = O+new Vector3(routes[i,0],routes[i,1],routes[i,2]);
                Vector3 b = O+new Vector3(routes[i,3],routes[i,4],routes[i,5]);
                Vector3 mid = (a+b)*0.5f;
                Vector3 dir = b-a;
                float len = dir.magnitude;

                // Visible rope
                var cable = CreateBlock("Cable "+i, mid+new Vector3(0,-0.1f,0),
                    new Vector3(0.12f, 0.12f, len), rope, paths.transform);
                cable.transform.LookAt(b);

                // Walkable invisible platform
                var plat = CreateInvisibleWall("Ninja Plat "+i, mid,
                    new Vector3(0.8f,0.4f,len), paths.transform);
                plat.transform.LookAt(b);
            }
        }

        private static void CreateMorningMist(Vector3 origin, Transform parent)
        {
            // Low-lying volumetric mist layer — sits at Y=0..3
            GameObject mist = new GameObject("Morning Mist (Volumetric)");
            mist.transform.position = origin+new Vector3(0,1.5f,0);
            mist.transform.SetParent(parent);

            ParticleSystem ps = mist.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startColor    = new Color(0.92f,0.95f,0.98f,0.22f);
            m.startLifetime = new ParticleSystem.MinMaxCurve(18f,28f);
            m.startSpeed    = new ParticleSystem.MinMaxCurve(0.05f,0.3f);
            m.startSize     = new ParticleSystem.MinMaxCurve(10f,28f);
            m.maxParticles  = 300;
            m.simulationSpace = ParticleSystemSimulationSpace.World;

            var em = ps.emission; em.rateOverTime = 5f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(200,1,200);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-0.4f,0.4f);
            vel.y = new ParticleSystem.MinMaxCurve(0.01f,0.05f);
            vel.z = new ParticleSystem.MinMaxCurve(-0.2f,0.5f);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(
                new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},
                new[]{new GradientAlphaKey(0f,0f),new GradientAlphaKey(0.22f,0.25f),
                      new GradientAlphaKey(0.22f,0.75f),new GradientAlphaKey(0f,1f)});
            col.color = new ParticleSystem.MinMaxGradient(g);

            var rend = mist.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            rend.sortingOrder   = 10;
        }


        // ╔══════════════════════════════════════════════════════════════════════════╗
        //  YAMATO — EXISTING PROP BUILDERS (enhanced)
        // ╚══════════════════════════════════════════════════════════════════════════╝

        private static void CreateStoneLantern(Vector3 pos, Transform parent, Material stone)
        {
            GameObject lantern = new GameObject("Stone Lantern (Toro)");
            lantern.transform.position = pos;
            lantern.transform.SetParent(parent);

            CreateBlock("Base",    pos+new Vector3(0,0.22f,0), new Vector3(0.95f,0.45f,0.95f), stone, lantern.transform);
            CreateBlock("Post",    pos+new Vector3(0,1.15f,0), new Vector3(0.38f,1.90f,0.38f), stone, lantern.transform);
            CreateBlock("Cap",     pos+new Vector3(0,2.25f,0), new Vector3(0.85f,0.38f,0.85f), stone, lantern.transform);
            CreateBlock("Housing", pos+new Vector3(0,2.88f,0), new Vector3(0.72f,0.72f,0.72f), stone, lantern.transform);
            var roof = CreateBlock("Roof",pos+new Vector3(0,3.40f,0),new Vector3(1.15f,0.28f,1.15f),stone,lantern.transform);
            roof.transform.rotation = Quaternion.Euler(0,45,0);
            // Top finial
            CreateBlock("Finial",  pos+new Vector3(0,3.72f,0), new Vector3(0.12f,0.38f,0.12f), stone, lantern.transform);
            // Warm glow
            AddPointLight(lantern.transform, new Vector3(0,2.88f,0),
                new Color(1f,0.76f,0.30f),1.9f,6.5f,"Lantern Glow");
        }

        private static void CreateKoiPond(Vector3 pos, Transform parent,
            Material stone, Material water, Material gold)
        {
            GameObject pond = new GameObject("Koi Pond");
            pond.transform.position = pos;
            pond.transform.SetParent(parent);

            CreateBlock("Pond Basin",   pos+new Vector3(0,-0.35f, 0), new Vector3(14f,0.65f,10f), stone, pond.transform);
            CreateBlock("Water Surface",pos+new Vector3(0, 0.08f, 0), new Vector3(13f,0.07f, 9f), water, pond.transform);
            CreateBlock("Edge N",       pos+new Vector3(0, 0.22f,5.2f),new Vector3(14.5f,0.45f,0.7f),stone,pond.transform);
            CreateBlock("Edge S",       pos+new Vector3(0, 0.22f,-5.2f),new Vector3(14.5f,0.45f,0.7f),stone,pond.transform);
            CreateBlock("Edge W",       pos+new Vector3(-7.2f,0.22f,0),new Vector3(0.7f,0.45f,10.5f),stone,pond.transform);
            CreateBlock("Edge E",       pos+new Vector3( 7.2f,0.22f,0),new Vector3(0.7f,0.45f,10.5f),stone,pond.transform);

            // Stepping stones
            foreach (float sx in new[]{ -4f,-1.5f,1.5f,4f })
                CreateBlock("Step",pos+new Vector3(sx,0.15f,0),new Vector3(1.3f,0.22f,1.3f),stone,pond.transform);

            // Koi fish
            for (int i=0;i<12;i++)
            {
                var fish = CreateBlock("Koi "+i,
                    pos+new Vector3(Random.Range(-5f,5f),-0.12f,Random.Range(-3.5f,3.5f)),
                    new Vector3(0.35f,0.18f,0.80f), gold, pond.transform);
                fish.transform.rotation = Quaternion.Euler(0,Random.Range(0,360),0);
                fish.AddComponent<RoamingAnimal>().moveSpeed = 1.0f;
            }

            // Feature rocks with moss
            Material mossR = MakeMat(null,0.06f); mossR.color = new Color(0.22f,0.44f,0.18f);
            var r1 = CreateBlock("Rock 1",pos+new Vector3(-5f,0.7f,-2.5f),new Vector3(2.2f,1.4f,1.6f),stone,pond.transform);
            r1.transform.rotation = Quaternion.Euler(8,35,5);
            var r2 = CreateBlock("Rock 2",pos+new Vector3( 4f,0.5f, 2.0f),new Vector3(1.4f,1.0f,1.2f),mossR,pond.transform);
            r2.transform.rotation = Quaternion.Euler(-5,130,4);
            // Water shimmer glow
            AddPointLight(pond.transform,new Vector3(0,0.5f,0),new Color(0.28f,0.62f,0.80f),2f,8f,"Pond Glow");
        }

        private static void CreateZenGarden(Vector3 pos, Transform parent,
            Material stone, Material sand)
        {
            GameObject zen = new GameObject("Zen Garden");
            zen.transform.position = pos;
            zen.transform.SetParent(parent);

            CreateBlock("Sand Bed",   pos+new Vector3(0,-0.06f,0),   new Vector3(18f,0.22f,14f),  sand, zen.transform);
            CreateBlock("Border N",   pos+new Vector3(0,0.35f, 7.2f),new Vector3(18.8f,0.55f,0.55f),stone,zen.transform);
            CreateBlock("Border S",   pos+new Vector3(0,0.35f,-7.2f),new Vector3(18.8f,0.55f,0.55f),stone,zen.transform);
            CreateBlock("Border W",   pos+new Vector3(-9.2f,0.35f,0),new Vector3(0.55f,0.55f,14.8f),stone,zen.transform);
            CreateBlock("Border E",   pos+new Vector3( 9.2f,0.35f,0),new Vector3(0.55f,0.55f,14.8f),stone,zen.transform);

            // Rake lines (12 rows)
            for (int i=0;i<12;i++)
                CreateBlock("Rake "+i,pos+new Vector3(0,0.08f,-5.5f+i*1.05f),new Vector3(16f,0.05f,0.18f),stone,zen.transform);

            // Three grouped feature rocks
            float[,] rd = { {-5f,1.1f,2.2f,1.5f,2.0f,1.4f},{1.5f,0.9f,-2f,2.4f,1.7f,1.1f},{5f,0.7f,2.8f,1.1f,1.3f,1.0f} };
            for (int i=0;i<3;i++)
            {
                var rock = CreateBlock("Rock "+i,
                    pos+new Vector3(rd[i,0],rd[i,1],rd[i,2]),
                    new Vector3(rd[i,3],rd[i,4],rd[i,5]), stone, zen.transform);
                rock.transform.rotation = Quaternion.Euler(Random.Range(-10,10),Random.Range(0,360),Random.Range(-10,10));
            }
            // Moss cushions
            Material moss = MakeMat(null,0.05f); moss.color = new Color(0.20f,0.44f,0.16f);
            CreateBlock("Moss L",pos+new Vector3(-3.5f,0.6f,-1.5f),new Vector3(1.1f,1.1f,1.1f),moss,zen.transform);
            CreateBlock("Moss R",pos+new Vector3( 3.5f,0.6f, 2.5f),new Vector3(0.9f,0.9f,0.9f),moss,zen.transform);
            // Small stone basin (tsukubai)
            CreateBlock("Tsukubai",  pos+new Vector3(-7f,0.5f,-5f),new Vector3(1.4f,0.8f,1.4f),stone,zen.transform);
            CreateBlock("Tsuk Water",pos+new Vector3(-7f,0.9f,-5f),new Vector3(1.0f,0.1f,1.0f),MakeMat(null,0.9f),zen.transform);
        }

        private static void CreateBambooClump(Vector3 pos, Transform parent, Material bamboo)
        {
            GameObject clump = new GameObject("Bamboo Clump");
            clump.transform.position = pos;
            clump.transform.SetParent(parent);

            int count = Random.Range(5,10);
            for (int i=0;i<count;i++)
            {
                float ox = Random.Range(-1.4f,1.4f);
                float oz = Random.Range(-1.4f,1.4f);
                float h  = Random.Range(7f,12f);
                // Stalk — two-tone: darker base
                Material bambooDark = MakeMat(null,0.12f); bambooDark.color = new Color(0.34f,0.58f,0.18f);
                CreateBlock("Stalk",pos+new Vector3(ox,h*0.5f,oz),new Vector3(0.20f,h,0.20f),i%3==0?bambooDark:bamboo,clump.transform);
                // Node rings (characteristic bamboo segments)
                int nodes = Mathf.RoundToInt(h/1.8f);
                for (int n=0;n<nodes;n++)
                    CreateBlock("Node",pos+new Vector3(ox,n*1.8f+0.9f,oz),new Vector3(0.26f,0.12f,0.26f),bambooDark,clump.transform);
                // Leaf tufts
                for (int j=0;j<4;j++)
                {
                    var leaf = CreateBlock("Leaf",
                        pos+new Vector3(ox+Random.Range(-1.0f,1.0f),h-Random.Range(0f,2f),oz+Random.Range(-1.0f,1.0f)),
                        new Vector3(1.4f,0.14f,0.9f), bamboo, clump.transform);
                    leaf.transform.rotation = Quaternion.Euler(Random.Range(-25,25),Random.Range(0,360),Random.Range(-25,25));
                }
            }
        }

        private static void CreateMainShrine(Vector3 pos, Transform parent,
            Material red, Material darkRed, Material wood, Material gold, Material stone,
            Material path)
        {
            GameObject shrine = new GameObject("Main Shrine (Haiden)");
            shrine.transform.position = pos;
            shrine.transform.SetParent(parent);

            // Tiered stone platform
            CreateBlock("Platform 1",  pos+new Vector3(0,1.0f,0),  new Vector3(26f,2.2f,22f), stone, shrine.transform);
            CreateBlock("Platform 2",  pos+new Vector3(0,2.4f,0),  new Vector3(18f,1.2f,16f), stone, shrine.transform);
            // Main shrine body
            CreateBlock("Walls",       pos+new Vector3(0,5.5f,0),  new Vector3(15f,6f,11f),   wood,  shrine.transform);
            // Front porch (hisashi)
            CreateBlock("Porch Floor", pos+new Vector3(0,3.2f,-7.5f),new Vector3(15f,0.5f,5f),wood,  shrine.transform);
            foreach (float px in new[]{-6f,-2f,2f,6f})
                CreateBlock("Porch Post",pos+new Vector3(px,5.5f,-7.5f),new Vector3(0.55f,5.5f,0.55f),red,shrine.transform);
            // Two-tier irimoya roof
            CreateBlock("Roof Lower",  pos+new Vector3(0,9.5f,0),  new Vector3(20f,0.7f,15f), red,      shrine.transform);
            CreateBlock("Roof Upper",  pos+new Vector3(0,11.8f,0), new Vector3(13f,0.7f,10f), darkRed,  shrine.transform);
            CreateBlock("Ridge Beam",  pos+new Vector3(0,12.8f,0), new Vector3(0.6f,0.6f,11f),wood,     shrine.transform);
            // Gold finials (chigi) — four
            foreach (float fx in new[]{-7f,7f}) foreach (float fz in new[]{-5f,5f})
                CreateBlock("Finial",  pos+new Vector3(fx,13.2f,fz),new Vector3(0.35f,1.8f,0.35f),gold,shrine.transform);
            // Curved eave tips
            CreateBlock("Eave Front",  pos+new Vector3(0,9.2f,-8f),  new Vector3(20f,0.4f,4f),   red,  shrine.transform).transform.rotation=Quaternion.Euler(15,0,0);
            CreateBlock("Eave Back",   pos+new Vector3(0,9.2f, 6.5f),new Vector3(20f,0.4f,4f),   red,  shrine.transform).transform.rotation=Quaternion.Euler(-15,0,0);
            // Offering box (saisen-bako)
            CreateBlock("Offering Box",pos+new Vector3(0,3.7f,-5.5f),new Vector3(2.8f,1.1f,1.7f),wood,shrine.transform);
            // Shimenawa rope with gold zig-zag streamers
            CreateBlock("Shimenawa",   pos+new Vector3(0,10.0f,-5.8f),new Vector3(17f,0.35f,0.35f),wood,shrine.transform);
            for (int i=0;i<7;i++)
                CreateBlock("Shide "+i,pos+new Vector3(-7.5f+i*2.5f,9.0f,-5.8f),
                    new Vector3(0.12f,Random.Range(0.8f,1.6f),0.12f),gold,shrine.transform);
            // Flanking stone lanterns
            CreateStoneLantern(pos+new Vector3(-4.5f,-1f,-9f),shrine.transform,stone);
            CreateStoneLantern(pos+new Vector3( 4.5f,-1f,-9f),shrine.transform,stone);
            // Shrine light
            AddPointLight(shrine.transform,new Vector3(0,4f,0),new Color(1f,0.78f,0.30f),4f,12f,"Shrine Inner Glow");
        }

        private static void CreateBellTower(Vector3 pos, Transform parent,
            Material wood, Material gold, Material stone)
        {
            GameObject tower = new GameObject("Bell Tower (Shoro)");
            tower.transform.position = pos;
            tower.transform.SetParent(parent);

            // Stone base
            CreateBlock("Stone Base",  pos+new Vector3(0,0.8f,0),  new Vector3(9f,1.8f,9f),    stone, tower.transform);
            // Four carved posts
            foreach (Vector3 c in new[]{
                new Vector3(-3f,4f, 3f),new Vector3(3f,4f, 3f),
                new Vector3(-3f,4f,-3f),new Vector3(3f,4f,-3f)})
                CreateBlock("Post",pos+c,new Vector3(0.5f,7.5f,0.5f),wood,tower.transform);
            // Cross-braces (four planes)
            CreateBlock("Brace N",pos+new Vector3(0,5.5f, 3f),new Vector3(6.5f,0.4f,0.4f),wood,tower.transform);
            CreateBlock("Brace S",pos+new Vector3(0,5.5f,-3f),new Vector3(6.5f,0.4f,0.4f),wood,tower.transform);
            CreateBlock("Brace W",pos+new Vector3(-3f,5.5f,0),new Vector3(0.4f,0.4f,6.5f),wood,tower.transform);
            CreateBlock("Brace E",pos+new Vector3( 3f,5.5f,0),new Vector3(0.4f,0.4f,6.5f),wood,tower.transform);
            // Bonsho bell (sphere)
            var bell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bell.name = "Bonsho Bell";
            bell.transform.SetParent(tower.transform);
            bell.transform.position   = pos+new Vector3(0,8f,0);
            bell.transform.localScale = new Vector3(1.6f,2.0f,1.6f);
            bell.GetComponent<Renderer>().sharedMaterial = gold;
            bell.isStatic = true;
            // Striking log (bonsho uchi)
            CreateBlock("Striking Log",pos+new Vector3(2.5f,8f,0),new Vector3(0.4f,0.4f,4f),wood,tower.transform).transform.rotation=Quaternion.Euler(0,15,0);
            // Roof cap
            CreateBlock("Bell Roof",   pos+new Vector3(0,10.5f,0),new Vector3(7f,0.6f,7f),  wood, tower.transform);
            CreateBlock("Roof Eave L", pos+new Vector3(-4f,10.2f,0),new Vector3(3f,0.4f,8f),wood, tower.transform).transform.rotation=Quaternion.Euler(0,0,18);
            CreateBlock("Roof Eave R", pos+new Vector3( 4f,10.2f,0),new Vector3(3f,0.4f,8f),wood, tower.transform).transform.rotation=Quaternion.Euler(0,0,-18);
            CreateBlock("Peak",        pos+new Vector3(0,11.5f,0), new Vector3(0.35f,1.5f,0.35f),gold,tower.transform);
            // Bell glow
            AddPointLight(tower.transform,new Vector3(0,8.2f,0),new Color(1f,0.82f,0.28f),2.5f,8f,"Bell Tower Glow");
        }

        private static void CreateTeaHouse(Vector3 pos, Transform parent,
            Material wood, Material paper, Material red, Material stone)
        {
            GameObject th = new GameObject("Tea House (Chashitsu)");
            th.transform.position = pos;
            th.transform.rotation = Quaternion.Euler(0,35,0);
            th.transform.SetParent(parent);

            CreateBlock("Foundation", pos+new Vector3(0,0.45f,0), new Vector3(10f,0.9f,9f),  stone, th.transform);
            CreateBlock("Wall Back",  pos+new Vector3(0,3.5f,4f), new Vector3(9f,6f,0.45f),  wood,  th.transform);
            CreateBlock("Wall L",     pos+new Vector3(-4.5f,3.5f,0),new Vector3(0.45f,6f,8f),wood,  th.transform);
            CreateBlock("Wall R",     pos+new Vector3( 4.5f,3.5f,0),new Vector3(0.45f,6f,8f),wood,  th.transform);
            // Shoji panels on front — slightly offset from each other
            CreateBlock("Shoji L",    pos+new Vector3(-2f,3.5f,-3.5f),new Vector3(3.8f,5f,0.18f), paper, th.transform);
            CreateBlock("Shoji R",    pos+new Vector3( 2f,3.5f,-3.5f),new Vector3(3.8f,5f,0.18f), paper, th.transform);
            // Engawa (cedar veranda)
            CreateBlock("Veranda",    pos+new Vector3(0,1.2f,-6.2f),new Vector3(10f,0.35f,3f),wood,  th.transform);
            // Veranda posts
            foreach (float vx in new[]{-4f,0f,4f})
                CreateBlock("Vr Post",pos+new Vector3(vx,2.5f,-6.2f),new Vector3(0.35f,3.5f,0.35f),wood,th.transform);
            // Hip roof (two pitch panels + ridge)
            var rL = CreateBlock("Roof L",pos+new Vector3(-3.2f,8f,0),new Vector3(5f,0.55f,11f),red,th.transform);
            rL.transform.rotation = Quaternion.Euler(0,0, 22);
            var rR = CreateBlock("Roof R",pos+new Vector3( 3.2f,8f,0),new Vector3(5f,0.55f,11f),red,th.transform);
            rR.transform.rotation = Quaternion.Euler(0,0,-22);
            CreateBlock("Ridge",      pos+new Vector3(0,9f,0),    new Vector3(0.45f,0.45f,11f), wood, th.transform);
            // Curved eave tips
            CreateBlock("Eave Front", pos+new Vector3(0,7.5f,-5.5f),new Vector3(12f,0.45f,3f),red,  th.transform).transform.rotation=Quaternion.Euler(18,0,0);
            // Interior furnishings
            Material tatami  = MakeMat(null,0.06f); tatami.color  = new Color(0.72f,0.75f,0.50f);
            Material cushion = MakeMat(null,0.08f); cushion.color = new Color(0.18f,0.40f,0.24f);
            CreateBlock("Tatami",     pos+new Vector3(0,1.0f,1.5f),  new Vector3(8f,0.12f,5f),    tatami,  th.transform);
            CreateBlock("Tea Table",  pos+new Vector3(0,1.25f,1.5f), new Vector3(3.2f,0.14f,2.2f),wood,    th.transform);
            CreateBlock("Cushion L",  pos+new Vector3(-1.3f,1.2f,1.2f),new Vector3(0.9f,0.14f,0.9f),cushion,th.transform);
            CreateBlock("Cushion R",  pos+new Vector3( 1.3f,1.2f,1.2f),new Vector3(0.9f,0.14f,0.9f),cushion,th.transform);
            // Warm lamp glow inside
            AddPointLight(th.transform,new Vector3(0,3f,1.5f),new Color(1f,0.76f,0.32f),3f,8f,"Tea House Lamp");
        }

        private static void CreateBambooFenceSection(Vector3 pos, Transform parent,
            Material bamboo, Material wood, bool rotate90 = false)
        {
            GameObject sec = new GameObject("Bamboo Fence Section");
            sec.transform.position = pos;
            sec.transform.SetParent(parent);
            if (rotate90) sec.transform.rotation = Quaternion.Euler(0,90,0);

            CreateBlock("Rail Top",   pos+new Vector3(0,2.2f,0),new Vector3(5.5f,0.22f,0.22f),wood,  sec.transform);
            CreateBlock("Rail Mid",   pos+new Vector3(0,1.4f,0),new Vector3(5.5f,0.18f,0.18f),wood,  sec.transform);
            CreateBlock("Rail Bot",   pos+new Vector3(0,0.7f,0),new Vector3(5.5f,0.18f,0.18f),wood,  sec.transform);
            for (int i=0;i<9;i++)
                CreateBlock("Cane "+i,
                    pos+new Vector3(-2.5f+i*0.58f,1.8f,0),
                    new Vector3(0.18f,3.6f,0.18f), bamboo, sec.transform);
        }

        private static void CreateCherryBlossomWeather(Vector3 origin, Transform parent)
        {
            GameObject weather = new GameObject("Cherry Blossom Weather");
            weather.transform.position = origin+new Vector3(0,28f,0);
            weather.transform.SetParent(parent);

            ParticleSystem ps = weather.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startColor    = new Color(1f,0.76f,0.86f);
            m.startLifetime = new ParticleSystem.MinMaxCurve(12f,20f);
            m.startSpeed    = new ParticleSystem.MinMaxCurve(0.4f,2.2f);
            m.startSize     = new ParticleSystem.MinMaxCurve(0.07f,0.25f);
            m.maxParticles  = 2500;

            var rend = weather.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            rend.renderMode     = ParticleSystemRenderMode.Billboard;

            var emission = ps.emission;
            emission.rateOverTime = 130f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(230,1,230);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-1.0f,1.0f);
            vel.y = new ParticleSystem.MinMaxCurve(-1.4f, -1.3f);
            vel.z = new ParticleSystem.MinMaxCurve(-0.5f,1.0f);

            var noise = ps.noise;
            noise.enabled   = true;
            noise.strength  = 0.6f;
            noise.frequency = 0.28f;
        }


        // ╔══════════════════════════════════════════════════════════════════════════╗
        //  SHARED BUILDERS — Portals, Sakura, Torii, Pagoda
        // ╚══════════════════════════════════════════════════════════════════════════╝

        private static void CreatePortalGateway(string name, Vector3 pos, Transform parent,
            Material pillarMat, Color glowColor, Color lightColor, Color particleColor,
            Vector3 destination, bool isReturn)
        {
            GameObject group = new GameObject(name);
            group.transform.position = pos;
            group.transform.SetParent(parent);
            if (!isReturn) group.transform.rotation = Quaternion.Euler(0,-28f,0);

            // Arch
            CreateBlock("Pillar L", pos+new Vector3(-3.0f,3.8f,0),new Vector3(0.9f,7.6f,0.9f), pillarMat, group.transform);
            CreateBlock("Pillar R", pos+new Vector3( 3.0f,3.8f,0),new Vector3(0.9f,7.6f,0.9f), pillarMat, group.transform);
            CreateBlock("Arch",     pos+new Vector3(0,7.6f,0),     new Vector3(8.8f,1.2f,1.2f), pillarMat, group.transform);
            CreateBlock("Sub Arch", pos+new Vector3(0,6.6f,0),     new Vector3(7.2f,0.6f,0.7f), pillarMat, group.transform);
            // Rune carvings on pillars
            for (int i=0;i<3;i++)
            {
                CreateBlock("Rune L "+i,pos+new Vector3(-3.05f,2f+i*1.8f,0),new Vector3(0.12f,0.8f,0.95f),
                    MakeMat(null,0.8f), group.transform).GetComponent<Renderer>().sharedMaterial.color = glowColor;
                CreateBlock("Rune R "+i,pos+new Vector3( 3.05f,2f+i*1.8f,0),new Vector3(0.12f,0.8f,0.95f),
                    MakeMat(null,0.8f), group.transform).GetComponent<Renderer>().sharedMaterial.color = glowColor;
            }
            // Glowing portal plane
            Material glowMat = new Material(Shader.Find("Unlit/Color")); glowMat.color = glowColor;
            GameObject glowObj = CreateBlock("Portal Glow",
                pos+new Vector3(0,3.5f,0),new Vector3(5.2f,6.8f,0.45f),glowMat,group.transform);
            if (isReturn) glowObj.transform.rotation = Quaternion.Euler(0,-90,0);
            // Portal light
            Light pl = glowObj.AddComponent<Light>();
            pl.type = LightType.Point; pl.color = lightColor;
            pl.intensity = 6f; pl.range = 20f;
            // Trigger collider
            BoxCollider bc = glowObj.GetComponent<BoxCollider>() ?? glowObj.AddComponent<BoxCollider>();
            bc.isTrigger = true; bc.size = new Vector3(1.1f,1.1f,7f);
            // Portal script
            var script = glowObj.AddComponent<JapanPortal>();
            script.destinationTarget = destination;
            script.isReturnPortal    = isReturn;
            // Particle vortex
            ParticleSystem ps = glowObj.AddComponent<ParticleSystem>();
            var pM = ps.main;
            pM.startColor   = particleColor;
            pM.startSize    = new ParticleSystem.MinMaxCurve(0.07f,0.35f);
            pM.startLifetime= 5f;
            pM.startSpeed   = 2.5f;
            var emission = ps.emission;
            emission.rateOverTime = 50;
            var pSh = ps.shape; pSh.shapeType = ParticleSystemShapeType.Box; pSh.scale = new Vector3(4.5f,6.5f,1f);
            glowObj.GetComponent<ParticleSystemRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            var pV = ps.velocityOverLifetime;
            pV.enabled = true;
            pV.x = new ParticleSystem.MinMaxCurve(-0.6f,0.6f);
            pV.y = new ParticleSystem.MinMaxCurve(-0.61f,0.6f);
            pV.z = new ParticleSystem.MinMaxCurve(-3.5f,-1.2f);
        }

        private static void CreateSakuraTree(Vector3 pos, Transform parent,
            Material petals, Material wood)
        {
            GameObject tree = new GameObject("Sakura Tree");
            tree.transform.position = pos;
            tree.transform.SetParent(parent);

            float h = Random.Range(3f,5.5f);
            CreateBlock("Trunk", pos+new Vector3(0,h*0.5f,0),new Vector3(0.55f,h,0.55f),wood,tree.transform);
            // Branching trunk sections
            CreateBlock("Branch L",pos+new Vector3(-0.8f,h*0.7f,0),new Vector3(0.3f,h*0.5f,0.3f),wood,tree.transform).transform.rotation=Quaternion.Euler(0,0,20);
            CreateBlock("Branch R",pos+new Vector3( 0.8f,h*0.7f,0),new Vector3(0.3f,h*0.5f,0.3f),wood,tree.transform).transform.rotation=Quaternion.Euler(0,0,-20);
            // Canopy
            for (int i=0;i<6;i++)
            {
                var c = CreateBlock("Canopy_"+i,
                    pos+new Vector3(Random.Range(-2.5f,2.5f),Random.Range(h,h+3.5f),Random.Range(-2.5f,2.5f)),
                    new Vector3(Random.Range(3f,5.5f),Random.Range(2f,3.8f),Random.Range(3f,5.5f)),
                    petals, tree.transform);
                c.transform.rotation = Quaternion.Euler(Random.Range(-18,18),Random.Range(0,360),Random.Range(-18,18));
            }
            // Per-tree petal emitter
            var ps = tree.AddComponent<ParticleSystem>();
            var m = ps.main;
            m.startColor    = new Color(1f,0.72f,0.86f);
            m.startSize     = 0.14f;
            m.startLifetime = 6f;
            m.startSpeed    = 0.6f;
            m.maxParticles  = 70;
            var emission = ps.emission;
            emission.rateOverTime = 5f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 3f;
            tree.GetComponent<ParticleSystemRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }

        private static void CreateToriiGate(string name, Vector3 pos, Material red, Material wood, Transform parent)
        {
            GameObject gate = new GameObject(name);
            gate.transform.position = pos;
            gate.transform.SetParent(parent);

            // Authentic proportions
            CreateBlock("Pillar L",    pos+new Vector3(-3.2f,3.5f,0),new Vector3(0.80f,7.2f,0.80f),red,gate.transform);
            CreateBlock("Pillar R",    pos+new Vector3( 3.2f,3.5f,0),new Vector3(0.80f,7.2f,0.80f),red,gate.transform);
            CreateBlock("Kasagi",      pos+new Vector3(0,7.1f,0),    new Vector3(9.0f,0.80f,0.95f), red,gate.transform);
            CreateBlock("Nuki",        pos+new Vector3(0,5.8f,0),    new Vector3(7.5f,0.50f,0.55f), red,gate.transform);
            CreateBlock("Shimagi",     pos+new Vector3(0,7.6f,0),    new Vector3(9.8f,0.40f,0.45f), red,gate.transform);
            CreateBlock("Gakuzan",     pos+new Vector3(0,6.4f,0),    new Vector3(0.55f,2.0f,1.1f),  red,gate.transform);
            // Komainu (guardian dog) statues flanking
            Material guardian = MakeMat(null,0.35f); guardian.color = new Color(0.78f,0.74f,0.68f);
            CreateBlock("Komainu L",   pos+new Vector3(-2.5f,0.8f,-1.5f),new Vector3(0.6f,1.2f,0.8f),guardian,gate.transform);
            CreateBlock("Komainu R",   pos+new Vector3( 2.5f,0.8f,-1.5f),new Vector3(0.6f,1.2f,0.8f),guardian,gate.transform);
        }

        private static void CreatePagoda(Vector3 pos, Transform parent,
            Material red, Material darkRed, Material wood, Material tile)
        {
            GameObject pagoda = new GameObject("Pagoda");
            pagoda.transform.position = pos;
            pagoda.transform.SetParent(parent);

            Material grayMat = MakeMat(null,0.28f); grayMat.color = new Color(0.54f,0.54f,0.58f);
            CreateBlock("Stone Base",pos+new Vector3(0,1.2f,0),new Vector3(22,2.5f,22),grayMat,pagoda.transform);

            // Five tiers (traditional pagoda has 5 stories)
            for (int i=0;i<5;i++)
            {
                float h = 3.5f+i*4.8f;
                float w = 16f-i*2.6f;
                CreateBlock("Tier "+i,    pos+new Vector3(0,h,0),         new Vector3(w,5f,w),     red,     pagoda.transform);
                CreateBlock("Eave T "+i,  pos+new Vector3(0,h+3.0f,0),    new Vector3(w+2f,0.55f,w+2f),tile,pagoda.transform);
                // Curved eave uplift
                foreach (float s in new[]{-1f,1f})
                    CreateBlock("Eave Tip "+i,
                        pos+new Vector3(s*(w/2f+0.5f),h+3.2f,0),
                        new Vector3(1.5f,0.55f,w+2f), tile, pagoda.transform).transform.rotation=Quaternion.Euler(0,0,s*(-10));
            }
            // Sorin (spire)
            CreateBlock("Sorin", pos+new Vector3(0,29f,0), new Vector3(0.55f,14f,0.55f), wood, pagoda.transform);
            // Wind bells at each tier corner
            Material bell = MakeMat(null,0.85f); bell.color = new Color(0.88f,0.74f,0.22f);
            for (int i=0;i<5;i++)
            {
                float h = 3.5f+i*4.8f+3.2f;
                float w = (16f-i*2.6f)/2f+1.2f;
                foreach (Vector3 bc in new[]{
                    new Vector3(-w,h, w),new Vector3(w,h, w),
                    new Vector3(-w,h,-w),new Vector3(w,h,-w)})
                    CreateBlock("Wind Bell",pos+bc,new Vector3(0.22f,0.38f,0.22f),bell,pagoda.transform);
            }
        }


        // ╔══════════════════════════════════════════════════════════════════════════╗
        //  PLAYER / CAMERA / COMBAT / UI
        // ╚══════════════════════════════════════════════════════════════════════════╝

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
            trail.time       = 0.24f;
            trail.startWidth = 0.60f;
            trail.endWidth   = 0f;
            trail.emitting   = false;
            trail.material   = new Material(Shader.Find("Sprites/Default"))
                               { color = new Color(0.22f,0.82f,1f,0.5f) };

            // Weapon — Leviathan Axe
            Material weaponMat = MakeMat(null,0.58f); weaponMat.color = new Color(0.35f,0.38f,0.42f);
            Material bladeMat  = MakeMat(null,0.92f); bladeMat.color  = new Color(0.78f,0.92f,1.0f);
            Material runeMat   = MakeMat(null,0.80f); runeMat.color   = new Color(0.30f,0.56f,1.0f);

            GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.name = "Axe Handle";
            handle.transform.SetParent(player.transform);
            handle.transform.localPosition = new Vector3(0.6f,0,0.5f);
            handle.transform.localScale    = new Vector3(0.12f,1.3f,0.12f);
            DestroyImmediate(handle.GetComponent<BoxCollider>());
            handle.GetComponent<Renderer>().sharedMaterial = weaponMat;

            // Rune carvings on haft
            for (int i=0;i<3;i++)
            {
                var rune = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rune.name = "Rune "+i;
                rune.transform.SetParent(handle.transform);
                rune.transform.localPosition = new Vector3(1.2f,(-0.3f+i*0.35f),0);
                rune.transform.localScale    = new Vector3(0.2f,0.15f,0.15f);
                DestroyImmediate(rune.GetComponent<BoxCollider>());
                rune.GetComponent<Renderer>().sharedMaterial = runeMat;
            }

            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Axe Blade";
            blade.transform.SetParent(handle.transform);
            blade.transform.localPosition = new Vector3(0,0.48f,2.2f);
            blade.transform.localScale    = new Vector3(7f,0.35f,4.5f);
            DestroyImmediate(blade.GetComponent<BoxCollider>());
            blade.GetComponent<Renderer>().sharedMaterial = bladeMat;

            // Blade rune glow
            AddPointLight(handle.transform, new Vector3(0,0.5f,0.3f),
                new Color(0.3f,0.55f,1f),1.2f,3f,"Blade Rune Glow");

            var combat = player.AddComponent<PlayerCombat>();
            combat.weaponTransform = handle.transform;
            return player;
        }

        private static Camera SetupCamera(Transform target)
        {
            GameObject rig = new GameObject("Isometric Camera Rig");
            Camera cam = rig.AddComponent<Camera>();
            cam.tag              = "MainCamera";
            cam.orthographic     = true;
            cam.orthographicSize = 12f;
            rig.AddComponent<CameraJuiceManager>();

            var iso = rig.AddComponent<IsometricCameraFollow>();
            SerializedObject so = new SerializedObject(iso);
            so.FindProperty("target").objectReferenceValue    = target;
            so.FindProperty("followOffset").vector3Value      = new Vector3(-15f,22f,-15f);
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
            trig.size = new Vector3(28,12,28);

            var roomCtrl    = room.AddComponent<RoomController>();
            var waveSpawner = room.AddComponent<WaveSpawner>();

            Material gateMat = new Material(Shader.Find("Standard")) { color = new Color(0.7f,0.08f,0.08f) };
            GameObject gate = CreateBlock("Arena Locking Gate",
                new Vector3(0,-5f,-18f),new Vector3(9,9,2.2f),gateMat,room.transform);
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


        // ╔══════════════════════════════════════════════════════════════════════════╗
        //  ENTITY HELPERS
        // ╚══════════════════════════════════════════════════════════════════════════╝

        private static void CreateAnimal(string name, Vector3 pos, Vector3 scale,
            Material mat, float speed, Transform parent)
        {
            GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
            a.name = name;
            a.transform.position   = pos;
            a.transform.localScale = scale;
            a.transform.SetParent(parent);
            a.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var rb = a.AddComponent<Rigidbody>();
            rb.mass        = 2f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            a.AddComponent<RoamingAnimal>().moveSpeed = speed;
            a.GetComponent<BoxCollider>().material =
                new PhysicsMaterial { bounciness=0,staticFriction=0,dynamicFriction=0 };
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

            GameObject weapon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            weapon.name = "Weapon";
            weapon.transform.SetParent(enemy.transform);
            weapon.transform.localPosition = new Vector3(0.6f,0,0.5f);
            weapon.transform.localScale    = new Vector3(0.22f,1.6f,0.22f);
            Object.DestroyImmediate(weapon.GetComponent<BoxCollider>());
            weapon.GetComponent<Renderer>().sharedMaterial =
                new Material(Shader.Find("Standard")){ color = new Color(0.15f,0.15f,0.18f) };
        }


        // ╔══════════════════════════════════════════════════════════════════════════╗
        //  PRIMITIVE HELPERS
        // ╚══════════════════════════════════════════════════════════════════════════╝

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
            SerializedObject   so   = new SerializedObject(asset[0]);
            SerializedProperty tags = so.FindProperty("tags");
            for (int i=0;i<tags.arraySize;++i)
                if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize-1).stringValue = tag;
            so.ApplyModifiedProperties();
            so.Update();
        }

        private static Texture2D GenNoise(Color a, Color b, float scale)
        {
            int sz = 256;
            Texture2D tex = new Texture2D(sz,sz);
            float ox = Random.Range(0f,9999f);
            float oy = Random.Range(0f,9999f);
            for (int x=0;x<sz;x++)
                for (int y=0;y<sz;y++)
                {
                    float s = Mathf.Pow(Mathf.PerlinNoise(
                        (float)x/sz*scale+ox, (float)y/sz*scale+oy), 1.6f);
                    tex.SetPixel(x,y,Color.Lerp(a,b,s));
                }
            tex.Apply();
            return tex;
        }
        // Overloads to match calls from BuildMasterpieceWorld

    private static void CreateGreatHall(Vector3 pos, Transform parent, Material wood, Material woodDark, Material thatch, Material stone, Material pathMat, Material bannerRed)
    {
        // Placeholder implementation using provided materials.
        // Build a basic hall structure.
        CreateBlock("Great Hall Base", pos, new Vector3(20f, 6f, 20f), wood, parent);
    }

    private static void CreateHut(Vector3 pos, Transform parent, Material wood, Material thatch, Material stone, float rotation)
    {
        GameObject hut = new GameObject("Nordic Hut");
        hut.transform.position = pos;
        hut.transform.rotation = Quaternion.Euler(0, rotation, 0);
        hut.transform.SetParent(parent);
        // Basic walls
        CreateBlock("Cabin Walls", pos + new Vector3(0, 1.5f, 0), new Vector3(5, 3, 5), wood, hut.transform);
        // Simple roof using thatch material
        CreateBlock("Roof", pos + new Vector3(0, 3.5f, 0), new Vector3(6, 0.5f, 6), thatch, hut.transform);
    }

    private static void CreateTorch(Vector3 pos, Transform parent)
    {
        GameObject torch = CreateBlock("Torch Pole", pos, new Vector3(0.3f, 4f, 0.3f), new Material(Shader.Find("Standard")){color = new Color(0.2f,0.1f,0.05f)}, parent);
        GameObject flame = new GameObject("Flame Lit");
        flame.transform.position = pos + new Vector3(0, 2f, 0);
        flame.transform.SetParent(torch.transform);
        Light l = flame.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(1f, 0.6f, 0.1f);
        l.intensity = 3f;
        l.range = 10f;
    }

    private static void CreateBanner(Vector3 pos, Transform parent, Material bannerMat, Material woodMat)
    {
        CreateBlock("Flag Pole", pos, new Vector3(0.2f, 6f, 0.2f), woodMat, parent);
        CreateBlock("Banner Cloth", pos + new Vector3(1.5f, 2f, 0), new Vector3(3f, 4f, 0.1f), bannerMat, parent);
    }

    private static void CreatePineTree(Vector3 pos, Transform parent, Material foliage, Material wood1, Material wood2)
    {
        GameObject tree = new GameObject("Nordic Pine");
        tree.transform.position = pos;
        tree.transform.SetParent(parent);
        CreateBlock("Trunk", pos + new Vector3(0, 1, 0), new Vector3(0.6f, 2, 0.6f), wood1, tree.transform);
        for(int i=0; i<3; i++) {
            GameObject layer = CreateBlock("NeedleLayer", pos + new Vector3(0, 2.5f + (i * 1.5f), 0), new Vector3(3f - (i*0.8f), 1.5f, 3f - (i*0.8f)), foliage, tree.transform);
            layer.transform.rotation = Quaternion.Euler(0, 45, 45);
        }
    }

    private static void CreateDryMoatApproach(Vector3 pos, Transform parent, Material a, Material b, Material c, Material d, Material e)
    {
        CreateBlock("Dry Moat", pos + new Vector3(0, 0, -20f), new Vector3(20f, 1f, 10f), a, parent);
    }

    private static void CreateNohTheatre(Vector3 pos, Transform parent, Material a, Material b, Material c, Material d)
    {
        CreateBlock("Noh Theatre", pos, new Vector3(10f, 4f, 10f), a, parent);
    }

    private static void CreateButsudanAltar(Vector3 pos, Transform parent, Material a, Material b, Material c, Material d)
    {
        CreateBlock("Butsudan Altar", pos, new Vector3(4f, 3f, 2f), a, parent);
    }

    private static void CreateOceanMenuUI(Transform parent, GameObject player, Camera cam)
    {
        Vector3 oceanPos = new Vector3(20000f, 0f, 20000f);
        
        // 1. Ocean Plane
        GameObject ocean = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ocean.name = "Ocean Menu Plane";
        ocean.transform.SetParent(parent);
        ocean.transform.position = oceanPos;
        ocean.transform.localScale = new Vector3(80f, 1f, 80f);
        Material waterMat = new Material(Shader.Find("Standard"));
        waterMat.color = new Color(0.05f, 0.15f, 0.25f, 0.95f);
        // Make it metallic/glossy for water look
        waterMat.SetFloat("_Metallic", 0.1f);
        waterMat.SetFloat("_Glossiness", 0.9f);
        ocean.GetComponent<Renderer>().sharedMaterial = waterMat;

        // 2. The Boat
        GameObject boat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        boat.name = "Menu Boat";
        boat.transform.SetParent(parent);
        boat.transform.position = oceanPos + new Vector3(0, 0.25f, 0);
        boat.transform.localScale = new Vector3(2.5f, 0.8f, 5f);
        Material boatMat = new Material(Shader.Find("Standard"));
        boatMat.color = new Color(0.18f, 0.1f, 0.05f);
        boat.GetComponent<Renderer>().sharedMaterial = boatMat;

        // Simple Mast
        CreateBlock("Mast", oceanPos + new Vector3(0, 2f, 1f), new Vector3(0.2f, 4f, 0.2f), boatMat, boat.transform);

        // 3. Setup Player on the Boat
        player.transform.position = oceanPos + new Vector3(0, 1.5f, -1f);
        player.transform.SetParent(boat.transform);
        // Face forward
        player.transform.rotation = Quaternion.Euler(0, 0, 0);
        
        // 4. Setup Camera View
        if (cam != null && boat != null) {
            var isoCam = cam.GetComponent<NordicWilds.CameraSystems.IsometricCameraFollow>();
            if (isoCam != null) {
                isoCam.target = boat.transform;
                cam.transform.position = boat.transform.position + new Vector3(-12f, 16f, -12f);
            }
        }

        // 5. Create UI Canvas Overlay
        GameObject canvasObj = new GameObject("MainMenuCanvas");
        canvasObj.transform.SetParent(parent);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject evtSystem = new GameObject("EventSystem");
            evtSystem.transform.SetParent(parent);
            evtSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            evtSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Title
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(canvasObj.transform, false);
        var titleTxt = titleObj.AddComponent<UnityEngine.UI.Text>();
        titleTxt.text = "NORDIC WILDS\nYamato Awakening";
        titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleTxt.fontSize = 80;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.color = Color.white;
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(800, 200);
        titleRect.anchoredPosition = Vector2.zero;

        // Fader Panel
        GameObject faderObj = new GameObject("FaderPanel");
        faderObj.transform.SetParent(canvasObj.transform, false);
        var faderImg = faderObj.AddComponent<UnityEngine.UI.Image>();
        faderImg.color = Color.black;
        var faderRect = faderObj.GetComponent<RectTransform>();
        faderRect.anchorMin = Vector2.zero;
        faderRect.anchorMax = Vector2.one;
        faderRect.offsetMin = Vector2.zero;
        faderRect.offsetMax = Vector2.zero;
        var faderGroup = faderObj.AddComponent<CanvasGroup>();
        faderGroup.alpha = 0f;
        faderGroup.blocksRaycasts = false;

        // Button
        GameObject btnObj = new GameObject("StartButton");
        btnObj.transform.SetParent(canvasObj.transform, false);
        var btnImg = btnObj.AddComponent<UnityEngine.UI.Image>();
        btnImg.color = new Color(0.6f, 0.1f, 0.1f);
        var btn = btnObj.AddComponent<UnityEngine.UI.Button>();
        var btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.3f);
        btnRect.anchorMax = new Vector2(0.5f, 0.3f);
        btnRect.sizeDelta = new Vector2(250, 60);
        btnRect.anchoredPosition = Vector2.zero;

        // Button Text
        GameObject btnTextObj = new GameObject("BtnText");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        var btnTxt = btnTextObj.AddComponent<UnityEngine.UI.Text>();
        btnTxt.text = "START GAME";
        btnTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        btnTxt.fontSize = 32;
        btnTxt.fontStyle = FontStyle.Bold;
        btnTxt.alignment = TextAnchor.MiddleCenter;
        btnTxt.color = Color.white;
        var btnTxtRect = btnTextObj.GetComponent<RectTransform>();
        btnTxtRect.anchorMin = Vector2.zero;
        btnTxtRect.anchorMax = Vector2.one;
        btnTxtRect.offsetMin = Vector2.zero;
        btnTxtRect.offsetMax = Vector2.zero;

        // Attach controller
        var mmc = canvasObj.AddComponent<NordicWilds.UI.MainMenuController>();
        mmc.faderGroup = faderGroup;
        mmc.boat = boat;
        mmc.player = player.transform;
        mmc.mainCamera = cam;
    }

}
}
