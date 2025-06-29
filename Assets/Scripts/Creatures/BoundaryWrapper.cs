using System.Collections.Generic;
using EvolutionSimulator.Environment;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class BoundaryWrapper : MonoBehaviour
    {
        [Header("Wrapping Settings")]
        [SerializeField]
        private bool enableWrapping = true;

        [SerializeField]
        private float wrapBuffer = 1f; // How far outside bounds before wrapping

        [Header("Debug")]
        [SerializeField]
        private bool showWrapEvents = false;

        private Boundaries boundaries;
        private List<CreatureController> trackedCreatures = new List<CreatureController>();
        private PopulationManager populationManager;

        void Awake()
        {
            boundaries = GetComponent<Boundaries>();
            populationManager = GetComponent<PopulationManager>();

            if (boundaries == null)
            {
                boundaries = FindFirstObjectByType<Boundaries>();
                if (boundaries == null)
                {
                    Debug.LogError("BoundaryWrapper requires Boundaries component in scene!");
                }
            }
        }

        void Update()
        {
            if (!enableWrapping || boundaries == null)
                return;

            UpdateCreatureList();
            WrapCreatures();
        }

        void UpdateCreatureList()
        {
            trackedCreatures.RemoveAll(creature => creature == null);

            if (populationManager != null)
            {
                foreach (GameObject creatureObj in populationManager.ActiveCreatures)
                {
                    if (creatureObj != null)
                    {
                        CreatureController controller =
                            creatureObj.GetComponent<CreatureController>();
                        if (controller != null && !trackedCreatures.Contains(controller))
                        {
                            trackedCreatures.Add(controller);
                        }
                    }
                }
            }
        }

        void WrapCreatures()
        {
            Bounds worldBounds = boundaries.WorldBounds;

            foreach (CreatureController creature in trackedCreatures)
            {
                if (creature == null)
                    continue;

                Vector3 position = creature.transform.position;
                Vector3 wrappedPosition = position;
                bool needsWrap = false;

                // Check X bounds
                if (position.x > worldBounds.max.x + wrapBuffer)
                {
                    wrappedPosition.x = worldBounds.min.x - wrapBuffer;
                    needsWrap = true;
                }
                else if (position.x < worldBounds.min.x - wrapBuffer)
                {
                    wrappedPosition.x = worldBounds.max.x + wrapBuffer;
                    needsWrap = true;
                }

                // Check Y bounds
                if (position.y > worldBounds.max.y + wrapBuffer)
                {
                    wrappedPosition.y = worldBounds.min.y - wrapBuffer;
                    needsWrap = true;
                }
                else if (position.y < worldBounds.min.y - wrapBuffer)
                {
                    wrappedPosition.y = worldBounds.max.y + wrapBuffer;
                    needsWrap = true;
                }

                if (needsWrap)
                {
                    creature.transform.position = wrappedPosition;

                    if (showWrapEvents)
                    {
                        Debug.Log($"Wrapped {creature.name} from {position} to {wrappedPosition}");
                    }
                }
            }
        }

        public void AddCreature(CreatureController creature)
        {
            if (creature != null && !trackedCreatures.Contains(creature))
            {
                trackedCreatures.Add(creature);
            }
        }

        public void RemoveCreature(CreatureController creature)
        {
            trackedCreatures.Remove(creature);
        }
    }
}
