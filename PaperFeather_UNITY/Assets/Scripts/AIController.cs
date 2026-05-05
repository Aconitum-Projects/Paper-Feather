using UnityEngine;
using System.Collections;

public class AIController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 6f;
    public float crouchDuration = 0.8f;

    [Header("Lane System")]
    public int laneIndex = 0;
    public float laneWidth = 2.5f;
    private float targetLaneX;

    [Header("Obstacle Detection")]
    public float detectionDistance = 3f;
    public LayerMask obstacleLayer;
    public float jumpThreshold = 1.2f;
    public float crouchHeight = 1.5f;
    public float dodgeDistance = 1.5f;

    [Header("Physics")]
    public float lateralAcceleration = 8f;
    public float maxLateralSpeed = 4f;

    [Header("Debug")]
    public bool showDebugVisuals = true;
    public bool showLaneMarkers = true;
    public bool showDetectionZones = true;

    // State machine
    private enum AIState { Moving, Jumping, Crouching, SlidingLateral, Dodging }
    private AIState currentState = AIState.Moving;

    // Components
    private Rigidbody rb;
    private Vector3 moveDirection;

    // State tracking
    private float stateTimer = 0f;
    private float lateralVelocity = 0f;
    private float blockedTimer = 0f;
    private const float BLOCKED_THRESHOLD = 0.3f;
    private float lastForwardZ = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        moveDirection = transform.forward;
        
        // Set lane position based on laneIndex
        targetLaneX = laneIndex * laneWidth;
        transform.position = new Vector3(targetLaneX, transform.position.y, transform.position.z);
    }

    void Update()
    {
        UpdateState();
        AnalyzeObstacles();
        MaintainLane();
        DrawDebugInfo();
    }

    void FixedUpdate()
    {
        // Always move forward, regardless of state
        MoveForward();
        ApplyMovement();
    }

    void UpdateState()
    {
        switch (currentState)
        {
            case AIState.Moving:
                // Moving is handled by constant forward movement in FixedUpdate
                break;
            case AIState.Jumping:
                UpdateJump();
                break;
            case AIState.Crouching:
                UpdateCrouch();
                break;
            case AIState.SlidingLateral:
                UpdateLateralSlide();
                break;
        }
    }

    void AnalyzeObstacles()
    {
        // Check if AI is blocked (not moving forward)
        float forwardProgress = rb.position.z - lastForwardZ;
        lastForwardZ = rb.position.z;

        if (Mathf.Abs(forwardProgress) < 0.01f)
            blockedTimer += Time.deltaTime;
        else
            blockedTimer = 0f;

        bool isBlocked = blockedTimer > BLOCKED_THRESHOLD;

        // Only analyze obstacles if moving OR blocked (trying to escape)
        if (currentState != AIState.Moving && !isBlocked) return;

        // Origins
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 highRayOrigin = transform.position + Vector3.up * crouchHeight;
        Vector3 leftRayOrigin = transform.position + Vector3.up * 1f;

        RaycastHit frontHitInfo;
        bool frontHit = Physics.Raycast(rayOrigin, transform.forward, out frontHitInfo, detectionDistance, obstacleLayer);
        float obstacleHeight = frontHit ? frontHitInfo.collider.bounds.size.y : 0f;

        // Ceiling
        RaycastHit ceilingHitInfo;
        bool ceilingHit = Physics.Raycast(highRayOrigin, transform.forward, out ceilingHitInfo, detectionDistance, obstacleLayer);

        // Side checks
        Vector3 leftDir = (transform.forward + transform.right * 0.5f).normalized;
        Vector3 rightDir = (transform.forward - transform.right * 0.5f).normalized;
        bool leftHit = Physics.Raycast(leftRayOrigin, leftDir, detectionDistance * 1.2f, obstacleLayer);
        bool rightHit = Physics.Raycast(leftRayOrigin, rightDir, detectionDistance * 1.2f, obstacleLayer);
        bool leftClear = !leftHit;
        bool rightClear = !rightHit;

        // Decision priority: Jump > Slide/Dodge > Crouch
        if (frontHit && obstacleHeight > jumpThreshold)
        {
            TryJump();
            return;
        }

        // Prefer sidestep/dodge before crouch
        if (rightClear && !leftClear)
        {
            TryDodge(1); // dodge right
            return;
        }
        if (leftClear && !rightClear)
        {
            TryDodge(-1); // dodge left
            return;
        }

        // If blocked, be more aggressive: try any available side, otherwise random
        if (isBlocked)
        {
            if (leftClear)
                TryDodge(-1);
            else if (rightClear)
                TryDodge(1);
            else
            {
                if (Random.Range(0, 2) == 0) TryDodge(-1); else TryDodge(1);
            }
            return;
        }

        // Only consider crouch if not dodging or jumping
        if (ceilingHit)
        {
            TryCrouch();
            return;
        }

        // Debug visualization
        Debug.DrawRay(rayOrigin, transform.forward * detectionDistance, Color.red);
        Debug.DrawRay(highRayOrigin, transform.forward * detectionDistance, Color.green);
        Debug.DrawRay(leftRayOrigin, leftDir * detectionDistance * 1.2f, Color.yellow);
        Debug.DrawRay(leftRayOrigin, rightDir * detectionDistance * 1.2f, Color.cyan);
    }

    void TryJump()
    {
        if (currentState == AIState.Moving || currentState == AIState.Crouching)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
            currentState = AIState.Jumping;
            blockedTimer = 0f;
        }
    }

    void UpdateJump()
    {
        // Jump ends on ground collision (handled in OnCollisionEnter)
    }

    void TryCrouch()
    {
        if (currentState == AIState.Moving)
        {
            StartCoroutine(CrouchCoroutine());
            blockedTimer = 0f;
        }
    }

    IEnumerator CrouchCoroutine()
    {
        currentState = AIState.Crouching;
        Vector3 originalScale = transform.localScale;
        Vector3 crouchedScale = new Vector3(originalScale.x, originalScale.y * 0.5f, originalScale.z);

        transform.localScale = crouchedScale;
        stateTimer = crouchDuration;

        while (stateTimer > 0)
        {
            stateTimer -= Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        currentState = AIState.Moving;
        blockedTimer = 0f;
    }

    void UpdateCrouch()
    {
        // Handled by coroutine
    }

    void TryDodge(int direction)
    {
        // Can dodge from Moving or if stuck in other states
        if (currentState == AIState.Moving || currentState == AIState.Jumping || blockedTimer > BLOCKED_THRESHOLD)
        {
            // Adjust target lane position for dodge
            targetLaneX += direction * dodgeDistance * 0.5f;
            currentState = AIState.SlidingLateral;
            stateTimer = 0.4f;  // Duration of dodge
            blockedTimer = 0f;
        }
    }

    void UpdateLateralSlide()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            // Return to original lane
            targetLaneX = laneIndex * laneWidth;
            currentState = AIState.Moving;
        }
    }

    void MoveForward()
    {
        rb.MovePosition(rb.position + transform.forward * moveSpeed * Time.deltaTime);
    }

    void MaintainLane()
    {
        float distanceToLane = targetLaneX - rb.position.x;
        float targetVelocity = Mathf.Clamp(distanceToLane * lateralAcceleration, -maxLateralSpeed, maxLateralSpeed);
        lateralVelocity = Mathf.Lerp(lateralVelocity, targetVelocity, Time.deltaTime * 6f);
    }

    void ApplyMovement()
    {
        rb.linearVelocity = new Vector3(lateralVelocity, rb.linearVelocity.y, rb.linearVelocity.z);
    }

    void DrawDebugInfo()
    {
        if (!showDebugVisuals) return;

        // Draw lane markers
        if (showLaneMarkers)
        {
            Vector3 lanePos = new Vector3(targetLaneX, transform.position.y, transform.position.z);
            Vector3 currentPos = new Vector3(rb.position.x, transform.position.y, transform.position.z);
            
            // Draw target lane line (white)
            Debug.DrawLine(lanePos + Vector3.left * laneWidth * 0.5f, lanePos + Vector3.right * laneWidth * 0.5f, Color.white, 0f, false);
            
            // Draw current position (yellow if not aligned, green if aligned)
            Color posColor = Mathf.Abs(rb.position.x - targetLaneX) < 0.2f ? Color.green : Color.yellow;
            Debug.DrawLine(currentPos - Vector3.forward * 0.5f, currentPos + Vector3.forward * 0.5f, posColor, 0f, false);
            
            // Draw lane boundaries (cyan)
            float laneLeft = laneIndex * laneWidth - laneWidth * 0.5f;
            float laneRight = laneIndex * laneWidth + laneWidth * 0.5f;
            Vector3 leftBound = new Vector3(laneLeft, transform.position.y, transform.position.z);
            Vector3 rightBound = new Vector3(laneRight, transform.position.y, transform.position.z);
            Debug.DrawLine(leftBound - Vector3.forward * 1f, leftBound + Vector3.forward * 1f, Color.cyan, 0f, false);
            Debug.DrawLine(rightBound - Vector3.forward * 1f, rightBound + Vector3.forward * 1f, Color.cyan, 0f, false);
        }

        // Draw detection zones
        if (showDetectionZones)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
            Vector3 highRayOrigin = transform.position + Vector3.up * crouchHeight;
            Vector3 leftRayOrigin = transform.position + Vector3.up * 1f;

            // Forward detection box (red - jump area)
            Debug.DrawLine(rayOrigin, rayOrigin + transform.forward * detectionDistance, Color.red, 0f, false);
            Debug.DrawLine(rayOrigin + Vector3.left * 0.3f, rayOrigin + transform.forward * detectionDistance + Vector3.left * 0.3f, Color.red, 0f, false);
            Debug.DrawLine(rayOrigin + Vector3.right * 0.3f, rayOrigin + transform.forward * detectionDistance + Vector3.right * 0.3f, Color.red, 0f, false);

            // Ceiling detection (green - crouch area)
            Debug.DrawLine(highRayOrigin, highRayOrigin + transform.forward * detectionDistance, Color.green, 0f, false);
            Debug.DrawLine(highRayOrigin + Vector3.left * 0.3f, highRayOrigin + transform.forward * detectionDistance + Vector3.left * 0.3f, Color.green, 0f, false);
            Debug.DrawLine(highRayOrigin + Vector3.right * 0.3f, highRayOrigin + transform.forward * detectionDistance + Vector3.right * 0.3f, Color.green, 0f, false);

            // Side detection (yellow/cyan - dodge area)
            Vector3 leftDir = (transform.forward + transform.right * 0.5f).normalized;
            Vector3 rightDir = (transform.forward - transform.right * 0.5f).normalized;
            Debug.DrawLine(leftRayOrigin, leftRayOrigin + leftDir * detectionDistance * 1.2f, Color.yellow, 0f, false);
            Debug.DrawLine(leftRayOrigin, leftRayOrigin + rightDir * detectionDistance * 1.2f, Color.cyan, 0f, false);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugVisuals) return;

        // Draw lane width and index info
        float laneLeft = laneIndex * laneWidth - laneWidth * 0.5f;
        float laneRight = laneIndex * laneWidth + laneWidth * 0.5f;
        Vector3 laneStart = new Vector3(laneIndex * laneWidth, transform.position.y + 0.1f, transform.position.z - 5f);
        Vector3 laneEnd = new Vector3(laneIndex * laneWidth, transform.position.y + 0.1f, transform.position.z + 5f);

        // Draw lane center (magenta)
        Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
        Gizmos.DrawLine(laneStart, laneEnd);

        // Draw lane boundaries (cyan)
        Vector3 leftStart = new Vector3(laneLeft, transform.position.y + 0.1f, transform.position.z - 5f);
        Vector3 leftEnd = new Vector3(laneLeft, transform.position.y + 0.1f, transform.position.z + 5f);
        Vector3 rightStart = new Vector3(laneRight, transform.position.y + 0.1f, transform.position.z - 5f);
        Vector3 rightEnd = new Vector3(laneRight, transform.position.y + 0.1f, transform.position.z + 5f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(leftStart, leftEnd);
        Gizmos.DrawLine(rightStart, rightEnd);

        // Draw lane width box
        Vector3 boxCenter = new Vector3(laneIndex * laneWidth, transform.position.y + 0.5f, transform.position.z);
        Vector3 boxSize = new Vector3(laneWidth, 2f, 10f);
        Gizmos.color = new Color(0f, 1f, 1f, 0.1f);
        Gizmos.DrawCube(boxCenter, boxSize);
    }

    void OnDrawGizmos()
    {
        // Always draw lane markers in editor (even when not selected)
        float laneLeft = laneIndex * laneWidth - laneWidth * 0.5f;
        float laneRight = laneIndex * laneWidth + laneWidth * 0.5f;

        // Draw simple lane lines (always visible)
        Vector3 laneCenter = new Vector3(laneIndex * laneWidth, transform.position.y, transform.position.z);
        Vector3 leftBound = new Vector3(laneLeft, transform.position.y, transform.position.z);
        Vector3 rightBound = new Vector3(laneRight, transform.position.y, transform.position.z);

        // Draw lane center (thin magenta line)
        Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
        Gizmos.DrawLine(laneCenter - Vector3.forward * 10f, laneCenter + Vector3.forward * 10f);

        // Draw lane boundaries (thick cyan lines)
        Gizmos.color = Color.cyan;
        for (int i = 0; i < 20; i++)
        {
            float z1 = transform.position.z - 10f + (i * 1f);
            float z2 = z1 + 0.8f;
            Gizmos.DrawLine(new Vector3(laneLeft, transform.position.y, z1), new Vector3(laneLeft, transform.position.y, z2));
            Gizmos.DrawLine(new Vector3(laneRight, transform.position.y, z1), new Vector3(laneRight, transform.position.y, z2));
        }

        // Draw AI position
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Vector3.one * 0.3f);
    }

    void OnGUI()
    {
        if (!showDebugVisuals) return;

        GUI.color = Color.white;
        GUILayout.BeginArea(new Rect(10, 10, 350, 250));

        GUILayout.Label($"<color=yellow><b>AI DEBUG INFO</b></color>", new GUIStyle(GUI.skin.label) { richText = true });
        GUILayout.Label($"Lane Index: {laneIndex}");
        GUILayout.Label($"Current X: {rb.position.x:F2}");
        GUILayout.Label($"Target Lane X: {targetLaneX:F2}");
        GUILayout.Label($"Lane Width: {laneWidth:F2}");
        GUILayout.Label($"Lateral Velocity: {lateralVelocity:F2}");

        string stateColor = currentState switch
        {
            AIState.Moving => "green",
            AIState.Jumping => "yellow",
            AIState.Crouching => "blue",
            AIState.SlidingLateral => "magenta",
            AIState.Dodging => "red",
            _ => "white"
        };

        GUILayout.Label($"State: <color={stateColor}><b>{currentState}</b></color>", new GUIStyle(GUI.skin.label) { richText = true });
        
        if (currentState == AIState.SlidingLateral || currentState == AIState.Crouching)
        {
            GUILayout.Label($"State Timer: {stateTimer:F2}s");
        }

        // Show blocked status
        string blockedColor = blockedTimer > BLOCKED_THRESHOLD ? "red" : "green";
        GUILayout.Label($"Blocked Timer: <color={blockedColor}>{blockedTimer:F2}s / {BLOCKED_THRESHOLD}s</color>", new GUIStyle(GUI.skin.label) { richText = true });

        GUILayout.EndArea();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Ground contact resets jump state
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.7f)
        {
            if (currentState == AIState.Jumping)
            {
                currentState = AIState.Moving;
            }
        }
    }
}
