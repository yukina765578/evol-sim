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

        [SerializeField]
        private float matingRange = 3f;

        [SerializeField]
        private float matingCheckInterval = 1f;

        [Header("Debug")]
        [SerializeField]
        private bool showMatingRange = true;

        private PopulationManager populationManager;
        private CreatureSpawner creatureSpawner;
        private CreatureEnergy creatureEnergy;
        private float matingTimer = 0f;
        private GameObject lastMatingPartner;
        private float cooldownTimer = 0f;
        private const float MATING_COOLDOWN = 5f;

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

        void Update()
        {
            matingTimer += Time.deltaTime;
            cooldownTimer += Time.deltaTime;

            if (matingTimer >= matingCheckInterval)
            {
                matingTimer = 0f;
                CheckForMatingOpportunities();
            }
        }

        void CheckForMatingOpportunities()
        {
            if (!creatureEnergy.IsReproductionReady || cooldownTimer < MATING_COOLDOWN)
                return;

            // Find potential mates within range
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(
                transform.position,
                matingRange
            );

            foreach (var collider in nearbyColliders)
            {
                // Skip if it's part of this creature
                if (collider.transform.root == transform)
                    continue;

                // Check if it's a creature
                GameObject potentialMate = collider.transform.root.gameObject;
                if (!potentialMate.CompareTag("Creature"))
                    continue;

                // Avoid immediate re-mating with same partner
                if (potentialMate == lastMatingPartner)
                    continue;

                // Check if potential mate is ready for reproduction
                CreatureEnergy mateEnergy = potentialMate.GetComponent<CreatureEnergy>();
                if (mateEnergy == null || !mateEnergy.IsReproductionReady)
                    continue;

                // Check mate's cooldown
                ReproductionController mateController =
                    potentialMate.GetComponent<ReproductionController>();
                if (mateController != null && mateController.cooldownTimer < MATING_COOLDOWN)
                    continue;

                // Attempt reproduction
                if (TryReproduce(potentialMate))
                {
                    lastMatingPartner = potentialMate;
                    cooldownTimer = 0f;

                    // Reset mate's cooldown too
                    if (mateController != null)
                        mateController.cooldownTimer = 0f;

                    break; // Only mate once per check
                }
            }
        }

        void OnReproductionStateChanged(bool isReady)
        {
            // Visual feedback - nodes already change color in CreatureEnergy
        }

        bool TryReproduce(GameObject partner)
        {
            if (populationManager.ActiveCreatureCount >= populationManager.MaxPopulationSize - 1)
                return false; // Need space for 2 offspring

            var (parentGenome1, parentBrain1) = ExtractGenomesFromCreature(gameObject);
            var (parentGenome2, parentBrain2) = ExtractGenomesFromCreature(partner);

            if (parentGenome1 == null || parentGenome2 == null)
                return false;

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

                return true;
            }

            return false;
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

        void OnDrawGizmosSelected()
        {
            if (showMatingRange)
            {
                Gizmos.color =
                    creatureEnergy != null && creatureEnergy.IsReproductionReady
                        ? Color.red
                        : Color.gray;

                // Draw circle using multiple line segments for 2D visualization
                Vector3 center = transform.position;
                int segments = 32;
                float angleStep = 360f / segments;

                for (int i = 0; i < segments; i++)
                {
                    float angle1 = i * angleStep * Mathf.Deg2Rad;
                    float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

                    Vector3 point1 =
                        center
                        + new Vector3(
                            Mathf.Cos(angle1) * matingRange,
                            Mathf.Sin(angle1) * matingRange,
                            0
                        );
                    Vector3 point2 =
                        center
                        + new Vector3(
                            Mathf.Cos(angle2) * matingRange,
                            Mathf.Sin(angle2) * matingRange,
                            0
                        );

                    Gizmos.DrawLine(point1, point2);
                }
            }
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
