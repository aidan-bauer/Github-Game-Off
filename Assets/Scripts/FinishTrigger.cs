using UnityEngine;
using UnityEngine.Events;

public class FinishTrigger : MonoBehaviour {

    GameManager manager;

	// Use this for initialization
	void Awake () {
        manager = FindObjectOfType<GameManager>();
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerCollide"))
        {
            manager.Finish();
        }
    }
}
