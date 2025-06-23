using UnityEngine;

namespace EvolutionSimulator.Creatures
{
    public class SimpleCreature : MonoBehaviour
    {
        [Header("Creature Settings")]
        [SerializeField]
        private float rotationSpeed = 90f; // degrees per second

        [SerializeField]
        private float segmentLength = 2f;

        [Header("Visual Settings")]
        [SerializeField]
        private float nodeSize = 1f;

        [SerializeField]
        private Color nodeColor = Color.blue;

        [SerializeField]
        private Color segmentColor = Color.white;

        [SerializeField]
        private float segmentWidth = 0.1f;

        private Node node;
        private Segment segment;
        private float currentAngle = 0f;

        void Start()
        {
            node = GetComponent<Node>();
            if (node == null)
            {
                node = gameObject.AddComponent<Node>();
            }

            node.Initialize(nodeSize, nodeColor);

            segment = GetComponent<Segment>();
            if (segment == null)
            {
                segment = gameObject.AddComponent<Segment>();
            }

            segment.Initialize(segmentLength, segmentWidth, segmentColor);
        }

        void Update()
        {
            currentAngle += rotationSpeed * Time.deltaTime;
            if (currentAngle >= 360f)
            {
                currentAngle -= 360f;
            }

            segment.UpdateRotation(transform.position, currentAngle);
        }

        void OnValidate()
        {
            rotationSpeed = Mathf.Clamp(rotationSpeed, 0f, 360f);
            segmentLength = Mathf.Clamp(segmentLength, 0.5f, 10f);
            nodeSize = Mathf.Clamp(nodeSize, 0.1f, 5f);
            segmentWidth = Mathf.Clamp(segmentWidth, 0.01f, 1f);
        }
    }
}
