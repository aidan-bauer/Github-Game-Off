using UnityEngine;

public class Checkpoint : MonoBehaviour {

    Generator gen;

    private void Awake()
    {
        gen = FindObjectOfType<Generator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement playerMovement = GetComponent<PlayerMovement>();
            playerMovement.ResetPosition = transform.position + Vector3.up;
            playerMovement.ResetRotation = transform.rotation;
            gen.currentWaypoint++;
            GetComponentInChildren<WaypointUI>().waypoint = gen.Waypoints[gen.currentWaypoint];
        }
    }
}
