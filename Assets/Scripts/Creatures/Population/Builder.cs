using EvolutionSimulator.Creatures.Biology;
using EvolutionSimulator.Creatures.Core;
using EvolutionSimulator.Creatures.Genetics;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Population
{
    public static class Builder
    {
        public static GameObject BuildCreature(CreatureGenome genome, Vector3 position)
        {
            // Create single GameObject for entire creature
            GameObject creatureObj = new GameObject("Creature");
            creatureObj.transform.position = position;

            // Set tag and layer safely
            SetupGameObjectProperties(creatureObj);

            // Add core physics component
            SetupPhysics(creatureObj);

            // Add biological systems
            SetupBiology(creatureObj);

            // Add main controller (this will create and coordinate all other systems)
            var controller = creatureObj.AddComponent<Controller>();
            controller.Initialize(genome);

            return creatureObj;
        }

        static void SetupGameObjectProperties(GameObject creatureObj)
        {
            // Set tag safely
            try
            {
                creatureObj.tag = "Creature";
            }
            catch (UnityException)
            {
                Debug.LogWarning("Create 'Creature' tag in Tag Manager for better organization");
            }

            // Set layer safely
            int creatureLayer = LayerMask.NameToLayer("Creatures");
            if (creatureLayer == -1)
            {
                Debug.LogWarning(
                    "Create 'Creatures' layer in Layer Manager for better performance"
                );
            }
            else
            {
                creatureObj.layer = creatureLayer;
            }
        }

        static void SetupPhysics(GameObject creatureObj)
        {
            // Add main physics body for swimming
            var rigidbody = creatureObj.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f; // Swimming - no gravity
            rigidbody.linearDamping = 0f;
            rigidbody.angularDamping = 0f;
            rigidbody.mass = 1f; // Default mass, adjusted by systems
        }

        static void SetupBiology(GameObject creatureObj)
        {
            // Add energy system
            var energy = creatureObj.AddComponent<Energy>();

            // Add reproduction controller
            var reproductionController = creatureObj.AddComponent<ReproductionController>();
        }

        // Factory method for creating creatures with specific properties
        public static GameObject BuildCreatureWithProperties(
            CreatureGenome genome,
            Vector3 position,
            float scale = 1f,
            bool enableReproduction = true
        )
        {
            GameObject creature = BuildCreature(genome, position);

            // Apply scaling
            if (scale != 1f)
            {
                creature.transform.localScale = Vector3.one * scale;
            }

            // Configure reproduction
            if (!enableReproduction)
            {
                var reproductionController = creature.GetComponent<ReproductionController>();
                if (reproductionController != null)
                {
                    reproductionController.enabled = false;
                }
            }

            return creature;
        }

        // Factory method for creating test creatures
        public static GameObject BuildTestCreature(Vector3 position)
        {
            CreatureGenome testGenome = Randomizer.GenerateRandomGenome();
            return BuildCreature(testGenome, position);
        }

        // Factory method for creating creatures from parents (for reproduction)
        public static GameObject BuildOffspring(
            CreatureGenome parentGenome1,
            CreatureGenome parentGenome2,
            Vector3 position
        )
        {
            CreatureGenome offspringGenome = GeneticCrossover.CrossoverGenomes(
                parentGenome1,
                parentGenome2
            );
            return BuildCreature(offspringGenome, position);
        }

        // Utility method to validate creature creation
        public static bool ValidateCreature(GameObject creature)
        {
            if (creature == null)
                return false;

            var controller = creature.GetComponent<Controller>();
            if (controller == null)
            {
                Debug.LogError("Creature missing Controller component!");
                return false;
            }

            var rigidbody = creature.GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                Debug.LogError("Creature missing Rigidbody2D component!");
                return false;
            }

            var energy = creature.GetComponent<Energy>();
            if (energy == null)
            {
                Debug.LogError("Creature missing Energy component!");
                return false;
            }

            return true;
        }

        // Utility method to get creature stats
        public static CreatureStats GetCreatureStats(GameObject creature)
        {
            var controller = creature.GetComponent<Controller>();
            if (controller == null)
                return default(CreatureStats);

            var energy = creature.GetComponent<Energy>();

            return new CreatureStats
            {
                nodeCount = controller.GetNodeCount(),
                segmentCount = controller.GetSegmentCount(),
                currentEnergy = energy?.CurrentEnergy ?? 0f,
                maxEnergy = energy?.MaxEnergy ?? 0f,
                velocity = controller.GetCurrentVelocity(),
                efficiency = controller.GetMovementEfficiency(),
                isGrounded = false, // Swimming creatures are never grounded
                isReproductionReady = energy?.IsReproductionReady ?? false,
            };
        }

        // Method to clone existing creature
        public static GameObject CloneCreature(GameObject originalCreature, Vector3 position)
        {
            var controller = originalCreature.GetComponent<Controller>();
            if (controller == null)
            {
                Debug.LogError("Cannot clone creature without Controller component!");
                return null;
            }

            CreatureGenome genome = controller.GetGenome();
            return BuildCreature(genome, position);
        }

        // Method to create creature from serialized data
        public static GameObject BuildCreatureFromData(CreatureData data, Vector3 position)
        {
            // Convert serialized data back to genome
            CreatureGenome genome = DataToGenome(data);
            return BuildCreature(genome, position);
        }

        // Helper method to convert data to genome
        static CreatureGenome DataToGenome(CreatureData data)
        {
            // Implementation depends on your serialization format
            // This is a placeholder - implement based on your data structure
            NodeGene[] nodes = new NodeGene[data.nodeCount];

            // Convert data back to NodeGene array
            // ... implementation depends on your data format

            return new CreatureGenome(nodes);
        }
    }

    // Helper struct for creature statistics
    [System.Serializable]
    public struct CreatureStats
    {
        public int nodeCount;
        public int segmentCount;
        public float currentEnergy;
        public float maxEnergy;
        public Vector2 velocity;
        public float efficiency;
        public bool isGrounded; // Always false for swimming creatures
        public bool isReproductionReady;
    }

    // Helper struct for creature serialization
    [System.Serializable]
    public struct CreatureData
    {
        public int nodeCount;
        public int segmentCount;
        public float[] nodePositions;
        public float[] segmentAngles;
        // Add other serializable data as needed
    }
}
