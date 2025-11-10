using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Activation des actions")]
    public bool canMove = true;
    public bool canRotate = true;
    public bool canJump = true;
    public bool canDoInteraction = true;
    public bool canCrouch = true;
    
    [Space(20)]
    [Header("Déplacement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;

    [Space(20)]
    [Header("Saut")]
    public float jumpForce = 5f;
    public int maxJumps = 2;

    [Space(20)]
    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    [Space(20)]
    [Header("Physique Saut")]
    [SerializeField] private float riseMultiplier = 1.5f;
    [SerializeField] private float fallMultiplier = 2.5f;

    [Space(20)]
    [Header("Crouch (accroupi)")]
    public KeyCode crouchKey = KeyCode.LeftShift;
    public float crouchScaleY = 0.5f;
    private Vector3 originalScale;
    private bool isCrouching = false;

    private Rigidbody rb;
    private Animator anim;
    private int jumpCount;
    private bool isInInteractZone = false;

    private float rotationInput;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        anim = GetComponent<Animator>();

        originalScale = transform.localScale;
    }

    void Update()
    {
        if (canRotate)
        {
            float mouseX = Input.GetAxis("Mouse X");
            transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime);
        }

        moveInput = canMove ? Input.GetAxisRaw("Vertical") : 0f;
        rotationInput = canMove ? Input.GetAxisRaw("Horizontal") : 0f;

        if (canJump && Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            float appliedJumpForce = jumpForce;
            if (jumpCount == 1) appliedJumpForce *= 0.7f;
            Vector3 vel = rb.linearVelocity;
            vel.y = appliedJumpForce;
            rb.linearVelocity = vel;
            jumpCount++;
            anim.SetBool("isJumping", true);
        }

        if (canDoInteraction && isInInteractZone && Input.GetKeyDown(interactKey))
            Debug.Log("Interaction déclenchée !");

        if (canCrouch)
        {
            if (Input.GetKeyDown(crouchKey)) Crouch();
            else if (Input.GetKeyUp(crouchKey)) StandUp();
        }
    }

    void FixedUpdate()
    {
        Vector3 move = transform.forward * moveInput + transform.right * rotationInput;
        rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);

        if (rb.linearVelocity.y > 0)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (riseMultiplier - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts[0].normal.y > 0.5f)
        {
            jumpCount = 0;
            anim.SetBool("isJumping", false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InteractiveTrigger"))
        {
            isInInteractZone = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractiveTrigger"))
        {
            isInInteractZone = false;
        }
    }
    void Crouch()
    {
        if (isCrouching) return;
        Vector3 newScale = new Vector3(originalScale.x, crouchScaleY, originalScale.z);
        transform.localScale = newScale;
        isCrouching = true;

        if (anim != null) anim.SetBool("isCrouching", true);
    }

    void StandUp()
    {
        if (!isCrouching) return;
        transform.localScale = originalScale;
        isCrouching = false;

        if (anim != null) anim.SetBool("isCrouching", false);
    }
}