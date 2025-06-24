using UnityEngine;
using UnityEngine.Events;

namespace EvolutionSimulator.Environment
{
    public class NoiseManager : MonoBehaviour
    {
        [Header("Noise Settings")]
        [SerializeField]
        private NoiseSettings noiseSettings = new NoiseSettings();

        [Header("Randomization")]
        [SerializeField]
        private bool randomizeOnStart = true;

        [Header("Events")]
        public UnityEvent OnNoiseUpdated = new UnityEvent();

        private float timeOffset = 0f;
        private Boundaries boundaries;

        public NoiseSettings Settings => noiseSettings;
        public float CurrentTimeOffset => timeOffset;
        public Bounds WorldBounds => boundaries?.WorldBounds ?? new Bounds();

        void Awake()
        {
            boundaries = GetComponent<Boundaries>();
            if (boundaries == null)
            {
                Debug.LogError("NoiseManager requires Boundaries component!");
            }
            if (randomizeOnStart)
            {
                RandomizeNoise();
            }
        }

        void Update()
        {
            if (noiseSettings.animate)
            {
                timeOffset += noiseSettings.animationSpeed * Time.deltaTime;
                OnNoiseUpdated?.Invoke();
            }
        }

        [ContextMenu("Randomize Noise")]
        public void RandomizeNoise()
        {
            // Force randomization by generating new offset
            noiseSettings.offset = new Vector2(
                Random.Range(-1000f, 1000f),
                Random.Range(-1000f, 1000f)
            );

            // Notify systems of change
            OnNoiseUpdated?.Invoke();
        }

        void OnValidate()
        {
            if (Application.isPlaying)
                OnNoiseUpdated?.Invoke();
        }
    }
}
