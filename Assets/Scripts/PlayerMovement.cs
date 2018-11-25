using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public Transform cameraRef;

    public KeyCode[] controls = new KeyCode[] {
        KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D
    };
    public KeyCode handBreak = KeyCode.Space;
    public KeyCode[] shifters = new KeyCode[] {
        KeyCode.UpArrow, KeyCode.DownArrow
    };

    public AxelInfo[] axels;
    public GearInfo[] gears = new GearInfo[] {
        new GearInfo(-5f, -20f),    //reverse
        new GearInfo(15f, 50f),
        new GearInfo(25f, 100f)
    };
    public int currentGear = 1;
    public float gasForce = 50f;
    public float brakeForce = 75f;
    public float maxSteerAngle = 20f;
    public int turnRate = 2;
    public float antiRoll = 10f;

    [Header("Wheel Values")]
    public float wheelMass = 20f;
    public float suspensionSpring = 1500f;
    public float suspensionSpringDamper = 2500f;
    [Range(0f, 1f)]
    public float targetPosition = 0.5f;
    [Header("Forward Wheel Friction")]
    [Range(0f, 1f)]
    public float forwardExtremumSlip = 0.4f;
    [Range(0f, 1f)]
    public float forwardExtremumValue = 1f;
    [Range(0f, 1f)]
    public float forwardAsymptoteSlip = 0.8f;
    [Range(0f, 1f)]
    public float forwardAsymptoteValue = 0.5f;
    [Range(0f, 1f)]
    public float forwardStiffness = 1f;
    [Header("Sideways Wheel Friction")]
    [Range(0f, 1f)]
    public float sidewaysExtremumSlip = 0.2f;
    [Range(0f, 1f)]
    public float sidewaysExtremumValue = 1f;
    [Range(0f, 1f)]
    public float sidewaysAsymptoteSlip = 0.5f;
    [Range(0f, 1f)]
    public float sidewaysAsymptoteValue = 0.75f;
    [Range(0f, 1f)]
    public float sidewaysStiffness = 1f;

    bool gasPress, brakePress/*, steerPress*/;
    float turnAngle;
    
    Rigidbody rigid;

    private void Awake()
    {
        JointSpring suspension = new JointSpring();
        suspension.spring = suspensionSpring;
        suspension.damper = suspensionSpringDamper;
        suspension.targetPosition = targetPosition;

        WheelFrictionCurve forward = new WheelFrictionCurve();
        forward.extremumSlip = forwardExtremumSlip;
        forward.extremumValue = forwardExtremumValue;
        forward.asymptoteSlip = forwardAsymptoteSlip;
        forward.asymptoteValue = forwardAsymptoteValue;
        forward.stiffness = forwardStiffness;

        WheelFrictionCurve sideways = new WheelFrictionCurve();
        sideways.extremumSlip = sidewaysExtremumSlip;
        sideways.extremumValue = sidewaysExtremumValue;
        sideways.asymptoteSlip = sidewaysAsymptoteSlip;
        sideways.asymptoteValue = sidewaysAsymptoteValue;
        sideways.stiffness = sidewaysStiffness;

        foreach (AxelInfo axel in axels)
        {
            axel.leftWheel.mass = wheelMass;
            axel.leftWheel.suspensionSpring = suspension;
            axel.leftWheel.forwardFriction = forward;
            axel.leftWheel.sidewaysFriction = sideways;

            axel.rightWheel.mass = wheelMass;
            axel.rightWheel.suspensionSpring = suspension;
            axel.rightWheel.forwardFriction = forward;
            axel.rightWheel.sidewaysFriction = sideways;
        }
    }

    // Use this for initialization
    void Start () {
        rigid = GetComponent<Rigidbody>();
        rigid.centerOfMass.Set(0, -1, 0);

        Camera.main.transform.position = cameraRef.position;
        Camera.main.transform.rotation = cameraRef.rotation;
    }
	
	// Update is called once per frame
	void Update () {
        //controls
        if (Input.GetKey(controls[0]))
        {
            gasPress = true;
            brakePress = false;
        }
        else if (Input.GetKey(controls[2]))
        {
            gasPress = false;
            brakePress = true;
        }
        else
        {
            brakePress = false;
            gasPress = false;
        }

        if (Input.GetKey(controls[1]))
        {
            turnAngle = -turnRate;
            //steerPress = true;
        }
        else if (Input.GetKey(controls[3]))
        {
            turnAngle = turnRate;
            //steerPress = true;
        }
        else
        {
            turnAngle = 0;
            //steerPress = false;
        }

        if (Input.GetKeyDown(shifters[0]))
        {
            UpShift();
        }

        if (Input.GetKeyDown(shifters[1]))
        {
            DownShift();
        }
    }

    private void FixedUpdate()
    {
        foreach (AxelInfo axel in axels)
        {
            //apply key presses to accel/decel
            if (axel.power)
            {
                if (gasPress)
                {
                    axel.leftWheel.brakeTorque = 0f;
                    axel.rightWheel.brakeTorque = 0f;

                    //power
                    if (axel.leftWheel.motorTorque <= gears[currentGear].TopSpeed)
                    {
                        Debug.Log("accelerating!");
                        axel.leftWheel.motorTorque += gears[currentGear].Acceleration * Time.fixedDeltaTime;
                        axel.rightWheel.motorTorque += gears[currentGear].Acceleration * Time.fixedDeltaTime;
                    }

                    axel.leftWheel.motorTorque = Mathf.Clamp(axel.leftWheel.motorTorque, 0f, gears[currentGear].TopSpeed);
                    axel.rightWheel.motorTorque = Mathf.Clamp(axel.rightWheel.motorTorque, 0f, gears[currentGear].TopSpeed);
                }
                else if (brakePress)
                {
                    axel.leftWheel.motorTorque = 0f;
                    axel.rightWheel.motorTorque = 0f;

                    axel.leftWheel.brakeTorque = brakeForce;
                    axel.rightWheel.brakeTorque = brakeForce;
                }
                else
                {
                    axel.leftWheel.brakeTorque = 0f;
                    axel.rightWheel.brakeTorque = 0f;

                    axel.leftWheel.motorTorque = 0f;
                    axel.rightWheel.motorTorque = 0f;
                }
            }

            //apply steering controls info
            if (axel.steering)
            {
                /*if (axel.leftWheel.steerAngle <= maxSteerAngle)
                {
                    axel.leftWheel.steerAngle += turnRate * Time.fixedDeltaTime;
                    axel.rightWheel.steerAngle += turnRate * Time.fixedDeltaTime;
                }

                axel.leftWheel.steerAngle = Mathf.Clamp(axel.leftWheel.steerAngle, 0f, maxSteerAngle);
                axel.rightWheel.steerAngle = Mathf.Clamp(axel.rightWheel.steerAngle, 0f, maxSteerAngle);*/

                //if steer key pressed turn the wheel
                //when key is released return wheel to neutral position

                axel.leftWheel.steerAngle = maxSteerAngle * turnAngle;
                axel.rightWheel.steerAngle = maxSteerAngle * turnAngle;
            }

            //anti-roll bars
            //assigning variables here so each axel get its own values
            WheelHit leftHit, rightHit;
            float travelLeft, travelRight, antiRollForce;

            bool groundedLeft = axel.leftWheel.GetGroundHit(out leftHit);
            bool groundedRight = axel.rightWheel.GetGroundHit(out rightHit);

            if (groundedLeft)
                travelLeft = (-axel.leftWheel.transform.InverseTransformPoint(leftHit.point).y - axel.leftWheel.radius) / axel.leftWheel.suspensionDistance;
            else
                travelLeft = 1.0f;

            if (groundedRight)
                travelRight = (-axel.rightWheel.transform.InverseTransformPoint(rightHit.point).y - axel.rightWheel.radius) / axel.rightWheel.suspensionDistance;
            else
                travelRight = 1.0f;

            antiRollForce = (travelLeft - travelRight) * antiRoll;

            if (groundedLeft)
                rigid.AddForceAtPosition(Vector3.up * -antiRollForce, axel.leftWheel.transform.position);
            if (groundedRight)
                rigid.AddForceAtPosition(Vector3.up * antiRollForce, axel.rightWheel.transform.position);

            updateVisualWheels(axel.leftWheel);
            updateVisualWheels(axel.rightWheel);
        }
    }

    void updateVisualWheels(WheelCollider collider)
    {
        //if collider has no children exit the function
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    void UpShift()
    {
        if (currentGear < gears.Length - 1)
            currentGear++;
    }

    void DownShift()
    {
        if (currentGear > 0)
            currentGear--;
    }

    //update wheels at start with editor information
    void UpdateWheels()
    {

    }
}


[System.Serializable]
public class GearInfo
{
    [SerializeField] float acceleration;
    [SerializeField] float topSpeed;

    public float TopSpeed {
        get {
            return topSpeed;
        }
    }

    public float Acceleration
    {
        get
        {
            return acceleration;
        }
    }

    public GearInfo(float _acceleration, float _topSpeed)
    {
        acceleration = _acceleration;
        topSpeed = _topSpeed;
    }
}