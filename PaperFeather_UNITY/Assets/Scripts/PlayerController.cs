using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Déplacement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;

    [Header("Saut")]
    public float jumpForce = 5f;
    public int maxJumps = 2;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Physique Saut")]
    [SerializeField] private float riseMultiplier = 1.5f;
    [SerializeField] private float fallMultiplier = 2.5f;

    private Rigidbody rb;
    private int jumpCount;
    private bool canInteract = false;

    private float rotationInput;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        rotationInput = Input.GetAxisRaw("Horizontal");
        moveInput = Input.GetAxisRaw("Vertical");

        transform.Rotate(Vector3.up, rotationInput * rotationSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;
            jumpCount++;
        }

        if (canInteract && Input.GetKeyDown(interactKey))
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
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InteractiveTrigger"))
        {
            canInteract = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteractiveTrigger"))
        {
            canInteract = false;
        }
    }
}