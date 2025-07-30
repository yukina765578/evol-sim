using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class Node : MonoBehaviour
    {
        private float size = 1f;
        private Color nodeColor = Color.blue;
        private Vector3 prevPosition;
        private Energy energyComponent;

        void Awake()
        {
            prevPosition = transform.localPosition;
        }

        void Start()
        {
            energyComponent = GetComponentInParent<Energy>();
        }

        public void InitializePhysicsOnly(float nodeSize, Color color)
        {
            size = nodeSize;
            nodeColor = color;
            prevPosition = transform.localPosition;
            energyComponent = GetComponentInParent<Energy>();
        }

        void LateUpdate()
        {
            prevPosition = transform.localPosition;
        }

        public Vector2 GetPositionDelta()
        {
            Vector2 delta = (Vector2)(transform.localPosition - prevPosition);
            return delta;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public float GetSize()
        {
            return size;
        }

        public Color GetColor()
        {
            if (energyComponent != null && energyComponent.IsReproductionReady)
            {
                return Color.red;
            }
            return nodeColor;
        }
    }
}
