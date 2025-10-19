using UnityEngine;

namespace ImbroglioCombat.Utils
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        public float panSpeed = 10f;
        public float zoomSpeed = 5f;
        public float minZoom = 2f;
        public float maxZoom = 10f;

        [Header("Boundaries")]
        public bool useBoundaries = true;
        public Vector2 minBounds = new Vector2(-20, -20);
        public Vector2 maxBounds = new Vector2(20, 20);

        [Header("Smoothing")]
        public bool useSmoothMovement = true;
        public float smoothTime = 0.3f;

        private Camera cam;
        private Vector3 targetPosition;
        private float targetZoom;
        private Vector3 velocity = Vector3.zero;

        void Start()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = Camera.main;
            }

            targetPosition = transform.position;
            targetZoom = cam.orthographicSize;
        }

        void Update()
        {
            HandleCameraMovement();
        }

        void HandleCameraMovement()
        {
            // Apply smooth movement
            if (useSmoothMovement)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);
            }
            else
            {
                transform.position = targetPosition;
                cam.orthographicSize = targetZoom;
            }
        }

        public void MoveCamera(Vector3 delta)
        {
            targetPosition += delta * panSpeed * Time.deltaTime;

            if (useBoundaries)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }
        }

        public void ZoomCamera(float delta)
        {
            targetZoom -= delta * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        public void FocusOnPosition(Vector3 position)
        {
            targetPosition = new Vector3(position.x, position.y, transform.position.z);
        }

        public void SetZoom(float zoom)
        {
            targetZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        }

        public void ResetCamera()
        {
            targetPosition = new Vector3(0, 0, -10);
            targetZoom = 5f;
        }
    }
}

