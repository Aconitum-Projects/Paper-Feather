using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Activation des actions")]
    public bool canMove = true;
    public bool canRotate = true;
    public bool canJump = true;
    public bool canDoInteraction = true;
    
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
    }

    void Update()
    {
        rotationInput = canRotate ? Input.GetAxisRaw("Horizontal") : 0f;
        moveInput = canMove ? Input.GetAxisRaw("Vertical") : 0f;

        if (canRotate)
            transform.Rotate(Vector3.up, rotationInput * rotationSpeed * Time.deltaTime);

        if (canJump && Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            float appliedJumpForce = jumpForce;

            if (jumpCount == 1)
            {
                appliedJumpForce *= 0.7f;
            }

            Vector3 vel = rb.linearVelocity;
            vel.y = appliedJumpForce;
            rb.linearVelocity = vel;

            jumpCount++;

            anim.SetBool("isJumping", true);
        }
        
        if (canDoInteraction && isInInteractZone && Input.GetKeyDown(interactKey))
        {
            Debug.Log("Interaction déclenchée !");
        }
    }

    void FixedUpdate()
    {
        Vector3 move = transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (riseMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
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
}