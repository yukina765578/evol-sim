using UnityEngine;

namespace EvolutionSimulator.Creatures.Biology
{
    public class ReproductionController : MonoBehaviour
    {
        [Header("Reproduction Settings")]
        [SerializeField]
        private float reproductionEnergyCost = 80f;

        [SerializeField]
        private float offspringSpawnDistance = 3f;

        private Manager populationManager;
        private Spawner creatureSpawner;
        private Energy creatureEnergy;

        void Awake()
        {
            populationManager = FindFirstObjectByType<Manager>();
            creatureSpawner = FindFirstObjectByType<Spawner>();
            creatureEnergy = GetComponent<Energy>();

            if (populationManager == null)
                Debug.LogError("ReproductionController requires Manager in scene!");
            if (creatureSpawner == null)
                Debug.LogError("ReproductionController requires Spawner in scene!");
        }

        void Start()
        {
            creatureEnergy.OnReproductionReadyChanged.AddListener(OnReproductionStateChanged);
        }
    }
}
