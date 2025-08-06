using System.Collections.Generic;
using EvolutionSimulator.Creatures.Biology;
using EvolutionSimulator.Creatures.Core;
using EvolutionSimulator.Creatures.Detectors;
using EvolutionSimulator.Creatures.Genetics;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Population
{
    public static class Builder
    {
        private const float SEGMENT_LENGTH = 2f;
        private const float NODE_SIZE = 1f;
        private const float SEGMENT_WIDTH = 0.1f;
        private static readonly Color NODE_COLOR = Color.blue;
        private static readonly Color SEGMENT_COLOR = Color.white;

        public static GameObject BuildCreature(CreatureGenome genome, Vector3 position)
        {
            GameObject creatureObj = new GameObject("Creature");
            creatureObj.transform.position = position;

            // Set tag and layer safely
            try
            {
                creatureObj.tag = "Creature";
            }
            catch (UnityException)
            {
                Debug.LogWarning("Create 'Creature' tag in Tag Manager");
            }

            int creatureLayer = LayerMask.NameToLayer("Creatures");
            if (creatureLayer == -1)
            {
                Debug.LogWarning("Create 'Creature' layer in Layer Manager");
            }
            else
            {
                creatureObj.layer = creatureLayer;
            }

            // Add physics component
            var rigidbody = creatureObj.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;
            rigidbody.linearVelocity = Vector2.zero;
            rigidbody.angularVelocity = 0f;

            // Add energy system
            var energy = creatureObj.AddComponent<Energy>();
            var reproductionController = creatureObj.AddComponent<ReproductionController>();

            // Calculate initial node positions (data-only)
            Vector3[] initialNodePositions = CalculateInitialNodePositions(genome);

            // Create SegmentRenderer with position data
            var segmentRenderer = creatureObj.AddComponent<SegmentRenderer>();
            segmentRenderer.Initialize(genome, initialNodePositions);

            // Create NodeRenderer for visual nodes
            var nodeRenderer = creatureObj.AddComponent<NodeRenderer>();
            nodeRenderer.Initialize(genome.NodeCount);

            // Setup detectors on main creature
            SetupDetectors(creatureObj);

            // Add controller last (after body is built)
            var controller = creatureObj.AddComponent<Controller>();
            controller.Initialize(genome, segmentRenderer);

            return creatureObj;
        }

        static Vector3[] CalculateInitialNodePositions(CreatureGenome genome)
        {
            Vector3[] positions = new Vector3[genome.NodeCount];

            // Root node at origin
            positions[0] = Vector3.zero;

            // Calculate child node positions based on genome
            for (int i = 1; i < genome.NodeCount; i++)
            {
                NodeGene nodeGenome = genome.nodes[i];
                Vector3 parentPosition = positions[nodeGenome.parentIndex];

                // Calculate child position based on base angle
                float angle = nodeGenome.baseAngle * Mathf.Deg2Rad;
                Vector3 nodePosition =
                    parentPosition
                    + new Vector3(
                        SEGMENT_LENGTH * Mathf.Cos(angle),
                        SEGMENT_LENGTH * Mathf.Sin(angle),
                        0
                    );

                positions[i] = nodePosition;
            }

            return positions;
        }

        static void SetupDetectors(GameObject creatureObj)
        {
            // Create Sensors container
            GameObject sensorsContainer = new GameObject("Sensors");
            sensorsContainer.transform.SetParent(creatureObj.transform);
            sensorsContainer.transform.localPosition = Vector3.zero;
            sensorsContainer.layer = creatureObj.layer;

            // Create food detector
            GameObject foodDetectorObj = new GameObject("FoodDetector");
            foodDetectorObj.transform.SetParent(sensorsContainer.transform);
            foodDetectorObj.transform.localPosition = Vector3.zero;
            foodDetectorObj.layer = creatureObj.layer;
            foodDetectorObj.AddComponent<FoodDetector>();

            // Create creature detector
            GameObject creatureDetectorObj = new GameObject("CreatureDetector");
            creatureDetectorObj.transform.SetParent(sensorsContainer.transform);
            creatureDetectorObj.transform.localPosition = Vector3.zero;
            creatureDetectorObj.layer = creatureObj.layer;
            creatureDetectorObj.AddComponent<CreatureDetector>();
        }
    }
}
