using UnityEngine;

[System.Serializable]
public class AxelInfo
{
    public WheelCollider leftWheel, rightWheel;
    public bool steering;
    public bool power;

    public AxelInfo(WheelCollider _leftWheel, WheelCollider _rightWheel, bool _steering, bool _power)
    {
        leftWheel = _leftWheel;
        rightWheel = _rightWheel;
        steering = _steering;
        power = _power;
    }
}