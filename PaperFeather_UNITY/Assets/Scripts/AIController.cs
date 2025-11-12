using UnityEngine;
using System.Collections;

public class AIController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float crouchDuration = 1f;
    public float slideDistance = 2f;
    public float slideSpeed = 3f;

    [Header("Detection")]
    public float rayDistance = 2f;
    public LayerMask obstacleLayer;

    [Tooltip("Minimum height in front to trigger a jump")]
    public float obstacleHeightThreshold = 1.2f;

    [Tooltip("Height from ground to check for low ceilings (for crouching)")]
    public float ceilingCheckHeight = 1.5f;

    private Rigidbody rb;
    private bool isCrouching = false;
    private bool isJumping = false;
    private bool isSliding = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (!isSliding)
        {
            MoveForward();
            DetectObstacleAndAct();
        }
    }

    void MoveForward()
    {
        rb.MovePosition(rb.position + transform.forward * moveSpeed * Time.deltaTime);
    }

    void DetectObstacleAndAct()
    {
        RaycastHit hit;

        // 🔴 Bas → obstacle sur le sol (jump)
        Vector3 lowRayOrigin = transform.position + Vector3.up * 0.3f;
        Debug.DrawRay(lowRayOrigin, transform.forward * rayDistance, Color.red);

        // 🟢 Haut → obstacle suspendu (crouch)
        Vector3 highRayOrigin = transform.position + Vector3.up * ceilingCheckHeight;
        Debug.DrawRay(highRayOrigin, transform.forward * rayDistance, Color.green);

        // 🟣 Gauche et 🟡 Droite → pour vérifier s'il y a un passage libre
        Vector3 leftRayOrigin = transform.position + Vector3.up * 1f;  // hauteur moyenne
        Vector3 rightRayOrigin = transform.position + Vector3.up * 1f;

        Vector3 leftDir = (transform.forward - transform.right).normalized;
        Vector3 rightDir = (transform.forward + transform.right).normalized;

        Debug.DrawRay(leftRayOrigin, leftDir * rayDistance, Color.magenta);
        Debug.DrawRay(rightRayOrigin, rightDir * rayDistance, Color.yellow);

        // Si le rayon du bas touche → obstacle au sol
        if (Physics.Raycast(lowRayOrigin, transform.forward, out hit, rayDistance, obstacleLayer))
        {
            float obstacleHeight = hit.collider.bounds.size.y;

            if (!isJumping && obstacleHeight > obstacleHeightThreshold)
            {
                Jump();
                return;
            }
        }

        // Si le rayon du haut touche → obstacle suspendu
        if (Physics.Raycast(highRayOrigin, transform.forward, out hit, rayDistance, obstacleLayer))
        {
            if (!isCrouching)
            {
                StartCoroutine(Crouch());
                return;
            }
        }

        // Si bloqué → test gauche/droite
        bool leftClear = !Physics.Raycast(leftRayOrigin, leftDir, rayDistance, obstacleLayer);
        bool rightClear = !Physics.Raycast(rightRayOrigin, rightDir, rayDistance, obstacleLayer);

        if (!leftClear && !rightClear)
            return; // coincé de partout

        if (leftClear)
            SlideSide(-1);
        else if (rightClear)
            SlideSide(1);
    }
    
    void SlideSide(int direction)
    {
        // direction = -1 → gauche, 1 → droite
        Vector3 moveDir = Quaternion.Euler(0, 30f * direction, 0) * transform.forward;
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.deltaTime);
    }

    void TrySlideAroundObstacle()
    {
        // 🔹 Ray gauche / droite
        Vector3 leftOrigin = transform.position + Vector3.up * 1f;
        Vector3 rightOrigin = transform.position + Vector3.up * 1f;

        bool leftBlocked = Physics.Raycast(leftOrigin, -transform.right, rayDistance, obstacleLayer);
        bool rightBlocked = Physics.Raycast(rightOrigin, transform.right, rayDistance, obstacleLayer);

        Debug.DrawRay(leftOrigin, -transform.right * rayDistance, Color.cyan);
        Debug.DrawRay(rightOrigin, transform.right * rayDistance, Color.magenta);

        if (!leftBlocked)
        {
            StartCoroutine(SlideToSide(-transform.right));
        }
        else if (!rightBlocked)
        {
            StartCoroutine(SlideToSide(transform.right));
        }
        else
        {
            Debug.Log($"{gameObject.name} : bloqué de partout !");
        }
    }

    IEnumerator SlideToSide(Vector3 direction)
    {
        Debug.Log("Slide " + (direction == transform.right ? "→ droite" : "← gauche"));
        isSliding = true;

        Vector3 start = rb.position;
        Vector3 end = start + direction * slideDistance;
        float elapsed = 0f;

        while (elapsed < slideDistance / slideSpeed)
        {
            rb.MovePosition(Vector3.Lerp(start, end, elapsed * slideSpeed / slideDistance));
            elapsed += Time.deltaTime;
            yield return null;
        }

        isSliding = false;
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
}
