using System.Collections.Generic;
using EvolutionSimulator.Creatures.Genetics;
using EvolutionSimulator.Creatures.Rendering;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class Controller : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField]
        private bool showThrustDebug = false;

        [SerializeField]
        private bool showVelocityDebug = false;

        private CreatureGenome genome;
        private CreatureRenderData renderData;
        private bool isDead = false;

        private Rigidbody2D creatureRigidbody;
        private List<Segment> segments = new List<Segment>();
        private List<Node> nodes = new List<Node>();

        private float startTime;
        private const float STARTUP_DELAY = 5f;
        private bool canApplyThrust = false;
        private const float VELOCITY_FREEZE_DURATION = 0.5f;
        private bool isSpawnedCreature = false;

        public bool IsSpawnedCreature => isSpawnedCreature;

        public CreatureGenome GetGenome() => genome;

        public CreatureRenderData GetRenderData() => renderData;

        public void Initialize(CreatureGenome creatureGenome)
        {
            genome = creatureGenome;
            if (genome == null)
            {
                Debug.LogError("Controller requires valid CreatureGenome!");
                return;
            }
            startTime = Time.time;
        }

        public void SetRenderData(CreatureRenderData data)
        {
            renderData = data;
        }

        void Start()
        {
            SetupComponents();
        }

        void Update()
        {
            if (!canApplyThrust && Time.time - startTime > STARTUP_DELAY)
            {
                canApplyThrust = true;
            }

            if (isSpawnedCreature)
            {
                float spawnElapsed = Time.time - startTime;
                if (spawnElapsed < VELOCITY_FREEZE_DURATION)
                {
                    creatureRigidbody.linearVelocity = Vector2.zero;
                    creatureRigidbody.angularVelocity = 0f;
                }
                if (spawnElapsed >= 1f)
                {
                    isSpawnedCreature = false;
                }
            }

            UpdateSegmentRotations();
            if (canApplyThrust)
            {
                ApplyThrust();
            }
            UpdateRenderData();
        }

        public void SetSpawnedCreature(bool isSpawned)
        {
            isSpawnedCreature = isSpawned;
        }

        void SetupComponents()
        {
            creatureRigidbody = GetComponent<Rigidbody2D>();
            if (creatureRigidbody == null)
                creatureRigidbody = gameObject.AddComponent<Rigidbody2D>();

            creatureRigidbody.gravityScale = 0f;
            segments.AddRange(GetComponentsInChildren<Segment>());
            nodes.AddRange(GetComponentsInChildren<Node>());
        }

        void UpdateSegmentRotations()
        {
            foreach (Segment segment in segments)
            {
                segment.UpdateRotation();
            }
        }

        void ApplyThrust()
        {
            Vector2 totalThrust = Vector2.zero;
            Vector2 totalDrag = Vector2.zero;

            Vector2 currentVelocity = creatureRigidbody.linearVelocity;
            float maxTotalDrag = currentVelocity.magnitude * 0.3f;
            float maxDragPerSegment = maxTotalDrag / segments.Count;

            foreach (Segment segment in segments)
            {
                totalThrust += segment.GetThrust();
                totalDrag += segment.GetWaterDrag(currentVelocity, maxDragPerSegment);
            }

            creatureRigidbody.AddForce(totalThrust, ForceMode2D.Force);
            if (totalDrag.magnitude > 0f)
            {
                creatureRigidbody.AddForce(totalDrag, ForceMode2D.Force);
            }
        }

        void UpdateRenderData()
        {
            if (renderData == null)
                return;

            // Update root position
            renderData.rootPosition = transform.position;

            // Update node world positions
            for (int i = 0; i < nodes.Count && i < renderData.nodes.Length; i++)
            {
                renderData.nodes[i].worldPosition = nodes[i].transform.position;

                // Update color based on energy state
                var energy = GetComponent<Energy>();
                if (energy != null && energy.IsReproductionReady)
                {
                    renderData.nodes[i].color = Color.red;
                }
                else
                {
                    renderData.nodes[i].color = Color.blue;
                }
            }

            // Update segment endpoints
            for (int i = 0; i < segments.Count && i < renderData.segments.Length; i++)
            {
                var segment = segments[i];
                var parentNode = segment.GetParentNode();
                var childNode = segment.GetChildNode();

                if (parentNode != null && childNode != null)
                {
                    renderData
                        .segments[i]
                        .UpdatePoints(parentNode.transform.position, childNode.transform.position);
                }
            }
        }

        public void HandleDeath(string cause)
        {
            if (isDead)
                return;
            isDead = true;

            if (renderData != null)
            {
                renderData.isAlive = false;
            }

            Destroy(gameObject);
        }
    }
}
