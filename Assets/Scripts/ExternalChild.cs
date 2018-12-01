using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalChild : MonoBehaviour {

    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private bool matchRotation;
    
    [SerializeField] private bool rotateY;
    [SerializeField] private bool maintainHeight;

    Vector3 pos, rot;

    private void Awake()
    {
        rot = Vector3.zero;
    }

    // Update is called once per frame
    void LateUpdate () {
        pos = target.position + offset;
        rot = target.rotation.eulerAngles + rotationOffset;

        if (maintainHeight)
            pos.y = offset.y;

        if (rotateY)
        {
            rot.x = 90;
            rot.z = 0;
            //transform.rotation = Quaternion.Euler(rot);
        }

        if (matchRotation)
            transform.rotation = Quaternion.Euler(rot);

        transform.position = pos;
    }
}
