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
            foreach (string n in new[]{ "Nordic World Root","Player","Isometric Camera Rig",
                                         "MinimalHUDOverlay","WorldMapOverlay","YamatoArrivalDialogue",
                                         "MainMenuCanvas","EventSystem" })
            {
                GameObject existing = GameObject.Find(n);
                if (existing != null)
                    DestroyImmediate(existing);
            }

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
                dirLight.intensity      = 0.62f;
                dirLight.transform.rotation = Quaternion.Euler(28f, -62f, 0f);
                dirLight.shadows        = LightShadows.Soft;
                dirLight.shadowStrength = 0.45f;
                dirLight.shadowBias     = 0.04f;
            }

            // Trilight ambient: cold sky / neutral equator / cool-shadow ground
            RenderSettings.ambientMode         = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor     = new Color(0.35f, 0.44f, 0.65f);
            RenderSettings.ambientEquatorColor = new Color(0.26f, 0.34f, 0.52f);
            RenderSettings.ambientGroundColor  = new Color(0.14f, 0.22f, 0.38f);
            RenderSettings.ambientIntensity    = 0.75f;
            RenderSettings.reflectionIntensity = 0.25f;

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

            // --- Layer 3: open natural boundary ---
            // Removed the huge slab cliff walls; they looked like misplaced rectangles
            // and blocked movement/visibility around the generated world.

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
            for (int i = 0; i < 34; i++)
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

            // Tavern dialogue trigger inside the great hall — patrons share lore + hints.
            GameObject tavernZone = new GameObject("Tavern Dialogue Zone");
            tavernZone.transform.position = new Vector3(0f, 1.2f, 22f);
            tavernZone.transform.SetParent(worldRoot.transform);
            BoxCollider tavernCol = tavernZone.AddComponent<BoxCollider>();
            tavernCol.size = new Vector3(28f, 4f, 10f);
            tavernZone.AddComponent<NordicWilds.UI.TavernDialogue>();

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
            for (int i = 0; i < 92; i++)
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
            for (int i = 0; i < 16; i++)
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
            for (int i = 0; i < 10; i++)
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
            for (int i = 0; i < 8; i++)
            {
                Vector3 pos = new Vector3(Random.Range(-185f,185f), Random.Range(0.5f,2f), Random.Range(-185f,185f));
                var b = CreateBlock("Glacial Boulder", pos,
                    new Vector3(Random.Range(2f,7f), Random.Range(1.5f,5f), Random.Range(2f,6f)),
                    Random.value > 0.5f ? stoneMat : mossRockMat, worldRoot.transform);
                b.transform.rotation = Quaternion.Euler(
                    Random.Range(-20,20), Random.Range(0,360), Random.Range(-20,20));
            }

            // ── Frostheim → Yamato Portal ──────────────────────────────────────────
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

            // ── Frostheim Village Structure (palisade + raid encounter) ────────────
            CreateFrostheimVillagePerimeter(worldRoot.transform, woodMat, woodDarkMat, stoneMat, bannerRedMat, bannerBlackMat);
            CreateFrostheimRaidEncounter(worldRoot.transform);
            CreateLightSnowfall(worldRoot.transform);

            // ── YAMATO Realm ────────────────────────────────────────────────────────
            CreateYamatoRealm(worldRoot.transform);

            Vector3 forestBoatStart = new Vector3(-630f, 1.05f, -675f);
            Vector3 forestControlStart = new Vector3(-620f, 1.05f, -628f);
            CreateCoastalForestLanding(worldRoot.transform, forestControlStart);

            // ── Player, Camera, Encounter ───────────────────────────────────────────
            GameObject player = SetupPlayer("Player");

            Camera cam = SetupCamera(player.transform);
            SetupHadesEncounter(worldRoot.transform);

            var pController = player.GetComponent<PlayerController>();
            SerializedObject pSo = new SerializedObject(pController);
            pSo.FindProperty("isometricCameraTransform").objectReferenceValue = cam.transform;
            pSo.ApplyModifiedProperties();

            // Setup Ocean Main Menu Start
            CreateOceanMenuUI(worldRoot.transform, player, cam, forestBoatStart, forestControlStart);
            CreateRegionMusicController(worldRoot.transform);

            // Global mission tracker — pinned objective banner that persists across realms.
            new GameObject("MissionTracker").AddComponent<NordicWilds.UI.MissionTracker>();
            // Quick onboarding card (WASD / Shift / Space / LMB) — fades after intro.
            new GameObject("ControlsTutorial").AddComponent<NordicWilds.UI.ControlsTutorial>();

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
            CreateYamatoDistantBackdrop(O, worldRoot, stoneMat, whiteMat, bambooMat, mossMat);

            // Keep Yamato open and playable. The old cliff wall and raised hill blocks
            // looked like giant stray rectangles and blocked the shrine buildings.

            // Invisible play bounds
            GameObject yBounds = new GameObject("Yamato Boundaries"); yBounds.transform.SetParent(worldRoot);
            CreateInvisibleWall("YN", O+new Vector3(   0,5f,118f),new Vector3(232,22,2), yBounds.transform);
            CreateInvisibleWall("YS", O+new Vector3(   0,5f,-118f),new Vector3(232,22,2), yBounds.transform);
            CreateInvisibleWall("YW", O+new Vector3(-118f,5f,   0),new Vector3(2,22,232), yBounds.transform);
            CreateInvisibleWall("YE", O+new Vector3( 118f,5f,   0),new Vector3(2,22,232), yBounds.transform);

            // ── Dry Moat Castle Approach (south entry) ────────────────────────────
            CreateYamatoArrivalDock(O, worldRoot, waterMat, woodMat, darkStone);

            // ── Torii Gate Procession (Fushimi Inari-style) ───────────────────────
            //  Rule of Thirds: procession occupies the central third of the Z axis
            for (int i = 0; i < 6; i++)
                CreateToriiGate("Torii "+i, O+new Vector3(0,0f,(i*13f)-8f), redMat, woodMat, worldRoot);

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

            // ── Shrine Approach: flat walkable stone path ─────────────────────────
            for (int i = 0; i < 12; i++)
                CreateBlock("Shrine Path Stone "+i,
                    O+new Vector3(0,0.08f,52f+(i*3.4f)),
                    new Vector3(11f,0.12f,2.4f), stoneMat, worldRoot);

            // ── Shrine Complex on the Hill ─────────────────────────────────────────
            CreateMainShrine(    O+new Vector3(  0f,0f,76f), worldRoot, redMat,darkRedMat,woodMat,goldMat,stoneMat,pathMat);
            CreatePagoda(        O+new Vector3( 26f,0f,76f), worldRoot, redMat,darkRedMat,woodMat,tileGrayM);
            CreateBellTower(     O+new Vector3(-26f,0f,76f), worldRoot, woodMat,goldMat,stoneMat);
            CreateTeaHouse(O+new Vector3(-28f,0,34f), worldRoot, woodMat,paperMat,redMat,stoneMat);

            // ── Koi Pond with waterfall cascade ───────────────────────────────────
            CreateKoiPond(O+new Vector3(-30f,0,18f), worldRoot, stoneMat,waterMat,goldMat);
            CreateWaterfallCascade(O+new Vector3(-42f,0,22f), worldRoot, waterMat,stoneMat,mossMat);

            // ── Zen Garden ────────────────────────────────────────────────────────
            CreateZenGarden(O+new Vector3(30f,0,15f), worldRoot, stoneMat,sandMat);

            // ── Gorintō Stupa Field (stone memorial towers, layer of dread) ────────

            // ── Ornamental Bridge over the stream ─────────────────────────────────

            // ── Watermill on the stream ────────────────────────────────────────────

            // ── Wisteria Arcade (visual corridor, Layer 3 depth cue) ──────────────

            // ── Lantern Festival Pennants across the path ──────────────────────────

            // ── Tea House ─────────────────────────────────────────────────────────

            // ── Bamboo Groves ─────────────────────────────────────────────────────
            //  Keep bamboo to the side strips ONLY, well clear of the dock corridor and
            //  the new dojo position. (Previously bamboo crowded the dojo and overlapped
            //  the south landing area.)
            for (int i = 0; i < 9; i++)
            {
                float z = -36f + i * 14f;
                // Skip clumps that would crowd the dock approach (south) or the dojo (NE)
                if (z < -30f) continue;
                CreateBambooClump(O+new Vector3(-92f,0,z), worldRoot, bambooMat);
                if (z < 38f || z > 60f)
                    CreateBambooClump(O+new Vector3( 92f,0,z), worldRoot, bambooMat);
            }

            // ── Sakura Trees ──────────────────────────────────────────────────────
            Vector3[] sakuraSpots =
            {
                new Vector3(-42f,0.5f,10f), new Vector3(42f,0.5f,10f),
                new Vector3(-38f,0.5f,54f), new Vector3(38f,0.5f,54f),
                new Vector3(-14f,0.5f,98f), new Vector3(14f,0.5f,98f),
                new Vector3(-62f,0.5f,-6f), new Vector3(62f,0.5f,-6f)
            };
            foreach (Vector3 spot in sakuraSpots)
                CreateSakuraTree(O + spot, worldRoot, sakuraMat, woodMat);

            // ── Bamboo Perimeter Fence ─────────────────────────────────────────────

            for (int i=0;i<26;i++)
            {
                float x = -65f + i * 5f;
                if (Mathf.Abs(x) < 9f)
                    continue;
                CreateBambooFenceSection(O+new Vector3(x,0,-65f), worldRoot,bambooMat,woodMat,false);
            }
            for (int i=0;i<26;i++) CreateBambooFenceSection(O+new Vector3(-65f+i*5f,0, 65f), worldRoot,bambooMat,woodMat,false);
            for (int i=0;i<26;i++) CreateBambooFenceSection(O+new Vector3(-65f,0,-65f+i*5f), worldRoot,bambooMat,woodMat,true);
            for (int i=0;i<26;i++) CreateBambooFenceSection(O+new Vector3( 65f,0,-65f+i*5f), worldRoot,bambooMat,woodMat,true);
            // Particle weather is intentionally off until we have proper petal/mist
            // sprites. The old square billboards read as random floating rectangles.

            // ── Yamato Wildlife ───────────────────────────────────────────────────
            Material deerMat   = MakeMat(null,0.12f); deerMat.color  = new Color(0.64f,0.46f,0.28f);
            Material craneMat  = MakeMat(null,0.12f); craneMat.color = Color.white;
            Material foxMatJ   = MakeMat(null,0.10f); foxMatJ.color  = new Color(0.90f,0.55f,0.20f); // red fox
            CreateAnimal("Deer",  O+new Vector3(-52f,0.5f,42f),new Vector3(0.7f,1.4f,1.6f),deerMat, 2.2f,worldRoot);
            CreateAnimal("Crane", O+new Vector3(-28f,0.4f,18f),new Vector3(0.6f,1.0f,0.4f),craneMat,1.6f,worldRoot);
            CreateAnimal("Crane", O+new Vector3(-34f,0.4f,21f),new Vector3(0.6f,1.0f,0.4f),craneMat,1.6f,worldRoot);
            CreateAnimal("Red Fox",O+new Vector3(46f,0.4f,12f),new Vector3(0.8f,1.0f,1.5f),foxMatJ, 2.4f,worldRoot);

            // ── Yamato Enemies ─────────────────────────────────────────────────────
            Material samuraiMat = MakeMat(null,0.28f); samuraiMat.color = new Color(0.12f,0.18f,0.32f);
            Material ninjaMat   = MakeMat(null,0.04f); ninjaMat.color   = new Color(0.04f,0.04f,0.05f);
            Material ronin      = MakeMat(null,0.15f); ronin.color      = new Color(0.38f,0.32f,0.24f); // wandering ronin
            Material oniMat     = MakeMat(null,0.08f); oniMat.color     = new Color(0.70f,0.08f,0.12f); // Oni demon
            Material yHitFlash  = MakeMat(null,0.00f); yHitFlash.color  = Color.white;

            Vector3[] npcSpots =
            {
                new Vector3(-12f,0.6f,20f), new Vector3(12f,0.6f,20f),
                new Vector3(-18f,0.6f,58f), new Vector3(18f,0.6f,58f),
                new Vector3(-34f,0.6f,30f), new Vector3(34f,0.6f,30f)
            };
            for (int i=0;i<npcSpots.Length;i++)
                CreateNeutralNpc(i < 4 ? "Samurai NPC" : "Ronin NPC", O+npcSpots[i], i < 4 ? samuraiMat : ronin, worldRoot);
            CreateBambooDojoEntranceAndInterior(O, worldRoot, woodMat, woodLightMat, paperMat, redMat, darkRedMat, stoneMat, bambooMat, oniMat, yHitFlash);

            // ── Return Portal → Frostheim ──────────────────────────────────────────
            //  Pushed far past the shrine complex so the player has to journey through
            //  Yamato to reach it. Surrounded by warding stones and given a heavier glow.
            Vector3 portalPos = O + new Vector3(0f, 0f, 108f);
            CreatePortalGateway(
                "Return Portal (To Frostheim)",
                portalPos,
                worldRoot,
                darkRedMat,
                new Color(0.32f, 0.62f, 1.00f, 0.85f),
                new Color(0.28f, 0.72f, 1.00f),
                new Color(0.78f, 0.92f, 1.00f),
                new Vector3(0f, 2f, -18f),
                isReturn: true);

            // Warding stone circle around the portal — makes it feel like a place of power
            Material wardStone = MakeMat(null, 0.32f); wardStone.color = new Color(0.18f, 0.20f, 0.26f);
            for (int i = 0; i < 10; i++)
            {
                float ang = i * 36f;
                Vector3 off = Quaternion.Euler(0f, ang, 0f) * new Vector3(8.5f, 0f, 0f);
                GameObject ward = CreateBlock("Portal Ward Stone " + i,
                    portalPos + off + new Vector3(0f, 1.4f, 0f),
                    new Vector3(1.0f, Mathf.Lerp(2.6f, 4.4f, (i % 3) / 2f), 0.9f),
                    wardStone, worldRoot);
                ward.transform.rotation = Quaternion.Euler(Random.Range(-4f, 4f), ang, Random.Range(-4f, 4f));
            }
            // Raised stone platform under the portal
            CreateBlock("Portal Platform", portalPos + new Vector3(0f, -0.20f, 0f), new Vector3(14f, 0.4f, 14f), wardStone, worldRoot);
            CreateBlock("Portal Inner Disc", portalPos + new Vector3(0f, 0.05f, 0f), new Vector3(9f, 0.12f, 9f), darkRedMat, worldRoot);

            // Two extra strong point lights — the portal should be visible from far away
            // and feel intimidating to approach.
            AddPointLight(worldRoot, portalPos + new Vector3(0f, 4f, 0f), new Color(0.30f, 0.70f, 1.0f), 9f, 60f, "Portal Far Glow");
            AddPointLight(worldRoot, portalPos + new Vector3(0f, 1f, -3f), new Color(0.50f, 0.90f, 1.0f), 6f, 30f, "Portal Approach Glow");

            // A second dramatic torii framing the portal approach
            CreateToriiGate("Portal Outer Torii", portalPos + new Vector3(0f, 0f, -22f), darkRedMat, woodMat, worldRoot);
            CreateToriiGate("Portal Inner Torii", portalPos + new Vector3(0f, 0f, -10f), darkRedMat, woodMat, worldRoot);
        }

        private static void CreateCoastalForestLanding(Transform parent, Vector3 startPos)
        {
            Vector3 O = new Vector3(-620f, 0f, -610f);
            GameObject root = new GameObject("Coastal Forest Landing");
            root.transform.SetParent(parent);

            Material grass = MakeMat(null, 0.08f); grass.color = new Color(0.18f, 0.36f, 0.22f);
            Material moss = MakeMat(null, 0.08f); moss.color = new Color(0.12f, 0.28f, 0.16f);
            Material sand = MakeMat(null, 0.04f); sand.color = new Color(0.54f, 0.48f, 0.34f);
            Material water = MakeMat(null, 0.88f); water.color = new Color(0.08f, 0.28f, 0.36f, 0.82f);
            Material wood = MakeMat(null, 0.20f); wood.color = new Color(0.24f, 0.13f, 0.06f);
            Material darkWood = MakeMat(null, 0.12f); darkWood.color = new Color(0.11f, 0.07f, 0.04f);
            Material pine = MakeMat(null, 0.05f); pine.color = new Color(0.08f, 0.24f, 0.15f);
            Material autumn = MakeMat(null, 0.06f); autumn.color = new Color(0.72f, 0.34f, 0.12f);
            Material oak = MakeMat(null, 0.06f); oak.color = new Color(0.18f, 0.34f, 0.14f);
            Material birch = MakeMat(null, 0.06f); birch.color = new Color(0.56f, 0.69f, 0.42f);
            Material birchBark = MakeMat(null, 0.10f); birchBark.color = new Color(0.78f, 0.76f, 0.66f);
            Material stone = MakeMat(null, 0.26f); stone.color = new Color(0.34f, 0.38f, 0.38f);
            Material snow = MakeMat(null, 0.06f); snow.color = new Color(0.78f, 0.88f, 0.92f);
            Material foeMat = MakeMat(null, 0.10f); foeMat.color = new Color(0.22f, 0.25f, 0.30f);
            Material forestBossMat = MakeMat(null, 0.32f); forestBossMat.color = new Color(0.62f, 0.18f, 0.08f);
            Material hitFlash = MakeMat(null, 0.00f); hitFlash.color = Color.white;
            Material trailMat = MakeMat(null, 0.06f); trailMat.color = new Color(0.46f, 0.37f, 0.24f);
            Material flowerMat = MakeMat(null, 0.04f); flowerMat.color = new Color(0.64f, 0.72f, 0.50f);
            Material emberMat = MakeMat(null, 0.22f); emberMat.color = new Color(1.0f, 0.38f, 0.08f);

            CreateBlock("Forest Landing Ground", O + new Vector3(0f, -0.5f, 0f), new Vector3(178f, 1f, 154f), grass, root.transform);
            CreateBlock("Forest Landing Moss", O + new Vector3(0f, -1.15f, 0f), new Vector3(178f, 1f, 154f), moss, root.transform);
            CreateBlock("Landing Beach", O + new Vector3(0f, -0.44f, -47f), new Vector3(132f, 0.16f, 22f), sand, root.transform);
            CreateSceneryBlock("Cold Coastal Water", O + new Vector3(0f, -0.55f, -78f), new Vector3(170f, 0.18f, 58f), water, root.transform);
            CreateForestSafetyRim(O, root.transform, stone, moss, pine, darkWood);

            CreateDock(O + new Vector3(0f, 0f, -46f), root.transform, wood, darkWood);
            GameObject landedBoat = CreateLandedLongship(O + new Vector3(-10f, 0.35f, -68f), root.transform);
            CreateForestBackdrop(O, root.transform, stone, snow, pine, moss);

            CreateBlock("Main Forest Trail South", O + new Vector3(0f, 0.04f, -25f), new Vector3(8.2f, 0.08f, 46f), trailMat, root.transform);
            CreateBlock("Main Forest Trail North", O + new Vector3(0f, 0.045f, 22f), new Vector3(7.2f, 0.08f, 54f), trailMat, root.transform);
            CreateBlock("Trail Bend West", O + new Vector3(-19f, 0.05f, 18f), new Vector3(34f, 0.08f, 6f), trailMat, root.transform);
            CreateBlock("Trail Bend East", O + new Vector3(18f, 0.05f, 32f), new Vector3(30f, 0.08f, 6f), trailMat, root.transform);
            CreateBlock("Dock Trail Boards", O + new Vector3(0f, 0.08f, -45f), new Vector3(7f, 0.12f, 11f), wood, root.transform);

            Vector3[] treePositions =
            {
                new Vector3(-54f,0f,-22f), new Vector3(-49f,0f,-6f), new Vector3(-52f,0f,14f), new Vector3(-44f,0f,32f),
                new Vector3(54f,0f,-18f), new Vector3(48f,0f,2f), new Vector3(52f,0f,22f), new Vector3(42f,0f,39f),
                new Vector3(-30f,0f,48f), new Vector3(-16f,0f,54f), new Vector3(18f,0f,54f), new Vector3(34f,0f,48f)
            };
            foreach (Vector3 p in treePositions)
                CreatePineTree(O + p, root.transform, pine, darkWood, wood);

            CreateForestTreeDiversity(O, root.transform, pine, oak, autumn, birch, darkWood, wood, birchBark);
            CreateForestClutter(O, root.transform, pine, darkWood, wood, stone, moss, flowerMat);
            CreateAbandonedForestSettlement(O, root.transform, wood, darkWood, stone, moss, trailMat, emberMat);

            GameObject leaf = CreateLeafCompanion(O + new Vector3(3.5f, 1.0f, -16f), root.transform);

            ForestArtifact[] artifacts = new ForestArtifact[3];
            artifacts[0] = CreateForestArtifact("Artifact of Roots", O + new Vector3(-36f, 0.85f, 14f), root.transform);
            artifacts[1] = CreateForestArtifact("Artifact of Tides", O + new Vector3(35f, 0.85f, 29f), root.transform);
            artifacts[2] = CreateForestArtifact("Artifact of Dawn", O + new Vector3(-8f, 0.85f, 48f), root.transform);
            foreach (ForestArtifact artifact in artifacts)
            {
                if (artifact != null)
                    artifact.gameObject.SetActive(false);
            }
            CreateArtifactNest(O + new Vector3(-36f, 0f, 14f), root.transform, stone, moss, darkWood, flowerMat);
            CreateArtifactNest(O + new Vector3(35f, 0f, 29f), root.transform, stone, moss, darkWood, flowerMat);
            CreateArtifactNest(O + new Vector3(-8f, 0f, 48f), root.transform, stone, moss, darkWood, flowerMat);

            GameObject trailFocus = new GameObject("Trail Camera Focus");
            trailFocus.transform.SetParent(root.transform);
            trailFocus.transform.position = O + new Vector3(0f, 1f, -34f);

            CreateForestFinalBossClearing(O + new Vector3(0f, 0f, 58f), root.transform, stone, moss, darkWood, emberMat);

            Health[] foes = new Health[5];
            Vector3[] foePositions =
            {
                new Vector3(-16f,1f,-12f), new Vector3(16f,1f,-8f), new Vector3(-20f,1f,16f),
                new Vector3(20f,1f,18f), new Vector3(0f,1f,30f)
            };

            for (int i = 0; i < foes.Length; i++)
            {
                GameObject foe = CreateEnemy("Forest Foe " + (i + 1), O + foePositions[i], foeMat, hitFlash, root.transform);
                foes[i] = foe.GetComponent<Health>();
                foe.SetActive(false);
            }

            GameObject finalBoss = CreateEnemy("The Ashen Jarl", O + new Vector3(0f, 1f, 56f), forestBossMat, hitFlash, root.transform);
            finalBoss.transform.localScale = new Vector3(2.05f, 2.05f, 2.05f);
            Health finalBossHealth = finalBoss.GetComponent<Health>();
            if (finalBossHealth != null)
            {
                SerializedObject healthSo = new SerializedObject(finalBossHealth);
                healthSo.FindProperty("maxHealth").floatValue = 440f;
                healthSo.ApplyModifiedProperties();
            }
            finalBoss.SetActive(false);

            GameObject boatTrigger = new GameObject("Boat Boarding Interaction");
            boatTrigger.transform.SetParent(root.transform);
            boatTrigger.transform.position = O + new Vector3(-10f, 1.15f, -65f);
            BoxCollider boatCollider = boatTrigger.AddComponent<BoxCollider>();
            boatCollider.size = new Vector3(10f, 4f, 10f);
            ForestBoatInteraction boatInteraction = boatTrigger.AddComponent<ForestBoatInteraction>();
            if (landedBoat != null)
                boatInteraction.boatRoot = landedBoat.transform;

            if (landedBoat != null)
            {
                boatTrigger.transform.SetParent(landedBoat.transform, worldPositionStays: true);
                boatTrigger.transform.localRotation = Quaternion.identity;
            }

            var quest = root.AddComponent<ForestQuestController>();
            quest.leaf = leaf.transform;
            quest.artifacts = artifacts;
            quest.protectors = foes;
            quest.finalBoss = finalBossHealth;
            quest.trailFocus = trailFocus.transform;
            quest.boatInteraction = boatInteraction;
            quest.yamatoDestination = new Vector3(10000f, 1.05f, 9980f);
            quest.yamatoBoatLandingPoint = new Vector3(10000f, 1.05f, 9958f);
            quest.yamatoBoatApproachPoint = new Vector3(10000f, 0.35f, 9936f);
            quest.yamatoDockedBoatPoint = new Vector3(10000f, 0.35f, 9958f);
            quest.boatSeaDirection = new Vector3(0f, 0f, -1f);
            quest.landingActivationPoint = startPos;

            AddPointLight(root.transform, O + new Vector3(0f, 3f, -42f), new Color(0.45f, 0.66f, 0.82f), 2.4f, 18f, "Coastal Moon Wash");
        }

        private static void CreateDock(Vector3 pos, Transform parent, Material wood, Material darkWood)
        {
            GameObject dock = new GameObject("Landing Dock");
            dock.transform.SetParent(parent);
            for (int i = 0; i < 8; i++)
                CreateBlock("Dock Plank " + i, pos + new Vector3(0f, 0.25f, -i * 3.2f), new Vector3(8.5f, 0.28f, 2.4f), wood, dock.transform);

            for (int i = 0; i < 5; i++)
            {
                float z = -i * 5.8f;
                CreateBlock("Dock Post L " + i, pos + new Vector3(-4.8f, -0.6f, z), new Vector3(0.45f, 2.4f, 0.45f), darkWood, dock.transform);
                CreateBlock("Dock Post R " + i, pos + new Vector3(4.8f, -0.6f, z), new Vector3(0.45f, 2.4f, 0.45f), darkWood, dock.transform);
            }
        }

        private static GameObject CreateLandedLongship(Vector3 pos, Transform parent)
        {
            GameObject boat = new GameObject("Landed Menu Longship");
            boat.transform.SetParent(parent);
            boat.transform.position = pos;
            boat.transform.rotation = Quaternion.Euler(0f, 18f, 0f);

            if (CreateCinematicShip(boat.transform) == null)
                CreateFallbackLongship(boat.transform);

            return boat;
        }

        private static GameObject CreateLeafCompanion(Vector3 pos, Transform parent)
        {
            GameObject leaf = new GameObject("Leaf");
            leaf.transform.SetParent(parent);
            leaf.transform.position = pos;

            Material body = MakeMat(null, 0.18f); body.color = new Color(0.18f, 0.46f, 0.20f);
            Material cloak = MakeMat(null, 0.10f); cloak.color = new Color(0.10f, 0.28f, 0.13f);
            Material skin = MakeMat(null, 0.12f); skin.color = new Color(0.70f, 0.55f, 0.36f);
            Material glow = MakeMat(null, 0.45f); glow.color = new Color(0.68f, 1.00f, 0.46f);

            CreateBlock("Leaf Body", pos + new Vector3(0f, 0.8f, 0f), new Vector3(0.82f, 1.35f, 0.62f), body, leaf.transform, false);
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Leaf Head";
            head.transform.SetParent(leaf.transform);
            head.transform.position = pos + new Vector3(0f, 1.72f, 0f);
            head.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            head.GetComponent<Renderer>().sharedMaterial = skin;
            DestroyImmediate(head.GetComponent<Collider>());

            CreateBlock("Leaf Cloak", pos + new Vector3(0f, 0.85f, 0.18f), new Vector3(1.0f, 1.15f, 0.18f), cloak, leaf.transform, false);
            GameObject sprout = CreateBlock("Leaf Sprout", pos + new Vector3(0f, 2.12f, 0f), new Vector3(0.9f, 0.12f, 0.36f), glow, leaf.transform, false);
            sprout.transform.rotation = Quaternion.Euler(0f, 20f, -18f);
            AddPointLight(leaf.transform, new Vector3(0f, 2.0f, 0f), new Color(0.45f, 1f, 0.45f), 0.9f, 4f, "Leaf Gentle Glow");
            return leaf;
        }

        private static ForestArtifact CreateForestArtifact(string name, Vector3 pos, Transform parent)
        {
            Material baseMat = MakeMat(null, 0.42f);
            baseMat.color = new Color(0.42f, 0.85f, 0.72f);
            Material coreMat = MakeMat(null, 0.55f);
            coreMat.color = new Color(0.95f, 0.86f, 0.45f);

            GameObject artifact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            artifact.name = name;
            artifact.transform.SetParent(parent);
            artifact.transform.position = pos;
            artifact.transform.localScale = new Vector3(0.72f, 0.72f, 0.72f);
            artifact.GetComponent<Renderer>().sharedMaterial = baseMat;

            ForestArtifact script = artifact.AddComponent<ForestArtifact>();
            script.ArtifactName = name;

            SphereCollider trigger = artifact.GetComponent<SphereCollider>();
            if (trigger != null)
            {
                trigger.isTrigger = true;
                trigger.radius = 2.8f;
            }

            GameObject shard = CreateSceneryBlock("Artifact Inner Shard", pos + new Vector3(0f, 0.05f, 0f), new Vector3(0.24f, 0.88f, 0.24f), coreMat, artifact.transform);
            shard.transform.rotation = Quaternion.Euler(18f, 32f, 8f);
            AddPointLight(artifact.transform, Vector3.zero, new Color(0.72f, 1f, 0.72f), 0.7f, 5.2f, name + " Glow");
            return script;
        }

        private static void CreateForestTreeDiversity(Vector3 O, Transform parent, Material pine, Material oak, Material autumn, Material birch, Material darkWood, Material wood, Material birchBark)
        {
            Vector3[] heroTrees =
            {
                new Vector3(-62f,0f,42f), new Vector3(58f,0f,44f), new Vector3(-66f,0f,-4f), new Vector3(64f,0f,4f),
                new Vector3(-40f,0f,-30f), new Vector3(42f,0f,-30f), new Vector3(-24f,0f,62f), new Vector3(27f,0f,60f)
            };

            for (int i = 0; i < heroTrees.Length; i++)
            {
                if (i % 4 == 0)
                    CreateOakTree(O + heroTrees[i], parent, oak, darkWood);
                else if (i % 4 == 1)
                    CreateAutumnTree(O + heroTrees[i], parent, autumn, darkWood);
                else if (i % 4 == 2)
                    CreateBirchTree(O + heroTrees[i], parent, birch, birchBark);
                else
                    CreatePineTree(O + heroTrees[i], parent, pine, darkWood, wood);
            }

            for (int i = 0; i < 70; i++)
            {
                Vector3 local = new Vector3(Random.Range(-76f, 76f), 0f, Random.Range(-48f, 68f));
                if (IsOnForestTrail(local))
                    continue;

                int type = i % 5;
                if (type == 0)
                    CreateAutumnTree(O + local, parent, autumn, darkWood);
                else if (type == 1)
                    CreateOakTree(O + local, parent, oak, darkWood);
                else if (type == 2)
                    CreateBirchTree(O + local, parent, birch, birchBark);
                else
                    CreatePineTree(O + local, parent, pine, darkWood, wood);
            }
        }

        private static void CreateAutumnTree(Vector3 pos, Transform parent, Material foliage, Material wood)
        {
            GameObject tree = new GameObject("Autumn Tree");
            tree.transform.position = pos;
            tree.transform.SetParent(parent);

            CreateBlock("Autumn Trunk", pos + new Vector3(0f, 1.35f, 0f), new Vector3(0.7f, 2.7f, 0.7f), wood, tree.transform);
            CreateSceneryBlock("Autumn Crown A", pos + new Vector3(0f, 3.3f, 0f), new Vector3(3.6f, 2.2f, 3.6f), foliage, tree.transform);
            CreateSceneryBlock("Autumn Crown B", pos + new Vector3(-0.9f, 2.85f, 0.4f), new Vector3(2.5f, 1.7f, 2.5f), foliage, tree.transform);
            CreateSceneryBlock("Autumn Crown C", pos + new Vector3(0.9f, 2.95f, -0.35f), new Vector3(2.4f, 1.6f, 2.4f), foliage, tree.transform);
        }

        private static void CreateOakTree(Vector3 pos, Transform parent, Material foliage, Material wood)
        {
            GameObject tree = new GameObject("Old Oak");
            tree.transform.position = pos;
            tree.transform.SetParent(parent);

            CreateBlock("Oak Trunk", pos + new Vector3(0f, 1.15f, 0f), new Vector3(1.0f, 2.3f, 1.0f), wood, tree.transform);
            CreateBlock("Oak Low Branch", pos + new Vector3(1.15f, 2.35f, 0.15f), new Vector3(2.8f, 0.32f, 0.32f), wood, tree.transform).transform.rotation = Quaternion.Euler(0f, 20f, 8f);
            CreateBlock("Oak Bent Branch", pos + new Vector3(-1.05f, 2.55f, -0.25f), new Vector3(2.5f, 0.28f, 0.28f), wood, tree.transform).transform.rotation = Quaternion.Euler(0f, -24f, -10f);
            CreateSceneryBlock("Oak Crown Main", pos + new Vector3(0f, 3.45f, 0f), new Vector3(4.2f, 2.4f, 4.2f), foliage, tree.transform);
            CreateSceneryBlock("Oak Crown Side", pos + new Vector3(1.55f, 3.0f, 0.2f), new Vector3(2.8f, 1.7f, 2.8f), foliage, tree.transform);
        }

        private static void CreateBirchTree(Vector3 pos, Transform parent, Material foliage, Material bark)
        {
            GameObject tree = new GameObject("Birch Tree");
            tree.transform.position = pos;
            tree.transform.SetParent(parent);

            CreateBlock("Birch Pale Trunk", pos + new Vector3(0f, 1.45f, 0f), new Vector3(0.48f, 2.9f, 0.48f), bark, tree.transform);
            CreateBlock("Birch Dark Scar A", pos + new Vector3(0f, 1.15f, -0.25f), new Vector3(0.52f, 0.08f, 0.04f), bark, tree.transform);
            CreateBlock("Birch Dark Scar B", pos + new Vector3(0f, 1.95f, -0.25f), new Vector3(0.52f, 0.08f, 0.04f), bark, tree.transform);
            CreateSceneryBlock("Birch Leaf Top", pos + new Vector3(0f, 3.55f, 0f), new Vector3(2.6f, 2.1f, 2.6f), foliage, tree.transform);
            CreateSceneryBlock("Birch Leaf Low", pos + new Vector3(0.55f, 2.85f, 0.15f), new Vector3(2.0f, 1.5f, 2.0f), foliage, tree.transform);
        }

        private static void CreateAbandonedForestSettlement(Vector3 O, Transform parent, Material wood, Material darkWood, Material stone, Material moss, Material trail, Material ember)
        {
            GameObject settlement = new GameObject("Abandoned Forest Settlement");
            settlement.transform.SetParent(parent);

            CreateAbandonedHut(O + new Vector3(-50f, 0f, -30f), settlement.transform, wood, darkWood, stone, -18f);
            CreateAbandonedHut(O + new Vector3(49f, 0f, -24f), settlement.transform, wood, darkWood, stone, 22f);
            CreateAbandonedHut(O + new Vector3(-59f, 0f, 26f), settlement.transform, wood, darkWood, stone, 12f);
            CreateAbandonedHut(O + new Vector3(58f, 0f, 31f), settlement.transform, wood, darkWood, stone, -28f);

            CreateCampfire(O + new Vector3(-25f, 0f, -23f), settlement.transform, stone, darkWood, ember, true);
            CreateCampfire(O + new Vector3(28f, 0f, 10f), settlement.transform, stone, darkWood, ember, false);
            CreateBench(O + new Vector3(-31f, 0f, -18f), settlement.transform, wood, darkWood, 28f);
            CreateBench(O + new Vector3(24f, 0f, 15f), settlement.transform, wood, darkWood, -12f);
            CreateBench(O + new Vector3(41f, 0f, -13f), settlement.transform, wood, darkWood, 78f);

            for (int i = 0; i < 18; i++)
            {
                Vector3 local = new Vector3(Random.Range(-60f, 60f), 0.08f, Random.Range(-38f, 44f));
                if (IsOnForestTrail(local))
                    continue;

                GameObject plank = CreateBlock("Abandoned Plank", O + local, new Vector3(Random.Range(1.2f, 2.8f), 0.12f, 0.35f), i % 3 == 0 ? moss : darkWood, settlement.transform);
                plank.transform.rotation = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(0f, 360f), Random.Range(-5f, 5f));
            }

            CreateBlock("Old Footpath West", O + new Vector3(-38f, 0.06f, -17f), new Vector3(20f, 0.08f, 3.0f), trail, settlement.transform);
            CreateBlock("Old Footpath East", O + new Vector3(38f, 0.06f, -8f), new Vector3(22f, 0.08f, 3.0f), trail, settlement.transform);
        }

        private static void CreateAbandonedHut(Vector3 pos, Transform parent, Material wood, Material darkWood, Material stone, float rotation)
        {
            GameObject hut = new GameObject("Abandoned Forest Hut");
            hut.transform.position = pos;
            hut.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            hut.transform.SetParent(parent);

            CreateBlock("Hut Stone Footing", pos + new Vector3(0f, 0.18f, 0f), new Vector3(6.2f, 0.36f, 5.4f), stone, hut.transform);
            CreateBlock("Hut Back Wall", pos + new Vector3(0f, 1.55f, 2.5f), new Vector3(6f, 2.9f, 0.42f), wood, hut.transform);
            CreateBlock("Hut Left Wall", pos + new Vector3(-3f, 1.45f, 0f), new Vector3(0.42f, 2.7f, 5f), wood, hut.transform);
            CreateBlock("Hut Right Broken Wall", pos + new Vector3(3f, 1.0f, -0.4f), new Vector3(0.42f, 1.8f, 3.7f), wood, hut.transform);
            CreateBlock("Hut Roof Left", pos + new Vector3(-1.5f, 3.55f, 0f), new Vector3(3.6f, 0.45f, 6.2f), darkWood, hut.transform).transform.rotation = Quaternion.Euler(0f, rotation, -12f);
            CreateBlock("Hut Roof Right Fallen", pos + new Vector3(1.9f, 2.0f, -0.6f), new Vector3(3.4f, 0.34f, 4.8f), darkWood, hut.transform).transform.rotation = Quaternion.Euler(0f, rotation + 14f, 22f);
        }

        private static void CreateCampfire(Vector3 pos, Transform parent, Material stone, Material wood, Material ember, bool lit)
        {
            GameObject fire = new GameObject(lit ? "Low Campfire" : "Cold Campfire");
            fire.transform.position = pos;
            fire.transform.SetParent(parent);

            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(1.15f, 0.18f, 0f);
                CreateBlock("Fire Ring Stone", pos + offset, new Vector3(0.42f, 0.28f, 0.42f), stone, fire.transform);
            }

            CreateBlock("Charred Log A", pos + new Vector3(0f, 0.28f, 0f), new Vector3(2.2f, 0.22f, 0.22f), wood, fire.transform).transform.rotation = Quaternion.Euler(0f, 25f, 0f);
            CreateBlock("Charred Log B", pos + new Vector3(0f, 0.34f, 0f), new Vector3(1.8f, 0.2f, 0.2f), wood, fire.transform).transform.rotation = Quaternion.Euler(0f, -34f, 0f);

            if (lit)
            {
                CreateSceneryBlock("Small Ember Glow", pos + new Vector3(0f, 0.58f, 0f), new Vector3(0.85f, 0.5f, 0.85f), ember, fire.transform);
                AddPointLight(fire.transform, new Vector3(0f, 1.1f, 0f), new Color(1f, 0.38f, 0.12f), 1.7f, 7f, "Campfire Glow");
            }
        }

        private static void CreateBench(Vector3 pos, Transform parent, Material wood, Material darkWood, float rotation)
        {
            GameObject bench = new GameObject("Abandoned Bench");
            bench.transform.position = pos;
            bench.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            bench.transform.SetParent(parent);

            CreateBlock("Bench Seat", pos + new Vector3(0f, 0.72f, 0f), new Vector3(3.6f, 0.24f, 0.74f), wood, bench.transform);
            CreateBlock("Bench Back", pos + new Vector3(0f, 1.18f, 0.44f), new Vector3(3.6f, 0.22f, 0.28f), wood, bench.transform);
            CreateBlock("Bench Leg L", pos + new Vector3(-1.35f, 0.34f, 0f), new Vector3(0.22f, 0.68f, 0.22f), darkWood, bench.transform);
            CreateBlock("Bench Leg R", pos + new Vector3(1.35f, 0.34f, 0f), new Vector3(0.22f, 0.68f, 0.22f), darkWood, bench.transform);
        }

        private static void CreateForestClutter(Vector3 O, Transform parent, Material pine, Material darkWood, Material wood, Material stone, Material moss, Material flower)
        {
            for (int i = 0; i < 180; i++)
            {
                Vector3 local = new Vector3(Random.Range(-60f, 60f), 0f, Random.Range(-36f, 52f));
                if (IsOnForestTrail(local))
                    continue;

                CreatePineTree(O + local, parent, pine, darkWood, wood);
            }

            for (int i = 0; i < 135; i++)
            {
                Vector3 local = new Vector3(Random.Range(-58f, 58f), 0.18f, Random.Range(-38f, 50f));
                if (IsOnForestTrail(local))
                    continue;

                GameObject rock = CreateBlock("Forest Stone", O + local, new Vector3(Random.Range(0.55f, 1.7f), Random.Range(0.3f, 0.9f), Random.Range(0.55f, 1.7f)), i % 3 == 0 ? moss : stone, parent);
                rock.transform.rotation = Quaternion.Euler(Random.Range(-8f, 8f), Random.Range(0f, 360f), Random.Range(-8f, 8f));
            }

            for (int i = 0; i < 70; i++)
            {
                Vector3 local = new Vector3(Random.Range(-55f, 55f), 0.35f, Random.Range(-35f, 48f));
                if (IsOnForestTrail(local))
                    continue;

                GameObject log = CreateBlock("Fallen Forest Log", O + local, new Vector3(0.55f, 0.55f, Random.Range(3.0f, 7.5f)), darkWood, parent);
                log.transform.rotation = Quaternion.Euler(Random.Range(-6f, 6f), Random.Range(0f, 360f), Random.Range(-6f, 6f));
            }

            for (int i = 0; i < 190; i++)
            {
                Vector3 local = new Vector3(Random.Range(-62f, 62f), 0.09f, Random.Range(-42f, 52f));
                if (IsOnForestTrail(local))
                    continue;

                GameObject tuft = CreateSceneryBlock("Ground Fern", O + local, new Vector3(Random.Range(0.45f, 1.2f), 0.08f, Random.Range(0.45f, 1.2f)), flower, parent);
                tuft.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(-12f, 12f));
            }
        }

        private static void CreateArtifactNest(Vector3 pos, Transform parent, Material stone, Material moss, Material wood, Material fern)
        {
            for (int i = 0; i < 7; i++)
            {
                float angle = i * 51.4f + Random.Range(-8f, 8f);
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(Random.Range(1.5f, 2.8f), 0.18f, 0f);
                GameObject rock = CreateBlock("Artifact Nest Stone", pos + offset, new Vector3(Random.Range(0.45f, 1.1f), Random.Range(0.25f, 0.65f), Random.Range(0.45f, 1.1f)), i % 2 == 0 ? moss : stone, parent);
                rock.transform.rotation = Quaternion.Euler(Random.Range(-8f, 8f), Random.Range(0f, 360f), Random.Range(-8f, 8f));
            }

            GameObject fallenBranch = CreateBlock("Artifact Hiding Branch", pos + new Vector3(0.7f, 0.28f, -1.2f), new Vector3(0.28f, 0.28f, 3.4f), wood, parent);
            fallenBranch.transform.rotation = Quaternion.Euler(4f, Random.Range(0f, 360f), 8f);

            for (int i = 0; i < 10; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-3.4f, 3.4f), 0.1f, Random.Range(-3.4f, 3.4f));
                if (offset.sqrMagnitude < 1.2f)
                    offset += offset.normalized * 1.2f;

                GameObject tuft = CreateSceneryBlock("Artifact Fern Cover", pos + offset, new Vector3(Random.Range(0.55f, 1.4f), 0.08f, Random.Range(0.55f, 1.4f)), fern, parent);
                tuft.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(-10f, 10f));
            }
        }

        private static void CreateForestFinalBossClearing(Vector3 pos, Transform parent, Material stone, Material moss, Material wood, Material ember)
        {
            GameObject clearing = new GameObject("Forest Final Boss Clearing");
            clearing.transform.SetParent(parent);

            CreateBlock("Boss Clearing Ground", pos + new Vector3(0f, 0.04f, 0f), new Vector3(32f, 0.10f, 24f), moss, clearing.transform);
            CreateBlock("Boss Clearing Ash Mark", pos + new Vector3(0f, 0.12f, 0f), new Vector3(18f, 0.08f, 13f), stone, clearing.transform);

            for (int i = 0; i < 10; i++)
            {
                float angle = i * 36f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * new Vector3(14f, 0f, 0f);
                GameObject rune = CreateBlock("Forest Boss Rune Stone " + i, pos + offset + new Vector3(0f, 1.6f, 0f), new Vector3(0.9f, Random.Range(2.5f, 4.2f), 0.75f), i % 2 == 0 ? stone : moss, clearing.transform);
                rune.transform.rotation = Quaternion.Euler(Random.Range(-4f, 4f), angle, Random.Range(-5f, 5f));
            }

            CreateBlock("Fallen Hall Rib Left", pos + new Vector3(-7f, 1.0f, 2f), new Vector3(0.55f, 2.0f, 13f), wood, clearing.transform).transform.rotation = Quaternion.Euler(0f, -18f, 23f);
            CreateBlock("Fallen Hall Rib Right", pos + new Vector3(7f, 1.0f, 2f), new Vector3(0.55f, 2.0f, 13f), wood, clearing.transform).transform.rotation = Quaternion.Euler(0f, 18f, -23f);

            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -9f : 9f;
                CreateBlock("Boss Clearing Brazier", pos + new Vector3(x, 0.7f, -7f), new Vector3(1.8f, 1.2f, 1.8f), stone, clearing.transform);
                CreateSceneryBlock("Boss Clearing Ember", pos + new Vector3(x, 1.55f, -7f), new Vector3(1.0f, 0.55f, 1.0f), ember, clearing.transform);
                AddPointLight(clearing.transform, pos + new Vector3(x, 2.2f, -7f), new Color(1f, 0.35f, 0.08f), 2.0f, 8f, "Forest Boss Fire Glow");
            }
        }

        private static void CreateForestSafetyRim(Vector3 O, Transform parent, Material stone, Material moss, Material pine, Material wood)
        {
            GameObject rim = new GameObject("Forest Safety Mountains");
            rim.transform.SetParent(parent);

            CreateBlock("Forest North Ridge Land", O + new Vector3(0f, -0.35f, 74f), new Vector3(190f, 0.7f, 20f), moss, rim.transform);
            CreateBlock("Forest West Ridge Land", O + new Vector3(-88f, -0.35f, 0f), new Vector3(18f, 0.7f, 150f), moss, rim.transform);
            CreateBlock("Forest East Ridge Land", O + new Vector3(88f, -0.35f, 0f), new Vector3(18f, 0.7f, 150f), moss, rim.transform);
            CreateBlock("Forest South Ridge Land L", O + new Vector3(-63f, -0.35f, -82f), new Vector3(62f, 0.7f, 22f), moss, rim.transform);
            CreateBlock("Forest South Ridge Land R", O + new Vector3(63f, -0.35f, -82f), new Vector3(62f, 0.7f, 22f), moss, rim.transform);

            CreateInvisibleWall("Forest North Safety Wall", O + new Vector3(0f, 5f, 83f), new Vector3(188f, 18f, 2f), rim.transform);
            CreateInvisibleWall("Forest West Safety Wall", O + new Vector3(-96f, 5f, 0f), new Vector3(2f, 18f, 152f), rim.transform);
            CreateInvisibleWall("Forest East Safety Wall", O + new Vector3(96f, 5f, 0f), new Vector3(2f, 18f, 152f), rim.transform);
            CreateInvisibleWall("Forest Dock Edge Safety L", O + new Vector3(-58f, 5f, -84f), new Vector3(72f, 18f, 2f), rim.transform);
            CreateInvisibleWall("Forest Dock Edge Safety R", O + new Vector3(58f, 5f, -84f), new Vector3(72f, 18f, 2f), rim.transform);
            CreateInvisibleWall("Forest Sea Channel End Safety", O + new Vector3(0f, 5f, -108f), new Vector3(42f, 18f, 2f), rim.transform);

            for (int i = 0; i < 14; i++)
            {
                float x = -82f + i * 12.5f;
                float h = Random.Range(9f, 18f);
                CreateBlock("Forest Ridge Mountain N " + i, O + new Vector3(x, h * 0.5f - 0.6f, 86f), new Vector3(Random.Range(9f, 16f), h, Random.Range(8f, 13f)), i % 2 == 0 ? stone : moss, rim.transform);
            }

            for (int i = 0; i < 16; i++)
            {
                float z = -72f + i * 10f;
                float h = Random.Range(8f, 17f);
                CreateBlock("Forest Ridge Mountain W " + i, O + new Vector3(-97f, h * 0.5f - 0.6f, z), new Vector3(Random.Range(8f, 13f), h, Random.Range(8f, 14f)), i % 2 == 0 ? stone : moss, rim.transform);
                CreateBlock("Forest Ridge Mountain E " + i, O + new Vector3(97f, h * 0.5f - 0.6f, z), new Vector3(Random.Range(8f, 13f), h, Random.Range(8f, 14f)), i % 2 == 1 ? stone : moss, rim.transform);
            }

            for (int i = 0; i < 12; i++)
            {
                float x = -84f + i * 15.2f;
                if (Mathf.Abs(x) < 28f)
                    continue;

                float h = Random.Range(7f, 15f);
                CreateBlock("Forest South Channel Mountain " + i, O + new Vector3(x, h * 0.5f - 0.6f, -92f), new Vector3(Random.Range(10f, 18f), h, Random.Range(8f, 15f)), i % 2 == 0 ? stone : moss, rim.transform);
            }

            for (int i = 0; i < 22; i++)
            {
                float z = -62f + i * 8f;
                float side = i % 2 == 0 ? -1f : 1f;
                CreatePineTree(O + new Vector3(side * Random.Range(76f, 88f), 0f, z), rim.transform, pine, wood, wood);
            }
        }

        private static bool IsOnForestTrail(Vector3 local)
        {
            if (Mathf.Abs(local.x) < 8f && local.z > -50f && local.z < 54f)
                return true;
            if (Mathf.Abs(local.z - 18f) < 6f && local.x > -38f && local.x < 4f)
                return true;
            if (Mathf.Abs(local.z - 32f) < 6f && local.x > -2f && local.x < 38f)
                return true;
            if ((new Vector2(local.x + 36f, local.z - 14f)).sqrMagnitude < 36f)
                return true;
            if ((new Vector2(local.x - 35f, local.z - 29f)).sqrMagnitude < 36f)
                return true;
            if ((new Vector2(local.x + 8f, local.z - 48f)).sqrMagnitude < 36f)
                return true;

            return false;
        }

        private static void CreateForestBackdrop(Vector3 O, Transform parent, Material stone, Material snow, Material pine, Material moss)
        {
            GameObject backdrop = new GameObject("Coastal Forest Distant Mountains");
            backdrop.transform.SetParent(parent);

            Material farStone = MakeMat(null, 0.16f); farStone.color = new Color(0.24f, 0.31f, 0.34f);
            float[] xs = { -80f, -42f, 0f, 46f, 86f };
            float[] heights = { 26f, 38f, 46f, 34f, 28f };
            for (int i = 0; i < xs.Length; i++)
            {
                CreateSceneryBlock("Forest Far Mountain " + i, O + new Vector3(xs[i], heights[i], 78f + i * 3f),
                    new Vector3(38f, heights[i] * 2f, 30f), farStone, backdrop.transform, Quaternion.Euler(0f, Random.Range(-6f, 6f), 0f));
                CreateSceneryBlock("Forest Far Snow " + i, O + new Vector3(xs[i], heights[i] * 1.82f, 78f + i * 3f),
                    new Vector3(16f, heights[i] * 0.30f, 14f), snow, backdrop.transform);
            }

            for (int i = 0; i < 34; i++)
            {
                float x = -64f + i * 4f;
                if (Mathf.Abs(x) < 9f)
                    continue;

                float h = Random.Range(5f, 10f);
                CreateSceneryBlock("Forest Wall Trunk " + i, O + new Vector3(x, h * 0.5f, 56f), new Vector3(0.55f, h, 0.55f), moss, backdrop.transform);
                CreateSceneryBlock("Forest Wall Crown " + i, O + new Vector3(x, h + 2.1f, 56f), new Vector3(4f, 5f, 4f), pine, backdrop.transform);
            }
        }

        private static void CreateYamatoDistantBackdrop(Vector3 O, Transform parent,
            Material stone, Material snow, Material bamboo, Material moss)
        {
            GameObject backdrop = new GameObject("Yamato Distant Backdrop");
            backdrop.transform.SetParent(parent);

            Material farStone = MakeMat(null, 0.18f); farStone.color = new Color(0.22f, 0.30f, 0.28f);
            Material farPine = MakeMat(null, 0.10f); farPine.color = new Color(0.10f, 0.24f, 0.16f);
            Material haze = MakeMat(null, 0.06f); haze.color = new Color(0.46f, 0.54f, 0.50f);

            float[] xs = { -145f, -95f, -42f, 16f, 72f, 132f };
            float[] heights = { 30f, 44f, 36f, 50f, 34f, 42f };
            for (int i = 0; i < xs.Length; i++)
            {
                Vector3 peak = O + new Vector3(xs[i], heights[i], 146f + i * 4f);
                CreateSceneryBlock("Far Mountain " + i, peak, new Vector3(46f, heights[i] * 2f, 34f), farStone, backdrop.transform,
                    Quaternion.Euler(0f, Random.Range(-8f, 8f), 0f));
                CreateSceneryBlock("Far Snow Cap " + i, peak + new Vector3(0f, heights[i] * 0.78f, 0f),
                    new Vector3(22f, heights[i] * 0.34f, 18f), snow, backdrop.transform);
            }

            for (int i = 0; i < 46; i++)
            {
                float x = -112f + i * 5f;
                if (Mathf.Abs(x) < 12f)
                    continue;

                float h = Random.Range(5f, 11f);
                CreateSceneryBlock("Distant Forest Trunk " + i, O + new Vector3(x, h * 0.5f, 111f + Random.Range(-3f, 8f)),
                    new Vector3(0.55f, h, 0.55f), moss, backdrop.transform);
                CreateSceneryBlock("Distant Forest Crown " + i, O + new Vector3(x, h + 2.2f, 111f + Random.Range(-3f, 8f)),
                    new Vector3(4.5f, 5.5f, 4.5f), i % 3 == 0 ? bamboo : farPine, backdrop.transform);
            }

            CreateSceneryBlock("Haze Plane North", O + new Vector3(0f, 10f, 124f), new Vector3(232f, 18f, 0.35f), haze, backdrop.transform);
        }

        private static void CreateYamatoFinalBoss(Vector3 O, Transform parent,
            Material bossMat, Material hitFlash, Material stone, Material bamboo, Material red)
        {
            Vector3 arena = O + new Vector3(0f, 0f, 104f);
            GameObject bossRoot = new GameObject("Yamato Final Boss Grove");
            bossRoot.transform.SetParent(parent);

            Material path = MakeMat(null, 0.16f); path.color = new Color(0.32f, 0.28f, 0.24f);
            CreateBlock("Boss Grove Path", arena + new Vector3(0f, 0.05f, -8f), new Vector3(18f, 0.12f, 30f), path, bossRoot.transform);
            CreateBlock("Boss Shrine Marker", arena + new Vector3(0f, 0.6f, 7f), new Vector3(8f, 1.2f, 3f), stone, bossRoot.transform);

            for (int i = 0; i < 18; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                float z = -18f + (i / 2) * 4.8f;
                CreateBambooClump(arena + new Vector3(side * Random.Range(12f, 22f), 0f, z), bossRoot.transform, bamboo);
            }

            GameObject boss = CreateEnemy("Main Villain - Oni Warlord", arena + new Vector3(0f, 1f, 2f), bossMat, hitFlash, bossRoot.transform);
            boss.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
            var health = boss.GetComponent<Health>();
            if (health != null)
            {
                SerializedObject healthSo = new SerializedObject(health);
                healthSo.FindProperty("maxHealth").floatValue = 320f;
                healthSo.ApplyModifiedProperties();
            }

            AddPointLight(bossRoot.transform, arena + new Vector3(0f, 3f, 6f), new Color(1f, 0.12f, 0.08f), 3.2f, 13f, "Villain Warning Glow");
        }

        private static void CreateBambooDojoEntranceAndInterior(Vector3 O, Transform parent,
            Material wood, Material lightWood, Material paper, Material red, Material darkRed,
            Material stone, Material bamboo, Material bossMat, Material hitFlash)
        {
            GameObject exterior = new GameObject("Bamboo Dojo Exterior");
            exterior.transform.SetParent(parent);
            // Push the enterable dojo OUT of the spawn area and over to the east-side
            // village cluster, alongside the shrine path. (Was at z = -82 right next
            // to the dock; player would walk into it immediately. Now placed north-east
            // along the path so player must walk past several houses to reach it.)
            Vector3 pos = O + new Vector3(46f, 0f, 48f);

            Material path = MakeMat(null, 0.08f); path.color = new Color(0.54f, 0.48f, 0.38f);
            Material shadow = MakeMat(null, 0.05f); shadow.color = new Color(0.08f, 0.07f, 0.06f);
            Material tatami = MakeMat(null, 0.05f); tatami.color = new Color(0.68f, 0.70f, 0.46f);

            // Approach stones leading from the main shrine path to the dojo entrance
            for (int i = 0; i < 8; i++)
                CreateBlock("Dojo Approach Stone " + i, pos + new Vector3(-i * 3.6f, 0.08f, -8f), new Vector3(3.2f, 0.12f, 3.2f), path, exterior.transform);

            // A few sakura/pine accents around it (NOT bamboo — bamboo was covering it before)
            for (int i = 0; i < 4; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                CreatePineTree(pos + new Vector3(side * 11f, 0f, -2f + i * 3f), exterior.transform, bamboo, wood, lightWood);
            }

            CreateBlock("Dojo Foundation", pos + new Vector3(0f, 0.45f, 0f), new Vector3(17f, 0.9f, 14f), stone, exterior.transform);
            CreateBlock("Dojo Back Wall", pos + new Vector3(0f, 3.6f, 5.8f), new Vector3(16f, 6f, 0.42f), wood, exterior.transform);
            CreateBlock("Dojo Left Wall", pos + new Vector3(-8f, 3.6f, 0f), new Vector3(0.42f, 6f, 12f), wood, exterior.transform);
            CreateBlock("Dojo Right Wall", pos + new Vector3(8f, 3.6f, 0f), new Vector3(0.42f, 6f, 12f), wood, exterior.transform);
            CreateBlock("Dojo Shoji Left", pos + new Vector3(-3.2f, 3.6f, -6.2f), new Vector3(5.2f, 5f, 0.18f), paper, exterior.transform);
            CreateBlock("Dojo Shoji Right", pos + new Vector3(3.2f, 3.6f, -6.2f), new Vector3(5.2f, 5f, 0.18f), paper, exterior.transform);
            CreateBlock("Dojo Threshold Opening", pos + new Vector3(0f, 1.1f, -6.45f), new Vector3(4.1f, 1.0f, 0.22f), shadow, exterior.transform);
            CreateBlock("Dojo Roof Lower", pos + new Vector3(0f, 7.6f, 0f), new Vector3(20f, 0.6f, 16f), red, exterior.transform);
            CreateBlock("Dojo Roof Upper", pos + new Vector3(0f, 9.3f, 0f), new Vector3(13f, 0.55f, 10f), darkRed, exterior.transform);
            AddPointLight(exterior.transform, pos + new Vector3(0f, 3.5f, -5.2f), new Color(1f, 0.66f, 0.30f), 2.2f, 9f, "Dojo Door Glow");

            GameObject trigger = new GameObject("Bamboo Dojo Entrance Trigger");
            trigger.transform.SetParent(exterior.transform);
            trigger.transform.position = pos + new Vector3(0f, 1.3f, -8.5f);
            BoxCollider triggerCollider = trigger.AddComponent<BoxCollider>();
            triggerCollider.size = new Vector3(8f, 4f, 7f);

            GameObject interior = new GameObject("Hidden Bamboo Dojo Interior");
            interior.transform.SetParent(parent);
            Vector3 I = O + new Vector3(92f, 0f, -80f);
            CreateBlock("Interior Floor", I + new Vector3(0f, -0.5f, 0f), new Vector3(46f, 1f, 42f), tatami, interior.transform);
            CreateBlock("Interior North Wall", I + new Vector3(0f, 3f, 21f), new Vector3(46f, 7f, 1f), wood, interior.transform);
            CreateBlock("Interior South Wall", I + new Vector3(0f, 3f, -21f), new Vector3(46f, 7f, 1f), wood, interior.transform);
            CreateBlock("Interior West Wall", I + new Vector3(-23f, 3f, 0f), new Vector3(1f, 7f, 42f), wood, interior.transform);
            CreateBlock("Interior East Wall", I + new Vector3(23f, 3f, 0f), new Vector3(1f, 7f, 42f), wood, interior.transform);
            // Interior is OPEN to the sky — no opaque roof. Previously a dark roof block
            // sat right above the play space and blacked out the interior. Replaced with
            // bright lanterns + a skylight fill so the player can actually see.
            CreateBlock("Boss Arena Mat", I + new Vector3(0f, 0.06f, 0f), new Vector3(24f, 0.12f, 18f), path, interior.transform);

            // Visible rafters (thin beams, do not block light)
            for (int i = 0; i < 5; i++)
            {
                float z = -16f + i * 8f;
                CreateBlock("Rafter " + i, I + new Vector3(0f, 6.6f, z), new Vector3(44f, 0.25f, 0.35f), wood, interior.transform);
            }

            for (int i = 0; i < 6; i++)
            {
                float x = -15f + i * 6f;
                CreateBlock("Interior Post " + i, I + new Vector3(x, 3f, -14f), new Vector3(0.55f, 6f, 0.55f), lightWood, interior.transform);
                CreateBlock("Interior Post Back " + i, I + new Vector3(x, 3f, 14f), new Vector3(0.55f, 6f, 0.55f), lightWood, interior.transform);
            }

            // Hanging paper lanterns (warm fill light so the interior reads clearly)
            Material lanternMat = MakeMat(null, 0.06f); lanternMat.color = new Color(1f, 0.86f, 0.52f);
            for (int i = 0; i < 4; i++)
            {
                float x = -15f + i * 10f;
                CreateBlock("Lantern Body " + i, I + new Vector3(x, 5.3f, 0f), new Vector3(1.2f, 1.4f, 1.2f), lanternMat, interior.transform);
                AddPointLight(interior.transform, I + new Vector3(x, 5f, 0f), new Color(1f, 0.78f, 0.42f), 2.6f, 14f, "Dojo Lantern " + i);
            }

            GameObject boss = CreateEnemy("Final Boss - Dojo Warlord", I + new Vector3(0f, 1f, 7f), bossMat, hitFlash, interior.transform);
            boss.transform.localScale = new Vector3(1.9f, 1.9f, 1.9f);
            Health bossHealth = boss.GetComponent<Health>();
            if (bossHealth != null)
            {
                SerializedObject healthSo = new SerializedObject(bossHealth);
                healthSo.FindProperty("maxHealth").floatValue = 420f;
                healthSo.ApplyModifiedProperties();
            }

            AddPointLight(interior.transform, I + new Vector3(0f, 6f, 4f), new Color(1f, 0.34f, 0.16f), 3.2f, 22f, "Dojo Boss Warning Glow");
            // Bright ambient sky-fill so even the corners are readable
            AddPointLight(interior.transform, I + new Vector3(0f, 9f, 0f), new Color(0.96f, 0.92f, 0.84f), 3.2f, 36f, "Dojo Skylight");

            YamatoBuildingEntrance entrance = trigger.AddComponent<YamatoBuildingEntrance>();
            entrance.interiorRoot = interior;
            entrance.destinationTarget = I + new Vector3(0f, 1.05f, -12f);
            entrance.prompt = "Press E to enter the dojo";
            interior.SetActive(false);
        }

        private static void CreateYamatoArrivalDock(Vector3 O, Transform parent, Material water, Material wood, Material stone)
        {
            GameObject arrival = new GameObject("Yamato Arrival Sea Shore");
            arrival.transform.SetParent(parent);

            // Wide sea expanse
            CreateSceneryBlock("Yamato Sea (Far)",   O + new Vector3(0f,  -0.55f, -110f), new Vector3(260f, 0.18f, 120f), water, arrival.transform);
            CreateSceneryBlock("Yamato Sea (Near)",  O + new Vector3(0f,  -0.50f,  -58f), new Vector3(180f, 0.20f,  60f), water, arrival.transform);

            // Sandy beach + curved shoreline (use wood-tinted sand via stone darkened with sand-mix)
            Material beach = MakeMat(null, 0.06f); beach.color = new Color(0.86f, 0.78f, 0.58f);
            Material wetSand = MakeMat(null, 0.10f); wetSand.color = new Color(0.62f, 0.54f, 0.40f);
            CreateBlock("Yamato Beach",           O + new Vector3(0f, -0.30f, -32f), new Vector3(150f, 0.22f, 22f), beach, arrival.transform);
            CreateBlock("Yamato Wet Sand Strip",  O + new Vector3(0f, -0.34f, -42f), new Vector3(150f, 0.18f,  6f), wetSand, arrival.transform);

            // Stone breakwater rocks dotted along the shoreline
            for (int i = 0; i < 12; i++)
            {
                float x = -70f + i * 12.5f;
                if (Mathf.Abs(x) < 14f) continue; // leave dock mouth clear
                Vector3 rp = O + new Vector3(x, -0.10f, -46f + Random.Range(-2f, 2f));
                GameObject rock = CreateBlock("Shore Boulder " + i, rp,
                    new Vector3(Random.Range(1.6f, 3.2f), Random.Range(0.9f, 2.0f), Random.Range(1.6f, 3.2f)),
                    stone, arrival.transform);
                rock.transform.rotation = Quaternion.Euler(Random.Range(-12f, 12f), Random.Range(0f, 360f), Random.Range(-12f, 12f));
            }

            // Long wooden dock extending into the sea (boat lands at the far head)
            CreateBlock("Dock Run",      O + new Vector3(0f,  0.12f, -34f), new Vector3(7f,  0.26f, 36f), wood, arrival.transform);
            CreateBlock("Dock Head Pad", O + new Vector3(0f,  0.16f, -54f), new Vector3(16f, 0.30f, 9f),  wood, arrival.transform);
            CreateBlock("Dock Side Rail L", O + new Vector3(-3.6f, 0.55f, -34f), new Vector3(0.18f, 0.5f, 34f), wood, arrival.transform);
            CreateBlock("Dock Side Rail R", O + new Vector3( 3.6f, 0.55f, -34f), new Vector3(0.18f, 0.5f, 34f), wood, arrival.transform);
            for (int i = 0; i < 9; i++)
            {
                float z = -52f + i * 4.4f;
                CreateBlock("Dock Post L " + i, O + new Vector3(-3.8f, -0.6f, z), new Vector3(0.45f, 2.4f, 0.45f), stone, arrival.transform);
                CreateBlock("Dock Post R " + i, O + new Vector3( 3.8f, -0.6f, z), new Vector3(0.45f, 2.4f, 0.45f), stone, arrival.transform);
            }
            // Lanterns along the dock
            for (int i = 0; i < 5; i++)
            {
                float z = -50f + i * 9f;
                CreateStoneLantern(O + new Vector3(-3.6f, 0.30f, z), arrival.transform, stone);
                CreateStoneLantern(O + new Vector3( 3.6f, 0.30f, z), arrival.transform, stone);
            }
            // Welcoming Torii at the dock head facing the sea
            Material toriiRed = MakeMat(null, 0.16f); toriiRed.color = new Color(0.84f, 0.12f, 0.12f);
            CreateToriiGate("Dock Welcome Torii", O + new Vector3(0f, 0.30f, -50f), toriiRed, wood, arrival.transform);

            // Soft warm light over the dock
            AddPointLight(arrival.transform, O + new Vector3(0f, 4f, -42f), new Color(1f, 0.78f, 0.42f), 2.6f, 22f, "Dock Lantern Wash");
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

        // ── Frostheim Village Palisade (gives the village a clear silhouette) ────
        private static void CreateFrostheimVillagePerimeter(Transform parent,
            Material wood, Material woodDark, Material stone, Material bannerRed, Material bannerBlack)
        {
            GameObject ring = new GameObject("Frostheim Village Palisade");
            ring.transform.SetParent(parent);

            // Three-sided wooden palisade enclosing the core village (south is open
            // so the entrance causeway still leads in). Logs angled like sharpened stakes.
            void StakeRow(Vector3 start, Vector3 step, int count, float rotY)
            {
                for (int i = 0; i < count; i++)
                {
                    Vector3 p = start + step * i;
                    GameObject stake = CreateBlock("Palisade Stake",
                        p, new Vector3(0.55f, Random.Range(4.2f, 5.4f), 0.55f),
                        i % 4 == 0 ? woodDark : wood, ring.transform);
                    stake.transform.rotation = Quaternion.Euler(Random.Range(-2f, 2f), rotY, Random.Range(-2f, 2f));
                }
            }
            // West wall
            StakeRow(new Vector3(-58f, 2.0f, -8f), new Vector3(0f, 0f, 2.6f), 28, 0f);
            // East wall
            StakeRow(new Vector3( 58f, 2.0f, -8f), new Vector3(0f, 0f, 2.6f), 28, 0f);
            // North wall (with break at center for altar approach)
            for (int i = 0; i < 30; i++)
            {
                float x = -56f + i * 4f;
                if (Mathf.Abs(x) < 8f) continue;
                GameObject stake = CreateBlock("Palisade Stake N",
                    new Vector3(x, 2.0f, 60f), new Vector3(0.55f, Random.Range(4.2f, 5.4f), 0.55f),
                    i % 4 == 0 ? woodDark : wood, ring.transform);
                stake.transform.rotation = Quaternion.Euler(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
            }

            // Reinforced corner watchposts (small platforms at each NW/NE corner)
            void CornerPost(Vector3 p, Material banner)
            {
                CreateBlock("Corner Post Pillar", p + new Vector3(0f, 3f, 0f), new Vector3(1.2f, 6f, 1.2f), woodDark, ring.transform);
                CreateBlock("Corner Post Crown", p + new Vector3(0f, 6.2f, 0f), new Vector3(2.2f, 0.4f, 2.2f), wood, ring.transform);
                CreateBanner(p + new Vector3(0f, 5f, 0f), ring.transform, banner, woodDark);
            }
            CornerPost(new Vector3(-58f, 0f,  60f), bannerRed);
            CornerPost(new Vector3( 58f, 0f,  60f), bannerBlack);
            CornerPost(new Vector3(-58f, 0f, -10f), bannerBlack);
            CornerPost(new Vector3( 58f, 0f, -10f), bannerRed);

            // Main gate posts (two big timber columns flanking the south entrance)
            CreateBlock("Gate Post L", new Vector3(-7f, 3.5f, -10f), new Vector3(1.4f, 7f, 1.4f), woodDark, ring.transform);
            CreateBlock("Gate Post R", new Vector3( 7f, 3.5f, -10f), new Vector3(1.4f, 7f, 1.4f), woodDark, ring.transform);
            CreateBlock("Gate Lintel",  new Vector3(0f, 7.3f, -10f), new Vector3(17f, 0.7f, 1.0f), wood,    ring.transform);
            CreateBlock("Gate Sign",    new Vector3(0f, 5.8f, -10f), new Vector3(5f, 1.4f, 0.2f),  woodDark, ring.transform);

            // A small structured row of standing weapon racks inside the gate — clearly Nordic
            Material steel = MakeMat(null, 0.55f); steel.color = new Color(0.66f, 0.70f, 0.74f);
            for (int i = 0; i < 4; i++)
            {
                float x = -10f + i * 6f;
                CreateBlock("Weapon Rack Beam", new Vector3(x, 1.8f, -4f), new Vector3(2.4f, 0.18f, 0.18f), wood, ring.transform);
                CreateBlock("Rack Axe",  new Vector3(x - 0.6f, 1.4f, -4f), new Vector3(0.18f, 1.4f, 0.18f), wood,  ring.transform);
                CreateBlock("Rack Blade",new Vector3(x + 0.6f, 1.4f, -4f), new Vector3(0.16f, 1.6f, 0.16f), steel, ring.transform);
            }

            // Heavy ground brazier near the gate — visible signifier of "Viking village"
            Material ember = MakeMat(null, 0.20f); ember.color = new Color(1f, 0.45f, 0.10f);
            void Brazier(Vector3 p)
            {
                CreateBlock("Brazier Bowl", p + new Vector3(0f, 1.2f, 0f), new Vector3(1.6f, 0.4f, 1.6f), woodDark, ring.transform);
                CreateBlock("Brazier Stand", p + new Vector3(0f, 0.5f, 0f), new Vector3(0.4f, 1.1f, 0.4f), woodDark, ring.transform);
                CreateSceneryBlock("Brazier Flame", p + new Vector3(0f, 1.8f, 0f), new Vector3(1.0f, 1.2f, 1.0f), ember, ring.transform);
                AddPointLight(ring.transform, p + new Vector3(0f, 2f, 0f), new Color(1f, 0.55f, 0.18f), 3.2f, 16f, "Brazier Glow");
            }
            Brazier(new Vector3(-12f, 0f, -10f));
            Brazier(new Vector3( 12f, 0f, -10f));
            Brazier(new Vector3(  0f, 0f,  10f));
            Brazier(new Vector3(-22f, 0f,  22f));
            Brazier(new Vector3( 22f, 0f,  22f));
        }

        // ── Frostheim Raid: pre-placed foes that engage the player on arrival ────
        private static void CreateFrostheimRaidEncounter(Transform parent)
        {
            GameObject raid = new GameObject("Frostheim Raid Encounter");
            raid.transform.SetParent(parent);

            Material raiderMat = MakeMat(null, 0.18f); raiderMat.color = new Color(0.18f, 0.16f, 0.20f);
            Material chiefMat  = MakeMat(null, 0.32f); chiefMat.color  = new Color(0.55f, 0.10f, 0.10f);
            Material hitFlash  = MakeMat(null, 0.00f); hitFlash.color  = Color.white;

            // Five raiders patrolling the village square
            Vector3[] raiderSpots =
            {
                new Vector3(-8f, 1f, 12f), new Vector3( 8f, 1f, 14f),
                new Vector3(-18f, 1f, 24f), new Vector3( 20f, 1f, 26f),
                new Vector3(  0f, 1f, 30f)
            };
            for (int i = 0; i < raiderSpots.Length; i++)
                CreateEnemy("Frostheim Raider " + (i + 1), raiderSpots[i], raiderMat, hitFlash, raid.transform);

            // Raid leader near the great hall steps
            GameObject chief = CreateEnemy("Raid Chieftain", new Vector3(0f, 1f, 38f), chiefMat, hitFlash, raid.transform);
            chief.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
            Health chiefHp = chief.GetComponent<Health>();
            if (chiefHp != null)
            {
                SerializedObject so = new SerializedObject(chiefHp);
                so.FindProperty("maxHealth").floatValue = 280f;
                so.ApplyModifiedProperties();
            }
        }

        // ── Soft snowfall layer (gentler than the blizzard, closer to camera) ────
        private static void CreateLightSnowfall(Transform parent)
        {
            GameObject snow = new GameObject("Frostheim Light Snowfall");
            snow.transform.position = new Vector3(0f, 28f, 0f);
            snow.transform.SetParent(parent);

            ParticleSystem ps = snow.AddComponent<ParticleSystem>();
            var rend = snow.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = new Material(Shader.Find("Sprites/Default")) { color = new Color(1f, 1f, 1f, 0.85f) };

            var m = ps.main;
            m.startLifetime = 8f;
            m.startSpeed    = new ParticleSystem.MinMaxCurve(1.2f, 2.4f);
            m.startSize     = new ParticleSystem.MinMaxCurve(0.10f, 0.22f);
            m.maxParticles  = 1400;
            m.gravityModifier = 0.05f;

            var em = ps.emission; em.rateOverTime = 220f;
            var sh = ps.shape;   sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(220f, 1f, 220f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);
            vel.y = new ParticleSystem.MinMaxCurve(-2.4f, -1.2f);
            vel.z = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
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

        private static GameObject CreatePortalGateway(string name, Vector3 pos, Transform parent,
            Material pillarMat, Color glowColor, Color lightColor, Color particleColor,
            Vector3 destination, bool isReturn)
        {
            // ── Build the portal in LOCAL space so rotating the group doesn't desync
            //    the pieces. (Previously pillars were placed in world coords then
            //    parented to a rotated group, which is what made the portal look
            //    sideways and the glow plane perpendicular to the pillars.)
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent);
            group.transform.position = pos;
            // The portal facing — the player walks through along the local +Z axis.
            // Approach side is local -Z. Both portals face their approaching player.
            group.transform.rotation = Quaternion.identity;

            // Torii-style gateway silhouette so it actually reads as Japanese.
            // Two tall vermillion pillars, a curved upper kasagi, and a lower nuki.
            CreateBlock("Pillar L", new Vector3(-3.2f, 3.8f, 0f), new Vector3(0.9f, 7.6f, 0.9f), pillarMat, group.transform);
            CreateBlock("Pillar R", new Vector3( 3.2f, 3.8f, 0f), new Vector3(0.9f, 7.6f, 0.9f), pillarMat, group.transform);
            CreateBlock("Kasagi (Top Beam)", new Vector3(0f, 7.7f, 0f), new Vector3(9.8f, 0.85f, 1.05f), pillarMat, group.transform);
            CreateBlock("Shimagi", new Vector3(0f, 7.05f, 0f), new Vector3(9.0f, 0.45f, 0.85f), pillarMat, group.transform);
            CreateBlock("Nuki (Cross Beam)", new Vector3(0f, 5.8f, 0f), new Vector3(7.6f, 0.55f, 0.6f), pillarMat, group.transform);
            CreateBlock("Gakuzan", new Vector3(0f, 6.5f, 0f), new Vector3(0.6f, 1.4f, 0.85f), pillarMat, group.transform);
            // Curved eave tips on the kasagi (slight tilt up, traditional shape)
            CreateBlock("Kasagi Tip L", new Vector3(-4.5f, 7.6f, 0f), new Vector3(2.4f, 0.4f, 1.0f), pillarMat, group.transform).transform.localRotation = Quaternion.Euler(0f, 0f, 14f);
            CreateBlock("Kasagi Tip R", new Vector3( 4.5f, 7.6f, 0f), new Vector3(2.4f, 0.4f, 1.0f), pillarMat, group.transform).transform.localRotation = Quaternion.Euler(0f, 0f, -14f);

            // Rune / charm streamers on the pillars — front faces only
            Material runeMat = MakeMat(null, 0.8f);
            runeMat.color = glowColor;
            for (int i = 0; i < 3; i++)
            {
                CreateBlock("Charm L " + i, new Vector3(-3.25f, 2f + i * 1.8f, -0.5f), new Vector3(0.14f, 0.85f, 0.06f), runeMat, group.transform);
                CreateBlock("Charm R " + i, new Vector3( 3.25f, 2f + i * 1.8f, -0.5f), new Vector3(0.14f, 0.85f, 0.06f), runeMat, group.transform);
            }

            // Glowing portal plane — fills the gateway opening, faces the approach (-Z).
            // Width ~= pillar gap, height ~= up to the nuki cross-beam.
            Material glowMat = new Material(Shader.Find("Unlit/Color"));
            glowMat.color = glowColor;
            GameObject glowObj = CreateBlock("Portal Glow", new Vector3(0f, 3.0f, 0f), new Vector3(5.6f, 5.4f, 0.18f), glowMat, group.transform);
            // Add a second softer back-glow plane for depth
            Material backGlow = new Material(Shader.Find("Unlit/Color"));
            backGlow.color = new Color(glowColor.r * 0.55f, glowColor.g * 0.55f, glowColor.b * 0.85f, glowColor.a);
            CreateBlock("Portal Back Glow", new Vector3(0f, 3.0f, 0.12f), new Vector3(5.2f, 5.0f, 0.06f), backGlow, group.transform);

            // Portal light at the centre of the opening (no parenting tricks).
            Light pl = glowObj.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = lightColor;
            pl.intensity = 6f;
            pl.range = 22f;

            // Trigger collider — depth runs through the gateway (local Z axis).
            BoxCollider bc = glowObj.GetComponent<BoxCollider>() ?? glowObj.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(5.4f, 5.2f, 4.5f);

            // Portal script
            var script = glowObj.AddComponent<JapanPortal>();
            script.destinationTarget = destination;
            script.isReturnPortal = isReturn;

            // Particle vortex inside the gateway — drifts toward the approach side so
            // it reads as "energy spilling out" rather than just floating dust.
            ParticleSystem ps = glowObj.AddComponent<ParticleSystem>();
            var pM = ps.main;
            pM.startColor = particleColor;
            pM.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.45f);
            pM.startLifetime = 4.5f;
            pM.startSpeed = 1.8f;
            pM.simulationSpace = ParticleSystemSimulationSpace.Local;
            var emission = ps.emission;
            emission.rateOverTime = 60f;
            var pSh = ps.shape;
            pSh.shapeType = ParticleSystemShapeType.Box;
            pSh.scale = new Vector3(4.2f, 4.8f, 0.4f);
            glowObj.GetComponent<ParticleSystemRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            var pV = ps.velocityOverLifetime;
            pV.enabled = true;
            pV.x = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
            pV.y = new ParticleSystem.MinMaxCurve(-0.2f, 0.6f);
            pV.z = new ParticleSystem.MinMaxCurve(-2.4f, -0.6f);
            // Fade in/out for softer particles
            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(particleColor, 0f), new GradientColorKey(particleColor, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.85f, 0.3f), new GradientAlphaKey(0.6f, 0.7f), new GradientAlphaKey(0f, 1f) });
            col.color = new ParticleSystem.MinMaxGradient(g);

            // Slow swirl on the front face for extra "alive" feel
            GameObject swirl = new GameObject("Portal Front Swirl");
            swirl.transform.SetParent(group.transform, false);
            swirl.transform.localPosition = new Vector3(0f, 3.0f, -0.3f);
            ParticleSystem swirlPs = swirl.AddComponent<ParticleSystem>();
            var sM = swirlPs.main;
            sM.startColor = new Color(lightColor.r, lightColor.g, lightColor.b, 0.8f);
            sM.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.45f);
            sM.startLifetime = 3.5f;
            sM.startSpeed = 0.4f;
            sM.maxParticles = 80;
            var sE = swirlPs.emission; sE.rateOverTime = 18f;
            var sSh = swirlPs.shape; sSh.shapeType = ParticleSystemShapeType.Circle; sSh.radius = 2.4f;
            swirl.GetComponent<ParticleSystemRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));

            // Now apply the portal's facing rotation. Both portals face the approaching
            // player so the glow plane is broadside-on, never sideways.
            // Frostheim->Yamato portal: approach from south (player walks +Z), keep identity.
            // Yamato->Frostheim portal: approach from south (player walks +Z), keep identity.
            // The caller positions the portals so the player approaches from the -Z side.
            return group;
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
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = new Color(0.34f, 0.38f, 0.44f);
            cam.allowHDR         = false;
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

        private static void CreateRegionMusicController(Transform parent)
        {
            GameObject music = new GameObject("Region Music Controller");
            music.transform.SetParent(parent);

            AudioSource source = music.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;

            RegionMusicController controller = music.AddComponent<RegionMusicController>();
            controller.forestClip = FindBestMusicClip(new[] { "viking", "forest", "norse", "nordic" });
            controller.yamatoClip = FindBestMusicClip(new[] { "japanese", "japan", "yamato", "shamisen", "sakura" });
        }

        private static AudioClip FindBestMusicClip(string[] keywords)
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null)
                    continue;

                string lower = (path + " " + clip.name).ToLowerInvariant();
                foreach (string keyword in keywords)
                {
                    if (lower.Contains(keyword))
                        return clip;
                }
            }

            return null;
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

        private static void CreateNeutralNpc(string name, Vector3 pos, Material mat, Transform parent)
        {
            GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npc.name = name;
            npc.transform.position = pos;
            npc.transform.localScale = new Vector3(0.9f, 1.05f, 0.9f);
            npc.transform.SetParent(parent);
            npc.GetComponent<MeshRenderer>().sharedMaterial = mat;

            Rigidbody rb = npc.AddComponent<Rigidbody>();
            rb.mass = 3f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            npc.AddComponent<RoamingAnimal>().moveSpeed = 1.2f;
        }

        private static GameObject CreateEnemy(string name, Vector3 pos,
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

            return enemy;
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
            m.mainTexture = null;
            m.SetFloat("_Glossiness", Mathf.Clamp(glossiness, 0f, 0.55f));
            m.color = tex != null ? tex.GetPixelBilinear(0.5f, 0.5f) : Color.white;
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

        private static GameObject CreateSceneryBlock(string name, Vector3 pos, Vector3 scale,
            Material mat, Transform parent, Quaternion? rotation = null)
        {
            GameObject cube = CreateBlock(name, pos, scale, mat, parent);
            if (rotation.HasValue)
                cube.transform.rotation = rotation.Value;

            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
                DestroyImmediate(collider);

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
            // Preserve the old material call sites while avoiding high-contrast texture maps.
            Texture2D tex = new Texture2D(1,1);
            tex.name = "Flat Material Swatch";
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.SetPixel(0,0,Color.Lerp(a,b,0.5f));
            tex.Apply();
            return tex;
        }
        // Overloads to match calls from BuildMasterpieceWorld

    private static void CreateGreatHall(Vector3 pos, Transform parent, Material wood, Material woodDark, Material thatch, Material stone, Material pathMat, Material bannerRed)
    {
        GameObject hall = new GameObject("Viking Great Hall");
        hall.transform.position = pos;
        hall.transform.SetParent(parent);

        CreateBlock("Longhouse Stone Footing", pos + new Vector3(0f, 0.32f, 0f), new Vector3(34f, 0.64f, 13f), stone, hall.transform);
        CreateBlock("Longhouse Timber Wall North", pos + new Vector3(0f, 2.2f, 5.8f), new Vector3(32f, 3.8f, 0.7f), wood, hall.transform);
        CreateBlock("Longhouse Timber Wall South", pos + new Vector3(0f, 2.2f, -5.8f), new Vector3(32f, 3.8f, 0.7f), wood, hall.transform);
        CreateBlock("Longhouse West Gable", pos + new Vector3(-16f, 2.2f, 0f), new Vector3(0.7f, 3.8f, 12f), woodDark, hall.transform);
        CreateBlock("Longhouse East Gable", pos + new Vector3(16f, 2.2f, 0f), new Vector3(0.7f, 3.8f, 12f), woodDark, hall.transform);

        CreateBlock("Great Hall Door", pos + new Vector3(0f, 1.6f, -6.25f), new Vector3(3.8f, 3.0f, 0.32f), woodDark, hall.transform);
        CreateBlock("Door Iron Bar", pos + new Vector3(0f, 2.2f, -6.45f), new Vector3(3.2f, 0.18f, 0.18f), stone, hall.transform);

        GameObject roofLeft = CreateBlock("Turf Roof Left Plane", pos + new Vector3(-4.2f, 5.1f, 0f), new Vector3(20f, 0.7f, 15.5f), thatch, hall.transform);
        roofLeft.transform.rotation = Quaternion.Euler(0f, 0f, 19f);
        GameObject roofRight = CreateBlock("Turf Roof Right Plane", pos + new Vector3(4.2f, 5.1f, 0f), new Vector3(20f, 0.7f, 15.5f), thatch, hall.transform);
        roofRight.transform.rotation = Quaternion.Euler(0f, 0f, -19f);
        CreateBlock("Longhouse Ridge Beam", pos + new Vector3(0f, 7.8f, 0f), new Vector3(0.7f, 0.7f, 17f), woodDark, hall.transform);

        for (int i = 0; i < 9; i++)
        {
            float x = -12f + i * 3f;
            CreateBlock("Wall Post North " + i, pos + new Vector3(x, 2.7f, 6.25f), new Vector3(0.38f, 4.2f, 0.38f), woodDark, hall.transform);
            CreateBlock("Wall Post South " + i, pos + new Vector3(x, 2.7f, -6.25f), new Vector3(0.38f, 4.2f, 0.38f), woodDark, hall.transform);
        }

        CreateBlock("Central Hearth", pos + new Vector3(0f, 0.82f, 0f), new Vector3(5f, 0.34f, 2.4f), stone, hall.transform);
        Material ember = MakeMat(null, 0.20f); ember.color = new Color(1f, 0.34f, 0.08f);
        CreateSceneryBlock("Hearth Embers", pos + new Vector3(0f, 1.06f, 0f), new Vector3(3.8f, 0.16f, 1.5f), ember, hall.transform);
        AddPointLight(hall.transform, pos + new Vector3(0f, 2.1f, 0f), new Color(1f, 0.42f, 0.12f), 2.5f, 10f, "Great Hall Hearth Glow");

        CreateBlock("Left Sleeping Bench", pos + new Vector3(-6f, 1.0f, 4.4f), new Vector3(14f, 0.45f, 1.2f), woodDark, hall.transform);
        CreateBlock("Right Sleeping Bench", pos + new Vector3(6f, 1.0f, -4.4f), new Vector3(14f, 0.45f, 1.2f), woodDark, hall.transform);
        CreateBlock("Hall Banner", pos + new Vector3(0f, 4.1f, -6.45f), new Vector3(3.2f, 3.8f, 0.18f), bannerRed, hall.transform);
        CreateBlock("Stone Path to Hall", pos + new Vector3(0f, 0.08f, -13f), new Vector3(7f, 0.12f, 12f), pathMat, hall.transform);
    }

    private static void CreateHut(Vector3 pos, Transform parent, Material wood, Material thatch, Material stone, float rotation)
    {
        GameObject hut = new GameObject("Nordic Family Hut");
        hut.transform.position = pos;
        hut.transform.rotation = Quaternion.Euler(0, rotation, 0);
        hut.transform.SetParent(parent);

        CreateBlock("Hut Stone Base", pos + new Vector3(0f, 0.24f, 0f), new Vector3(7.2f, 0.48f, 6.2f), stone, hut.transform);
        CreateBlock("Hut Back Wall", pos + new Vector3(0f, 1.8f, 2.8f), new Vector3(6.6f, 3.2f, 0.52f), wood, hut.transform);
        CreateBlock("Hut Front Wall L", pos + new Vector3(-2.4f, 1.8f, -2.8f), new Vector3(2.0f, 3.2f, 0.52f), wood, hut.transform);
        CreateBlock("Hut Front Wall R", pos + new Vector3(2.4f, 1.8f, -2.8f), new Vector3(2.0f, 3.2f, 0.52f), wood, hut.transform);
        CreateBlock("Hut Left Wall", pos + new Vector3(-3.3f, 1.8f, 0f), new Vector3(0.52f, 3.2f, 5.6f), wood, hut.transform);
        CreateBlock("Hut Right Wall", pos + new Vector3(3.3f, 1.8f, 0f), new Vector3(0.52f, 3.2f, 5.6f), wood, hut.transform);
        CreateBlock("Hut Door", pos + new Vector3(0f, 1.35f, -3.1f), new Vector3(1.8f, 2.5f, 0.32f), stone, hut.transform);

        GameObject roofLeft = CreateBlock("Hut Turf Roof Left", pos + new Vector3(-1.9f, 4.2f, 0f), new Vector3(4.5f, 0.48f, 7.2f), thatch, hut.transform);
        roofLeft.transform.rotation = Quaternion.Euler(0f, rotation, 24f);
        GameObject roofRight = CreateBlock("Hut Turf Roof Right", pos + new Vector3(1.9f, 4.2f, 0f), new Vector3(4.5f, 0.48f, 7.2f), thatch, hut.transform);
        roofRight.transform.rotation = Quaternion.Euler(0f, rotation, -24f);
        CreateBlock("Hut Ridge Beam", pos + new Vector3(0f, 5.35f, 0f), new Vector3(0.42f, 0.42f, 7.4f), wood, hut.transform);

        CreateBlock("Hut Small Bench", pos + new Vector3(-1.6f, 0.8f, 1.7f), new Vector3(2.2f, 0.28f, 0.8f), wood, hut.transform);
        AddPointLight(hut.transform, pos + new Vector3(0f, 2.0f, -1.5f), new Color(1f, 0.48f, 0.16f), 0.9f, 5f, "Hut Warm Window");
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

        private static void CreateOceanMenuUI(Transform parent, GameObject player, Camera cam, Vector3 gameStartPos, Vector3 autoWalkTargetPos)
    {
        Vector3 oceanPos = new Vector3(20000f, 0f, 20000f);

        GameObject ocean = new GameObject("Cinematic Menu Ocean");
        ocean.transform.SetParent(parent);
        ocean.transform.position = oceanPos;
        var oceanWater = ocean.AddComponent<OceanWater>();
        oceanWater.size = 720f;
        oceanWater.resolution = 120;                                  // higher mesh density -> finer waves
        oceanWater.foamAmount = 0.46f;                                // a bit more whitewater
        oceanWater.shallowColor = new Color(0.10f, 0.40f, 0.58f, 1f); // richer teal in the shallows
        oceanWater.deepColor    = new Color(0.012f, 0.06f, 0.14f, 1f);// deeper blue offshore
        oceanWater.smoothness = 0.96f;                                // glossier surface for sun glints
        oceanWater.metallic = 0.05f;
        ocean.GetComponent<MeshRenderer>().sharedMaterial = CreateMenuWaterMaterial();

        Material dockWood = MakeMat(null, 0.18f);
        dockWood.color = new Color(0.26f, 0.14f, 0.07f);
        Material dockDarkWood = MakeMat(null, 0.12f);
        dockDarkWood.color = new Color(0.12f, 0.07f, 0.04f);

        CreateDock(oceanPos + new Vector3(0f, 0.2f, 54f), parent, dockWood, dockDarkWood);

        GameObject boat = new GameObject("Menu Longship");
        boat.transform.SetParent(parent);
        // Raise boat root so the hull rests above the water surface (was 0.18 -> partly submerged).
        boat.transform.position = oceanPos + new Vector3(0f, 0.6f, 40f);
        boat.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        var boatBody = boat.AddComponent<Rigidbody>();
        boatBody.isKinematic = true;
        boatBody.interpolation = RigidbodyInterpolation.Interpolate;

        if (CreateCinematicShip(boat.transform) == null)
            CreateFallbackLongship(boat.transform);

        var boatBobber = boat.AddComponent<BoatBobber>();
        boatBobber.ocean = oceanWater;
        boatBobber.vertOffsetFromSurface = 0.55f;
        boatBobber.positionLerp = 2.4f;
        boatBobber.rotationLerp = 1.85f;
        boatBobber.maxRollDegrees = 3.6f;
        boatBobber.maxPitchDegrees = 2.7f;
        boatBobber.forwardSampleDistance = 7f;
        boatBobber.sideSampleDistance = 3.2f;
        boatBobber.lateralDriftAmplitude = 0.55f;
        boatBobber.lateralDriftFrequency = 0.035f;
        boatBobber.lateralDriftAxis = new Vector3(1f, 0f, 0.35f);

        CreateMenuHero(boat.transform);

        player.transform.SetParent(boat.transform, false);
        player.transform.localPosition = new Vector3(0f, 1.35f, -1.7f);
        player.transform.localRotation = Quaternion.Euler(0f, 160f, 0f);

        GameObject menuSun = new GameObject("Menu Low Sun");
        menuSun.transform.SetParent(parent);
        menuSun.transform.rotation = Quaternion.Euler(13f, -42f, 0f);
        Light sun = menuSun.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.color = new Color(1f, 0.62f, 0.34f);
        sun.intensity = 1.15f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.55f;

        GameObject rimLight = new GameObject("Menu Sail Rim Light");
        rimLight.transform.SetParent(boat.transform, false);
        rimLight.transform.localPosition = new Vector3(-7f, 5f, -5f);
        Light rim = rimLight.AddComponent<Light>();
        rim.type = LightType.Point;
        rim.color = new Color(0.55f, 0.78f, 1f);
        rim.intensity = 1.4f;
        rim.range = 20f;

        GameObject seaMist = CreateMenuMist(parent, oceanPos);
        ConfigureMenuLighting();

        if (cam != null)
        {
            var isoCam = cam.GetComponent<NordicWilds.CameraSystems.IsometricCameraFollow>();
            if (isoCam != null)
                isoCam.enabled = false;

            cam.orthographic = false;
            cam.fieldOfView = 38f;
            cam.nearClipPlane = 0.08f;
            cam.farClipPlane = 1100f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.035f, 0.06f, 0.095f);
            cam.allowHDR = false;

            var director = cam.GetComponent<NordicWilds.UI.MenuCameraDirector>();
            if (director == null)
                director = cam.gameObject.AddComponent<NordicWilds.UI.MenuCameraDirector>();
            director.target = boat.transform;
            director.idleOffset = new Vector3(-23.5f, 9.7f, -29f);
            director.idleEndOffset = new Vector3(-17.2f, 8.4f, -23f);
            director.idleDriftSpeed = 0.045f;
            director.idleSwayAmplitude = 0.42f;
            director.lookHeightOffset = 3.0f;
            director.launchPushInAmount = 9f;
            director.launchAscend = 2.8f;
            director.launchDuration = 3.0f;
            director.SnapToIdle();
        }

        GameObject canvasObj = new GameObject("MainMenuCanvas");
        canvasObj.transform.SetParent(parent);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject evtSystem = new GameObject("EventSystem");
            evtSystem.transform.SetParent(parent);
            evtSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            evtSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Font menuFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/ZombiSoft/TinyHealthSystem/Demo/Fonts/Pixelnauts.ttf")
            ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
            ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject menuRoot = new GameObject("MenuRoot");
        menuRoot.transform.SetParent(canvasObj.transform, false);
        var menuRootRect = menuRoot.AddComponent<RectTransform>();
        menuRootRect.anchorMin = Vector2.zero;
        menuRootRect.anchorMax = Vector2.one;
        menuRootRect.offsetMin = Vector2.zero;
        menuRootRect.offsetMax = Vector2.zero;
        var menuRootGroup = menuRoot.AddComponent<CanvasGroup>();

        CanvasGroup titleGroup;
        RectTransform titleRect;
        CreateMenuTitle(menuRoot.transform, menuFont, out titleGroup, out titleRect);

        GameObject promptObj = CreateMenuText(
            "PromptText",
            "THE TIDE WAITS",
            menuRoot.transform,
            menuFont,
            28,
            new Color(0.80f, 0.89f, 0.95f, 0.9f),
            new Vector2(0.5f, 0.19f),
            new Vector2(520f, 44f));
        var promptGroup = promptObj.AddComponent<CanvasGroup>();
        promptGroup.alpha = 0.88f;

        GameObject buttons = new GameObject("MenuButtons");
        buttons.transform.SetParent(menuRoot.transform, false);
        var buttonsRect = buttons.AddComponent<RectTransform>();
        buttonsRect.anchorMin = new Vector2(0.5f, 0.095f);
        buttonsRect.anchorMax = new Vector2(0.5f, 0.095f);
        buttonsRect.sizeDelta = new Vector2(780f, 74f);
        buttonsRect.anchoredPosition = Vector2.zero;
        var buttonsGroup = buttons.AddComponent<CanvasGroup>();

        var startButton = CreateMenuButton("StartButton", "START GAME", buttons.transform, menuFont, new Vector2(-165f, 0f), new Vector2(290f, 64f));
        var quitButton = CreateMenuButton("QuitButton", "QUIT", buttons.transform, menuFont, new Vector2(165f, 0f), new Vector2(190f, 58f));

        GameObject faderObj = new GameObject("FaderPanel");
        faderObj.transform.SetParent(canvasObj.transform, false);
        var faderImg = faderObj.AddComponent<UnityEngine.UI.Image>();
        faderImg.color = Color.black;
        StretchRect(faderObj.GetComponent<RectTransform>());
        var faderGroup = faderObj.AddComponent<CanvasGroup>();
        faderGroup.alpha = 0f;
        faderGroup.blocksRaycasts = false;
        faderObj.transform.SetAsLastSibling();

        var titleAnimator = canvasObj.AddComponent<NordicWilds.UI.MenuTitleAnimator>();
        titleAnimator.titleGroup = titleGroup;
        titleAnimator.titleRect = titleRect;
        titleAnimator.pressPromptGroup = promptGroup;
        titleAnimator.buttonsGroup = buttonsGroup;

        var mmc = canvasObj.AddComponent<NordicWilds.UI.MainMenuController>();
        mmc.faderGroup = faderGroup;
        mmc.boat = boat;
        mmc.player = player.transform;
        mmc.mainCamera = cam;
        mmc.gameStartPos = gameStartPos;
        mmc.autoWalkFromBoatOnStart = true;
        mmc.autoWalkTargetPos = autoWalkTargetPos;
        mmc.autoWalkDuration = 4.0f;
        mmc.startsInYamato = false;
        mmc.cameraDirector = cam != null ? cam.GetComponent<NordicWilds.UI.MenuCameraDirector>() : null;
        mmc.titleAnimator = titleAnimator;
        mmc.menuRootGroup = menuRootGroup;
        mmc.startButton = startButton;
        mmc.quitButton = quitButton;
        mmc.boatBobber = boatBobber;
        mmc.menuOnlyObjects = new[] { ocean, menuSun, seaMist };
        mmc.hidePlayerOnMenu = true;
        mmc.boatSailDuration = 3.25f;
        mmc.boatSailDistance = 31f;
        mmc.fadeOutDuration = 1.45f;
        mmc.menuUiFadeDuration = 0.45f;
        mmc.blackHold = 0.55f;
        mmc.fadeInDuration = 1.25f;
    }

    private static Material CreateMenuWaterMaterial()
    {
        Shader shader = Shader.Find("Standard");
        Material mat = new Material(shader != null ? shader : Shader.Find("Diffuse"));
        mat.name = "OceanMat_Menu_Cinematic";
        mat.color = new Color(0.06f, 0.22f, 0.34f, 1f);
        if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic", 0.05f);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.96f);
        // Subtle warm reflective tint so sunset light glints off crests instead of
        // looking like flat painted plastic.
        if (mat.HasProperty("_SpecColor"))  mat.SetColor("_SpecColor", new Color(1f, 0.78f, 0.55f));
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(0.02f, 0.05f, 0.08f));
        }
        return mat;
    }

    private static GameObject CreateCinematicShip(Transform parent)
    {
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Imported/boat/source/ship.fbx");
        if (source == null)
            return null;

        GameObject ship = PrefabUtility.InstantiatePrefab(source) as GameObject;
        if (ship == null)
            ship = Object.Instantiate(source);

        ship.name = "Imported Viking Ship";
        ship.transform.SetParent(parent, false);
        ship.transform.localPosition = Vector3.zero;
        ship.transform.localRotation = Quaternion.identity;
        ship.transform.localScale = Vector3.one;

        Renderer[] renderers = ship.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return ship;

        Bounds initial = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            initial.Encapsulate(renderers[i].bounds);
        if (initial.size.x > initial.size.z)
            ship.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

        // Waterline lifted so the hull sits ON the surface, not partly submerged.
        // (The previous -0.55 sank the hull bottom 0.55 below water and made the
        // ship look flooded.)
        FitShipToMenuScale(ship, 22f, 0.45f);
        ApplyShipMenuMaterials(ship);
        return ship;
    }

    private static void FitShipToMenuScale(GameObject ship, float targetLength, float waterline)
    {
        Renderer[] renderers = ship.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float length = Mathf.Max(bounds.size.x, bounds.size.z);
        if (length > 0.001f)
            ship.transform.localScale *= targetLength / length;

        renderers = ship.GetComponentsInChildren<Renderer>(true);
        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        Vector3 offset = ship.transform.position - bounds.center;
        offset.y = ship.transform.parent.position.y + waterline - bounds.min.y;
        ship.transform.position += offset;
    }

    private static void ApplyShipMenuMaterials(GameObject ship)
    {
        Material wood = MakeMat(null, 0.28f); wood.color = new Color(0.30f, 0.16f, 0.075f);
        Material darkWood = MakeMat(null, 0.20f); darkWood.color = new Color(0.12f, 0.07f, 0.045f);
        Material sail = MakeMat(null, 0.18f); sail.color = new Color(0.72f, 0.58f, 0.42f);
        Material shield = MakeMat(null, 0.38f); shield.color = new Color(0.55f, 0.10f, 0.06f);
        Material metal = MakeMat(null, 0.62f); metal.color = new Color(0.62f, 0.55f, 0.45f);
        Material rope = MakeMat(null, 0.12f); rope.color = new Color(0.36f, 0.27f, 0.18f);

        foreach (Renderer renderer in ship.GetComponentsInChildren<Renderer>(true))
        {
            Material[] mats = renderer.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
            {
                string n = (renderer.name + " " + (mats[i] != null ? mats[i].name : "")).ToLowerInvariant();
                if (n.Contains("sail"))
                    mats[i] = sail;
                else if (n.Contains("shield"))
                    mats[i] = shield;
                else if (n.Contains("metal") || n.Contains("bronze") || n.Contains("iron"))
                    mats[i] = metal;
                else if (n.Contains("rope"))
                    mats[i] = rope;
                else if (n.Contains("keel") || n.Contains("trim"))
                    mats[i] = darkWood;
                else
                    mats[i] = wood;
            }
            renderer.sharedMaterials = mats;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }
    }

    private static GameObject CreateFallbackLongship(Transform parent)
    {
        GameObject root = new GameObject("Fallback Cinematic Longship");
        root.transform.SetParent(parent, false);
        Material wood = MakeMat(null, 0.24f); wood.color = new Color(0.26f, 0.13f, 0.06f);
        Material sail = MakeMat(null, 0.16f); sail.color = new Color(0.74f, 0.57f, 0.42f);
        CreateMenuHeroPart(root.transform, PrimitiveType.Cube, "Hull", new Vector3(0f, 0.1f, 0f), new Vector3(4.2f, 0.75f, 17f), wood);
        CreateMenuHeroPart(root.transform, PrimitiveType.Cube, "Raised Bow", new Vector3(0f, 1.25f, 8.15f), new Vector3(2.3f, 2.2f, 0.38f), wood);
        CreateMenuHeroPart(root.transform, PrimitiveType.Cube, "Raised Stern", new Vector3(0f, 1.05f, -8.15f), new Vector3(2.1f, 1.8f, 0.38f), wood);
        CreateMenuHeroPart(root.transform, PrimitiveType.Cube, "Mast", new Vector3(0f, 3.1f, 1.2f), new Vector3(0.22f, 6.2f, 0.22f), wood);
        CreateMenuHeroPart(root.transform, PrimitiveType.Cube, "Sail", new Vector3(0f, 3.5f, 1.2f), new Vector3(0.12f, 3.2f, 4.8f), sail);
        return root;
    }

    private static void CreateMenuHero(Transform boat)
    {
        Material cloak = MakeMat(null, 0.22f); cloak.color = new Color(0.18f, 0.25f, 0.33f);
        Material fur = MakeMat(null, 0.12f); fur.color = new Color(0.62f, 0.55f, 0.44f);
        Material skin = MakeMat(null, 0.16f); skin.color = new Color(0.77f, 0.62f, 0.46f);
        Material leather = MakeMat(null, 0.18f); leather.color = new Color(0.24f, 0.13f, 0.07f);
        Material metal = MakeMat(null, 0.70f); metal.color = new Color(0.60f, 0.67f, 0.72f);

        GameObject hero = new GameObject("Menu Protagonist On Rail");
        hero.transform.SetParent(boat, false);
        hero.transform.localPosition = new Vector3(2.25f, 0.95f, 4.7f);
        hero.transform.localRotation = Quaternion.Euler(0f, -118f, 0f);

        CreateMenuHeroPart(hero.transform, PrimitiveType.Capsule, "Seated Body", new Vector3(0f, 0.62f, 0f), new Vector3(0.62f, 0.82f, 0.56f), cloak);
        CreateMenuHeroPart(hero.transform, PrimitiveType.Sphere, "Head", new Vector3(0f, 1.38f, 0.03f), new Vector3(0.44f, 0.44f, 0.44f), skin);
        CreateMenuHeroPart(hero.transform, PrimitiveType.Cube, "Fur Collar", new Vector3(0f, 1.05f, 0f), new Vector3(0.92f, 0.18f, 0.72f), fur);
        CreateMenuHeroPart(hero.transform, PrimitiveType.Cube, "Left Arm Resting", new Vector3(-0.52f, 0.74f, 0.25f), new Vector3(0.18f, 0.18f, 0.88f), leather).transform.localRotation = Quaternion.Euler(13f, 0f, -17f);
        CreateMenuHeroPart(hero.transform, PrimitiveType.Cube, "Right Arm Resting", new Vector3(0.52f, 0.72f, 0.16f), new Vector3(0.18f, 0.18f, 0.78f), leather).transform.localRotation = Quaternion.Euler(-10f, 0f, 18f);
        CreateMenuHeroPart(hero.transform, PrimitiveType.Cube, "Left Boot Over Edge", new Vector3(-0.22f, 0.02f, 0.58f), new Vector3(0.24f, 0.24f, 1.02f), leather).transform.localRotation = Quaternion.Euler(34f, 0f, -5f);
        CreateMenuHeroPart(hero.transform, PrimitiveType.Cube, "Right Boot Over Edge", new Vector3(0.28f, 0.0f, 0.52f), new Vector3(0.24f, 0.24f, 0.92f), leather).transform.localRotation = Quaternion.Euler(26f, 0f, 7f);
        CreateMenuHeroPart(hero.transform, PrimitiveType.Cube, "Axe Across Lap", new Vector3(0.05f, 0.62f, 0.35f), new Vector3(1.15f, 0.09f, 0.09f), metal).transform.localRotation = Quaternion.Euler(0f, 0f, -13f);
    }

    private static GameObject CreateMenuHeroPart(Transform parent, PrimitiveType primitive, string name, Vector3 localPos, Vector3 localScale, Material mat)
    {
        GameObject part = GameObject.CreatePrimitive(primitive);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPos;
        part.transform.localScale = localScale;
        Collider c = part.GetComponent<Collider>();
        if (c != null) DestroyImmediate(c);
        Renderer r = part.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = mat;
        return part;
    }

    private static GameObject CreateMenuMist(Transform parent, Vector3 oceanPos)
    {
        GameObject mistRoot = new GameObject("Menu Sea Mist");
        mistRoot.transform.SetParent(parent);
        mistRoot.transform.position = oceanPos + new Vector3(0f, 1.2f, 0f);

        // ── Layer 1: dense LOW mist hugging the water surface (the bulk of the fog)
        GameObject low = new GameObject("Mist Low Layer");
        low.transform.SetParent(mistRoot.transform, false);
        low.transform.localPosition = new Vector3(0f, -0.5f, 0f);
        ConfigureMistLayer(low,
            startLifetime: new Vector2(14f, 22f),
            startSpeed: new Vector2(0.05f, 0.18f),
            startSize: new Vector2(14f, 28f),
            maxParticles: 600,
            rateOverTime: 60f,
            shape: new Vector3(160f, 1f, 160f),
            colorStart: new Color(0.78f, 0.86f, 0.94f),
            colorEnd: new Color(0.40f, 0.52f, 0.66f),
            peakAlpha: 0.32f);

        // ── Layer 2: drifting MID-height wisps that catch the sun
        GameObject mid = new GameObject("Mist Mid Wisps");
        mid.transform.SetParent(mistRoot.transform, false);
        mid.transform.localPosition = new Vector3(0f, 3.5f, 0f);
        ConfigureMistLayer(mid,
            startLifetime: new Vector2(10f, 16f),
            startSpeed: new Vector2(0.10f, 0.30f),
            startSize: new Vector2(8f, 16f),
            maxParticles: 280,
            rateOverTime: 22f,
            shape: new Vector3(150f, 1f, 150f),
            colorStart: new Color(1f, 0.86f, 0.66f),       // warm sunset tint
            colorEnd: new Color(0.50f, 0.58f, 0.66f),
            peakAlpha: 0.22f);

        // ── Layer 3: distant horizon haze — large slow puffs in the background
        GameObject horizon = new GameObject("Mist Horizon Haze");
        horizon.transform.SetParent(mistRoot.transform, false);
        horizon.transform.localPosition = new Vector3(0f, 6f, 60f);
        ConfigureMistLayer(horizon,
            startLifetime: new Vector2(20f, 30f),
            startSpeed: new Vector2(0.02f, 0.10f),
            startSize: new Vector2(30f, 60f),
            maxParticles: 80,
            rateOverTime: 4f,
            shape: new Vector3(280f, 1f, 60f),
            colorStart: new Color(0.85f, 0.82f, 0.74f),
            colorEnd: new Color(0.30f, 0.36f, 0.48f),
            peakAlpha: 0.18f);

        return mistRoot;
    }

    private static void ConfigureMistLayer(GameObject host,
        Vector2 startLifetime, Vector2 startSpeed, Vector2 startSize,
        int maxParticles, float rateOverTime, Vector3 shape,
        Color colorStart, Color colorEnd, float peakAlpha)
    {
        ParticleSystem ps = host.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(startLifetime.x, startLifetime.y);
        main.startSpeed    = new ParticleSystem.MinMaxCurve(startSpeed.x, startSpeed.y);
        main.startSize     = new ParticleSystem.MinMaxCurve(startSize.x, startSize.y);
        main.maxParticles  = maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

        var emission = ps.emission;
        emission.rateOverTime = rateOverTime;

        var sh = ps.shape;
        sh.shapeType = ParticleSystemShapeType.Box;
        sh.scale = shape;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.22f, 0.22f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.04f, 0.06f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.10f, 0.28f);

        // Subtle rotation for wispy feel
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-0.18f, 0.18f);

        // Drifting noise so the fog doesn't move in straight lines
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.22f;
        noise.frequency = 0.18f;
        noise.scrollSpeed = 0.12f;

        var color = ps.colorOverLifetime;
        color.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(colorStart, 0f), new GradientColorKey(colorEnd, 1f) },
            new[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(peakAlpha, 0.4f),
                new GradientAlphaKey(peakAlpha * 0.6f, 0.75f),
                new GradientAlphaKey(0f, 1f)
            });
        color.color = new ParticleSystem.MinMaxGradient(g);

        var renderer = host.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingFudge = -2f; // sort behind nearby props
    }

    private static void ConfigureMenuLighting()
    {
        RenderSettings.skybox = null;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.0048f;                              // softer falloff so distance reads
        RenderSettings.fogColor = new Color(0.20f, 0.26f, 0.34f);         // warmer, blends with sunset
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.32f, 0.40f, 0.50f);
        RenderSettings.ambientEquatorColor = new Color(0.28f, 0.30f, 0.34f);
        RenderSettings.ambientGroundColor = new Color(0.06f, 0.07f, 0.10f);
        RenderSettings.ambientIntensity = 0.85f;
        RenderSettings.reflectionIntensity = 0.55f;
    }

    private static void CreateMenuTitle(Transform parent, Font font, out CanvasGroup group, out RectTransform rect)
    {
        GameObject titleObj = CreateMenuText(
            "TitleText",
            "SON OF VINLAND",
            parent,
            font,
            118,
            new Color(0.92f, 0.80f, 0.56f, 1f),
            new Vector2(0.5f, 0.76f),
            new Vector2(1260f, 160f));
        rect = titleObj.GetComponent<RectTransform>();
        group = titleObj.AddComponent<CanvasGroup>();

        var outline = titleObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0.08f, 0.10f, 0.12f, 0.95f);
        outline.effectDistance = new Vector2(3f, -3f);

        GameObject ruleTop = CreateMenuText("TitleRuleTop", "RAVEN-BLOOD  |  IRON SEA  |  HOMEWARD FIRE", parent, font, 24, new Color(0.68f, 0.78f, 0.86f, 0.88f), new Vector2(0.5f, 0.875f), new Vector2(980f, 38f));
        GameObject ruleBottom = CreateMenuText("TitleRuleBottom", "A NORSE SAGA ON RESTLESS WATER", parent, font, 26, new Color(0.82f, 0.89f, 0.94f, 0.92f), new Vector2(0.5f, 0.655f), new Vector2(900f, 42f));
        ruleTop.transform.SetSiblingIndex(titleObj.transform.GetSiblingIndex());
        ruleBottom.transform.SetSiblingIndex(titleObj.transform.GetSiblingIndex() + 1);
    }

    private static GameObject CreateMenuText(string name, string text, Transform parent, Font font, int size, Color color, Vector2 anchor, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var uiText = obj.AddComponent<UnityEngine.UI.Text>();
        uiText.text = text;
        uiText.font = font;
        uiText.fontSize = size;
        uiText.fontStyle = FontStyle.Bold;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
        uiText.verticalOverflow = VerticalWrapMode.Overflow;
        uiText.color = color;
        var shadow = obj.AddComponent<UnityEngine.UI.Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.82f);
        shadow.effectDistance = new Vector2(2.5f, -2.5f);
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = Vector2.zero;
        return obj;
    }

    private static UnityEngine.UI.Button CreateMenuButton(string name, string label, Transform parent, Font font, Vector2 pos, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        var image = buttonObj.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.10f, 0.13f, 0.15f, 0.78f);
        var button = buttonObj.AddComponent<UnityEngine.UI.Button>();
        button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
        UnityEngine.UI.ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.12f, 0.16f, 0.18f, 0.86f);
        colors.highlightedColor = new Color(0.58f, 0.40f, 0.19f, 0.94f);
        colors.pressedColor = new Color(0.76f, 0.54f, 0.24f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.08f, 0.08f, 0.08f, 0.45f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        buttonObj.AddComponent<NordicWilds.UI.MenuButtonAnimator>();

        var outline = buttonObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = new Color(0.86f, 0.65f, 0.32f, 0.70f);
        outline.effectDistance = new Vector2(2f, -2f);

        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = pos;

        GameObject textObj = CreateMenuText("Text", label, buttonObj.transform, font, name == "StartButton" ? 30 : 24, new Color(0.96f, 0.90f, 0.76f, 1f), new Vector2(0.5f, 0.5f), size);
        StretchRect(textObj.GetComponent<RectTransform>());
        return button;
    }

    private static void StretchRect(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

}
}
