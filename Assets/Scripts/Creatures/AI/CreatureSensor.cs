using System.Collections.Generic;
using System.Linq;
using EvolutionSimulator.Environment;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class CreatureSensor : MonoBehaviour
    {
        [Header("Sensor Settings")]
        [SerializeField]
        private float sensorRadius = 10f;

        [SerializeField]
        private LayerMask foodLayerMask = -1;

        private CreatureEnergy creatureEnergy;

        // Cached sensor data
        public float FoodDistance { get; private set; } = 1f;
        public float FoodDirection { get; private set; } = 0f;
        public float FoodDetected { get; private set; } = 0f;
        public float OthersDistance { get; private set; } = 1f;
        public float OthersDirection { get; private set; } = 0f;
        public float OthersDetected { get; private set; } = 0f;
        public float MateDistance { get; private set; } = 1f;
        public float MateDirection { get; private set; } = 0f;
        public float MateDetected { get; private set; } = 0f;

        void Start()
        {
            creatureEnergy = GetComponent<CreatureEnergy>();
        }

        void Update()
        {
            if (!creatureEnergy.IsAlive)
                return;

            UpdateSensorData();
        }

        void UpdateSensorData()
        {
            // Scan for objects
            DetectFood();
            DetectCreatures();
        }

        void DetectFood()
        {
            Collider2D[] foodColliders = Physics2D.OverlapCircleAll(
                transform.position,
                sensorRadius,
                foodLayerMask
            );
            FoodItem nearestFood = null;
            float nearestDistance = float.MaxValue;
            int validFoodCount = 0;

            foreach (var collider in foodColliders)
            {
                FoodItem food = collider.GetComponent<FoodItem>();
                if (food != null && !food.IsConsumed)
                {
                    validFoodCount++;
                    float distance = Vector2.Distance(
                        transform.position,
                        collider.transform.position
                    );
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestFood = food;
                    }
                }
            }
            if (nearestFood != null)
            {
                FoodDetected = 1f;
                FoodDistance = Mathf.Clamp01(nearestDistance / sensorRadius);
                Vector2 direction = (
                    nearestFood.transform.position - transform.position
                ).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float relativeAngle = Mathf.DeltaAngle(transform.eulerAngles.z, angle);
                FoodDirection = Mathf.Clamp(relativeAngle / 180f, -1f, 1f);
            }
            else
            {
                FoodDetected = 0f;
                FoodDistance = 1f;
                FoodDirection = 0f;
            }
        }

        void DetectCreatures()
        {
            Node[] detectedNodes = Physics2D
                .OverlapCircleAll(transform.position, sensorRadius)
                .Select(c => c.GetComponent<Node>())
                .Where(n => n != null && n.transform.root != transform)
                .ToArray();

            var creatureGroups = detectedNodes.GroupBy(n => n.transform.root).ToList();

            ProcessOthers(creatureGroups);
            ProcessMates(creatureGroups);
        }

        void ProcessOthers(List<IGrouping<Transform, Node>> creatureGroups)
        {
            var others = creatureGroups.Where(g => !IsReproductionReady(g.Key.gameObject)).ToList();

            if (others.Any())
            {
                OthersDetected = 1f;
                var (distance, direction) = CalculateWeightedAverage(others);
                OthersDistance = distance;
                OthersDirection = direction;
            }
            else
            {
                OthersDetected = 0f;
                OthersDistance = 1f;
                OthersDirection = 0f;
            }
        }

        void ProcessMates(List<IGrouping<Transform, Node>> creatureGroups)
        {
            var mates = creatureGroups.Where(g => IsReproductionReady(g.Key.gameObject)).ToList();

            if (mates.Any())
            {
                MateDetected = 1f;
                var nearestMate = mates
                    .Select(g => new { Group = g, Distance = GetNearestNodeDistance(g) })
                    .OrderBy(x => x.Distance)
                    .First();

                MateDistance = Mathf.Clamp01(nearestMate.Distance / sensorRadius);
                Vector2 direction = GetAverageDirection(nearestMate.Group);
                MateDirection = GetRelativeDirection(direction);
            }
            else
            {
                MateDetected = 0f;
                MateDistance = 1f;
                MateDirection = 0f;
            }
        }

        (float distance, float direction) CalculateWeightedAverage(
            List<IGrouping<Transform, Node>> groups
        )
        {
            float totalWeight = 0f;
            float weightedDistance = 0f;
            Vector2 weightedDirection = Vector2.zero;

            foreach (var group in groups)
            {
                float distance = GetNearestNodeDistance(group);
                float weight = 1f / (distance + 0.1f);
                Vector2 direction = GetAverageDirection(group);

                weightedDistance += distance * weight;
                weightedDirection += direction * weight;
                totalWeight += weight;
            }

            return (
                Mathf.Clamp01(weightedDistance / totalWeight / sensorRadius),
                GetRelativeDirection(weightedDirection.normalized)
            );
        }

        float GetNearestNodeDistance(IGrouping<Transform, Node> group)
        {
            return group.Min(n => Vector2.Distance(transform.position, n.transform.position));
        }

        Vector2 GetAverageDirection(IGrouping<Transform, Node> group)
        {
            Vector2 sum = Vector2.zero;
            foreach (var node in group)
            {
                sum += (Vector2)(node.transform.position - transform.position);
            }
            return sum / group.Count();
        }

        float GetRelativeDirection(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float relativeAngle = Mathf.DeltaAngle(transform.eulerAngles.z, angle);
            return Mathf.Clamp(relativeAngle / 180f, -1f, 1f);
        }

        bool IsReproductionReady(GameObject creature)
        {
            var energy = creature.GetComponent<CreatureEnergy>();
            return energy != null && energy.IsReproductionReady;
        }
    }
}
