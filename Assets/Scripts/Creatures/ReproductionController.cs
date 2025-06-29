using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class ReproductionController : MonoBehaviour
    {
        [Header("Reproduction Settings")]
        [SerializeField]
        private float reproductionEnergyCost = 80f;

        [SerializeField]
        private float offspringSpawnDistance = 3f;

        private PopulationManager populationManager;
        private CreatureSpawner creatureSpawner;
        private CreatureEnergy creatureEnergy;

        void Awake()
        {
            populationManager = FindFirstObjectByType<PopulationManager>();
            creatureSpawner = FindFirstObjectByType<CreatureSpawner>();
            creatureEnergy = GetComponent<CreatureEnergy>();

            if (populationManager == null)
                Debug.LogError("ReproductionController requires PopulationManager in scene!");
            if (creatureSpawner == null)
                Debug.LogError("ReproductionController requires CreatureSpawner in scene!");
        }

        void Start()
        {
            creatureEnergy.OnReproductionReadyChanged.AddListener(OnReproductionStateChanged);
        }

        void OnReproductionStateChanged(bool isReady)
        {
            // Could add visual/audio feedback here
        }

        public void OnNodeCollision(GameObject partner)
        {
            if (!creatureEnergy.IsReproductionReady)
                return;

            CreatureEnergy partnerEnergy = partner.GetComponent<CreatureEnergy>();
            if (partnerEnergy == null || !partnerEnergy.IsReproductionReady)
                return;

            Debug.Log($"Node reproduction collision: {name} + {partner.name}");
            TryReproduce(partner);
        }

        void TryReproduce(GameObject partner)
        {
            // Check population limit
            if (populationManager.ActiveCreatureCount >= populationManager.PopulationSize)
                return;

            // Get genomes from both parents
            CreatureGenome parentGenome1 = ExtractGenomeFromCreature(gameObject);
            CreatureGenome parentGenome2 = ExtractGenomeFromCreature(partner);

            if (parentGenome1 == null || parentGenome2 == null)
                return;

            // Create offspring genome through crossover
            CreatureGenome offspringGenome = GeneticCrossover.CrossoverGenomes(
                parentGenome1,
                parentGenome2
            );

            // Apply mutation
            GeneticCrossover.MutateGenome(offspringGenome);

            // Spawn offspring
            Vector3 spawnPosition = GetOffspringSpawnPosition(partner.transform.position);
            GameObject offspring = CreatureBuilder.BuildCreature(offspringGenome, spawnPosition);

            if (offspring != null)
            {
                // Register offspring with population manager
                populationManager.RegisterExistingCreature(offspring);

                // Consume energy from both parents
                creatureEnergy.ConsumeEnergy(reproductionEnergyCost);
                partner.GetComponent<CreatureEnergy>().ConsumeEnergy(reproductionEnergyCost);

                Debug.Log($"Reproduction: {name} + {partner.name} = {offspring.name}");
            }
        }

        Vector3 GetOffspringSpawnPosition(Vector3 partnerPosition)
        {
            Vector3 midpoint = (transform.position + partnerPosition) / 2f;
            Vector2 randomOffset = Random.insideUnitCircle * offspringSpawnDistance;
            return midpoint + new Vector3(randomOffset.x, randomOffset.y, 0);
        }

        CreatureGenome ExtractGenomeFromCreature(GameObject creature)
        {
            // Extract genome from existing creature structure
            var segments = creature.GetComponentsInChildren<Segment>();
            if (segments.Length == 0)
                return null;

            // For now, generate random genome (will be replaced with actual extraction)
            return RandomGeneGenerator.GenerateRandomGenome();
        }

        void OnDestroy()
        {
            if (creatureEnergy != null)
                creatureEnergy.OnReproductionReadyChanged.RemoveListener(
                    OnReproductionStateChanged
                );
        }
    }
}
