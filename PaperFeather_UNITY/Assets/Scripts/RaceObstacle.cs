using UnityEngine;

public class RaceObstacle : MonoBehaviour
{
    public enum ObstacleType
    {
        Mud,            // Slows player when walked on
        Swinging,       // Moves back and forth like a pendulum
        OrganisedStatic,// Generates grouped static cubes
        Static          // Simple static obstacle
    }

    [Header("Obstacle Type")]
    public ObstacleType type;

    [Header("Mud Settings")]
    [Tooltip("Multiplier applied to player speed when on mud.")]
    public float slowMultiplier = 0.5f;
    [Tooltip("Duration in seconds the speed reduction lasts.")]
    public float slowDuration = 1f;

    [Header("Swinging Obstacle Settings")]
    [Tooltip("Starting position of the swinging obstacle.")]
    public Vector3 swingStartPos;
    [Tooltip("Ending position of the swinging obstacle.")]
    public Vector3 swingEndPos;
    [Tooltip("Movement speed of the swinging obstacle.")]
    public float swingSpeed = 2f;
    private bool movingToEnd = true;

    [Header("Organised Static Cubes Settings")]
    [Tooltip("Prefab used to generate cubes.")]
    public GameObject cubePrefab;
    [Tooltip("Minimum number of cubes to generate.")]
    public int minCubes = 5;
    [Tooltip("Maximum number of cubes to generate.")]
    public int maxCubes = 15;
    [Tooltip("Chance that cubes are grouped together.")]
    public float groupChance = 0.5f;
    [Tooltip("Spacing between cubes in a group.")]
    public float groupSpacing = 1.2f;

    private void Start()
    {
        if (type == ObstacleType.Swinging)
        {
            transform.position = swingStartPos;
            transform.position += Vector3.right;
        }
        else if (type == ObstacleType.OrganisedStatic)
        {
            GenerateOrganisedCubes();
        }
    }

    private void Update()
    {
        if (type == ObstacleType.Swinging)
        {
            MoveSwingingObstacle();
        }
    }

    private void MoveSwingingObstacle()
    {
        Vector3 target = movingToEnd ? swingEndPos : swingStartPos;
        transform.position = Vector3.MoveTowards(transform.position, target, swingSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
            movingToEnd = !movingToEnd;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (type)
            {
                case ObstacleType.Mud:
                    PlayerController player = other.GetComponent<PlayerController>();
                    if (player != null)
                        player.StartCoroutine(SlowPlayer(player));
                    break;
            }
        }
    }

    private System.Collections.IEnumerator SlowPlayer(PlayerController player)
    {
        float originalSpeed = player.moveSpeed;
        player.moveSpeed *= slowMultiplier;
        yield return new WaitForSeconds(slowDuration);
        player.moveSpeed = originalSpeed;
    }

    private void GenerateOrganisedCubes()
    {
        if (cubePrefab == null)
        {
            Debug.LogWarning("Cube prefab not assigned for OrganisedStatic!");
            return;
        }

        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            Debug.LogWarning("OrganisedStatic requires a BoxCollider!");
            return;
        }

        Vector3 size = box.size;
        Vector3 center = box.center;

        int maxAttempts = 100;
        int cubesPlaced = 0;
        int totalCubes = Random.Range(minCubes, maxCubes + 1);

        while (cubesPlaced < totalCubes && maxAttempts > 0)
        {
            maxAttempts--;

            float x = Random.Range(-size.x / 2f, size.x / 2f);
            float z = Random.Range(-size.z / 2f, size.z / 2f);

            Vector3 basePos = transform.position + center + new Vector3(x, 0f, z);

            int stackHeight = 1;
            if (Random.value < groupChance)
            {
                stackHeight = Random.Range(2, 5);
            }

            bool canPlace = true;
            foreach (Transform child in transform)
            {
                if (Vector3.Distance(new Vector3(child.position.x, 0f, child.position.z), new Vector3(basePos.x, 0f, basePos.z)) < 1f)
                {
                    canPlace = false;
                    break;
                }
            }

            if (!canPlace) continue;

            for (int h = 0; h < stackHeight; h++)
            {
                Vector3 cubePos = basePos + new Vector3(0f, h * 1f, 0f);
                Instantiate(cubePrefab, cubePos, Quaternion.identity, transform);
            }

            cubesPlaced++;
        }
    }

    private Vector3 GetRandomPositionInZone()
    {
        Vector3 halfScale = transform.localScale / 2f;
        Vector3 randomPos = new Vector3(
            Random.Range(-halfScale.x, halfScale.x),
            Random.Range(-halfScale.z, halfScale.z)
        );
        return transform.position + randomPos;
    }
}
