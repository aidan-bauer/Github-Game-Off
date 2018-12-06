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
    [Tooltip("Sets whether the offset will be applied with the parent object's rotation.")]
    [SerializeField] private bool absoluteOffset;

    Vector3 pos, rot;

    private void Awake()
    {
        rot = Vector3.zero;
    }

    // Update is called once per frame
    void LateUpdate () {
        if (!PauseManager.IsPaused)
        {
            pos = target.position + offset;
            rot = target.rotation.eulerAngles + rotationOffset;

            if (maintainHeight)
                pos.y = offset.y;

            if (rotateY)
            {
                rot.x = 90;
                rot.z = 0;
                if (absoluteOffset)
                {
                    transform.rotation = Quaternion.Euler(rot);

                } else
                {
                    transform.RotateAround(target.position, Vector3.up, 10f * Time.deltaTime);
                }
            }

            if (matchRotation)
            {
                transform.rotation = Quaternion.Euler(rot);
            }

            if (absoluteOffset)
            {
                transform.position = pos;
            } else
            {
                transform.localPosition = pos;
            }
        }
    }
}
