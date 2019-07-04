using UnityEngine;

public class WaypointUI : MonoBehaviour {

    [SerializeField] Transform player;
    public Transform waypoint;
    [SerializeField] float angle;
    [SerializeField] float dot;

    [SerializeField] RectTransform rotator;

    CameraManager camManager;

	// Use this for initialization
	void Start () {
        camManager = GetComponentInParent<CameraManager>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 waypointAngle = waypoint.position - player.position;
        waypointAngle.Normalize();
        Vector3 cameraRot = camManager.CarCameras[camManager.CurrentCameraIndex].transform.position + 
            camManager.CarCameras[camManager.CurrentCameraIndex].transform.right;
        cameraRot.Normalize();
        //dot = Vector3.Dot(waypointAngle, player.right);
        dot = Vector3.Dot(waypointAngle, cameraRot);
        angle = Quaternion.Angle(player.rotation, Quaternion.LookRotation(waypointAngle, Vector3.up));
        if (dot > 0)
        {
            angle *= -1;
        }
    }

    private void LateUpdate()
    {
        rotator.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public float CalculateAngle(Vector3 from, Vector3 to)
    {

        return Quaternion.FromToRotation(Vector3.up, to - from).eulerAngles.z;

    }
}
