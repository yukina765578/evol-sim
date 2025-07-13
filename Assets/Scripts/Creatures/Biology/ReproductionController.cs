using EvolutionSimulator.Creatures.Core;
using EvolutionSimulator.Creatures.Detectors;
using EvolutionSimulator.Creatures.Genetics;
using EvolutionSimulator.Creatures.Population;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Biology
{
    public class ReproductionController : MonoBehaviour
    {
        private float reproductionEnergyCost = 80f;

        private float offspringSpawnDistance = 3f;

        private bool logReproduction = false;

        private Manager populationManager;
        private Energy creatureEnergy;
        private CreatureDetector creatureDetector;

        void Awake()
        {
            populationManager = FindFirstObjectByType<Manager>();
            creatureEnergy = GetComponent<Energy>();
            creatureDetector = GetComponentInChildren<CreatureDetector>();

            if (populationManager == null)
                Debug.LogError("ReproductionController requires Manager in scene!");
            if (creatureEnergy == null)
                Debug.LogError("ReproductionController requires Energy component!");
            if (creatureDetector == null)
                Debug.LogError("ReproductionController requires CreatureDetector component!");
        }

        void Start()
        {
            if (creatureDetector != null)
            {
                creatureDetector.OnCreatureDetected.AddListener(OnCreatureDetected);
            }

            if (creatureEnergy != null)
            {
                creatureEnergy.OnReproductionReadyChanged.AddListener(OnReproductionStateChanged);
            }
        }

        void OnReproductionStateChanged(bool isReady)
        {
            if (logReproduction)
            {
                Debug.Log($"{name} reproduction ready: {isReady}");
            }
        }

        void OnCreatureDetected(GameObject partner)
        {
            Energy partnerEnergy = partner.GetComponent<Energy>();
            if (partnerEnergy == null)
                return;

            if (
                !partnerEnergy.IsAlive
                || !creatureEnergy.IsAlive
                || !creatureEnergy.IsReproductionReady
                || !partnerEnergy.IsReproductionReady
            )
                return;

            if (logReproduction)
            {
                Debug.Log($"Reproduction attempt: {name} + {partner.name}");
            }

            TryReproduce(partner);
        }

        void TryReproduce(GameObject partner)
        {
            if (populationManager.Creatures.Count >= populationManager.MaxPopulationSize)
            {
                if (logReproduction)
                    Debug.Log("Reproduction failed: Population at maximum");
                return;
            }

            CreatureGenome parentGenome1 = ExtractGenomeFromCreature(gameObject);
            CreatureGenome parentGenome2 = ExtractGenomeFromCreature(partner);

            if (parentGenome1 == null || parentGenome2 == null)
            {
                if (logReproduction)
                    Debug.Log("Reproduction failed: Could not extract genomes");
                return;
            }

            // Perform genetic crossover
            CreatureGenome offspringGenome = GeneticCrossover.CrossoverGenomes(
                parentGenome1,
                parentGenome2
            );

            // Spawn offspring at calculated position
            Vector3 spawnPosition = GetOffspringSpawnPosition(partner.transform.position);
            GameObject offspring = Builder.BuildCreature(offspringGenome, spawnPosition);

            if (offspring != null)
            {
                // Add to population manager's creature list
                // Note: Manager component should have a RegisterExistingCreature method
                // or we add directly to the internal list

                // Consume reproduction energy from both parents
                creatureEnergy.ConsumeEnergy(reproductionEnergyCost);
                partner.GetComponent<Energy>().ConsumeEnergy(reproductionEnergyCost);

                if (logReproduction)
                {
                    Debug.Log(
                        $"Successful reproduction: {name} + {partner.name} = {offspring.name}"
                    );
                }
            }
        }

        Vector3 GetOffspringSpawnPosition(Vector3 partnerPosition)
        {
            Vector3 midpoint = (transform.position + partnerPosition) / 2f;
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 offset =
                new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0)
                * offspringSpawnDistance;

            return midpoint + offset;
        }

        CreatureGenome ExtractGenomeFromCreature(GameObject creature)
        {
            Controller creatureController = creature.GetComponent<Controller>();
            if (creatureController == null)
            {
                Debug.LogWarning($"Creature {creature.name} has no Controller component!");
                return null;
            }
            return creatureController.GetGenome();
        }

        void OnDestroy()
        {
            if (creatureDetector != null)
            {
                creatureDetector.OnCreatureDetected.RemoveListener(OnCreatureDetected);
            }

            // Note: Ensure Energy component properly implements OnReproductionReadyChanged
            if (creatureEnergy != null)
            {
                creatureEnergy.OnReproductionReadyChanged.RemoveListener(
                    OnReproductionStateChanged
                );
            }
        }

        void OnValidate()
        {
            reproductionEnergyCost = Mathf.Max(0f, reproductionEnergyCost);
            offspringSpawnDistance = Mathf.Max(1f, offspringSpawnDistance);
        }
    }
}
