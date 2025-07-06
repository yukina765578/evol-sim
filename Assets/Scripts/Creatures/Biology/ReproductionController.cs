using EvolutionSimulator.Creature;
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

            var (parentGenome1, parentBrain1) = ExtractGenomesFromCreature(gameObject);
            var (parentGenome2, parentBrain2) = ExtractGenomesFromCreature(partner);

            if (parentGenome1 == null || parentGenome2 == null)
                return;

            var (
                offspring1Body,
                offspring2Body,
                offspring1Brain,
                offspring2Brain,
                offspring1Length,
                offspring2Length
            ) = GeneticCrossover.CrossoverGenomes(
                parentGenome1,
                parentBrain1,
                parentGenome2,
                parentBrain2
            );

            Vector3 partnerPos = partner.transform.position;
            Vector3 spawn1 = GetOffspringSpawnPosition(partnerPos, 0f);
            Vector3 spawn2 = GetOffspringSpawnPosition(partnerPos, 180f);

            GameObject offspring1 = CreatureBuilder.BuildCreature(
                offspring1Body,
                offspring1Brain,
                spawn1
            );
            GameObject offspring2 = CreatureBuilder.BuildCreature(
                offspring2Body,
                offspring2Brain,
                spawn2
            );

            if (offspring1 != null && offspring2 != null)
            {
                populationManager.RegisterExistingCreature(offspring1);
                populationManager.RegisterExistingCreature(offspring2);

                creatureEnergy.ConsumeEnergy(reproductionEnergyCost);
                partner.GetComponent<CreatureEnergy>().ConsumeEnergy(reproductionEnergyCost);

                Debug.Log(
                    $"Reproduction: {name} + {partner.name} = {offspring1.name} + {offspring2.name} "
                        + $"(Lengths: {offspring1Length}, {offspring2Length})"
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

        (CreatureGenome, NEATGenome) ExtractGenomesFromCreature(GameObject creature)
        {
            // Extract body genome (for now, generate random - will be replaced with actual extraction)
            CreatureGenome bodyGenome = RandomGeneGenerator.GenerateRandomGenome();

            // Extract brain genome
            CreatureBrain brain = creature.GetComponent<CreatureBrain>();
            NEATGenome brainGenome = null;

            if (brain != null)
            {
                // Get brain genome from existing brain component
                // For now, create basic brain matching creature's segment count
                int segmentCount = creature.GetComponentsInChildren<Segment>().Length;
                brainGenome = CreateBasicBrain(segmentCount);
            }

            return (bodyGenome, brainGenome);
        }

        NEATGenome CreateBasicBrain(int outputCount)
        {
            NEATGenome brain = new NEATGenome(12, outputCount);

            for (int i = 0; i < outputCount; i++)
            {
                for (int inputIdx = 0; inputIdx < Mathf.Min(4, 12); inputIdx++)
                {
                    if (Random.value < 0.7f) // 70% connection chance
                    {
                        int innovation = InnovationManager.Instance.GetConnectionInnovation(
                            inputIdx,
                            12 + i
                        );
                        brain.AddConnection(
                            new ConnectionGene(
                                inputIdx,
                                12 + i, // Output nodes start at ID 12
                                Random.Range(-1f, 1f),
                                innovation
                            )
                        );
                    }
                }
            }

            return brain;
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
