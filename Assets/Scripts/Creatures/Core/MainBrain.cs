using EvolutionSimulator.Creatures.Core;
using EvolutionSimulator.Creatures.Genetics;
using EvolutionSimulator.Creatures.Sensors;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class MainBrain : MonoBehaviour
    {
        private float[] weights; // 22 weights from genome
        private Energy energyComponent;
        private Rigidbody2D creatureRigidbody;

        // Current outputs
        public float MovementDirection { get; private set; } // 0 to 2π
        public float MovementIntensity { get; private set; } // 0 to 1

        public void Initialize(CreatureGenome genome)
        {
            weights = new float[GeneticsConstants.MAIN_BRAIN_WEIGHTS];
            System.Array.Copy(genome.mainBrainWeights, weights, weights.Length);

            energyComponent = GetComponent<Energy>();
            creatureRigidbody = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            if (weights == null)
                return;

            float[] inputs = CollectInputs();
            ExecuteNeuralNetwork(inputs);
        }

        // Add to MainBrain class
        private FoodSensor foodSensor;

        void Start()
        {
            foodSensor = GetComponentInChildren<FoodSensor>();
            if (foodSensor == null)
                Debug.LogError("MainBrain requires FoodSensor component!");
        }

        // Update CollectInputs() method:
        float[] CollectInputs()
        {
            return new float[GeneticsConstants.MAIN_BRAIN_INPUTS]
            {
                foodSensor?.FoodPresence ?? 0f, // Real food detection
                foodSensor?.FoodDirection ?? 0f, // Real food direction
                foodSensor?.FoodDistance ?? 1f, // Real food distance
                0f, // matePresence (TODO: CreatureSensor)
                0f, // mateDirection
                1f, // mateDistance
                energyComponent?.CurrentEnergy / energyComponent?.MaxEnergy ?? 0.5f,
                energyComponent?.IsReproductionReady == true ? 1f : 0f,
                Mathf.Clamp01(creatureRigidbody?.linearVelocity.magnitude ?? 0f),
                Vector2.SignedAngle(
                    Vector2.right,
                    creatureRigidbody?.linearVelocity ?? Vector2.right
                ) * Mathf.Deg2Rad,
            };
        }

        void ExecuteNeuralNetwork(float[] inputs)
        {
            // Perceptron 1: Movement Direction
            float directionSum = weights[10]; // bias
            for (int i = 0; i < 10; i++)
                directionSum += inputs[i] * weights[i];
            MovementDirection = Sigmoid(directionSum) * 2f * Mathf.PI;

            // Perceptron 2: Movement Intensity
            float intensitySum = weights[21]; // bias
            for (int i = 0; i < 10; i++)
                intensitySum += inputs[i] * weights[i + 11];
            MovementIntensity = Sigmoid(intensitySum);
        }

        float Sigmoid(float x) => 1f / (1f + Mathf.Exp(-x));
    }
}
