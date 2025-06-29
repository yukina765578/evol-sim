using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public static class GeneticCrossover
    {
        private const float MUTATION_RATE = 0.1f;

        public static CreatureGenome CrossoverGenomes(
            CreatureGenome parent1,
            CreatureGenome parent2
        )
        {
            var parent1Parents = parent1.GetParentGenes();
            var parent2Parents = parent2.GetParentGenes();
            var parent1Children = parent1.GetChildGenes();
            var parent2Children = parent2.GetChildGenes();

            // Pool genes by hierarchy
            var parentPool = new System.Collections.Generic.List<SegmentGene>();
            parentPool.AddRange(parent1Parents);
            parentPool.AddRange(parent2Parents);

            var childPool = new System.Collections.Generic.List<SegmentGene>();
            childPool.AddRange(parent1Children);
            childPool.AddRange(parent2Children);

            // Choose offspring parent count with mutation
            int offspringParentCount = ChooseOffspringParentCount(
                parent1Parents.Length,
                parent2Parents.Length
            );
            int offspringChildCount = Random.Range(1, Mathf.Max(1, childPool.Count + 1));

            // Create offspring genes
            var offspringParents = new SegmentGene[offspringParentCount];
            var offspringChildren = new SegmentGene[offspringChildCount];

            // Select parents from parent pool
            for (int i = 0; i < offspringParentCount; i++)
            {
                if (parentPool.Count > 0)
                {
                    int randomIndex = Random.Range(0, parentPool.Count);
                    offspringParents[i] = parentPool[randomIndex];
                    // Reset parent index and child count for proper structure
                    offspringParents[i] = new SegmentGene(
                        -1,
                        offspringParents[i].baseAngle,
                        offspringParents[i].oscSpeed,
                        offspringParents[i].maxAngle,
                        offspringParents[i].forwardRatio,
                        0
                    );
                }
            }

            // Select children from child pool
            for (int i = 0; i < offspringChildCount; i++)
            {
                if (childPool.Count > 0)
                {
                    int randomIndex = Random.Range(0, childPool.Count);
                    offspringChildren[i] = childPool[randomIndex];
                    // Assign to random parent and reset child count
                    int parentIndex = Random.Range(0, offspringParentCount);
                    offspringChildren[i] = new SegmentGene(
                        parentIndex,
                        offspringChildren[i].baseAngle,
                        offspringChildren[i].oscSpeed,
                        offspringChildren[i].maxAngle,
                        offspringChildren[i].forwardRatio,
                        0
                    );
                }
            }

            // Combine into single genome
            var allGenes = new SegmentGene[offspringParents.Length + offspringChildren.Length];
            System.Array.Copy(offspringParents, 0, allGenes, 0, offspringParents.Length);
            System.Array.Copy(
                offspringChildren,
                0,
                allGenes,
                offspringParents.Length,
                offspringChildren.Length
            );

            return new CreatureGenome(allGenes);
        }

        static int ChooseOffspringParentCount(int parent1Count, int parent2Count)
        {
            // Choose base parent count (50/50 from either parent)
            int baseCount = Random.value < 0.5f ? parent1Count : parent2Count;

            float rand = Random.value;
            if (rand < 0.5f)
                return baseCount; // 50% stay same
            if (rand < 0.7f)
                return Mathf.Max(1, baseCount - 1); // 20% -1
            if (rand < 0.9f)
                return baseCount + 1; // 20% +1
            return Random.value < 0.5f ? parent1Count : parent2Count; // 10% other parent
        }

        public static void MutateGenome(CreatureGenome genome)
        {
            for (int i = 0; i < genome.genes.Length; i++)
            {
                genome.genes[i] = MutateGene(genome.genes[i]);
            }
        }

        static SegmentGene MutateGene(SegmentGene gene)
        {
            var mutated = gene;

            if (Random.value < MUTATION_RATE)
            {
                mutated.baseAngle = Mathf.Clamp(
                    mutated.baseAngle + Random.Range(-18f, 18f),
                    0f,
                    360f
                );
            }

            if (Random.value < MUTATION_RATE)
            {
                mutated.oscSpeed = Mathf.Clamp(
                    mutated.oscSpeed + Random.Range(-0.4f, 0.4f),
                    0.5f,
                    8f
                );
            }

            if (Random.value < MUTATION_RATE)
            {
                mutated.maxAngle = Mathf.Clamp(
                    mutated.maxAngle + Random.Range(-18f, 18f),
                    -180f,
                    180f
                );
            }

            if (Random.value < MUTATION_RATE)
            {
                mutated.forwardRatio = Mathf.Clamp(
                    mutated.forwardRatio + Random.Range(-0.05f, 0.05f),
                    0.01f,
                    0.5f
                );
            }

            return mutated;
        }
    }
}
