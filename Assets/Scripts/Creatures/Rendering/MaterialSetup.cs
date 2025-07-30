using UnityEngine;

namespace EvolutionSimulator.Creatures.Rendering
{
    public class MaterialSetup : MonoBehaviour
    {
        [Header("Shaders")]
        [SerializeField]
        private Shader nodeShader;

        [SerializeField]
        private Shader segmentShader;

        [Header("Generated Materials")]
        [SerializeField]
        private Material nodeMaterial;

        [SerializeField]
        private Material segmentMaterial;

        [Header("Settings")]
        [SerializeField]
        private float nodeEdgeSoftness = 0.02f;

        [SerializeField]
        private float segmentEdgeSoftness = 0.02f;

        void Start()
        {
            CreateMaterials();
            AssignToRenderer();
        }

        [ContextMenu("Create Materials")]
        public void CreateMaterials()
        {
            // Find shaders if not assigned
            if (nodeShader == null)
                nodeShader = Shader.Find("EvolutionSimulator/NodeInstanced");
            if (segmentShader == null)
                segmentShader = Shader.Find("EvolutionSimulator/SegmentInstanced");

            if (nodeShader == null || segmentShader == null)
            {
                Debug.LogError("Required shaders not found! Make sure shaders are in project.");
                return;
            }

            // Create node material
            if (nodeMaterial == null)
            {
                nodeMaterial = new Material(nodeShader);
                nodeMaterial.name = "NodeInstanced_Material";
            }
            nodeMaterial.SetFloat("_EdgeSoftness", nodeEdgeSoftness);

            // Create segment material
            if (segmentMaterial == null)
            {
                segmentMaterial = new Material(segmentShader);
                segmentMaterial.name = "SegmentInstanced_Material";
            }
            segmentMaterial.SetFloat("_EdgeSoftness", segmentEdgeSoftness);

            Debug.Log("Materials created successfully!");
        }

        void AssignToRenderer()
        {
            var renderer = GetComponent<CreatureRenderer>();
            if (renderer == null)
            {
                renderer = FindFirstObjectByType<CreatureRenderer>();
            }

            if (renderer != null)
            {
                // Use reflection to set private fields
                var rendererType = typeof(CreatureRenderer);
                var nodeField = rendererType.GetField(
                    "nodeMaterial",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );
                var segmentField = rendererType.GetField(
                    "segmentMaterial",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );

                nodeField?.SetValue(renderer, nodeMaterial);
                segmentField?.SetValue(renderer, segmentMaterial);

                Debug.Log("Materials assigned to CreatureRenderer!");
            }
        }

        void OnValidate()
        {
            if (Application.isPlaying && nodeMaterial != null && segmentMaterial != null)
            {
                nodeMaterial.SetFloat("_EdgeSoftness", nodeEdgeSoftness);
                segmentMaterial.SetFloat("_EdgeSoftness", segmentEdgeSoftness);
            }
        }
    }
}
