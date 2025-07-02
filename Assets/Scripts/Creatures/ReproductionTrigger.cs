using UnityEngine;

namespace EvolutionSimulator.Creature
{
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
            reproductionController = controller;
            triggerCollider = GetComponent<CircleCollider2D>();
            CreateDebugCircle();
        }

        void CreateDebugCircle()
        {
            if (!showCollisionRadius)
                return;

            GameObject debugObj = new GameObject("CollisionDebug");
            debugObj.transform.SetParent(transform);
            debugObj.transform.localPosition = Vector3.zero;

            circleRenderer = debugObj.AddComponent<LineRenderer>();
            circleRenderer.material = new Material(Shader.Find("Sprites/Default"));
            circleRenderer.startColor = Color.yellow;
            circleRenderer.endColor = Color.yellow;
            circleRenderer.startWidth = 0.05f;
            circleRenderer.endWidth = 0.05f;
            circleRenderer.useWorldSpace = false;
            circleRenderer.sortingOrder = 5;

            DrawCircle();
        }

        void DrawCircle()
        {
            if (circleRenderer == null || triggerCollider == null)
                return;

            int segments = 32;
            circleRenderer.positionCount = segments + 1;
            float radius = triggerCollider.radius;

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * 2f * Mathf.PI / segments;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                circleRenderer.SetPosition(i, pos);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (reproductionController == null)
                return;

            Node otherNode = other.GetComponent<Node>();
            if (otherNode == null)
                return;

            GameObject otherCreature = otherNode.transform.root.gameObject;
            if (otherCreature == reproductionController.gameObject)
                return;

            reproductionController.OnNodeCollision(otherCreature);
        }

        void OnDestroy()
        {
            if (circleRenderer != null)
                DestroyImmediate(circleRenderer.gameObject);
        }
    }
}
