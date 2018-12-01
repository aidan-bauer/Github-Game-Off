using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] KeyCode cameraChangers = KeyCode.C;
    [SerializeField] Camera[] carCameras;
    [Range(1f, 179f)]
    [SerializeField] float fov = 35f;

    int currentCameraIndex = 0;

    // Use this for initialization
    void Start () {
        SetCameraProperties();

        ChangeCamera();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(cameraChangers))
        {
            currentCameraIndex++;
            if (currentCameraIndex>carCameras.Length-1)
            {
                currentCameraIndex = 0;
            }

            ChangeCamera();
        }
	}

    void SetCameraProperties()
    {
        if (carCameras.Length == 0)
        {
            Debug.LogWarning("There are no cameras assigned to the Camera Manager. Please assign a camera to the manager to change the camera settings.");
            return;
        }

        foreach (Camera cam in carCameras)
        {
            cam.fieldOfView = fov;
        }
    }

    void ChangeCamera ()
    {
        if (carCameras.Length == 0)
        {
            Debug.LogWarning("There are no cameras assigned to the Camera Manager. Please assign a camera to the manager to change the camera.");
            return;
        }

        for (int i = 0; i < carCameras.Length; i++)
        {
            if (i == currentCameraIndex)
            {
                carCameras[i].gameObject.SetActive(true);
            } else
            {
                carCameras[i].gameObject.SetActive(false);
            }
        }
    }
}
