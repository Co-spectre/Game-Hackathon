using UnityEngine;
using System.Collections;

public class RoamingAnimal : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float changeDirectionDelay = 3f;
    
    private Vector3 targetDirection;
    private float stateTimer;
    private bool isMoving;

    void Start()
    {
        PickNewDirection();
    }

    void Update()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0)
        {
            if (isMoving)
            {
                // Stop and wait
                isMoving = false;
                stateTimer = Random.Range(1f, 4f);
            }
            else
            {
                // Pick a new direction and move
                PickNewDirection();
                isMoving = true;
                stateTimer = Random.Range(2f, 5f);
            }
        }

        if (isMoving)
        {
            // Simple obstacle avoidance
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, transform.forward, 1f))
            {
                PickNewDirection(); // Turn if facing a wall/tree
            }
            
            // Move forward
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
    }

    void PickNewDirection()
    {
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        targetDirection = new Vector3(Mathf.Sin(randomAngle), 0f, Mathf.Cos(randomAngle)).normalized;
        transform.rotation = Quaternion.LookRotation(targetDirection);
    }
}
