using UnityEngine;

namespace NordicWilds.Visuals
{
    public enum CharacterVisualPreset
    {
        NordicEnemy,
        JapanEnemy,
        PlayerHero
    }

    public static class ProceduralCharacterVisual
    {
        private static Material nordicMaterial;
        private static Material japanMaterial;
        private static Material playerMaterial;

        public static void Build(Transform root, CharacterVisualPreset preset)
        {
            if (root == null || root.Find("RuntimeVisualModel") != null)
                return;

            Color bodyColor;
            Color accentColor;

            switch (preset)
            {
                case CharacterVisualPreset.JapanEnemy:
                    bodyColor = new Color(0.50f, 0.26f, 0.18f, 1f);
                    accentColor = new Color(0.85f, 0.65f, 0.35f, 1f);
                    break;
                case CharacterVisualPreset.PlayerHero:
                    bodyColor = new Color(0.28f, 0.36f, 0.45f, 1f);
                    accentColor = new Color(0.85f, 0.77f, 0.55f, 1f);
                    break;
                default:
                    bodyColor = new Color(0.18f, 0.22f, 0.28f, 1f);
                    accentColor = new Color(0.42f, 0.52f, 0.64f, 1f);
                    break;
            }

            Material bodyMaterial = GetMaterial(preset, bodyColor);
            Material accentMaterial = GetMaterial(preset, accentColor);

            MeshRenderer rootRenderer = root.GetComponent<MeshRenderer>();
            if (rootRenderer != null)
                rootRenderer.enabled = false;

            GameObject modelRoot = new GameObject("RuntimeVisualModel");
            modelRoot.transform.SetParent(root, false);
            modelRoot.transform.localPosition = Vector3.zero;
            modelRoot.transform.localRotation = Quaternion.identity;
            modelRoot.transform.localScale = Vector3.one;

            CreatePart(modelRoot.transform, PrimitiveType.Capsule, "Body", new Vector3(0f, 1.05f, 0f), new Vector3(0.85f, 1.25f, 0.75f), bodyMaterial);
            CreatePart(modelRoot.transform, PrimitiveType.Sphere, "Head", new Vector3(0f, 2.15f, 0f), new Vector3(0.72f, 0.72f, 0.72f), accentMaterial);
            CreatePart(modelRoot.transform, PrimitiveType.Cube, "ShoulderL", new Vector3(-0.42f, 1.55f, 0f), new Vector3(0.22f, 0.55f, 0.22f), bodyMaterial);
            CreatePart(modelRoot.transform, PrimitiveType.Cube, "ShoulderR", new Vector3(0.42f, 1.55f, 0f), new Vector3(0.22f, 0.55f, 0.22f), bodyMaterial);
            CreatePart(modelRoot.transform, PrimitiveType.Cube, "Weapon", new Vector3(0.72f, 1.05f, 0.12f), new Vector3(0.12f, 0.12f, 1.0f), accentMaterial);
        }

        private static void CreatePart(Transform parent, PrimitiveType primitiveType, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
                Object.Destroy(collider);

            MeshRenderer renderer = part.GetComponent<MeshRenderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
        }

        private static Material GetMaterial(CharacterVisualPreset preset, Color color)
        {
            Material cached = preset switch
            {
                CharacterVisualPreset.PlayerHero => playerMaterial,
                CharacterVisualPreset.JapanEnemy => japanMaterial,
                _ => nordicMaterial,
            };

            if (cached != null)
                return cached;

            Shader shader = Shader.Find("Standard");
            Material material = new Material(shader != null ? shader : Shader.Find("Diffuse"));
            material.color = color;
            material.SetFloat("_Glossiness", 0.15f);

            switch (preset)
            {
                case CharacterVisualPreset.PlayerHero:
                    playerMaterial = material;
                    break;
                case CharacterVisualPreset.JapanEnemy:
                    japanMaterial = material;
                    break;
                default:
                    nordicMaterial = material;
                    break;
            }

            return material;
        }
    }
}