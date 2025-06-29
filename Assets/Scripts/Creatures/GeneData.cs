using UnityEngine;

namespace EvolutionSimulator.Creature
{
    [System.Serializable]
    public struct SegmentGene
    {
        public int parentIndex; // -1 for parents, 0-n for children
        public float baseAngle; // 0-360 degrees
        public float oscSpeed; // 0.5-8.0 oscillation speed
        public float maxAngle; // -180 to 180 degrees
        public float forwardRatio; // 0.01-0.99 forward ratio
        public int childCount; // 0 for children, n for parents

        public SegmentGene(
            int parent,
            float angle,
            float speed,
            float max,
            float ratio,
            int children
        )
        {
            parentIndex = parent;
            baseAngle = Mathf.Clamp(angle, 0f, 360f);
            oscSpeed = Mathf.Clamp(speed, 0.5f, 8f);
            maxAngle = Mathf.Clamp(max, -180f, 180f);
            forwardRatio = Mathf.Clamp(ratio, 0.01f, 0.99f);
            childCount = Mathf.Max(0, children);
        }

        public bool IsParent => parentIndex == -1;
        public bool IsValid => parentIndex >= -1;
        public bool IsPadded => parentIndex == -1 && baseAngle == 0 && oscSpeed == 0.5f;
    }

    [System.Serializable]
    public class CreatureGenome
    {
        public SegmentGene[] genes;

        public CreatureGenome(SegmentGene[] allGenes)
        {
            genes = allGenes ?? new SegmentGene[0];
        }

        public SegmentGene[] GetParentGenes()
        {
            var parents = new System.Collections.Generic.List<SegmentGene>();
            foreach (var gene in genes)
                if (gene.IsParent && !gene.IsPadded)
                    parents.Add(gene);
            return parents.ToArray();
        }

        public SegmentGene[] GetChildGenes()
        {
            var children = new System.Collections.Generic.List<SegmentGene>();
            foreach (var gene in genes)
                if (!gene.IsParent && !gene.IsPadded)
                    children.Add(gene);
            return children.ToArray();
        }

        public int TotalActiveSegments
        {
            get
            {
                int count = 0;
                foreach (var gene in genes)
                    if (!gene.IsPadded)
                        count++;
                return count;
            }
        }
    }
}
