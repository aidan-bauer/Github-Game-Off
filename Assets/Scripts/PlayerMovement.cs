using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour {

    public Transform cameraRef;
    public Text gearText, speedText, rpmText, resetText;
    [Tooltip("Vehicle center of mass. 0 is the ground")]
    [SerializeField] Vector3 center = Vector3.zero;
    Vector3 resetPos;
    [HideInInspector] public Vector3 ResetPosition
    {
        set
        {
            resetPos = value;
        }
    }

    //controls
    public KeyCode[] controls = new KeyCode[] {
        KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D
    };
    public KeyCode handBreak = KeyCode.Space;
    public KeyCode[] shifters = new KeyCode[] {
        KeyCode.UpArrow, KeyCode.DownArrow
    };
    public KeyCode reset = KeyCode.R;

    public AxelInfo[] axels;
    public GearInfo[] gears = new GearInfo[] {
        new GearInfo(2.833f, "R"),    //reverse
        new GearInfo(3f, "1"),
        new GearInfo(2.125f, "2"),
        new GearInfo(1.611f, "3"),
        new GearInfo(1.316f, "4"),
        new GearInfo(1.118f, "5"),
        new GearInfo(0.96f, "6")
    };

    public int currentGear = 1;
    public float engineTorque = 500f;
    public float maxEngineRPM = 3000f;
    public float minEngineRPM = 1000f;
    float engineRPM = 0f;
    int gas = 0;
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
    [SerializeField] ParticleSystem trails;
    [SerializeField] Gradient trailColor;

    bool gasPress, brakePress/*, steerPress*/;
    bool shifting;
    float turnAngle;
    
    Rigidbody rigid;
    ParticleSystem.EmissionModule newEmission;
    ParticleSystem.MainModule newMain;
    ParticleSystem.EmissionModule[] tireDustEmissions;
    ParticleSystem.MainModule[] tireDustMains;

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

        tireDustEmissions = new ParticleSystem.EmissionModule[axels.Length * 2];
        tireDustMains = new ParticleSystem.MainModule[axels.Length * 2];

        newEmission = trails.emission;
        newEmission.rateOverTime = 0;
        tireDustEmissions[0] = axels[0].leftDust.emission;
        tireDustEmissions[1] = axels[0].rightDust.emission;
        tireDustEmissions[2] = axels[1].leftDust.emission;
        tireDustEmissions[3] = axels[1].rightDust.emission;
        //newEmission.rateOverTime = 0;

        newMain = trails.main;
        newMain.startSpeed = 0;
        tireDustMains[0] = axels[0].leftDust.main;
        tireDustMains[1] = axels[0].rightDust.main;
        tireDustMains[2] = axels[1].leftDust.main;
        tireDustMains[3] = axels[1].rightDust.main;
    }

    // Use this for initialization
    void Start () {
        rigid = GetComponent<Rigidbody>();
        rigid.centerOfMass = center;

        Camera.main.transform.position = cameraRef.position;
        Camera.main.transform.rotation = cameraRef.rotation;

        foreach (AxelInfo axel in axels)
        {
            axel.leftWheel.brakeTorque = brakeForce;
            axel.rightWheel.brakeTorque = brakeForce;
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (!PauseManager.IsPaused)
        {
            //controls
            if (Input.GetKey(controls[0]))
            {
                gasPress = true;
                brakePress = false;
                //trails.Emit(30);
                //trails.Play();
            }
            else if (Input.GetKey(controls[2]))
            {
                gasPress = false;
                brakePress = true;
                //trails.Stop();
            }
            else
            {
                brakePress = false;
                gasPress = false;
                //trails.Stop();
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

            if (Input.GetKeyDown(reset))
            {
                ResetCar();
            }

            //detect if the car is about to be on its roof
            if (transform.rotation.eulerAngles.z >= 110 && transform.rotation.eulerAngles.z <= 250)
            {
                resetText.gameObject.SetActive(true);
                //resetText.enabled = true;
            } else
            {
                resetText.gameObject.SetActive(false);
                //resetText.enabled = false;
            }

            //trails particle stystem;
            if(Input.GetKeyDown(controls[0]))
            {
                trails.Play();

                /*foreach (AxelInfo axel in axels)
                {
                    axel.leftDust.Play();
                    axel.rightDust.Play();
                }*/
            }

            if (Input.GetKeyUp(controls[0]))
            {
                trails.Stop();

                /*foreach (AxelInfo axel in axels)
                {
                    axel.leftDust.Stop();
                    axel.rightDust.Stop();
                }*/
            }

            if (gasPress /*&& currentGear > 1*/)
            {
                //taillight trails
                int particleRate = (int)rigid.velocity.magnitude * currentGear;
                particleRate = Mathf.Clamp(particleRate, 0, 250);
                newMain.startColor = trailColor.Evaluate(Mathf.Clamp01(rigid.velocity.magnitude * 2.237f / 90f));
                newEmission.rateOverTime = particleRate;
                //newMain.startSpeed = -rigid.velocity.magnitude * 2.237f * 0.5f;
                //newMain.startSpeed = -rigid.velocity.magnitude;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!PauseManager.IsPaused)
        {
            //get rpm from back tires, as 99% of the time they'll be providing the power
            engineRPM = (axels[1].leftWheel.rpm + axels[1].rightWheel.rpm) / 2f * gears[currentGear].GearRatio;

            foreach (AxelInfo axel in axels)
            {
                //apply key presses to accel/decel
                if (axel.power)
                {
                    if (gasPress)
                    {
                        //power
                        axel.leftWheel.brakeTorque = 0f;
                        axel.rightWheel.brakeTorque = 0f;

                        gas = 1;

                        //tire dust particles
                        for (int i = 0; i < tireDustEmissions.Length;  i++)
                        {
                            tireDustEmissions[i].rateOverTime = engineRPM / 10f;
                        }
                    }
                    else
                    {
                        axel.leftWheel.brakeTorque = 0f;
                        axel.rightWheel.brakeTorque = 0f;

                        axel.leftWheel.motorTorque = 0f;
                        axel.rightWheel.motorTorque = 0f;
                        gas = 0;
                    }

                    axel.leftWheel.motorTorque = engineTorque / gears[currentGear].GearRatio * gas;
                    axel.rightWheel.motorTorque = engineTorque / gears[currentGear].GearRatio * gas;
                } else
                {
                    axel.leftWheel.brakeTorque = 0f;
                    axel.rightWheel.brakeTorque = 0f;
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

                if (brakePress)
                {
                    //if we're slow enough, start backing up
                    if (engineRPM <= 10f)
                    {
                        //Debug.Log("reversing");
                        gas = -1;
                        ChangeGear(0);

                        axel.leftWheel.motorTorque = engineTorque / gears[currentGear].GearRatio * gas;
                        axel.rightWheel.motorTorque = engineTorque / gears[currentGear].GearRatio * gas;
                    } else
                    {
                        axel.leftWheel.brakeTorque = brakeForce;
                        axel.rightWheel.brakeTorque = brakeForce;
                    }
                }

                //determine what gear we should be in
                AutoShift();

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

                //tire dust particles
                if (gasPress)
                {
                    //axel.leftDust.rate
                }
            }

            speedText.text = (rigid.velocity.magnitude * 2.237f).ToString();
            rpmText.text = Mathf.Abs(engineRPM).ToString("000");
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

    void AutoShift()
    {
        //Checks to make sure what gear the car should be in. Loops through all gears, finds one that will fall within the max or min RPM,
        //then changes to that gear.
        if (!shifting)
        {
            if (engineRPM >= maxEngineRPM)
            {
                Debug.Log("changing gear");
                int appropriateGear = currentGear;

                for (int i = 1; i < gears.Length; i++)
                {
                    if (axels[1].leftWheel.rpm * gears[i].GearRatio < maxEngineRPM)
                    {
                        Debug.Log("upshift");
                        appropriateGear = i;
                        StartCoroutine(ChangeGear(appropriateGear));
                        break;
                    }
                }

                //StartCoroutine(ChangeGear(appropriateGear));
                //ChangeGear(appropriateGear);
            }

            if (engineRPM <= minEngineRPM)
            {
                int appropriateGear = currentGear;

                for (int i = 1; i < gears.Length; i++)
                {
                    if (axels[1].leftWheel.rpm * gears[i].GearRatio > minEngineRPM)
                    {
                        appropriateGear = i;
                        StartCoroutine(ChangeGear(appropriateGear));
                        break;
                    }
                }

                //StartCoroutine(ChangeGear(appropriateGear));
                //ChangeGear(appropriateGear);
            }
        }
    }

    /*void ChangeGear(int gear)
    {
        currentGear = gear;
        gearText.text = gears[currentGear].GearName;
    }*/

    IEnumerator ChangeGear(int gear)
    {
        shifting = true;
        yield return new WaitForSeconds(1f);
        currentGear = gear;
        gearText.text = gears[currentGear].GearName;
        shifting = false;
    }

    public void ResetCar()
    {
        transform.Translate(0, 5, 0, Space.World);
        //transform.position = resetPos + Vector3.up * 3f;
        Quaternion resetRot = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
        transform.rotation = resetRot;
        rigid.velocity = Vector3.zero;
    }
}


[System.Serializable]
public class GearInfo
{
    [SerializeField] float gearRatio;
    [SerializeField] string gearName;

    public float GearRatio
    {
        get
        {
            return gearRatio;
        }
    }

    public string GearName
    {
        get
        {
            return gearName;
        }
    }

    public GearInfo(float _gearRatio, string _gearName)
    {
        gearRatio = _gearRatio;
        gearName = _gearName;
    }
}