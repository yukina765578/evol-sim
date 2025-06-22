using UnityEngine;
using UnityEngine.InputSystem;

namespace Environment.Visualization
{
    public class CameraController : MonoBehaviour
    {
        [Header("Zoom Settings")]
        public float zoomSpeed = 100f;
        public float minZoom = 5f;
        public float maxZoom = 50f;

        private Camera cam;
        private Vector3 dragOrigin;
        
        void Start()
        {
            cam = GetComponent<Camera>();
            if (!cam.orthographic)
            {
                cam.orthographic = true;
            }
            cam.orthographicSize = 100f; // Set initial zoom level
        }
        
        void Update()
        {
            HandleZoom();
            HandlePanning();
            HandleReset();
        }
        
        void HandleZoom()
        {
            // Check if mouse exists
            if (Mouse.current == null) return;
            
            Vector2 scroll = Mouse.current.scroll.ReadValue();
            
            if (Mathf.Abs(scroll.y) > 0.1f) // Add threshold
            {
                float zoomDirection = scroll.y > 0 ? -1 : 1; // Invert if needed
                cam.orthographicSize += zoomDirection * zoomSpeed * Time.deltaTime * 10f;
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
            }
        }
        
        void HandlePanning()
        {
            if (Mouse.current == null) return;
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Vector3 mousePosition = Mouse.current.position.ReadValue();
                mousePosition.z = cam.nearClipPlane;
                dragOrigin = cam.ScreenToWorldPoint(mousePosition);
            }

            if (Mouse.current.rightButton.isPressed)
            {
                Vector3 mousePosition = Mouse.current.position.ReadValue();
                mousePosition.z = cam.nearClipPlane;
                Vector3 currentMouseWorld = cam.ScreenToWorldPoint(mousePosition);

                Vector3 difference = dragOrigin - currentMouseWorld;
                transform.position += difference;
            }

        }
        
        void HandleReset()
        {
            if (Keyboard.current == null) return;
            
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                transform.position = new Vector3(0, 0, -10);
                cam.orthographicSize = 50f;
            }
        }
    }
}

