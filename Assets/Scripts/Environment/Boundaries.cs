using UnityEngine;
using UnityEngine.Events;

namespace EvolutionSimulator.Environment
{
    public class Boundaries : MonoBehaviour
    {
        [Header("World Size")]
        [SerializeField]
        private float width = 100f;

        [SerializeField]
        private float height = 100f;

        [Header("Visuals")]
        [SerializeField]
        private bool drawBoundary = true;

        [SerializeField]
        private Color borderColor = Color.white;

        [SerializeField]
        private float borderThickness = 1f;

        public UnityEvent<Bounds> OnBoundsChanged = new UnityEvent<Bounds>();

        private GameObject[] borderObjects = new GameObject[4];

        public Bounds WorldBounds => new Bounds(Vector3.zero, new Vector3(width, height, 0));
        public Vector2 WorldSize => new Vector2(width, height);

        void Start()
        {
            CreateBorders();
            CreateColliders();
            NotifyBoundsChanged();
        }

        void CreateBorders()
        {
            string[] names = { "TopBorder", "BottomBorder", "LeftBorder", "RightBorder" };

            for (int i = 0; i < 4; i++)
            {
                borderObjects[i] = new GameObject(names[i]);
                borderObjects[i].transform.parent = transform;

                MeshFilter meshFilter = borderObjects[i].AddComponent<MeshFilter>();
                MeshRenderer renderer = borderObjects[i].AddComponent<MeshRenderer>();

                meshFilter.mesh = CreateBorderMesh(i);

                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = borderColor;
                renderer.enabled = drawBoundary;
                renderer.sortingOrder = 10;
            }
        }

        Mesh CreateBorderMesh(int borderIndex)
        {
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[4];

            float halfWidth = width / 2;
            float halfHeight = height / 2;

            switch (borderIndex)
            {
                case 0: // Top
                    vertices[0] = new Vector3(-halfWidth, halfHeight - borderThickness, 0);
                    vertices[1] = new Vector3(halfWidth, halfHeight - borderThickness, 0);
                    vertices[2] = new Vector3(halfWidth, halfHeight, 0);
                    vertices[3] = new Vector3(-halfWidth, halfHeight, 0);
                    break;
                case 1: // Bottom
                    vertices[0] = new Vector3(-halfWidth, -halfHeight, 0);
                    vertices[1] = new Vector3(halfWidth, -halfHeight, 0);
                    vertices[2] = new Vector3(halfWidth, -halfHeight + borderThickness, 0);
                    vertices[3] = new Vector3(-halfWidth, -halfHeight + borderThickness, 0);
                    break;
                case 2: // Left
                    vertices[0] = new Vector3(-halfWidth, -halfHeight, 0);
                    vertices[1] = new Vector3(-halfWidth + borderThickness, -halfHeight, 0);
                    vertices[2] = new Vector3(-halfWidth + borderThickness, halfHeight, 0);
                    vertices[3] = new Vector3(-halfWidth, halfHeight, 0);
                    break;
                case 3: // Right
                    vertices[0] = new Vector3(halfWidth - borderThickness, -halfHeight, 0);
                    vertices[1] = new Vector3(halfWidth, -halfHeight, 0);
                    vertices[2] = new Vector3(halfWidth, halfHeight, 0);
                    vertices[3] = new Vector3(halfWidth - borderThickness, halfHeight, 0);
                    break;
            }

            mesh.vertices = vertices;
            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();

            return mesh;
        }

        void CreateColliders()
        {
            // Top
            CreateBorderCollider(new Vector2(0, height / 2), new Vector2(width, borderThickness));
            // Bottom
            CreateBorderCollider(new Vector2(0, -height / 2), new Vector2(width, borderThickness));
            // Left
            CreateBorderCollider(new Vector2(-width / 2, 0), new Vector2(borderThickness, height));
            // Right
            CreateBorderCollider(new Vector2(width / 2, 0), new Vector2(borderThickness, height));
        }

        void CreateBorderCollider(Vector2 position, Vector2 size)
        {
            GameObject border = new GameObject("Collider");
            border.transform.parent = transform;
            border.transform.localPosition = position;

            BoxCollider2D collider = border.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        void NotifyBoundsChanged()
        {
            OnBoundsChanged?.Invoke(WorldBounds);
        }

        public bool IsWithinBounds(Vector3 position)
        {
            return Mathf.Abs(position.x) <= width / 2 && Mathf.Abs(position.y) <= height / 2;
        }

        void OnValidate()
        {
            width = Mathf.Max(width, 10f);
            height = Mathf.Max(height, 10f);
            borderThickness = Mathf.Max(borderThickness, 0.1f);
        }
    }
}
