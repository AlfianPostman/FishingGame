using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerControllerOld : MonoBehaviour
{
    // Character Setting
    [Header("Character Setting")]
    [Space(10)]
    public float baseSpeed = 3f;
    public float runSpeed = 5f;
    public float turnSmoothTime;
    public float dashForce = 1000f;
    public float dashCooldown = 2f;
    float turnSmoothVelocity;
    float moveSpeed = 0f;
    bool canDash = true;
    bool isDashing = false;
    
    // Hotkey
    bool dashButton;
    bool restartButton;
    [HideInInspector] public bool throwButton;
    [HideInInspector] public bool attackButton;
    [HideInInspector] public bool pickUpButton;
    [HideInInspector] public bool jumpButton;

    [HideInInspector] public bool ableToMove = false;
    Vector2 input;

    [Space(20)]
    [Header("References")]
    [Space(10)]
    public Transform cam;

    [Space(20)]
    [Header("Debug Mode")]
    [Space(10)]
    [SerializeField]
    bool DebugMode = false;

    Rigidbody rb;
    CameraTarget ct;
    // [HideInInspector] public Animator anim;

    void Start()
    {
        // anim = GetComponentInChildren<Animator>();

        rb = GetComponent<Rigidbody>();
        ct = GetComponent<CameraTarget>();
    }

    void FixedUpdate()
    {
        MyInputs();

        Movement();
    }

    void MyInputs() 
    {
        if (DebugMode) DebugModeInput();

        if (ableToMove)
        {
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1);
            dashButton = Input.GetKey(KeyCode.LeftShift);
            attackButton = Input.GetKey(KeyCode.Mouse0);
            pickUpButton = Input.GetKey(KeyCode.E);
            throwButton = Input.GetKey(KeyCode.Q);
            jumpButton = Input.GetKey(KeyCode.Space);
            restartButton = Input.GetKey(KeyCode.Escape);

            if(restartButton) SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            // anim.SetFloat("speed", input.magnitude);

            if (attackButton) {
                // wm.Attack();
            }
        }
    }

    void DebugModeInput()
    {
        // do debug only 
    }

    void Movement() 
    {
        moveSpeed = runSpeed;
        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;

        Vector3 forward = transform.TransformDirection(Vector3.forward) * 10;
        Debug.DrawRay(transform.position, forward, Color.green);

        // Normalize the camera angle
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward = camForward.normalized;
        camRight = camRight.normalized;

        // Controlling the rotation based on camera angle
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        // Camera target offset
        if(input.x != 0f || input.y != 0f)
        {
            //ct.Moving(true);
            // anim.SetBool("isMoving", true);
        }
        else
        {
            //ct.Moving(false);
            // anim.SetBool("isMoving", false);
        }

        if (dashButton && canDash && !isDashing) 
        {
            StartCoroutine(Dash());
        }

        if(jumpButton)
        {
            rb.AddForce(Vector3.up * 100f, ForceMode.Impulse);
        }

        // Start Moving
        if (ableToMove)
        {
            Vector3 yVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            rb.linearVelocity = input.x * camRight * moveSpeed + input.y * camForward * moveSpeed + yVelocity;
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        // Get the dash direction (forward or based on input)
        Vector3 dashDirection = transform.forward;

        // Apply the dash force
        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);

        // Wait for the dash duration
        yield return new WaitForSeconds(.2f);

        // Stop the dash (optional, depending on how you want to control the motion)
        rb.linearVelocity = Vector3.zero;

        isDashing = false;

        // Wait for the cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
