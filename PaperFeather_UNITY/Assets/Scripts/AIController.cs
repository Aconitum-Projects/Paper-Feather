using UnityEngine;
using System.Collections;

public class AIController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float crouchDuration = 1f;
    public float swaySpeed = 3f;           // vitesse du sway
    public float swayDuration = 1f;        // durée d'un mouvement latéral avant inversion

    [Header("Detection")]
    public float rayDistance = 2f;
    public LayerMask obstacleLayer;
    public float obstacleHeightThreshold = 1.2f;
    public float ceilingCheckHeight = 1.5f;

    private Rigidbody rb;
    private bool isCrouching = false;
    private bool isJumping = false;
    private bool isSwaying = false;
    private int swayDirection = 1;          // 1 = droite, -1 = gauche
    private float swayTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        MoveForward();
        DetectObstacleAndAct();
        HandleSway();
    }

    void MoveForward()
    {
        Vector3 move = transform.forward * moveSpeed * Time.deltaTime;

        // Si sway actif, ajouter mouvement latéral
        if (isSwaying)
            move += transform.right * swayDirection * swaySpeed * Time.deltaTime;

        rb.MovePosition(rb.position + move);
    }

    void DetectObstacleAndAct()
    {
        if (isSwaying) return; // ne pas détecter d'obstacle pendant le sway

        RaycastHit hit;

        Vector3 lowRayOrigin = transform.position + Vector3.up * 0.3f;
        Debug.DrawRay(lowRayOrigin, transform.forward * rayDistance, Color.red);

        Vector3 highRayOrigin = transform.position + Vector3.up * ceilingCheckHeight;
        Debug.DrawRay(highRayOrigin, transform.forward * rayDistance, Color.green);

        bool obstacleDetected = false;

        // Bas → obstacle au sol (jump)
        if (Physics.Raycast(lowRayOrigin, transform.forward, out hit, rayDistance, obstacleLayer))
        {
            float obstacleHeight = hit.collider.bounds.size.y;
            obstacleDetected = true;

            if (!isJumping && obstacleHeight > obstacleHeightThreshold)
            {
                Jump();
                return;
            }
        }

        // Haut → obstacle suspendu (crouch)
        if (Physics.Raycast(highRayOrigin, transform.forward, out hit, rayDistance, obstacleLayer))
        {
            obstacleDetected = true;

            if (!isCrouching)
            {
                StartCoroutine(Crouch());
                return;
            }
        }

        // Si bloqué (raycast détecte obstacle mais saut/crouch pas possible)
        if (obstacleDetected && !isJumping && !isCrouching)
        {
            StartSway();
        }
    }

    void Jump()
    {
        Debug.Log("Jump!");
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        isJumping = true;
    }

    IEnumerator Crouch()
    {
        Debug.Log("Crouch!");
        isCrouching = true;

        Vector3 originalScale = transform.localScale;
        transform.localScale = new Vector3(originalScale.x, originalScale.y * 0.5f, originalScale.z);

        yield return new WaitForSeconds(crouchDuration);

        transform.localScale = originalScale;
        isCrouching = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
            isJumping = false;
    }

    // ===== Sway Logic =====
    void StartSway()
    {
        if (!isSwaying)
        {
            isSwaying = true;
            swayTimer = swayDuration;
            swayDirection = Random.value > 0.5f ? 1 : -1; // gauche ou droite aléatoire
        }
    }

    void HandleSway()
    {
        if (!isSwaying) return;

        swayTimer -= Time.deltaTime;
        if (swayTimer <= 0f)
        {
            swayDirection *= -1;           // inverser direction
            swayTimer = swayDuration;
        }

        // On arrête le sway si le chemin devant est libre
        RaycastHit hit;
        Vector3 lowRayOrigin = transform.position + Vector3.up * 0.3f;
        if (!Physics.Raycast(lowRayOrigin, transform.forward, out hit, rayDistance, obstacleLayer))
        {
            isSwaying = false;
        }
    }
}
