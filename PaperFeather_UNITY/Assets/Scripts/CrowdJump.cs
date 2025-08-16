using UnityEngine;

public class CrowdJump : MonoBehaviour
{
    [Header("Jump Settings")]
    public float minJumpForce = 3f;
    public float maxJumpForce = 6f;
    public float minJumpInterval = 0.5f;
    public float maxJumpInterval = 1.5f;
    public float extraGravity = 2f;

    private Rigidbody[] crowdMembers;
    private float[] nextJumpTimes;

    void Start()
    {
        crowdMembers = GetComponentsInChildren<Rigidbody>();
        nextJumpTimes = new float[crowdMembers.Length];

        for (int i = 0; i < crowdMembers.Length; i++)
        {
            nextJumpTimes[i] = Time.time + Random.Range(minJumpInterval, maxJumpInterval);
        }
    }

    void Update()
    {
        for (int i = 0; i < crowdMembers.Length; i++)
        {
            if (Time.time >= nextJumpTimes[i])
            {
                Jump(crowdMembers[i]);
                nextJumpTimes[i] = Time.time + Random.Range(minJumpInterval, maxJumpInterval);
            }

            crowdMembers[i].AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }
    }

    void Jump(Rigidbody rb)
    {
        if (rb == null) return;

        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;
        rb.linearVelocity = vel;

        float jumpForce = Random.Range(minJumpForce, maxJumpForce);
        rb.linearVelocity += Vector3.up * jumpForce;
    }
}