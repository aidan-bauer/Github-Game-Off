using UnityEngine;

public class Checkpoint : MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = GetComponent<PlayerMovement>();
            playerMovement.ResetPosition = transform.position + Vector3.up;
        }
    }
}
