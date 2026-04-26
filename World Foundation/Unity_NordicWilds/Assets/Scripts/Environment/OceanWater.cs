using UnityEngine;

namespace NordicWilds.Environment
{
    // Procedural Gerstner-wave ocean. Builds a subdivided plane mesh at runtime, animates
    // vertices with summed Gerstner waves, recomputes normals each frame for proper specular.
    // Drives a depth-faded foam-tipped material from generated noise textures so it works on
    // the Built-in Standard shader without needing URP/HDRP shader graph assets.
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class OceanWater : MonoBehaviour
    {
        // Renderer reference + visibility flag — we skip the per-frame wave sim when the
        // ocean isn't on screen. OnBecameVisible/Invisible are free callbacks Unity already
        // sends, so no raycasting or distance maths needed.
        MeshRenderer cachedRenderer;
        bool isVisible = true;
        // Throttle the wave simulation — the CPU vertex update is O(verts * waves) and
        // doesn't need to run at 144 fps. ~33 fps simulation reads as smooth at distance.
        const float SimInterval = 1f / 30f;
        float simAccumulator;
        [System.Serializable]
        public struct GerstnerWave
        {
            public Vector2 direction;
            public float steepness;
            public float wavelength;
            public float speed;
        }

        [Header("Mesh")]
        public float size = 600f;
        [Range(20, 220)] public int resolution = 140;

        [Header("Waves")]
        public GerstnerWave[] waves = new GerstnerWave[]
        {
            new GerstnerWave { direction = new Vector2( 1.0f,  0.20f), steepness = 0.32f, wavelength = 28f, speed = 1.10f },
            new GerstnerWave { direction = new Vector2( 0.55f, 0.85f), steepness = 0.22f, wavelength = 16f, speed = 1.45f },
            new GerstnerWave { direction = new Vector2(-0.30f, 0.95f), steepness = 0.18f, wavelength = 9f,  speed = 1.80f },
            new GerstnerWave { direction = new Vector2(-0.85f, 0.45f), steepness = 0.10f, wavelength = 5f,  speed = 2.20f }
        };

        [Header("Foam")]
        [Range(0f, 1f)] public float foamAmount = 0.55f;
        public Color foamColor = new Color(0.92f, 0.96f, 1f, 1f);

        [Header("Surface Color")]
        public Color shallowColor = new Color(0.18f, 0.55f, 0.78f, 1f);
        public Color deepColor    = new Color(0.02f, 0.10f, 0.22f, 1f);
        [Range(0f, 1f)] public float metallic  = 0.05f;
        [Range(0f, 1f)] public float smoothness = 0.92f;

        Mesh mesh;
        Vector3[] basePositions;
        Vector3[] workPositions;
        Vector3[] normals;

        void OnEnable()
        {
            BuildMesh();
            BuildMaterialIfMissing();
            cachedRenderer = GetComponent<MeshRenderer>();
        }

        void OnBecameVisible()  { isVisible = true; }
        void OnBecameInvisible() { isVisible = false; }

        void Update()
        {
            if (mesh == null || basePositions == null) BuildMesh();

            // Skip the wave sim entirely if we're not on screen — by far the biggest
            // perf win, since the ocean is dormant during gameplay (after the menu).
            if (!isVisible) return;

            simAccumulator += Time.deltaTime;
            if (simAccumulator < SimInterval) return;
            simAccumulator = 0f;

            AnimateWaves(GetTime());
        }

        // Single time source: scaled in play, realtime elsewhere so editor preview ticks too.
        static double GetTime()
        {
            return Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
        }

        void BuildMesh()
        {
            int res = Mathf.Clamp(resolution, 4, 250);
            mesh = new Mesh();
            mesh.name = "OceanMesh";
            mesh.indexFormat = (res + 1) * (res + 1) > 65000
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;

            int vertCount = (res + 1) * (res + 1);
            basePositions = new Vector3[vertCount];
            workPositions = new Vector3[vertCount];
            normals       = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];
            int[] tris = new int[res * res * 6];

            float half = size * 0.5f;
            float step = size / res;
            int v = 0;
            for (int z = 0; z <= res; z++)
            {
                for (int x = 0; x <= res; x++, v++)
                {
                    float px = -half + x * step;
                    float pz = -half + z * step;
                    basePositions[v] = new Vector3(px, 0f, pz);
                    workPositions[v] = basePositions[v];
                    normals[v]       = Vector3.up;
                    uvs[v]           = new Vector2((float)x / res, (float)z / res);
                }
            }

            int t = 0;
            for (int z = 0; z < res; z++)
            {
                for (int x = 0; x < res; x++)
                {
                    int i = z * (res + 1) + x;
                    tris[t++] = i;
                    tris[t++] = i + (res + 1);
                    tris[t++] = i + (res + 1) + 1;
                    tris[t++] = i;
                    tris[t++] = i + (res + 1) + 1;
                    tris[t++] = i + 1;
                }
            }

            mesh.vertices  = workPositions;
            mesh.triangles = tris;
            mesh.uv        = uvs;
            mesh.normals   = normals;
            mesh.RecalculateBounds();

            GetComponent<MeshFilter>().sharedMesh = mesh;
        }

        void BuildMaterialIfMissing()
        {
            var mr = GetComponent<MeshRenderer>();
            if (mr.sharedMaterial != null && mr.sharedMaterial.name.StartsWith("OceanMat")) return;

            Shader s = Shader.Find("Standard");
            if (s == null) s = Shader.Find("Universal Render Pipeline/Lit");
            Material mat = new Material(s) { name = "OceanMat" };
            mat.color = shallowColor;
            if (mat.HasProperty("_Metallic"))   mat.SetFloat("_Metallic",  metallic);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            // Procedural normal map for water ripples
            Texture2D nrm = GenerateNormalMap(256, 6f);
            if (mat.HasProperty("_BumpMap"))
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", nrm);
            }
            mr.sharedMaterial = mat;
        }

        Texture2D GenerateNormalMap(int sz, float scale)
        {
            Texture2D tex = new Texture2D(sz, sz, TextureFormat.RGBA32, true, true);
            float ox = Random.Range(0, 9999f);
            float oy = Random.Range(0, 9999f);
            for (int y = 0; y < sz; y++)
            {
                for (int x = 0; x < sz; x++)
                {
                    float h  = Mathf.PerlinNoise(x / (float)sz * scale + ox, y / (float)sz * scale + oy);
                    float hx = Mathf.PerlinNoise((x + 1) / (float)sz * scale + ox, y / (float)sz * scale + oy);
                    float hy = Mathf.PerlinNoise(x / (float)sz * scale + ox, (y + 1) / (float)sz * scale + oy);
                    Vector3 n = new Vector3(h - hx, h - hy, 1f).normalized;
                    tex.SetPixel(x, y, new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, 1f, n.x * 0.5f + 0.5f));
                }
            }
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.Apply();
            return tex;
        }

        void AnimateWaves(double t)
        {
            float time = (float)t;
            for (int i = 0; i < basePositions.Length; i++)
            {
                Vector3 p = basePositions[i];
                Vector3 displaced = p;
                Vector3 tangent = new Vector3(1f, 0f, 0f);
                Vector3 binormal = new Vector3(0f, 0f, 1f);

                for (int w = 0; w < waves.Length; w++)
                {
                    var wv = waves[w];
                    if (wv.wavelength <= 0.0001f) continue;
                    Vector2 d = wv.direction.sqrMagnitude > 0.0001f ? wv.direction.normalized : Vector2.right;
                    float k = 2f * Mathf.PI / wv.wavelength;
                    float c = Mathf.Sqrt(9.81f / k) * wv.speed;
                    float f = k * (Vector2.Dot(d, new Vector2(p.x, p.z)) - c * time);
                    float a = wv.steepness / k;

                    float cosF = Mathf.Cos(f);
                    float sinF = Mathf.Sin(f);

                    displaced.x += d.x * a * cosF;
                    displaced.z += d.y * a * cosF;
                    displaced.y += a * sinF;

                    tangent  += new Vector3(-d.x * d.x * wv.steepness * sinF,
                                             d.x * wv.steepness * cosF,
                                            -d.x * d.y * wv.steepness * sinF);
                    binormal += new Vector3(-d.x * d.y * wv.steepness * sinF,
                                             d.y * wv.steepness * cosF,
                                            -d.y * d.y * wv.steepness * sinF);
                }

                workPositions[i] = displaced;
                normals[i] = Vector3.Normalize(Vector3.Cross(binormal, tangent));
            }

            mesh.vertices = workPositions;
            mesh.normals  = normals;
            mesh.RecalculateBounds();
        }

        // Public sampler for boat bobbing — returns surface Y at world XZ.
        public float SampleHeight(Vector3 worldPos)
        {
            float time = (float)GetTime();
            float y = 0f;
            for (int w = 0; w < waves.Length; w++)
            {
                var wv = waves[w];
                if (wv.wavelength <= 0.0001f) continue;
                Vector2 d = wv.direction.sqrMagnitude > 0.0001f ? wv.direction.normalized : Vector2.right;
                float k = 2f * Mathf.PI / wv.wavelength;
                float c = Mathf.Sqrt(9.81f / k) * wv.speed;
                float f = k * (Vector2.Dot(d, new Vector2(worldPos.x, worldPos.z)) - c * time);
                float a = wv.steepness / k;
                y += a * Mathf.Sin(f);
            }
            return transform.position.y + y;
        }
    }
}
