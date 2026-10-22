using UnityEngine;
using UnityEngine.InputSystem;

namespace TheFrightBeforeChristmas
{
    public class HandController : MonoBehaviour
    {
        [Header("Target & Movement")]
        [Tooltip("The body/pivot transform around which the hand rotates. Defaults to parent.")]
        [SerializeField] private Transform pivot;

        [Tooltip("Distance from the pivot to maintain. Set to 0 to keep initial distance.")]
        [SerializeField] private float distance = 0f;

        [Tooltip("Smoothing speed for following the mouse. 0 means instant.")]
        [SerializeField] private float smoothSpeed = 0f;

        [Header("Rotation")]
        [Tooltip("Whether the hand rotates to face the mouse position.")]
        [SerializeField] private bool rotateTowardsMouse = true;

        [Tooltip("Angular offset in degrees added to the rotation.")]
        [SerializeField] private float rotationOffset = 0f;

        private Camera _targetCamera;
        private Camera TargetCamera => _targetCamera != null ? _targetCamera : (_targetCamera = Camera.main);

        private void Awake()
        {
            if (pivot == null && transform.parent != null)
            {
                pivot = transform.parent;
            }

            if (distance <= 0f && pivot != null)
            {
                Vector3 offset = transform.position - pivot.position;
                distance = new Vector2(offset.x, offset.y).magnitude;
                if (distance <= 0f)
                {
                    distance = 1f;
                }
            }
        }

        private void Update()
        {
            FollowMouse();
        }

        private void FollowMouse()
        {
            if (Mouse.current == null || TargetCamera == null) return;

            Vector3 pivotPosition = pivot != null ? pivot.position : Vector3.zero;

            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

            // Project mouse position onto the XY plane at the pivot's Z depth
            Ray ray = TargetCamera.ScreenPointToRay(mouseScreenPos);
            Plane xyPlane = new Plane(Vector3.forward, new Vector3(0f, 0f, pivotPosition.z));

            if (!xyPlane.Raycast(ray, out float enter)) return;

            Vector3 mouseWorldPos = ray.GetPoint(enter);
            Vector2 direction = new Vector2(mouseWorldPos.x - pivotPosition.x, mouseWorldPos.y - pivotPosition.y);

            if (direction.sqrMagnitude < 0.0001f) return;

            direction.Normalize();

            // Calculate target position on the XY plane around the pivot
            Vector3 targetPosition = new Vector3(
                pivotPosition.x + direction.x * distance,
                pivotPosition.y + direction.y * distance,
                transform.position.z
            );

            if (smoothSpeed > 0f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = targetPosition;
            }

            if (rotateTowardsMouse)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + rotationOffset;
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

                if (smoothSpeed > 0f)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
                }
                else
                {
                    transform.rotation = targetRotation;
                }
            }
        }
    }
}