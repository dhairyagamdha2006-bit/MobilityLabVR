using System.Collections.Generic;
using UnityEngine;

namespace MobilityLabVR
{
    public static class RuntimePrimitiveFactory
    {
        private static readonly Dictionary<Color32, Material> Materials = new Dictionary<Color32, Material>();

        public static Material Material(Color color, bool emissive = false)
        {
            Color32 key = color;
            if (!emissive && Materials.TryGetValue(key, out Material cached)) return cached;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");
            Material material = new Material(shader)
            {
                name = emissive ? $"Runtime Emissive {key}" : $"Runtime Material {key}",
                color = color
            };
            if (emissive && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.4f);
            }

            if (!emissive) Materials[key] = material;
            return material;
        }

        public static GameObject Primitive(
            PrimitiveType type,
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Color color,
            bool keepCollider = false,
            Quaternion? localRotation = null,
            bool emissive = false)
        {
            GameObject gameObject = GameObject.CreatePrimitive(type);
            gameObject.name = name;
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localRotation = localRotation ?? Quaternion.identity;
            gameObject.transform.localScale = localScale;
            Renderer renderer = gameObject.GetComponent<Renderer>();
            renderer.sharedMaterial = Material(color, emissive);
            if (!keepCollider)
            {
                Collider collider = gameObject.GetComponent<Collider>();
                if (collider != null) Object.Destroy(collider);
            }
            return gameObject;
        }

        public static TextMesh WorldText(
            string name,
            Transform parent,
            string content,
            Vector3 localPosition,
            Quaternion localRotation,
            float characterSize,
            Color color,
            TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            GameObject gameObject = new GameObject(name, typeof(TextMesh));
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;
            gameObject.transform.localRotation = localRotation;
            TextMesh text = gameObject.GetComponent<TextMesh>();
            text.text = content;
            text.font = RuntimeUIFactory.Font;
            text.fontSize = 48;
            text.characterSize = characterSize;
            text.color = color;
            text.anchor = anchor;
            text.alignment = TextAlignment.Center;
            text.GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
            return text;
        }
    }
}
