using System.Collections.Generic;
using EvolutionSimulator.Creatures.Core;
using EvolutionSimulator.Environment;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Population
{
    public class BoundaryWrapper : MonoBehaviour
    {
        [Header("Wrapping Settings")]
        [SerializeField]
        private bool enableWrapping = true;

        [SerializeField]
        private float wrapBuffer = 1f;

        [Header("Debug")]
        [SerializeField]
        private bool showWrapEvents = false;

        private Boundaries boundaries;
        private List<Controller> trackedCreatures = new List<Controller>();
        private Manager populationManager;

        void Awake()
        {
            boundaries = GetComponent<Boundaries>();
            populationManager = GetComponent<Manager>();

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
                foreach (GameObject creatureObj in populationManager.Creatures)
                {
                    if (creatureObj != null)
                    {
                        Controller controller = creatureObj.GetComponent<Controller>();
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
            foreach (Controller creature in trackedCreatures)
            {
                if (creature == null)
                    continue;

                Vector2 position = creature.transform.position;
                bool wrapped = false;

                if (position.x < worldBounds.min.x - wrapBuffer)
                {
                    position.x = worldBounds.max.x + wrapBuffer;
                    wrapped = true;
                }
                else if (position.x > worldBounds.max.x + wrapBuffer)
                {
                    position.x = worldBounds.min.x - wrapBuffer;
                    wrapped = true;
                }

                if (position.y < worldBounds.min.y - wrapBuffer)
                {
                    position.y = worldBounds.max.y + wrapBuffer;
                    wrapped = true;
                }
                else if (position.y > worldBounds.max.y + wrapBuffer)
                {
                    position.y = worldBounds.min.y - wrapBuffer;
                    wrapped = true;
                }

                if (wrapped)
                {
                    creature.transform.position = position;

                    if (showWrapEvents)
                        Debug.Log($"Wrapped creature {creature.name} to new position: {position}");
                }
            }
        }
    }
}
