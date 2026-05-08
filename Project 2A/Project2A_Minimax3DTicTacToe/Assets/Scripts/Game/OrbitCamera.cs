using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The point the camera orbits around. Leave empty to use world origin (0, board center, 0).")]
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 2.25f, 0f);

    [Header("Orbit Settings")]
    public float rotateSpeed = 0.3f;
    public float zoomSpeed = 2f;
    public float minDistance = 5f;
    public float maxDistance = 30f;
    public float minPitch = -10f;  // degrees up
    public float maxPitch = 80f;   // degrees down

    [Header("Pan Settings")]
    public float panSpeed = 0.02f;

    // Internal state
    private float yaw;     
    private float pitch;  
    private float distance;
    private Vector3 panOffset = Vector3.zero;

    void Start()
    {
        // Initialize from current camera transform
        Vector3 toTarget = TargetPoint() - transform.position;
        distance = toTarget.magnitude;
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        UpdateCameraTransform();
    }

    void Update()
    {
        if (Mouse.current == null) return;

        Vector2 delta = Mouse.current.delta.ReadValue();

        // Right mouse button drag to orbit
        if (Mouse.current.rightButton.isPressed)
        {
            yaw += delta.x * rotateSpeed;
            pitch -= delta.y * rotateSpeed;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // Middle mouse button drag to pan
        if (Mouse.current.middleButton.isPressed)
        {
            Vector3 right = transform.right;
            Vector3 up = transform.up;
            panOffset += -right * delta.x * panSpeed - up * delta.y * panSpeed;
        }

        // Scroll wheel for zoom
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            distance -= scroll * zoomSpeed * 0.01f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // Keyboard fallback (R = reset view)
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetView();
        }

        UpdateCameraTransform();
    }

    private Vector3 TargetPoint()
    {
        Vector3 basePoint = (target != null) ? target.position : Vector3.zero;
        return basePoint + targetOffset + panOffset;
    }

    private void UpdateCameraTransform()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = TargetPoint() + offset;
        transform.rotation = rotation;
    }

    private void ResetView()
    {
        yaw = 0f;
        pitch = 20f;
        distance = 12f;
        panOffset = Vector3.zero;
    }
}