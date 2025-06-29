using System.Collections.Generic;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class CreatureController : MonoBehaviour
    {
        [Header("Physics")]
        [SerializeField]
        private float slidingFriction = 0.2f;

        [Header("Debug")]
        [SerializeField]
        private bool showThrustDebug = false;

        [SerializeField]
        private bool showVelocityDebug = false;

        private Rigidbody2D creatureRigidbody;
        private List<Segment> segments = new List<Segment>();
        private LineRenderer velocityDebugLine;

        void Start()
        {
            SetupComponents();
            SetupVelocityDebug();
        }

        void Update()
        {
            UpdateSegmentRotations();
            ApplyThrust();
            ApplySlidingFriction();
            UpdateVelocityDebug();
        }

        void SetupComponents()
        {
            creatureRigidbody = GetComponent<Rigidbody2D>();
            if (creatureRigidbody == null)
                creatureRigidbody = gameObject.AddComponent<Rigidbody2D>();

            creatureRigidbody.gravityScale = 0f;

            // Find all segment components
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
            velocityDebugLine.enabled = false;
        }

        void UpdateSegmentRotations()
        {
            foreach (var segment in segments)
            {
                segment.UpdateRotation();
                segment.SetDebugMode(showThrustDebug);
            }
        }

        void ApplyThrust()
        {
            Vector2 totalThrust = Vector2.zero;

            foreach (var segment in segments)
            {
                totalThrust += segment.GetThrust();
            }

            creatureRigidbody.AddForce(totalThrust);
        }

        void ApplySlidingFriction()
        {
            Vector2 velocity = creatureRigidbody.linearVelocity;
            float speed = velocity.magnitude;

            if (speed > 0.001f)
            {
                float frictionCoeff = slidingFriction / (speed + 0.1f);
                Vector2 frictionForce = -velocity * frictionCoeff;
                creatureRigidbody.AddForce(frictionForce);
            }
        }

        void UpdateVelocityDebug()
        {
            if (velocityDebugLine == null || segments.Count == 0)
                return;

            // Update debug line visibility
            velocityDebugLine.enabled = showVelocityDebug;

            if (!showVelocityDebug)
                return;

            Vector2 velocity = creatureRigidbody.linearVelocity;
            Vector3 rootPos = transform.position;

            Vector3 startPos = rootPos + (Vector3)velocity.normalized * 1.2f;
            Vector3 endPos = startPos + (Vector3)velocity.normalized * velocity.magnitude * 3f;

            velocityDebugLine.SetPosition(0, startPos);
            velocityDebugLine.SetPosition(1, endPos);
        }
    }
}
