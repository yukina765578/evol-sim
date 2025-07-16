using EvolutionSimulator.Creatures.Core;
using EvolutionSimulator.Creatures.Detectors;
using EvolutionSimulator.Creatures.Genetics;
using EvolutionSimulator.Creatures.Population;
using UnityEngine;
using UnityEngine.Events;

namespace EvolutionSimulator.Creatures.Biology
{
    public class ReproductionController : MonoBehaviour
    {
        private float reproductionEnergyCost = 80f;
        private float offspringSpawnDistance = 3f;
        private float reproductionCooldown = 5f;
        private bool logReproduction = false;

        private float lastReproductionTime = 0f;
        private bool isOnCooldown = false;

        private Manager populationManager;
        private Energy creatureEnergy;
        private CreatureDetector creatureDetector;

        private UnityAction<Controller> onCreatureDetectedAction;

        void Awake()
        {
            populationManager = FindFirstObjectByType<Manager>();
            creatureEnergy = GetComponent<Energy>();

            if (populationManager == null)
                Debug.LogError("ReproductionController requires Manager in scene!");
            if (creatureEnergy == null)
                Debug.LogError("ReproductionController requires Energy component!");
        }

        void Start()
        {
            creatureDetector = GetComponentInChildren<CreatureDetector>();
            if (creatureDetector != null)
            {
                onCreatureDetectedAction = OnCreatureDetected;
                creatureDetector.OnCreatureDetected.AddListener(onCreatureDetectedAction);
                EventDebugger.CreatureDetectionListeners++;
            }
        }

        void Update()
        {
            if (isOnCooldown)
            {
                if (Time.time - lastReproductionTime >= reproductionCooldown)
                {
                    isOnCooldown = false;
                }
            }
        }

        void OnCreatureDetected(Controller partnerController)
        {
            if (isOnCooldown)
                return;

            if (partnerController == null)
                return;

            Energy partnerEnergy = partnerController.GetComponent<Energy>();
            if (partnerEnergy == null)
                return;

            TryReproduce(partnerController.gameObject);
        }

        void TryReproduce(GameObject partner)
        {
            if (populationManager == null || partner == null)
                return;

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
                populationManager.RegisterExistingCreature(offspring);

                if (creatureEnergy != null)
                    creatureEnergy.ConsumeEnergy(reproductionEnergyCost);

                Energy partnerEnergy = partner.GetComponent<Energy>();
                if (partnerEnergy != null)
                    partnerEnergy.ConsumeEnergy(reproductionEnergyCost);

                lastReproductionTime = Time.time;
                isOnCooldown = true;

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
            if (creature == null)
                return null;

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
            if (creatureDetector != null && onCreatureDetectedAction != null)
            {
                creatureDetector.OnCreatureDetected.RemoveListener(onCreatureDetectedAction);
                EventDebugger.CreatureDetectionListeners--;
            }
        }

        void OnValidate()
        {
            reproductionEnergyCost = Mathf.Max(0f, reproductionEnergyCost);
            offspringSpawnDistance = Mathf.Max(1f, offspringSpawnDistance);
        }
    }
}
