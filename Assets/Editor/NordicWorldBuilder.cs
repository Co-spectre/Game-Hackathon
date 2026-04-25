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
    public class NordicWorldBuilder : EditorWindow
    {
        [MenuItem("World Foundation/Generate Masterpiece Environment")]
        [MenuItem("Nordic Wilds/Generate Masterpiece Environment")]
        public static void BuildMasterpieceWorld()
        {
            Debug.Log("Generating High-Standard Nordic Environment...");

            // 1. Tag Management (Ensure custom tags exist securely)
            EnsureTagExists("Player");
            EnsureTagExists("Campfire");
            EnsureTagExists("Enemy");

            // 2. Clear Old Scene to avoid overlapping generations
            DestroyImmediate(GameObject.Find("Nordic World Root"));
            DestroyImmediate(GameObject.Find("Player"));
            DestroyImmediate(GameObject.Find("Isometric Camera Rig"));

            GameObject worldRoot = new GameObject("Nordic World Root");

            // 3. Setup Atmospheric Materials & Procedural Textures
            // Generate Procedural Noise Textures for "more than just colors"
            Texture2D snowTex = GenerateNoiseTexture(new Color(0.85f, 0.9f, 0.95f), new Color(0.75f, 0.85f, 0.9f), 15f);
            Material snowMat = new Material(Shader.Find("Standard"));
            snowMat.enableInstancing = true; // Optimization: GPU Instancing
            snowMat.mainTexture = snowTex;
            snowMat.color = Color.white; // Tint via texture
            snowMat.SetFloat("_Glossiness", 0.1f); // Matte surface
            
            Texture2D stoneTex = GenerateNoiseTexture(new Color(0.3f, 0.35f, 0.4f), new Color(0.15f, 0.2f, 0.25f), 30f);
            Material stoneMat = new Material(Shader.Find("Standard"));
            stoneMat.enableInstancing = true; // Optimization: GPU Instancing
            stoneMat.mainTexture = stoneTex;
            stoneMat.color = Color.white;
            stoneMat.SetFloat("_Glossiness", 0.3f); 
            
            Texture2D pineTex = GenerateNoiseTexture(new Color(0.15f, 0.35f, 0.2f), new Color(0.1f, 0.25f, 0.15f), 10f);
            Material pineMat = new Material(Shader.Find("Standard"));
            pineMat.enableInstancing = true; // Optimization: GPU Instancing
            pineMat.mainTexture = pineTex;
            pineMat.color = Color.white;
            
            Texture2D woodTex = GenerateNoiseTexture(new Color(0.25f, 0.15f, 0.05f), new Color(0.15f, 0.1f, 0.02f), 50f); // High frequency for grain
            Material woodMat = new Material(Shader.Find("Standard"));
            woodMat.enableInstancing = true; // Optimization: GPU Instancing
            woodMat.mainTexture = woodTex;
            woodMat.color = Color.white;

            // 4. Lighting & Atmosphere (Moonlight/Cold Sun + Unity Environment Settings)
#if UNITY_2023_1_OR_NEWER
            Light dirLight = Object.FindFirstObjectByType<Light>();
#else
            Light dirLight = Object.FindObjectOfType<Light>();
#endif
            if (dirLight != null && dirLight.type == LightType.Directional)
            {
                dirLight.color = new Color(0.6f, 0.75f, 1f); // Cold blue ambient
                dirLight.intensity = 1.1f;
                dirLight.transform.rotation = Quaternion.Euler(35f, -60f, 0f); // Long dramatic shadows
                dirLight.shadows = LightShadows.Soft;
            }

            // Setup Ambient Fog to give the scene massive depth and hide the far walls
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.6f, 0.75f, 0.9f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.015f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientSkyColor = new Color(0.4f, 0.5f, 0.7f);
            RenderSettings.ambientEquatorColor = new Color(0.3f, 0.4f, 0.6f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.3f, 0.5f);

            // 5. Environmental Architecture (The "Level Design" Pass)
            // Main Arena Floor (Expanded into a vast Tundra)
            CreateBlock("Arena Floor", new Vector3(0, -0.5f, 0), new Vector3(400, 1, 400), snowMat, worldRoot.transform);

            // Grand Elevation (North Altar Step) - Gives verticality and depth
            CreateBlock("North Cliff Step", new Vector3(0, 1.5f, 95), new Vector3(400, 3, 210), snowMat, worldRoot.transform);
            
            // Giant Imposing Enclosure Walls (Pushed way out into the distance so they aren't visible, adding depth)
            CreateBlock("West Wall (Cliff)", new Vector3(-200, 5f, 0), new Vector3(10, 20, 400), stoneMat, worldRoot.transform);
            CreateBlock("East Wall (Cliff)", new Vector3(200, 5f, 0), new Vector3(10, 20, 400), stoneMat, worldRoot.transform);
            CreateBlock("South Wall", new Vector3(0, 5f, -200), new Vector3(400, 20, 10), stoneMat, worldRoot.transform);
            CreateBlock("North Wall", new Vector3(0, 5f, 200), new Vector3(400, 20, 10), stoneMat, worldRoot.transform);

            // Invisible Boundaries (Prevents the player from reaching the distant visual walls)
            GameObject invisibleBounds = new GameObject("Invisible Boundaries");
            invisibleBounds.transform.SetParent(worldRoot.transform);
            CreateInvisibleWall("North Bound", new Vector3(0, 5f, 75f), new Vector3(160, 20, 2), invisibleBounds.transform);
            CreateInvisibleWall("South Bound", new Vector3(0, 5f, -75f), new Vector3(160, 20, 2), invisibleBounds.transform);
            CreateInvisibleWall("West Bound", new Vector3(-80f, 5f, 0), new Vector3(2, 20, 160), invisibleBounds.transform);
            CreateInvisibleWall("East Bound", new Vector3(80f, 5f, 0), new Vector3(2, 20, 160), invisibleBounds.transform);

            // Frame the Entrance (An inviting corridor narrowing to build tension)
            CreateBlock("Entrance Path Floor", new Vector3(0, -0.5f, -40), new Vector3(8, 1, 40), snowMat, worldRoot.transform);
            
            // Generate Grand Stairs leading up to the North Cliff Step
            GameObject grandStairs = new GameObject("Grand Stairs");
            grandStairs.transform.position = new Vector3(0, 0, 42.5f);
            grandStairs.transform.SetParent(worldRoot.transform);
            for (int i = 0; i < 10; i++)
            {
                // Each step gets slightly higher and slightly further in Z
                Vector3 stepPos = new Vector3(0, (i * 0.3f) - 0.5f, 40f + (i * 0.5f));
                CreateBlock("Step_" + i, stepPos, new Vector3(12f, 0.3f, 0.5f), stoneMat, grandStairs.transform);
            }
            
            // Smooth Invisible Ramp for the Stairs so players and enemies don't get stuck on hard box collider corners!
            GameObject stairRamp = CreateInvisibleWall("Stair Ramp", new Vector3(0, 1.25f, 42.5f), new Vector3(12f, 0.5f, 6.5f), grandStairs.transform);
            stairRamp.transform.rotation = Quaternion.Euler(-32f, 0, 0); // Angles the ramp to seamlessly bridge the steps

            // Central Focal Point: The Great Runestone on the North Altar
            GameObject runestone = CreateBlock("Great Runestone", new Vector3(0, 4.5f, 50), new Vector3(3, 8, 2), stoneMat, worldRoot.transform);
            runestone.transform.rotation = Quaternion.Euler(0, 15, 10); // Tilted, ancient feel

            // 6. Mechanics Integration: The Campfire (Safe Zone within the freezing arena)
            GameObject campfireZone = new GameObject("Campfire Safe Zone");
            campfireZone.tag = "Campfire";
            campfireZone.transform.position = new Vector3(-12, 0.5f, 8);
            campfireZone.transform.SetParent(worldRoot.transform);
            
            // Campfire visuals
            CreateBlock("Fire Pit Base", new Vector3(0, 0, 0), new Vector3(2, 0.2f, 2), stoneMat, campfireZone.transform);
            GameObject fireLight = new GameObject("Fire Glow");
            fireLight.transform.SetParent(campfireZone.transform);
            fireLight.transform.localPosition = new Vector3(0, 1, 0);
            Light pLight = fireLight.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.color = new Color(1f, 0.5f, 0f); // Warm Orange
            pLight.intensity = 5f;
            pLight.range = 8f;

            // Box Trigger for the Temperature System (Warmth Radius)
            BoxCollider heatZone = campfireZone.AddComponent<BoxCollider>();
            heatZone.isTrigger = true;
            heatZone.size = new Vector3(8, 5, 8);

            // 6.5 immersive Pathways (Dirt/Snow trails)
            Material pathMat = new Material(Shader.Find("Standard"));
            pathMat.color = new Color(0.6f, 0.65f, 0.7f); // Packed dirty snow
            
            // Path from entrance to altar
            for (int i = 0; i < 20; i++)
            {
                Vector3 pathPos = new Vector3(Random.Range(-2f, 2f), 0.05f, -25 + (i * 4));
                var p = CreateBlock("Path Node", pathPos, new Vector3(6f, 0.1f, 6f), pathMat, worldRoot.transform);
                p.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }

            // 7. Micro-Detailing (Adding life via scattered Nordic Pines, Rocks, & Fallen Logs)
            // Generate dense forest edges and ruins to give natural boundaries and scale
            for (int i = 0; i < 600; i++) // Massively increased tree count for vast depth
            {
                // Push trees to the perimeter, expanding way out to 190
                Vector3 pos = new Vector3(Random.Range(-190f, 190f), 0.5f, Random.Range(-190f, 190f));
                if (pos.magnitude > 25f) CreatePineTree(pos, worldRoot.transform, pineMat, woodMat);
            }

            // ============================================
            // PORTAL TO ANCIENT JAPAN
            // ============================================
            CreateJapanRealm(worldRoot.transform);

            // Construct the physical portal gate in the Nordic Village
            Vector3 pPos = new Vector3(6f, 0f, -15f); // Placed right near the player's spawn point
            
            // Build an inviting Torii Gate-style arch out of dark stone & pink accents
            CreateBlock("Portal Pillar L", pPos + new Vector3(-2.8f, 3.5f, 0), new Vector3(0.8f, 7f, 0.8f), stoneMat, worldRoot.transform);
            CreateBlock("Portal Pillar R", pPos + new Vector3(2.8f, 3.5f, 0), new Vector3(0.8f, 7f, 0.8f), stoneMat, worldRoot.transform);
            CreateBlock("Portal Arch", pPos + new Vector3(0, 7f, 0), new Vector3(8f, 1f, 1f), stoneMat, worldRoot.transform);
            CreateBlock("Portal Sub Arch", pPos + new Vector3(0, 6.2f, 0), new Vector3(6.5f, 0.5f, 0.6f), stoneMat, worldRoot.transform);

            // Glowing inner gateway
            Material portalMagicalMat = new Material(Shader.Find("Unlit/Color"));
            portalMagicalMat.color = new Color(1f, 0.5f, 0.8f, 0.6f); // Semi-transparent pink

            GameObject portalGlow = CreateBlock("Portal Glow (Entrance)", pPos + new Vector3(0, 3.2f, 0), new Vector3(4.8f, 6.2f, 0.4f), portalMagicalMat, worldRoot.transform);
            
            // Add a vibrant pulsating pink point light so the snow glows around it
            Light portalLight = portalGlow.AddComponent<Light>();
            portalLight.type = LightType.Point;
            portalLight.color = new Color(1f, 0.4f, 0.8f);
            portalLight.intensity = 5f;
            portalLight.range = 15f;

            // Box Collider teleportation trigger
            BoxCollider pCol = portalGlow.GetComponent<BoxCollider>();
            if (pCol == null) pCol = portalGlow.AddComponent<BoxCollider>();
            pCol.isTrigger = true;
            pCol.size = new Vector3(1.1f, 1.1f, 6f); // Expand trigger depth so dash doesn't skip it
            var pScript = portalGlow.AddComponent<JapanPortal>();
            pScript.destinationTarget = new Vector3(10000, 2f, 10000); // Target Japan Realm

            // Majestic Sakura Petal vortex spilling out towards the player!
            ParticleSystem pPs = portalGlow.AddComponent<ParticleSystem>();
            var pMain = pPs.main;
            pMain.startColor = new Color(1f, 0.7f, 0.8f);
            pMain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.35f);
            pMain.startLifetime = 4f;
            pMain.startSpeed = 2f;
            var pe = pPs.emission; pe.rateOverTime = 35; // Thicker petals!
            var pShape = pPs.shape; 
            pShape.shapeType = ParticleSystemShapeType.Box;
            pShape.scale = new Vector3(4f, 6f, 1f);
            var pRenderer = portalGlow.GetComponent<ParticleSystemRenderer>();
            pRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            var pVel = pPs.velocityOverLifetime;
            pVel.enabled = true;
            pVel.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f); // ALL axes MUST have the same curve mode!
            pVel.y = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
            pVel.z = new ParticleSystem.MinMaxCurve(-3f, -1f); // Blow petals out the front of the gate!

            // Angle the entire portal to face the player slightly
            GameObject portalGroup = new GameObject("Portal Gateway Group");
            portalGroup.transform.SetParent(worldRoot.transform);
            foreach (Transform child in worldRoot.transform)
            {
                if (child.name.StartsWith("Portal")) child.SetParent(portalGroup.transform);
            }
            portalGroup.transform.rotation = Quaternion.Euler(0, -25f, 0);

            for (int i = 0; i < 200; i++) // Scale ruins for massive area
            {
                Vector3 pos = new Vector3(Random.Range(-180f, 180f), 1f, Random.Range(-180f, 180f));
                var s = CreateBlock("Ruin Stone", pos, new Vector3(Random.Range(1f, 5f), Random.Range(2f, 8f), Random.Range(1f, 5f)), stoneMat, worldRoot.transform);
                s.transform.rotation = Quaternion.Euler(Random.Range(-15, 15), Random.Range(0, 360), Random.Range(-15, 15));
            }

            // Scatter 60 Frozen Logs
            for (int i = 0; i < 60; i++) 
            {
                Vector3 pos = new Vector3(Random.Range(-150f, 150f), 0.5f, Random.Range(-150f, 150f));
                var l = CreateBlock("Fallen Log", pos, new Vector3(1f, 1f, Random.Range(4f, 8f)), woodMat, worldRoot.transform);
                l.transform.rotation = Quaternion.Euler(Random.Range(-10, 10), Random.Range(0, 360), Random.Range(-10, 10));
            }

            // Add more sprawling Nordic Huts on the outskirts (forming a small village)
            CreateHut(new Vector3(-25f, 0f, 15f), worldRoot.transform, woodMat, snowMat);
            CreateHut(new Vector3(30f, 0f, 5f), worldRoot.transform, woodMat, snowMat);
            CreateHut(new Vector3(-15f, 0f, -10f), worldRoot.transform, woodMat, snowMat);
            CreateHut(new Vector3(25f, 0f, 35f), worldRoot.transform, woodMat, snowMat);
            CreateHut(new Vector3(-35f, 0f, 35f), worldRoot.transform, woodMat, snowMat);

            // Generate an Interactive "Great Mead Hall" with enterable interior
            CreateGreatHall(new Vector3(0f, 0f, 25f), worldRoot.transform, woodMat, snowMat, pathMat);

            // Give the village a few extra light sources
            CreateTorch(new Vector3(-20f, 2f, 20f), worldRoot.transform);
            CreateTorch(new Vector3(25f, 2f, 15f), worldRoot.transform); 
            CreateBanner(new Vector3(-5f, 5f, 10f), worldRoot.transform);
            CreateBanner(new Vector3(5f, 5f, 10f), worldRoot.transform);

            // 7.2 Roaming Animals (Squirrels and Arctic Foxes)
            Material squirrelMat = new Material(Shader.Find("Standard"));
            squirrelMat.color = new Color(0.4f, 0.25f, 0.1f); // Brown
            Material foxMat = new Material(Shader.Find("Standard"));
            foxMat.color = new Color(0.85f, 0.9f, 0.95f); // Snowy gray/white

            for (int i = 0; i < 40; i++) // Squirrels
            {
                Vector3 pos = new Vector3(Random.Range(-70f, 70f), 0.25f, Random.Range(-70f, 70f));
                CreateAnimal("Squirrel", pos, new Vector3(0.5f, 0.5f, 0.8f), squirrelMat, 6f, worldRoot.transform);
            }
            for (int i = 0; i < 15; i++) // Foxes
            {
                Vector3 pos = new Vector3(Random.Range(-80f, 80f), 0.5f, Random.Range(-80f, 80f));
                CreateAnimal("Snow Fox", pos, new Vector3(0.8f, 1f, 1.6f), foxMat, 4f, worldRoot.transform);
            }

            // 7.3 Enemies (Combats Loop)
            Material enemyMat = new Material(Shader.Find("Standard"));
            enemyMat.color = new Color(0.8f, 0.1f, 0.1f); // Blood Red
            Material hitFlashMem = new Material(Shader.Find("Standard"));
            hitFlashMem.color = Color.white;
            
            for (int i = 0; i < 15; i++) // Spawn enemies around the map
            {
                Vector3 pos = new Vector3(Random.Range(-40f, 40f), 1f, Random.Range(10f, 60f));
                CreateEnemy("Draugr Thrall", pos, enemyMat, hitFlashMem, worldRoot.transform);
            }

            // 7.5. Dynamic Weather (Snowstorm Particle System)
            GameObject weatherSystem = new GameObject("Blizzard Weather");
            weatherSystem.transform.position = new Vector3(0, 30, 0); // High up to blanket the massive map
            weatherSystem.transform.SetParent(worldRoot.transform);
            ParticleSystem ps = weatherSystem.AddComponent<ParticleSystem>();
            
            // FIX PINK PARTICLES: Assign standard white material to the renderer
            ParticleSystemRenderer psRenderer = weatherSystem.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null) {
                psRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                psRenderer.sharedMaterial.color = new Color(1f, 1f, 1f, 0.8f);
            }

            var customPs = ps.main;
            customPs.startLifetime = 15f;
            customPs.startSpeed = 8f;
            customPs.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            customPs.maxParticles = 5000;
            var emission = ps.emission;
            emission.rateOverTime = 2000f; // Scale emission up significantly for massive arena
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(400, 1, 400); // Scale emission to cover the huge 400x400 floor
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-2f);
            vel.y = new ParticleSystem.MinMaxCurve(-4f);
            vel.z = new ParticleSystem.MinMaxCurve(0f);
            
            // Add World Collision so snow bounces/dies on the floor instead of falling infinitely
            var collision = ps.collision;
            collision.enabled = true;
            collision.type = ParticleSystemCollisionType.World;
            collision.bounce = 0.1f;
            collision.lifetimeLoss = 0.8f; // Kills particles quickly after they hit the floor
            collision.quality = ParticleSystemCollisionQuality.High;

            // 8. Player, Camera, & Encounter System Hookups
            GameObject player = SetupPlayer("Player");
            player.transform.position = new Vector3(0, 1f, -20); // Start way back in the entrance path
            
            Camera cam = SetupCamera(player.transform);
            SetupHadesEncounter(worldRoot.transform);

            // AUTO LINK CAMERA SO WASD WORKS ISO-CORRECTLY
            var pController = player.GetComponent<PlayerController>();
            SerializedObject pSo = new SerializedObject(pController);
            pSo.FindProperty("isometricCameraTransform").objectReferenceValue = cam.transform;
            pSo.ApplyModifiedProperties();

            // BUILD MINIMALIST UI HUD
            CreateMinimalUI();

            Debug.Log("Masterpiece Set Constructed.");
        }

        private static void CreateMinimalUI()
        {
            GameObject hudObj = new GameObject("MinimalHUDOverlay");
            
            // Link to the Minimal HUD Script (Now uses pure IMGUI / OnGUI to avoid package dependency issues)
            hudObj.AddComponent<MinimalHUD>();
        }

        private static GameObject CreateInvisibleWall(string name, Vector3 pos, Vector3 size, Transform parent)
        {
            GameObject wall = new GameObject(name);
            wall.transform.position = pos;
            wall.transform.SetParent(parent);
            BoxCollider col = wall.AddComponent<BoxCollider>();
            col.size = size;
            return wall;
        }

        private static GameObject CreateBlock(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent, bool isStatic = true)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = pos;
            cube.transform.localScale = scale;
            cube.transform.SetParent(parent);
            if (cube.GetComponent<Renderer>() != null)
                cube.GetComponent<Renderer>().sharedMaterial = mat;
            cube.isStatic = isStatic; // OPTIMIZATION: Allows Unity to Batch static meshes together
            return cube;
        }

        private static void CreatePineTree(Vector3 pos, Transform parent, Material foliage, Material wood)
        {
            GameObject tree = new GameObject("Nordic Pine");
            tree.transform.position = pos;
            tree.transform.SetParent(parent);

            // Trunk
            GameObject trunk = CreateBlock("Trunk", pos + new Vector3(0, 1, 0), new Vector3(0.6f, 2, 0.6f), wood, tree.transform);
            
            // Stylized Low-Poly Pine Needles (Rotated diamond cubes)
            for(int i=0; i<3; i++)
            {
                GameObject layer = CreateBlock("NeedleLayer", pos + new Vector3(0, 2.5f + (i * 1.5f), 0), new Vector3(3f - (i*0.8f), 1.5f, 3f - (i*0.8f)), foliage, tree.transform);
                layer.transform.rotation = Quaternion.Euler(0, 45, 45); // Diamond shaped
            }
        }

        private static void CreateGreatHall(Vector3 pos, Transform parent, Material wood, Material snow, Material pathMat)
        {
            GameObject hall = new GameObject("Great Mead Hall");
            hall.transform.position = pos;
            hall.transform.SetParent(parent);
            
            // Large wooden base with open front
            GameObject walls = new GameObject("Walls");
            walls.transform.SetParent(hall.transform);
            CreateBlock("Wall Back", pos + new Vector3(0, 3f, 8f), new Vector3(20, 6, 1), wood, walls.transform);
            CreateBlock("Wall Left", pos + new Vector3(-9.5f, 3f, 0), new Vector3(1, 6, 18), wood, walls.transform);
            CreateBlock("Wall Right", pos + new Vector3(9.5f, 3f, 0), new Vector3(1, 6, 18), wood, walls.transform);
            CreateBlock("Wall FrontLeft", pos + new Vector3(-6.5f, 3f, -8.5f), new Vector3(7, 6, 1), wood, walls.transform);
            CreateBlock("Wall FrontRight", pos + new Vector3(6.5f, 3f, -8.5f), new Vector3(7, 6, 1), wood, walls.transform);

            // Large Slanted Roofs (Parented to a "Roof" object so we can fade it)
            GameObject roofHolder = new GameObject("Roof Fader");
            roofHolder.transform.position = pos;
            roofHolder.transform.SetParent(hall.transform);

            GameObject roof1 = CreateBlock("Roof Left", pos + new Vector3(-5f, 7.5f, 0), new Vector3(12, 1f, 19), snow, roofHolder.transform);
            roof1.transform.rotation = Quaternion.Euler(0, 0, 25);
            GameObject roof2 = CreateBlock("Roof Right", pos + new Vector3(5f, 7.5f, 0), new Vector3(12, 1f, 19), snow, roofHolder.transform);
            roof2.transform.rotation = Quaternion.Euler(0, 0, -25);
            GameObject roofPeak = CreateBlock("Roof Peak", pos + new Vector3(0, 9.5f, 0), new Vector3(2, 1f, 19), wood, roofHolder.transform);

            // Add the interior component
            BoxCollider interiorTrigger = walls.AddComponent<BoxCollider>();
            interiorTrigger.isTrigger = true;
            interiorTrigger.size = new Vector3(18, 6, 16);
            interiorTrigger.center = new Vector3(0, 3, 0);

            var bInterior = walls.AddComponent<BuildingInterior>();
            bInterior.roofObject = roofHolder;

            // Interior Props
            CreateBlock("Jarl Table", pos + new Vector3(0, 1f, 5f), new Vector3(12, 0.5f, 2), wood, hall.transform);
            CreateBlock("Left Bench", pos + new Vector3(0, 0.5f, 3f), new Vector3(12, 0.5f, 1), wood, hall.transform);
            CreateBlock("Right Bench", pos + new Vector3(0, 0.5f, 7f), new Vector3(12, 0.5f, 1), wood, hall.transform);
            
            // Central fire within the hall
            GameObject hallFire = CreateBlock("Hall Fire", pos + new Vector3(0, 0.2f, -1f), new Vector3(4, 0.4f, 4), new Material(Shader.Find("Standard")){color = Color.black}, hall.transform);
            CreateTorch(pos + new Vector3(0, 0, -1f), hallFire.transform);

            // Lead-up Path
            CreateBlock("Hall Path", pos + new Vector3(0, 0.05f, -12f), new Vector3(6, 0.1f, 8), pathMat, hall.transform);
        }

        private static void CreateBanner(Vector3 pos, Transform parent)
        {
            GameObject flagPole = CreateBlock("Flag Pole", pos, new Vector3(0.2f, 6f, 0.2f), new Material(Shader.Find("Standard")){color = new Color(0.2f,0.1f,0.05f)}, parent);
            // Banners move, so they must not be STATIC!
            GameObject bannerObj = CreateBlock("Banner Cloth", pos + new Vector3(1.5f, 2f, 0), new Vector3(3f, 4f, 0.1f), new Material(Shader.Find("Standard")){color = new Color(0.7f,0.1f,0.1f)}, parent, false);
            
            SwayingObject sway = bannerObj.AddComponent<SwayingObject>();
            sway.swayAxis = Vector3.forward;
            sway.swayAmount = 15f;
            sway.swaySpeed = 3f;
        }

        private static void CreateHut(Vector3 pos, Transform parent, Material wood, Material snow)
        {
            GameObject hut = new GameObject("Nordic Hut");
            hut.transform.position = pos;
            hut.transform.SetParent(parent);
            
            hut.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            // Log cabin body
            CreateBlock("Cabin Walls", pos + new Vector3(0, 1.5f, 0), new Vector3(5, 3, 5), wood, hut.transform);

            // Left slant roof
            GameObject roof1 = CreateBlock("Roof Left", pos + new Vector3(-1.8f, 3.5f, 0), new Vector3(6, 0.5f, 6), snow, hut.transform);
            roof1.transform.rotation = hut.transform.rotation * Quaternion.Euler(0, 0, 35);

            // Right slant roof
            GameObject roof2 = CreateBlock("Roof Right", pos + new Vector3(1.8f, 3.5f, 0), new Vector3(6, 0.5f, 6), snow, hut.transform);
            roof2.transform.rotation = hut.transform.rotation * Quaternion.Euler(0, 0, -35);
        }

        private static GameObject SetupPlayer(string name)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = name;
            player.tag = "Player";
            
            Rigidbody rb = player.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            player.AddComponent<PlayerController>();
            player.AddComponent<Health>();
            player.AddComponent<TemperatureSystem>(); // Attached so the campfire interactions work instantly

            // Combat & Polish
            TrailRenderer dashTrail = player.AddComponent<TrailRenderer>();
            dashTrail.time = 0.2f;
            dashTrail.startWidth = 0.5f;
            dashTrail.endWidth = 0f;
            dashTrail.material = new Material(Shader.Find("Sprites/Default")) { color = new Color(0.2f, 0.8f, 1f, 0.5f) };
            dashTrail.emitting = false;

            // Simple visual weapon
            GameObject weapon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            weapon.name = "Leviathan Axe Handle";
            weapon.transform.SetParent(player.transform);
            weapon.transform.localPosition = new Vector3(0.6f, 0, 0.5f);
            weapon.transform.localScale = new Vector3(0.1f, 1.2f, 0.1f);
            DestroyImmediate(weapon.GetComponent<BoxCollider>()); // Don't block physics

            Material weaponMat = new Material(Shader.Find("Standard"));
            weaponMat.color = Color.gray;
            weapon.GetComponent<Renderer>().sharedMaterial = weaponMat;

            GameObject axeBlade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            axeBlade.name = "Axe Blade";
            axeBlade.transform.SetParent(weapon.transform);
            axeBlade.transform.localPosition = new Vector3(0, 0.4f, 2f);
            axeBlade.transform.localScale = new Vector3(6f, 0.3f, 4f);
            DestroyImmediate(axeBlade.GetComponent<BoxCollider>());
            Material bladeMat = new Material(Shader.Find("Standard"));
            bladeMat.color = new Color(0.8f, 0.9f, 1f); // Metallic ice blade
            axeBlade.GetComponent<Renderer>().sharedMaterial = bladeMat;

            var combat = player.AddComponent<PlayerCombat>();
            combat.weaponTransform = weapon.transform;

            return player;
        }

        private static Camera SetupCamera(Transform target)
        {
            GameObject camRig = new GameObject("Isometric Camera Rig");
            Camera cam = camRig.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 12f; // Wider angle to appreciate the new giant environment
            
            camRig.AddComponent<CameraJuiceManager>(); // Adds dynamic hit shake + hit stop!

            var iso = camRig.AddComponent<IsometricCameraFollow>();
            SerializedObject so = new SerializedObject(iso);
            so.FindProperty("target").objectReferenceValue = target;
            so.FindProperty("followOffset").vector3Value = new Vector3(-15, 20, -15);
            so.ApplyModifiedProperties();

            return cam;
        }

        private static void SetupHadesEncounter(Transform worldRoot)
        {
            GameObject room = new GameObject("Main Arena Encounter Trigger");
            room.transform.position = new Vector3(0, 0, 0);
            room.transform.SetParent(worldRoot);
            
            BoxCollider trigger = room.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(25, 10, 25);

            var roomController = room.AddComponent<RoomController>();
            var waveSpawner = room.AddComponent<WaveSpawner>();

            // Setup Event Gates (Using the Gap we created)
            GameObject physicalGate = CreateBlock("Arena Locking Gate", new Vector3(0, -5, -16), new Vector3(8, 8, 2), new Material(Shader.Find("Standard")) { color = Color.red }, room.transform);
            physicalGate.SetActive(false); // Hidden until player walks in

            SerializedObject roomSo = new SerializedObject(roomController);
            roomSo.FindProperty("waveSpawner").objectReferenceValue = waveSpawner;
            
            // Add locking door logic
            SerializedProperty eDoors = roomSo.FindProperty("entranceDoors");
            eDoors.arraySize = 1;
            eDoors.GetArrayElementAtIndex(0).objectReferenceValue = physicalGate;
            roomSo.ApplyModifiedProperties();
        }

        private static void EnsureTagExists(string tag)
        {
            UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if ((asset != null) && (asset.Length > 0))
            {
                SerializedObject so = new SerializedObject(asset[0]);
                SerializedProperty tags = so.FindProperty("tags");
                for (int i = 0; i < tags.arraySize; ++i)
                {
                    if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
                }
                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
                so.ApplyModifiedProperties();
                so.Update();
            }
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
            l.shadows = LightShadows.Soft;
        }

        private static Texture2D GenerateNoiseTexture(Color baseColor, Color noiseColor, float scale)
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size);
            float offsetX = Random.Range(0f, 9999f);
            float offsetY = Random.Range(0f, 9999f);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float xCoord = (float)x / size * scale + offsetX;
                    float yCoord = (float)y / size * scale + offsetY;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    // Add some sharp contrast to make it feel like "grain" or "cracks" 
                    sample = Mathf.Pow(sample, 1.5f);
                    tex.SetPixel(x, y, Color.Lerp(baseColor, noiseColor, sample));
                }
            }
            tex.Apply();
            return tex;
        }

        private static void CreateAnimal(string name, Vector3 pos, Vector3 scale, Material mat, float speed, Transform parent)
        {
            GameObject animal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            animal.name = name;
            animal.transform.position = pos;
            animal.transform.localScale = scale;
            animal.transform.SetParent(parent);
            
            MeshRenderer renderer = animal.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;

            Rigidbody rb = animal.AddComponent<Rigidbody>();
            rb.mass = 2f;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            RoamingAnimal ai = animal.AddComponent<RoamingAnimal>();
            ai.moveSpeed = speed;

            // Simple physics so the player can push them but they don't trip you up.
            BoxCollider col = animal.GetComponent<BoxCollider>();
            col.material = new PhysicsMaterial { bounciness = 0, staticFriction = 0, dynamicFriction = 0 };
        }

        private static void CreateEnemy(string name, Vector3 pos, Material mat, Material hitFlashMat, Transform parent)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = name;
            // enemy.tag = "Enemy"; // Removed to prevent Tag not defined crashes
            enemy.transform.position = pos;
            enemy.transform.SetParent(parent);
            
            MeshRenderer renderer = enemy.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;

            Rigidbody rb = enemy.AddComponent<Rigidbody>();
            rb.mass = 5f;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            EnemyAI ai = enemy.AddComponent<EnemyAI>();
            ai.SetHitFlashMaterial(hitFlashMat);

            // Give them a crude weapon to look threatening
            GameObject weapon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            weapon.name = "Enemy Club";
            weapon.transform.SetParent(enemy.transform);
            weapon.transform.localPosition = new Vector3(0.6f, 0, 0.5f);
            weapon.transform.localScale = new Vector3(0.2f, 1.5f, 0.2f);
            Object.DestroyImmediate(weapon.GetComponent<BoxCollider>());
            weapon.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Standard")) { color = Color.black };
        }

        // ======================================================================================================
        // THE ANCIENT JAPAN REALM (Springtime Sakura)
        // Built on a separate coordinate block (X: 10000, Z: 10000)
        // ======================================================================================================
        private static void CreateJapanRealm(Transform worldRoot)
        {
            Vector3 offset = new Vector3(10000f, 0f, 10000f);
            
            Texture2D grassTex = GenerateNoiseTexture(new Color(0.4f, 0.7f, 0.3f), new Color(0.3f, 0.6f, 0.2f), 20f);
            Material grassMat = new Material(Shader.Find("Standard"));
            grassMat.mainTexture = grassTex;
            grassMat.color = Color.white;

            Material woodMat = new Material(Shader.Find("Standard")) { color = new Color(0.2f, 0.1f, 0.05f) };
            Material redMat = new Material(Shader.Find("Standard")) { color = new Color(0.8f, 0.15f, 0.15f) };
            Material sakuraMat = new Material(Shader.Find("Standard")) { color = new Color(1f, 0.7f, 0.85f) };
            Material pathMat = new Material(Shader.Find("Standard")) { color = new Color(0.6f, 0.6f, 0.5f) };
            
            // Build the island
            CreateBlock("Ancient Japan Island", offset + new Vector3(0, -0.5f, 0), new Vector3(200, 1, 200), grassMat, worldRoot);

            // Sakura Trees
            for (int i = 0; i < 150; i++)
            {
                Vector3 treePos = offset + new Vector3(Random.Range(-90f, 90f), 0.5f, Random.Range(-90f, 90f));
                if (Mathf.Abs(treePos.x - offset.x) < 5f) continue; // clear center path
                CreateSakuraTree(treePos, worldRoot, sakuraMat, woodMat);
            }

            // Torii Gate Entrance
            CreateToriiGate("Main Entrance Torii", offset + new Vector3(0, 0, 0), redMat, worldRoot);
            
            // Japanese Pathway
            for (int i = 0; i < 10; i++)
                CreateBlock("Japan Path", offset + new Vector3(0, 0.05f, (i * 6f) + 5f), new Vector3(6, 0.1f, 5), pathMat, worldRoot);

            // Return Portal in Japan Region
            Vector3 rpPos = offset + new Vector3(10f, 0f, 5f);
            
            // Build an inviting Torii Gate-style arch out of dark stone & pink accents
            CreateBlock("Return Portal Pillar L", rpPos + new Vector3(-2.8f, 3.5f, 0), new Vector3(0.8f, 7f, 0.8f), redMat, worldRoot);
            CreateBlock("Return Portal Pillar R", rpPos + new Vector3(2.8f, 3.5f, 0), new Vector3(0.8f, 7f, 0.8f), redMat, worldRoot);
            CreateBlock("Return Portal Arch", rpPos + new Vector3(0, 7f, 0), new Vector3(8f, 1f, 1f), redMat, worldRoot);
            CreateBlock("Return Portal Sub Arch", rpPos + new Vector3(0, 6.2f, 0), new Vector3(6.5f, 0.5f, 0.6f), redMat, worldRoot);

            // Glowing inner gateway
            Material rPortalMagicalMat = new Material(Shader.Find("Unlit/Color"));
            rPortalMagicalMat.color = new Color(0.5f, 0.8f, 1f, 0.6f); // Semi-transparent blue

            GameObject rPortalGlow = CreateBlock("Portal Glow (Exit)", rpPos + new Vector3(0, 3.2f, 0), new Vector3(4.8f, 6.2f, 0.4f), rPortalMagicalMat, worldRoot);
            rPortalGlow.transform.rotation = Quaternion.Euler(0, -90, 0); // Face the path
            
            Light rPortalLight = rPortalGlow.AddComponent<Light>();
            rPortalLight.type = LightType.Point;
            rPortalLight.color = new Color(0.4f, 0.8f, 1f);
            rPortalLight.intensity = 5f;
            rPortalLight.range = 15f;

            BoxCollider rpCol = rPortalGlow.GetComponent<BoxCollider>();
            if (rpCol == null) rpCol = rPortalGlow.AddComponent<BoxCollider>();
            rpCol.isTrigger = true;
            rpCol.size = new Vector3(1.1f, 1.1f, 6f); // Expand trigger depth so dash doesn't skip it
            var rpScript = rPortalGlow.AddComponent<JapanPortal>();
            rpScript.destinationTarget = new Vector3(0, 2f, -15f); // Target Nordic Realm
            rpScript.isReturnPortal = true;

            // Snow vortex spilling out towards the player!
            ParticleSystem rPs = rPortalGlow.AddComponent<ParticleSystem>();
            var rMain = rPs.main;
            rMain.startColor = new Color(0.8f, 0.9f, 1f);
            rMain.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            rMain.startLifetime = 4f;
            rMain.startSpeed = 2f;
            var rpe = rPs.emission; rpe.rateOverTime = 35; // Thicker snow!
            var rShape = rPs.shape; 
            rShape.shapeType = ParticleSystemShapeType.Box;
            rShape.scale = new Vector3(4f, 6f, 1f);
            var rRenderer = rPortalGlow.GetComponent<ParticleSystemRenderer>();
            rRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            var rVel = rPs.velocityOverLifetime;
            rVel.enabled = true;
            rVel.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f); // ALL axes MUST have the same curve mode!
            rVel.y = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
            rVel.z = new ParticleSystem.MinMaxCurve(-2f, -1f);

            // Pagoda Temple Focus
            CreatePagoda(offset + new Vector3(0, 0, 60f), worldRoot, redMat, woodMat);
        }

        private static void CreateSakuraTree(Vector3 pos, Transform parent, Material petals, Material wood)
        {
            GameObject tree = new GameObject("Sakura Tree");
            tree.transform.position = pos;
            tree.transform.SetParent(parent);

            CreateBlock("Trunk", pos + new Vector3(0, 2f, 0), new Vector3(0.5f, 4f, 0.5f), wood, tree.transform);

            for (int i = 0; i < 4; i++)
            {
                GameObject p = CreateBlock("Sakura Canopy", pos + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(3f, 5f), Random.Range(-1.5f, 1.5f)), new Vector3(4f, 3f, 4f), petals, tree.transform);
                p.transform.rotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            }
        }

        private static void CreateToriiGate(string name, Vector3 pos, Material redMat, Transform parent)
        {
            GameObject gate = new GameObject(name);
            gate.transform.position = pos;
            gate.transform.SetParent(parent);

            CreateBlock("Pillar L", pos + new Vector3(-3, 3, 0), new Vector3(0.8f, 6, 0.8f), redMat, gate.transform);
            CreateBlock("Pillar R", pos + new Vector3(3, 3, 0), new Vector3(0.8f, 6, 0.8f), redMat, gate.transform);
            CreateBlock("CrossBar 1", pos + new Vector3(0, 5f, 0), new Vector3(8, 0.8f, 0.8f), redMat, gate.transform);
            CreateBlock("CrossBar 2", pos + new Vector3(0, 6f, 0), new Vector3(9, 0.5f, 0.5f), redMat, gate.transform);
            CreateBlock("Center Plaque", pos + new Vector3(0, 5.5f, 0), new Vector3(0.5f, 1.5f, 0.9f), redMat, gate.transform);
        }

        private static void CreatePagoda(Vector3 pos, Transform parent, Material red, Material wood)
        {
            GameObject pagoda = new GameObject("Pagoda Temple");
            pagoda.transform.position = pos;
            pagoda.transform.SetParent(parent);

            CreateBlock("Stone Base", pos + new Vector3(0, 1, 0), new Vector3(18, 2, 18), new Material(Shader.Find("Standard")) { color = Color.gray }, pagoda.transform);

            for (int i = 0; i < 3; i++)
            {
                float height = 3f + (i * 4f);
                float width = 12f - (i * 2.5f);
                CreateBlock("Tier Wall " + i, pos + new Vector3(0, height, 0), new Vector3(width, 4f, width), red, pagoda.transform);

                GameObject roofL = CreateBlock("Roof Left " + i, pos + new Vector3(-width/2.2f, height + 2f, 0), new Vector3(width*0.8f, 0.5f, width+1.5f), wood, pagoda.transform);
                roofL.transform.rotation = Quaternion.Euler(0, 0, 15);

                GameObject roofR = CreateBlock("Roof Right " + i, pos + new Vector3(width/2.2f, height + 2f, 0), new Vector3(width*0.8f, 0.5f, width+1.5f), wood, pagoda.transform);
                roofR.transform.rotation = Quaternion.Euler(0, 0, -15);
            }
            
            CreateBlock("Temple Spire", pos + new Vector3(0, 16f, 0), new Vector3(0.5f, 8f, 0.5f), wood, pagoda.transform);
        }
    }
}