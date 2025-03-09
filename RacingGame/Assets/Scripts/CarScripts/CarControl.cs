using Alteruna;
using UnityEngine;

public class CarControl : CommunicationBridge
{
    [Header("Car Properties")]
    public float motorTorque = 2000f;
    public float brakeTorque = 2000f;
    public float passiveSlowDown = 100f;
    public float maxSpeed = 20f;
    public float steeringRange = 30f;
    public float steeringRangeAtMaxSpeed = 10f;
    public float centreOfGravityOffset = -1f;
    public float nitroBoost = 10f;
    public float boostMaxSpeed = 50f;
    private float currentMaxSpeed = 20f;

    private WheelControl[] wheels;
    private Rigidbody rigidBody;

    private Alteruna.Avatar avatar;

    private bool holdingShift = false;
    // Start is called before the first frame update
    void Start()
    {
        avatar = GetComponent<Alteruna.Avatar>();
        if (!avatar.IsMe)
            return;

        rigidBody = GetComponent<Rigidbody>();
        //if (!Camera.IHaveARigidBody)
        //{
        //    GameObject.FindWithTag("MainCamera").GetComponent<Camera>().carRigidBody = rigidBody;
        //    Camera.IHaveARigidBody = true;
        //}
        // Adjust center of mass to improve stability and prevent rolling
        Vector3 centerOfMass = rigidBody.centerOfMass;
        centerOfMass.y += centreOfGravityOffset;
        rigidBody.centerOfMass = centerOfMass;

        // Get all wheel components attached to the car
        wheels = GetComponentsInChildren<WheelControl>();
    }
    private void Update()
    {
        holdingShift = Input.GetKey(KeyCode.LeftShift);
    }
    // FixedUpdate is called at a fixed time interval 
    void FixedUpdate()
    {
        float boost = 1;
        currentMaxSpeed = maxSpeed;
        if (holdingShift)
        {
            boost = nitroBoost;
            currentMaxSpeed = boostMaxSpeed;
        }

        if (!avatar.IsMe)
            return;
        // Get player input for acceleration and steering
        float vInput = Input.GetAxis("Vertical"); // Forward/backward input
        float hInput = Input.GetAxis("Horizontal"); // Steering input

        // Calculate current speed along the car's forward axis
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        float speedFactor = Mathf.InverseLerp(0, currentMaxSpeed, Mathf.Abs(forwardSpeed)); // Normalized speed factor

        // Reduce motor torque and steering at high speeds for better handling
        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        // Determine if the player is accelerating or trying to reverse
        bool isAccelerating = false;
        bool isBreaking = false;
        bool isPassive = false;

        if (vInput != 0)
        {
            isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);
            isBreaking = Mathf.Sign(vInput) != Mathf.Sign(forwardSpeed);
        }

        else isPassive = true;

        foreach (var wheel in wheels)
        {
            // Apply steering to wheels that support steering
            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
            }

            if (isAccelerating)
            {
                // Apply torque to motorized wheels
                if (wheel.motorized)
                {
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorque * boost;
                }
                // Release brakes when accelerating
                wheel.WheelCollider.brakeTorque = 0f;
            }
            else if (isBreaking)
            {
                // Apply brakes when reversing direction
                wheel.WheelCollider.motorTorque = 0f;
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
            }
            else if (isPassive)
            {
                wheel.WheelCollider.brakeTorque = Mathf.Sign(forwardSpeed) * passiveSlowDown;
            }
        }

        holdingShift = false;
    }
}