using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public static class RandomGeneGenerator
    {
        private const int MAX_TOTAL_SEGMENTS = 20;
        private const int MAX_PARENTS = 4;
        private const int MAX_CHILDREN_PER_PARENT = 4;

        public static CreatureGenome GenerateRandomGenome()
        {
            int parentCount = Random.Range(1, MAX_PARENTS + 1);
            int totalChildren = Random.Range(
                1,
                Mathf.Min(MAX_TOTAL_SEGMENTS - parentCount, parentCount * MAX_CHILDREN_PER_PARENT)
                    + 1
            );

            var genes = new SegmentGene[parentCount + totalChildren];

            // Generate parent segments
            for (int i = 0; i < parentCount; i++)
            {
                int childrenForThisParent = CalculateChildrenForParent(
                    i,
                    parentCount,
                    totalChildren
                );
                genes[i] = GenerateParentGene(childrenForThisParent);
            }

            // Generate child segments
            for (int i = 0; i < totalChildren; i++)
            {
                int parentIndex = Random.Range(0, parentCount);
                genes[parentCount + i] = GenerateChildGene(parentIndex);
            }

            return new CreatureGenome(genes);
        }

        private static int CalculateChildrenForParent(
            int parentIndex,
            int totalParents,
            int totalChildren
        )
        {
            int baseChildren = totalChildren / totalParents;
            int remainder = totalChildren % totalParents;
            return baseChildren + (parentIndex < remainder ? 1 : 0);
        }

        private static SegmentGene GenerateParentGene(int childCount)
        {
            return new SegmentGene(
                -1, // parentIndex for parents
                Random.Range(0f, 360f), // baseAngle
                Random.Range(0.5f, 8f), // oscSpeed
                Random.Range(-180f, 180f), // maxAngle
                Random.Range(0.01f, 0.5f), // forwardRatio
                childCount // childCount
            );
        }

        private static SegmentGene GenerateChildGene(int parentIndex)
        {
            return new SegmentGene(
                parentIndex, // parentIndex
                Random.Range(0f, 360f), // baseAngle
                Random.Range(0.5f, 8f), // oscSpeed
                Random.Range(-180f, 180f), // maxAngle
                Random.Range(0.01f, 0.5f), // forwardRatio
                0 // childCount (children have no children)
            );
        }
    }
}
