using UnityEngine;

public class BuildingInterior : MonoBehaviour
{
    public GameObject roofObject;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (roofObject != null)
            {
                roofObject.SetActive(false); // Hide roof when player enters
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (roofObject != null)
            {
                roofObject.SetActive(true); // Show roof when player leaves
            }
        }
    }
}
