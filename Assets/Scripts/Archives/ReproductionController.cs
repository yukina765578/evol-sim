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
            // Visual/audio feedback here
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
            if (populationManager.ActiveCreatureCount >= populationManager.MaxPopulationSize - 1)
                return; // Need space for 2 offspring

            CreatureGenome parentGenome1 = ExtractGenomeFromCreature(gameObject);
            CreatureGenome parentGenome2 = ExtractGenomeFromCreature(partner);

            if (parentGenome1 == null || parentGenome2 == null)
                return;

            var (offspring1Genome, offspring2Genome) = GeneticCrossover.CrossoverGenomes(
                parentGenome1,
                parentGenome2
            );

            Vector3 partnerPos = partner.transform.position;
            Vector3 spawn1 = GetOffspringSpawnPosition(partnerPos, 0f);
            Vector3 spawn2 = GetOffspringSpawnPosition(partnerPos, 180f);

            GameObject offspring1 = CreatureBuilder.BuildCreature(offspring1Genome, spawn1);
            GameObject offspring2 = CreatureBuilder.BuildCreature(offspring2Genome, spawn2);

            if (offspring1 != null && offspring2 != null)
            {
                populationManager.RegisterExistingCreature(offspring1);
                populationManager.RegisterExistingCreature(offspring2);

                creatureEnergy.ConsumeEnergy(reproductionEnergyCost);
                partner.GetComponent<CreatureEnergy>().ConsumeEnergy(reproductionEnergyCost);

                Debug.Log(
                    $"Reproduction: {name} + {partner.name} = {offspring1.name} + {offspring2.name}"
                );
            }
        }

        Vector3 GetOffspringSpawnPosition(Vector3 partnerPosition, float angleOffset)
        {
            Vector3 midpoint = (transform.position + partnerPosition) / 2f;
            float angle = (angleOffset + Random.Range(-30f, 30f)) * Mathf.Deg2Rad;
            Vector3 offset =
                new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * offspringSpawnDistance;
            return midpoint + offset;
        }

        CreatureGenome ExtractGenomeFromCreature(GameObject creature)
        {
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
