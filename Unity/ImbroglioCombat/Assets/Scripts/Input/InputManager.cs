using UnityEngine;
using ImbroglioCombat.Core;
using ImbroglioCombat.Units;

namespace ImbroglioCombat.Input
{
    public class InputManager : MonoBehaviour
    {
        [Header("Camera")]
        public Camera mainCamera;
        
        [Header("Touch Settings")]
        public float dragThreshold = 10f;
        public float doubleTapTime = 0.3f;
        
        [Header("Camera Movement")]
        public float panSpeed = 0.5f;
        public float zoomSpeed = 0.5f;
        public float minZoom = 2f;
        public float maxZoom = 10f;

        private Vector3 lastTouchPosition;
        private bool isDragging = false;
        private float lastTapTime = 0f;
        private HexTile lastTappedTile;

        void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        void Update()
        {
            HandleInput();
        }

        void HandleInput()
        {
            // Handle both touch and mouse input for testing
            #if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseInput();
            #else
            HandleTouchInput();
            #endif
        }

        void HandleMouseInput()
        {
            // Left click for selection and interaction
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = UnityEngine.Input.mousePosition;
                HandleTap(mousePos);
            }

            // Right click to deselect
            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.DeselectUnit();
                }
            }

            // Mouse drag for camera pan
            if (UnityEngine.Input.GetMouseButton(2) || (UnityEngine.Input.GetMouseButton(0) && UnityEngine.Input.GetKey(KeyCode.LeftShift)))
            {
                HandleCameraDrag(UnityEngine.Input.mousePosition);
            }

            // Mouse wheel for zoom
            float scroll = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                HandleZoom(scroll);
            }
        }

        void HandleTouchInput()
        {
            if (UnityEngine.Input.touchCount == 1)
            {
                Touch touch = UnityEngine.Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        lastTouchPosition = touch.position;
                        isDragging = false;
                        break;

                    case TouchPhase.Moved:
                        float distance = Vector3.Distance(touch.position, lastTouchPosition);
                        if (distance > dragThreshold)
                        {
                            isDragging = true;
                            HandleCameraDrag(touch.position);
                        }
                        lastTouchPosition = touch.position;
                        break;

                    case TouchPhase.Ended:
                        if (!isDragging)
                        {
                            HandleTap(touch.position);
                        }
                        isDragging = false;
                        break;
                }
            }
            else if (UnityEngine.Input.touchCount == 2)
            {
                // Pinch to zoom
                HandlePinchZoom();
            }
        }

        void HandleTap(Vector3 screenPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                // Check if we hit a unit
                Unit unit = hit.collider.GetComponent<Unit>();
                if (unit != null)
                {
                    HandleUnitTapped(unit);
                    return;
                }

                // Check if we hit a tile
                HexTile tile = hit.collider.GetComponent<HexTile>();
                if (tile != null)
                {
                    HandleTileTapped(tile);
                    return;
                }
            }

            // If nothing hit, deselect
            if (GameManager.Instance != null && GameManager.Instance.currentState == GameState.PlayerTurn)
            {
                GameManager.Instance.DeselectUnit();
            }
        }

        void HandleUnitTapped(Unit unit)
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            if (unit.isPlayerUnit)
            {
                // Select player unit
                GameManager.Instance.SelectUnit(unit);
            }
            else
            {
                // Try to attack enemy unit
                if (GameManager.Instance.selectedUnit != null && GameManager.Instance.selectedUnit.isPlayerUnit)
                {
                    GameManager.Instance.TryAttackUnit(GameManager.Instance.selectedUnit, unit);
                }
            }
        }

        void HandleTileTapped(HexTile tile)
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            // Check for double tap
            float currentTime = Time.time;
            if (tile == lastTappedTile && (currentTime - lastTapTime) < doubleTapTime)
            {
                HandleDoubleTap(tile);
                lastTappedTile = null;
                lastTapTime = 0f;
                return;
            }

            lastTappedTile = tile;
            lastTapTime = currentTime;

            // If a unit is selected, try to move it
            if (GameManager.Instance.selectedUnit != null)
            {
                GameManager.Instance.TryMoveUnit(tile);
            }
        }

        void HandleDoubleTap(HexTile tile)
        {
            Debug.Log($"Double tapped tile at {tile.gridPosition}");
            // Implement special actions on double tap if needed
        }

        void HandleCameraDrag(Vector3 currentPosition)
        {
            if (mainCamera == null)
            {
                return;
            }

            Vector3 difference = mainCamera.ScreenToWorldPoint(lastTouchPosition) - mainCamera.ScreenToWorldPoint(currentPosition);
            mainCamera.transform.position += difference * panSpeed;
            
            lastTouchPosition = currentPosition;
        }

        void HandleZoom(float zoomAmount)
        {
            if (mainCamera == null)
            {
                return;
            }

            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize -= zoomAmount * zoomSpeed;
                mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minZoom, maxZoom);
            }
            else
            {
                mainCamera.fieldOfView -= zoomAmount * zoomSpeed * 10f;
                mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView, 30f, 90f);
            }
        }

        void HandlePinchZoom()
        {
            Touch touch0 = UnityEngine.Input.GetTouch(0);
            Touch touch1 = UnityEngine.Input.GetTouch(1);

            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;

            HandleZoom(difference * 0.01f);
        }

        // Helper method to check if touch is over UI
        bool IsTouchOverUI(Vector2 touchPosition)
        {
            return UnityEngine.EventSystems.EventSystem.current != null && 
                   UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
    }
}

