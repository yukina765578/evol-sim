using UnityEngine;

namespace EvolutionSimulator.Creature
{
    [System.Serializable]
    public struct NodeGene
    {
        public int parentIndex; // -1 for root, 0-(n-1) for sequential connections
        public float baseAngle; // 0-360 degrees
        public float oscSpeed; // 0.5-8.0 oscillation speed
        public float maxAngle; // -180 to 180 degrees
        public float forwardRatio; // 0.01-0.99 forward ratio

        public NodeGene(int parent, float angle, float speed, float max, float ratio)
        {
            parentIndex = parent;

            if (parent == -1) // Root node - no movement
            {
                baseAngle = 0f;
                oscSpeed = 0f;
                maxAngle = 0f;
                forwardRatio = 0f;
            }
            else // Regular node - clamp values
            {
                baseAngle = Mathf.Clamp(angle, 0f, 360f);
                oscSpeed = Mathf.Clamp(speed, 0.5f, 8f);
                maxAngle = Mathf.Clamp(max, -180f, 180f);
                forwardRatio = Mathf.Clamp(ratio, 0.01f, 0.99f);
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
            nodes = nodeArray ?? new NodeGene[1] { new NodeGene(-1, 0, 2f, 45f, 0.2f) };
            ValidateGenome();
        }

        public int NodeCount => nodes.Length;
        public NodeGene RootNode =>
            nodes.Length > 0 ? nodes[0] : new NodeGene(-1, 0, 2f, 45f, 0.2f);

        void ValidateGenome()
        {
            // Ensure root node at position 0
            if (nodes.Length > 0 && !nodes[0].IsRoot)
            {
                nodes[0] = new NodeGene(
                    -1,
                    nodes[0].baseAngle,
                    nodes[0].oscSpeed,
                    nodes[0].maxAngle,
                    nodes[0].forwardRatio
                );
            }

            // Validate sequential connections
            for (int i = 1; i < nodes.Length; i++)
            {
                if (!nodes[i].IsValidAtPosition(i))
                {
                    // Fix invalid connection by connecting to previous node
                    nodes[i] = new NodeGene(
                        i - 1,
                        nodes[i].baseAngle,
                        nodes[i].oscSpeed,
                        nodes[i].maxAngle,
                        nodes[i].forwardRatio
                    );
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
