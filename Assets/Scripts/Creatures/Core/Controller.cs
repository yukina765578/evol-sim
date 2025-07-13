using System.Collections.Generic;
using EvolutionSimulator.Creatures.Genetics;
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
        private bool prevThrustDebug = false;
        private bool isDead = false;

        private Rigidbody2D creatureRigidbody;
        private List<Segment> segments = new List<Segment>();
        private LineRenderer velocityDebugLine;

        public CreatureGenome GetGenome() => genome;

        public void Initialize(CreatureGenome creatureGenome)
        {
            genome = creatureGenome;
            if (genome == null)
            {
                Debug.LogError("CreatureController requires a valid CreatureGenome!");
                return;
            }
        }

        void Start()
        {
            SetupComponents();
            SetupVelocityDebug();
        }

        void Update()
        {
            UpdateSegmentRotations();
            ApplyThrust();
            if (showThrustDebug != prevThrustDebug)
            {
                UpdateSegmentDebug();
                prevThrustDebug = showThrustDebug;
            }
            if (showVelocityDebug)
                UpdateVelocityDebug();
        }

        void SetupComponents()
        {
            creatureRigidbody = GetComponent<Rigidbody2D>();
            if (creatureRigidbody == null)
                creatureRigidbody = gameObject.AddComponent<Rigidbody2D>();

            creatureRigidbody.gravityScale = 0f;
            segments.AddRange(GetComponentsInChildren<Segment>());
        }

        void SetupVelocityDebug()
        {
            GameObject debugObj = new GameObject("VelocityDebug");
            debugObj.transform.SetParent(transform);

            velocityDebugLine = debugObj.AddComponent<LineRenderer>();
            velocityDebugLine.material = new Material(Shader.Find("Sprites/Default"));
            velocityDebugLine.positionCount = 2;
            velocityDebugLine.useWorldSpace = true;
            velocityDebugLine.sortingOrder = 10;
            velocityDebugLine.startWidth = 0.1f;
            velocityDebugLine.endWidth = 0.1f;
            velocityDebugLine.startColor = Color.red;
            velocityDebugLine.endColor = Color.red;
            velocityDebugLine.enabled = showVelocityDebug;
        }

        void UpdateSegmentRotations()
        {
            foreach (Segment segment in segments)
            {
                segment.UpdateRotation();
            }
        }

        void UpdateSegmentDebug()
        {
            foreach (Segment segment in segments)
            {
                segment.SetDebugMode(showThrustDebug);
            }
        }

        void ApplyThrust()
        {
            Vector2 totalThrust = Vector2.zero;
            Vector2 totalDrag = Vector2.zero;

            Vector2 currentVelocity = creatureRigidbody.linearVelocity;
            float maxTotalDrag = currentVelocity.magnitude * 0.9f;
            float maxDragPerSegment = maxTotalDrag / segments.Count;

            foreach (Segment segment in segments)
            {
                totalThrust += segment.GetThrust();
                totalDrag += segment.GetWaterDrag(currentVelocity, maxDragPerSegment);
            }

            creatureRigidbody.AddForce(totalThrust, ForceMode2D.Force);
        }

        void UpdateVelocityDebug()
        {
            if (velocityDebugLine == null || segments.Count == 0)
            {
                Debug.LogError(
                    "VelocityDebugLine or segments not set up correctly in CreatureController."
                );
                return;
            }
            velocityDebugLine.enabled = showVelocityDebug;

            if (showVelocityDebug)
            {
                Vector2 velocity = creatureRigidbody.linearVelocity;
                Vector3 rootPosition = transform.position;
                Vector3 startPosition = rootPosition + (Vector3)velocity.normalized * 1.2f;
                Vector3 endPosition = startPosition + (Vector3)velocity * 2f;

                velocityDebugLine.SetPosition(0, startPosition);
                velocityDebugLine.SetPosition(1, endPosition);
            }
        }

        public void HandleDeath(string cause)
        {
            if (isDead)
                return; // Prevent multiple calls
            isDead = true;

            Destroy(gameObject);
        }
    }
}
