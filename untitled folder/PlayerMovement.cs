using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CODMovementFull : MonoBehaviour
{
    [Header("References")]
    public Transform cameraHolder;
    public Camera cam;

    [Header("Movement Speeds")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float crouchSpeed = 2.5f;
    public float slideSpeed = 12f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.5f;
    public float gravity = -25f;

    [Header("Look")]
    public float sensitivity = 2f;
    public float maxLookAngle = 85f;

    [Header("FOV")]
    public float normalFOV = 75f;
    public float sprintFOV = 88f;
    public float fovSmooth = 8f;

    [Header("Slide")]
    public float slideTime = 0.7f;
    public float slideBoost = 1.2f;

    private CharacterController controller;

    private Vector3 moveVelocity;
    private Vector3 verticalVelocity;

    private float xRotation;
    private bool grounded;

    private bool crouching;
    private bool sliding;
    private float slideTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam.fieldOfView = normalFOV;
    }

    void Update()
    {
        Look();
        Move();
        Jump();
        Gravity();
        Crouch();
        Slide();
        FOV();
    }

    // ---------------- LOOK ----------------
    void Look()
    {
        float mx = Input.GetAxis("Mouse X") * sensitivity;
        float my = Input.GetAxis("Mouse Y") * sensitivity;

        xRotation -= my;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mx);
    }

    // ---------------- MOVE ----------------
    void Move()
    {
        grounded = controller.isGrounded;

        float x = 0;
        float z = 0;

        if (InputManager.Instance.GetKey("Forward")) z += 1;
        if (InputManager.Instance.GetKey("Backward")) z -= 1;
        if (InputManager.Instance.GetKey("Right")) x += 1;
        if (InputManager.Instance.GetKey("Left")) x -= 1;

        Vector3 dir = (transform.right * x + transform.forward * z).normalized;

        bool sprint =
            InputManager.Instance.GetKey("Sprint") &&
            z > 0 &&
            !crouching &&
            !sliding;

        float speed =
            sliding ? slideSpeed :
            crouching ? crouchSpeed :
            sprint ? sprintSpeed :
            walkSpeed;

        moveVelocity = Vector3.Lerp(moveVelocity, dir * speed, 10f * Time.deltaTime);

        controller.Move(moveVelocity * Time.deltaTime);
    }

    // ---------------- JUMP ----------------
    void Jump()
    {
        if (InputManager.Instance.GetKeyDown("Jump") && grounded && !crouching && !sliding)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    // ---------------- GRAVITY ----------------
    void Gravity()
    {
        if (grounded && verticalVelocity.y < 0)
            verticalVelocity.y = -2f;

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    // ---------------- CROUCH ----------------
    void Crouch()
    {
        if (sliding) return;

        crouching = InputManager.Instance.GetKey("Crouch");

        float targetHeight = crouching ? 1f : 2f;

        controller.height = Mathf.Lerp(controller.height, targetHeight, 10f * Time.deltaTime);
        controller.center = new Vector3(0, controller.height / 2f, 0);
    }

    // ---------------- SLIDE ----------------
    void Slide()
    {
        if (!grounded) return;

        if (InputManager.Instance.GetKeyDown("Crouch") &&
            InputManager.Instance.GetKey("Sprint") &&
            moveVelocity.magnitude > 6f)
        {
            sliding = true;
            slideTimer = slideTime;
        }

        if (sliding)
        {
            slideTimer -= Time.deltaTime;

            moveVelocity += transform.forward * slideBoost;

            if (slideTimer <= 0)
                sliding = false;
        }
    }

    // ---------------- FOV ----------------
    void FOV()
    {
        bool sprint =
            InputManager.Instance.GetKey("Sprint") &&
            InputManager.Instance.GetKey("Forward");

        float targetFOV =
            sliding ? sprintFOV :
            sprint ? sprintFOV :
            normalFOV;

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, fovSmooth * Time.deltaTime);
    }
}