using UnityEngine;

namespace EvolutionSimulator.Creature
{
    [System.Serializable]
    public struct NodeGene
    {
        public int parentIndex; // -1 for root, 0-(n-1) for sequential connections
        public float baseAngle; // 0-360 degrees - initial joint orientation
        public float maxAngle; // -180 to 180 degrees - movement constraint

        public NodeGene(int parent, float angle, float max)
        {
            parentIndex = parent;

            if (parent == -1) // Root node - no movement
            {
                baseAngle = 0f;
                maxAngle = 0f;
            }
            else // Regular node - clamp values
            {
                baseAngle = Mathf.Clamp(angle, 0f, 360f);
                maxAngle = Mathf.Clamp(max, -180f, 180f);
            }
        }

        public bool IsRoot => parentIndex == -1;
        public bool IsValid => parentIndex >= -1;

        // Validate sequential connection rule
        public bool IsValidAtPosition(int position)
        {
            if (IsRoot)
                return position == 0;
            return parentIndex >= 0 && parentIndex < position;
        }
    }

    [System.Serializable]
    public class CreatureGenome
    {
        public NodeGene[] nodes;

        public CreatureGenome(NodeGene[] nodeArray)
        {
            nodes = nodeArray ?? new NodeGene[1] { new NodeGene(-1, 0, 0) };
            ValidateGenome();
        }

        public int NodeCount => nodes.Length;
        public NodeGene RootNode => nodes.Length > 0 ? nodes[0] : new NodeGene(-1, 0, 0);

        void ValidateGenome()
        {
            // Ensure root node at position 0
            if (nodes.Length > 0 && !nodes[0].IsRoot)
            {
                nodes[0] = new NodeGene(-1, nodes[0].baseAngle, nodes[0].maxAngle);
            }

            // Validate sequential connections
            for (int i = 1; i < nodes.Length; i++)
            {
                if (!nodes[i].IsValidAtPosition(i))
                {
                    // Fix invalid connection by connecting to previous node
                    nodes[i] = new NodeGene(i - 1, nodes[i].baseAngle, nodes[i].maxAngle);
                }
            }
        }

        public NodeGene[] GetChildrenOf(int parentIndex)
        {
            var children = new System.Collections.Generic.List<NodeGene>();
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].parentIndex == parentIndex)
                    children.Add(nodes[i]);
            }
            return children.ToArray();
        }

        public CreatureGenome Clone()
        {
            var clonedNodes = new NodeGene[nodes.Length];
            System.Array.Copy(nodes, clonedNodes, nodes.Length);
            return new CreatureGenome(clonedNodes);
        }
    }
}
