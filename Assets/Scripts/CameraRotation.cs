using UnityEngine;

public class CameraRotation : MonoBehaviour {

    public bool invertY;
    [Range(0.1f, 5f)]
    public float sensitivity = 1f;
    public float minVerticalRot = -10;
    public float maxVerticalRot = 90;
    Vector3 cameraRot;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!PauseManager.IsPaused)
        {
            if (cameraRot.x < minVerticalRot)
            {
                cameraRot.x = minVerticalRot;
            } else if (cameraRot.x > maxVerticalRot)
            {
                cameraRot.x = maxVerticalRot;
            } else
            {
                if (invertY)
                {
                    cameraRot.x += Input.GetAxis("Mouse Y");
                } else
                {
                    cameraRot.x -= Input.GetAxis("Mouse Y");
                }
            }

            cameraRot.y += Input.GetAxis("Mouse X");
            cameraRot.z = 0;
            cameraRot *= sensitivity;
        }
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(cameraRot);
    }
}
