using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreatureController : MonoBehaviour
{
    //TODO: get legs into a data structure
    //TODO: force leg side priotrities to alternate
    //TODO: implement support for multiple gaits
    //TODO: implement debug flags to turn certain debug visuals on and off

    // root body variables
    [SerializeField] float distanceFromGround;
    [SerializeField] float desiredDistanceFromGround;

    [SerializeField] float moveSpeed;
    [SerializeField] float moveAcceleration;

    [SerializeField] float turnSpeed;
    [SerializeField] float turnAcceleration;

    private float maxAngleToTarget;
    private float maxDistToTarget;
    private float minDistToTarget;

    Vector3 currentVelocity;
    public Vector3 CurrentVelocity
    {
        get { return currentVelocity; }
    }


    float currentAngularVelocity;
    public float CurrentAngularVelocity
    {
        get { return currentAngularVelocity; }
    }
    public float TurnSpeed
    {
        get { return turnSpeed; }
    }

    [SerializeField] float damper;

    [SerializeField] Transform root;
    [SerializeField] Transform target;

    //leg stepper objects

    [SerializeField] LegStepper[] leftLegs;
    [SerializeField] LegStepper[] rightLegs;

    // to be replaced by arrays above

    //[SerializeField] LegStepper FRL;
    //[SerializeField] LegStepper FLL;

    //[SerializeField] LegStepper MRL;
   // [SerializeField] LegStepper MLL;

   // [SerializeField] LegStepper RRL;
    //[SerializeField] LegStepper RLL;

    //public AnimationCurve sensitivityCurve;


    enum Gait
    {
        Alternate,
        Wave
    }

    CreatureController()
    {
        currentVelocity = new Vector3(0, 0, 0);
        currentAngularVelocity = 0;
    }


    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(LegUpdateCoroutine());
    }

    public float tripodGaitDistance(bool lr)
    {
        float distance = 0;

        if (lr == false)
        {
            for (int i = 0; i < leftLegs.Length; i++)
            {
                if (i % 2 == 0)
                {
                    distance += leftLegs[i].DistanceFromHome;
                }
                else
                {
                    distance += rightLegs[i].DistanceFromHome;
                }
            }
        }
        else
        {
            for (int i = 0; i < leftLegs.Length; i++)
            {
                if (i % 2 == 0)
                {
                    distance += rightLegs[i].DistanceFromHome;
                }
                else
                {
                    distance += leftLegs[i].DistanceFromHome;
                }
            }
        }

        return distance;
    }

    public bool tripodGaitIsMoving(bool lr)
    {
        bool isMoving = false;

        if (lr == false)
        {
            for (int i = 0; i < leftLegs.Length; i++)
            {
                if (i % 2 == 0)
                {
                    if (leftLegs[i].moving)
                        isMoving = true;
                }
                else
                {
                    if (rightLegs[i].moving)
                        isMoving = true;
                }
            }
        }
        else
        {
            for (int i = 0; i < leftLegs.Length; i++)
            {
                if (i % 2 == 0)
                {
                    if (rightLegs[i].moving)
                        isMoving = true;
                }
                else
                {
                    if (leftLegs[i].moving)
                        isMoving = true;
                }
            }
        }

        return isMoving;
    }

    IEnumerator LegUpdateCoroutine()
    {

        while (true)
        {
            if (tripodGaitDistance(true) / leftLegs.Length > tripodGaitDistance(false) / leftLegs.Length)
            {
                do
                {
                    for (int i = 0; i < leftLegs.Length; i++)
                    {
                        if (i % 2 == 0)
                        {
                            rightLegs[i].TryMove();
                        }
                        else
                        {
                            leftLegs[i].TryMove();
                        }
                    }

                    yield return null;
                }
                while (tripodGaitIsMoving(true));
            }
            else
            {
                do
                {
                    for (int i = 0; i < leftLegs.Length; i++)
                    {
                        if (i % 2 == 0)
                        {
                            leftLegs[i].TryMove();
                        }
                        else
                        {
                            rightLegs[i].TryMove();
                        }
                    }

                    yield return null;
                }
                while (tripodGaitIsMoving(false));
            }
        }
    }

    //Based on leg placement, how should the body be oriented?
    public void CalculateOrientation()
    {


    }


    void RootMotionUpdate()
    {
        //targetMovement();

        move();


        // change root height based on leg heights

        orientBodyOffset();

        //this code is evil and WILL release daemons into this dimension. Use at own risk.

        orientBodyLeftRight();

        ////front back rotation

        orientBodyFrontBack();
    }

    public void targetMovement()
    {
        // rotate root to face target
        Vector3 towardTarget = target.position - root.transform.position;
        // Vector toward target on the local XZ plane
        Vector3 towardTargetProjected = Vector3.ProjectOnPlane(towardTarget, transform.up);
        // Get the angle from the gecko's forward direction to the direction toward toward our target
        // Here we get the signed angle around the up vector so we know which direction to turn in
        float angToTarget = Vector3.SignedAngle(root.transform.forward, towardTargetProjected, root.transform.up);

        float targetAngularVelocity = 0;

        // If we are within the max angle (i.e. approximately facing the target)
        // leave the target angular velocity at zero
        if (Mathf.Abs(angToTarget) > maxAngleToTarget)
        {
            // Angles in Unity are clockwise, so a positive angle here means to our right
            if (angToTarget > 0)
            {
                targetAngularVelocity = turnSpeed;
            }
            // Invert angular speed if target is to our left
            else
            {
                targetAngularVelocity = -turnSpeed;
            }
        }

        // Use our smoothing function to gradually change the velocity
        currentAngularVelocity = Mathf.Lerp(
        currentAngularVelocity,
        targetAngularVelocity,
        1 - Mathf.Exp(-turnAcceleration * Time.deltaTime)
      );

        // Rotate the transform around the Y axis in world space, 
        // making sure to multiply by delta time to get a consistent angular velocity
        root.transform.Rotate(0, Time.deltaTime * currentAngularVelocity, 0, Space.World);

        Vector3 targetVelocity = Vector3.zero;

        // Don't move if we're facing away from the target, just rotate in place
        if (Mathf.Abs(angToTarget) < 90)
        {
            float distToTarget = Vector3.Distance(transform.position, target.position);

            // If we're too far away, approach the target
            if (distToTarget > maxDistToTarget)
            {
                targetVelocity = moveSpeed * towardTargetProjected.normalized;
            }
            // If we're too close, reverse the direction and move away
            else if (distToTarget < minDistToTarget)
            {
                targetVelocity = moveSpeed * -towardTargetProjected.normalized;
            }
        }

        currentVelocity = Vector3.Lerp(
          currentVelocity,
          targetVelocity,
          1 - Mathf.Exp(-moveAcceleration * Time.deltaTime)
        );

        // Apply the velocity
        root.transform.position += currentVelocity * Time.deltaTime;
    }

    public void move()
    {
        float targetAngularVelocity = 0;

        // If we are within the max angle (i.e. approximately facing the target)
        // leave the target angular velocity at zero

        // Angles in Unity are clockwise, so a positive angle here means to our right

        if (Input.GetKey("d"))
            targetAngularVelocity = Mathf.Lerp(turnSpeed, turnSpeed*0.6f, Vector3.Magnitude(currentVelocity)); // this lerp inversely slows turn speed relative to speed;

        if (Input.GetKey("a"))
            targetAngularVelocity = Mathf.Lerp(-turnSpeed, -turnSpeed * 0.6f, Vector3.Magnitude(currentVelocity)/speed);



        // Use our smoothing function to gradually change the velocity
        currentAngularVelocity = Mathf.Lerp(
        currentAngularVelocity,
        targetAngularVelocity,
        1 - Mathf.Exp(-turnAcceleration * Time.deltaTime)
        );

        // Rotate the transform around the Y axis in world space, 
        // making sure to multiply by delta time to get a consistent angular velocity
        root.transform.Rotate(0, Time.deltaTime * currentAngularVelocity, 0, Space.World);


        Vector3 targetVelocity = Vector3.zero;

        if (Input.GetKey("w"))
        {
            targetVelocity = moveSpeed * root.transform.forward;
        }
        if (Input.GetKey("s"))
        {
            targetVelocity = moveSpeed * -root.transform.forward;
        }

        currentVelocity = Vector3.Lerp(
         currentVelocity,
         targetVelocity,
         1 - Mathf.Exp(-moveAcceleration * Time.deltaTime)
       );

        // Apply the velocity
        root.transform.position += currentVelocity * Time.deltaTime;
    }

    float speed = 0;

    public void orientBodyOffset()
    {
        float averageHeight = 0;

        for (int i = 0; i < leftLegs.Length; i++)
        {
            averageHeight += leftLegs[i].Y + rightLegs[i].Y;
        }

        //float averageHeight = (FRL.transform.position.y + FLL.transform.position.y + MRL.transform.position.y + MLL.transform.position.y + RRL.transform.position.y + RLL.transform.position.y);

        if (averageHeight != 0)
            averageHeight /= (leftLegs.Length + rightLegs.Length);

        float offset = averageHeight;

        float heightOffset = distanceFromGround;

        //root.position = new Vector3(root.position.x, heightOffset + offset, root.position.z);
        root.position = new Vector3(root.position.x, Mathf.SmoothDamp(root.position.y,  heightOffset + offset , ref speed, 0.2f), root.position.z);
    }

    /// <summary>
    /// Orients the root
    /// </summary>
    public void orientBodyLeftRight()
    {
        //left right rotation

        float leftLegHeight = 0;
        float rightLegHeight = 0;

        foreach (LegStepper leg in leftLegs)
        {
            leftLegHeight += leg.transform.position.y;
        }

        foreach (LegStepper leg in rightLegs)
        {
            rightLegHeight += leg.transform.position.y;
        }

        float delta = leftLegHeight - rightLegHeight;

        float rotationDegrees = delta * 2f;

        Quaternion test = Quaternion.Euler(root.transform.eulerAngles.x, root.transform.eulerAngles.y, rotationDegrees * -1); //new Vector3(root.transform.eulerAngles.x, root.transform.eulerAngles.y, rotationDegrees * -1);

        root.transform.rotation = Quaternion.Slerp(
            root.transform.rotation,
            test,
            1 - Mathf.Exp(-damper * Time.deltaTime)
        );
    }

    public void orientBodyFrontBack()
    {
        float fronttLegHeight = leftLegs[0].transform.position.y + rightLegs[0].transform.position.y;
        float backLegHeight = leftLegs[leftLegs.Length - 1].transform.position.y + rightLegs[rightLegs.Length - 1].transform.position.y;

        float delta = fronttLegHeight - backLegHeight;

        float rotationDegrees = delta * 2f;


        Quaternion test = Quaternion.Euler(rotationDegrees * -1, root.transform.eulerAngles.y, root.transform.eulerAngles.z);

        root.transform.rotation = Quaternion.Slerp(
            root.transform.rotation,
            test,
            1 - Mathf.Exp(-damper * Time.deltaTime)
        );

        // cover for body rotations

        if (root.transform.eulerAngles.x > 180)
        {
            root.transform.rotation = Quaternion.Euler(root.transform.eulerAngles.x - 360, root.transform.eulerAngles.y, root.transform.eulerAngles.z); //root.transform.eulerAngles = new Vector3(root.transform.eulerAngles.x - 360, root.transform.eulerAngles.y, root.transform.eulerAngles.z);

        }
        if (root.transform.eulerAngles.z > 180)
        {
            root.transform.rotation = Quaternion.Euler(root.transform.eulerAngles.x, root.transform.eulerAngles.y - 360, root.transform.eulerAngles.z); //root.transform.eulerAngles = new Vector3(root.transform.eulerAngles.x, root.transform.eulerAngles.y - 360, root.transform.eulerAngles.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //RootMotionUpdate();
       // Debug.Log("Linear Velocity: " + currentVelocity + "; Angular Velocity: " + currentAngularVelocity);
    }

    private void LateUpdate()
    {
        //CalculateOrientation();
        RootMotionUpdate();
    }

    private void OnDrawGizmos()
    {
        if (maxDistToTarget > Vector3.Distance(root.transform.position, target.position) && minDistToTarget < Vector3.Distance(root.transform.position, target.position))
        {
            Handles.color = Color.green;
        }
        else 
        {
            Handles.color = Color.red;
        }
 

       // Handles.DrawLine(root.transform.position, target.position);

    }
}
