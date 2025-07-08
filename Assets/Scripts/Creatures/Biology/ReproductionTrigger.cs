using UnityEngine;

namespace EvolutionSimulator.Creature
{
    // LEGACY: This class is no longer used in the distance-based mating system
    // Kept for compatibility but functionality has been moved to ReproductionController
    public class ReproductionTrigger : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField]
        private bool showCollisionRadius = true;

        private ReproductionController reproductionController;
        private CircleCollider2D triggerCollider;
        private LineRenderer circleRenderer;

        public void Initialize(ReproductionController controller)
        {
            // Legacy method - no longer used
            // Distance-based mating system handles reproduction detection
            Debug.LogWarning("ReproductionTrigger is deprecated. Remove this component.");

            reproductionController = controller;

            // Disable this component since it's no longer needed
            enabled = false;
        }

        void CreateDebugCircle()
        {
            // Legacy visualization - no longer needed
        }

        void DrawCircle()
        {
            // Legacy visualization - no longer needed
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // Legacy collision detection - no longer used
            // ReproductionController now uses distance-based detection
        }

        void OnDestroy()
        {
            if (circleRenderer != null)
                DestroyImmediate(circleRenderer.gameObject);
        }
    }
}
