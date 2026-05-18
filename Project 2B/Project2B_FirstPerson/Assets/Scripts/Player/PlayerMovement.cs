using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // movement settings
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;
    public float mouseSensitivity = 200f;

    // refs
    private CharacterController controller;
    private Transform cam;

    // state
    private float verticalVelocity;
    private float pitch;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleMove();
    }

    void HandleLook()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mx);

        pitch -= my;
        pitch = Mathf.Clamp(pitch, -85f, 85f);
        cam.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    void HandleMove()
    {
        // WASD input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 dir = transform.right * x + transform.forward * z;

        // sprint when holding shift
        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -1f;

        // jump
        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            verticalVelocity = Mathf.Sqrt(-2f * gravity * jumpHeight);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = dir * speed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }
}