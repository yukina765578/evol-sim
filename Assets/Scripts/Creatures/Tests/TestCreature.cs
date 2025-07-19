using EvolutionSimulator.Creatures.Core;
using EvolutionSimulator.Creatures.Genetics;
using EvolutionSimulator.Creatures.Population;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolutionSimulator.Creatures.Test
{
    public class TestCreature : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField]
        private bool spawnOnStart = true;

        [SerializeField]
        private Vector3 spawnPosition = Vector3.zero;

        [Header("Debug Options")]
        [SerializeField]
        private bool showCreatureStats = true;

        [SerializeField]
        private bool enableThrustDebug = false;

        [SerializeField]
        private bool enableVelocityDebug = false;

        [Header("Sample Genes")]
        [SerializeField]
        private NodeGene[] testNodes = new NodeGene[]
        {
            new NodeGene(-1, 0f, 0f, 0f, 0f), // Root node (no movement)
            new NodeGene(0, 0f, 3f, 45f, 0.2f), // Connect to root
            new NodeGene(0, 120f, 3.5f, -45f, 0.6f), // Also connect to root
            new NodeGene(1, 45f, 4f, -60f, 0.5f), // Connect to node 1
            new NodeGene(2, 180f, 2.5f, 120f, 0.3f), // Connect to node 2
        };

        private GameObject currentCreature;
        private Controller currentController;
        private CreatureStats currentStats;

        void Start()
        {
            if (spawnOnStart)
                SpawnTestCreature();
        }

        void Update()
        {
            HandleInput();
            UpdateStats();
        }

        void HandleInput()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                SpawnTestCreature();

            if (Keyboard.current.cKey.wasPressedThisFrame)
                ClearCreature();

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                RandomizeNodes();
                SpawnTestCreature();
            }

            if (Keyboard.current.tKey.wasPressedThisFrame)
                ToggleThrustDebug();

            if (Keyboard.current.vKey.wasPressedThisFrame)
                ToggleVelocityDebug();
        }

        void UpdateStats()
        {
            if (currentCreature != null)
            {
                currentStats = Builder.GetCreatureStats(currentCreature);
            }
        }

        [ContextMenu("Spawn Test Creature")]
        public void SpawnTestCreature()
        {
            ClearCreature();

            try
            {
                // Create genome from test nodes
                CreatureGenome genome = new CreatureGenome(testNodes);

                // Use new Builder system - single GameObject
                currentCreature = Builder.BuildCreature(genome, spawnPosition);

                if (currentCreature != null)
                {
                    currentController = currentCreature.GetComponent<Controller>();

                    // Apply debug settings
                    if (currentController != null)
                    {
                        // Set debug modes through inspector or directly
                        var renderSystem = currentCreature.GetComponent<RenderSystem>();
                        if (renderSystem != null)
                        {
                            renderSystem.SetupDebugMode(enableThrustDebug, enableVelocityDebug);
                        }
                    }

                    // Validate the creature
                    if (Builder.ValidateCreature(currentCreature))
                    {
                        Debug.Log(
                            $"✅ Successfully spawned creature with {genome.NodeCount} nodes at {spawnPosition}"
                        );
                        Debug.Log(
                            $"📊 Creature has {currentController.GetNodeCount()} nodes and {currentController.GetSegmentCount()} segments"
                        );
                    }
                    else
                    {
                        Debug.LogError("❌ Creature validation failed!");
                    }
                }
                else
                {
                    Debug.LogError("❌ Failed to create creature - Builder returned null");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error spawning creature: {e.Message}\n{e.StackTrace}");
            }
        }

        [ContextMenu("Clear Creature")]
        public void ClearCreature()
        {
            if (currentCreature != null)
            {
                DestroyImmediate(currentCreature);
                currentCreature = null;
                currentController = null;
                Debug.Log("🧹 Cleared current creature");
            }
        }

        [ContextMenu("Randomize Nodes")]
        public void RandomizeNodes()
        {
            try
            {
                CreatureGenome randomGenome = Randomizer.GenerateRandomGenome();
                testNodes = randomGenome.nodes;
                Debug.Log($"🎲 Generated random genome with {testNodes.Length} nodes");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Error generating random genome: {e.Message}");
            }
        }

        [ContextMenu("Test Performance")]
        public void TestPerformance()
        {
            if (currentCreature == null)
            {
                Debug.LogWarning("⚠️ No creature to test performance on");
                return;
            }

            // Test GameObject count reduction
            int gameObjectCount = currentCreature.transform.childCount + 1;
            Debug.Log($"📈 Performance Test:");
            Debug.Log(
                $"   GameObject Count: {gameObjectCount} (vs ~{testNodes.Length * 2} in old system)"
            );
            Debug.Log(
                $"   Reduction: {(1f - (float)gameObjectCount / (testNodes.Length * 2f)) * 100f:F1}%"
            );

            // Test memory efficiency
            if (currentController != null)
            {
                Debug.Log($"   Memory: Using data structures instead of MonoBehaviour components");
                Debug.Log(
                    $"   Systems: {currentCreature.GetComponents<MonoBehaviour>().Length} components total"
                );
            }
        }

        void ToggleThrustDebug()
        {
            enableThrustDebug = !enableThrustDebug;
            if (currentCreature != null)
            {
                var renderSystem = currentCreature.GetComponent<RenderSystem>();
                if (renderSystem != null)
                {
                    renderSystem.SetupDebugMode(enableThrustDebug, enableVelocityDebug);
                }
            }
            Debug.Log($"🔧 Thrust Debug: {(enableThrustDebug ? "ON" : "OFF")}");
        }

        void ToggleVelocityDebug()
        {
            enableVelocityDebug = !enableVelocityDebug;
            if (currentCreature != null)
            {
                var renderSystem = currentCreature.GetComponent<RenderSystem>();
                if (renderSystem != null)
                {
                    renderSystem.SetupDebugMode(enableThrustDebug, enableVelocityDebug);
                }
            }
            Debug.Log($"🔧 Velocity Debug: {(enableVelocityDebug ? "ON" : "OFF")}");
        }

        [ContextMenu("Clone Current Creature")]
        public void CloneCreature()
        {
            if (currentCreature == null)
            {
                Debug.LogWarning("⚠️ No creature to clone");
                return;
            }

            Vector3 clonePosition = spawnPosition + Vector3.right * 5f;
            GameObject clone = Builder.CloneCreature(currentCreature, clonePosition);

            if (clone != null)
            {
                Debug.Log($"👯 Cloned creature at {clonePosition}");
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 300));

            GUILayout.Label("🧬 Single GameObject Test Creature Controls:");
            GUILayout.Label("Space: Spawn Test Creature");
            GUILayout.Label("C: Clear Current Creature");
            GUILayout.Label("R: Randomize Nodes and Spawn");
            GUILayout.Label("T: Toggle Thrust Debug");
            GUILayout.Label("V: Toggle Velocity Debug");

            GUILayout.Space(10);

            if (currentCreature != null && showCreatureStats)
            {
                GUILayout.Label($"🎯 Current Creature: {currentCreature.name}");
                GUILayout.Label(
                    $"📊 Nodes: {currentStats.nodeCount} | Segments: {currentStats.segmentCount}"
                );
                GUILayout.Label($"📍 Position: {currentCreature.transform.position}");
                GUILayout.Label($"🏃 Velocity: {currentStats.velocity.magnitude:F2} m/s");
                GUILayout.Label(
                    $"⚡ Energy: {currentStats.currentEnergy:F1}/{currentStats.maxEnergy:F1}"
                );
                GUILayout.Label(
                    $"💚 Reproduction Ready: {(currentStats.isReproductionReady ? "YES" : "NO")}"
                );
                GUILayout.Label($"🌍 Grounded: {(currentStats.isGrounded ? "YES" : "NO")}");
                GUILayout.Label($"🎯 Efficiency: {currentStats.efficiency:F3}");

                GUILayout.Space(5);
                GUILayout.Label(
                    $"🔧 Debug - Thrust: {(enableThrustDebug ? "ON" : "OFF")} | Velocity: {(enableVelocityDebug ? "ON" : "OFF")}"
                );

                // System status
                var systems = currentCreature.GetComponents<MonoBehaviour>();
                GUILayout.Label($"🖥️ Systems: {systems.Length} components");

                // Performance info
                int totalGameObjects = FindObjectsByType<GameObject>(
                    FindObjectsSortMode.None
                ).Length;
                GUILayout.Label($"📈 Scene GameObjects: {totalGameObjects}");
            }
            else if (currentCreature == null)
            {
                GUILayout.Label("❌ No creature spawned.");
                GUILayout.Label("Press Space to spawn a test creature");
            }

            GUILayout.EndArea();
        }

        void OnValidate()
        {
            // Update debug settings when changed in inspector
            if (Application.isPlaying && currentCreature != null)
            {
                var renderSystem = currentCreature.GetComponent<RenderSystem>();
                if (renderSystem != null)
                {
                    renderSystem.SetupDebugMode(enableThrustDebug, enableVelocityDebug);
                }
            }
        }
    }
}
