using UnityEngine;

namespace EvolutionSimulator.Creatures.Genetics
{
    public static class GeneticsConstants
    {
        public const float MIN_OSC_SPEED = 1f;
        public const float MAX_OSC_SPEED = 8.0f;
        public const float MIN_MAX_ANGLE = -180.0f;
        public const float MAX_MAX_ANGLE = 180.0f;
        public const float MIN_FORWARD_RATIO = 0.05f;
        public const float MAX_FORWARD_RATIO = 0.5f;
        public const int MIN_NODES = 3;
        public const int MAX_NODES = 20;
        public const float MIN_BASE_ANGLE = 0.0f;
        public const float MAX_BASE_ANGLE = 360.0f;
    }
}
