using UnityEngine;

namespace EvolutionSimulator.Creatures.Genetics
{
    public static class GeneticsConstants
    {
        public const float MIN_OSC_SPEED = 1.5f;
        public const float MAX_OSC_SPEED = 8.0f;
        public const float MIN_MAX_ANGLE = -180.0f;
        public const float MAX_MAX_ANGLE = 180.0f;
        public const float MIN_FORWARD_RATIO = 0.1f;
        public const float MAX_FORWARD_RATIO = 0.5f;
        public const int MIN_NODES = 3;
        public const int MAX_NODES = 20;
        public const float MIN_BASE_ANGLE = 0.0f;
        public const float MAX_BASE_ANGLE = 360.0f;

        // Neural Network Constants
        public const int MAIN_BRAIN_INPUTS = 10;
        public const int MAIN_BRAIN_OUTPUTS = 2;
        public const int MAIN_BRAIN_WEIGHTS = MAIN_BRAIN_OUTPUTS * (MAIN_BRAIN_INPUTS + 1); // 10 inputs + 1 bias

        public const int SEGMENT_BRAIN_INPUTS = 6;
        public const int SEGMENT_BRAIN_OUTPUTS = 2;
        public const int SEGMENT_BRAIN_WEIGHTS = SEGMENT_BRAIN_OUTPUTS * (SEGMENT_BRAIN_INPUTS + 1); // 6 inputs + 1 bias

        // Neural Network weight ranges
        public const float MIN_NEURAL_WEIGHT = -1.0f;
        public const float MAX_NEURAL_WEIGHT = 1.0f;
        public const float NEURAL_NETWORK_MUTATION_RATE = 0.1f; // 10% chance to mutate each weight
        public const float NEURAL_MUTATION_STRENGTH = 0.1f; // How much weights can change during mutation
    }
}
