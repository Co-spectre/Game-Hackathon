using UnityEngine;

public class SwayingObject : MonoBehaviour
{
    public float swaySpeed = 2f;
    public float swayAmount = 5f;
    public Vector3 swayAxis = Vector3.right;

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.rotation;
        swaySpeed += Random.Range(-0.5f, 0.5f); // Randomize per object so they don't look uniform
    }

    void Update()
    {
        // Simple sine wave rotation over time to simulate intense wind 
        float angle = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        transform.rotation = startRotation * Quaternion.Euler(swayAxis * angle);
    }
}
