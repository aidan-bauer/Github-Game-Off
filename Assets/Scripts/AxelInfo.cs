using UnityEngine;

[System.Serializable]
public class AxelInfo
{
    public WheelCollider leftWheel, rightWheel;
    public ParticleSystem leftDust, rightDust;
    public bool steering;
    public bool power;

    public AxelInfo(WheelCollider _leftWheel, WheelCollider _rightWheel, ParticleSystem _leftDust, ParticleSystem _rightDust, bool _steering, bool _power)
    {
        leftWheel = _leftWheel;
        rightWheel = _rightWheel;
        leftDust = _leftDust;
        rightDust = _rightDust;
        steering = _steering;
        power = _power;
        //leftDust = _dust.GetComponentsInChildren<ParticleSystem>()[0];
        //rightDust = _dust.GetComponentsInChildren<ParticleSystem>()[1];
    }
}