using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreatureController : MonoBehaviour
{

    [SerializeField] public Transform root;

    // root body variables
    [SerializeField] float distanceFromGround;

    [SerializeField] float maxMoveSpeed;
    [SerializeField] float moveAcceleration;

    [SerializeField] float maxTurnSpeed;
    [SerializeField] float turnAcceleration;

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
    public float MaxTurnSpeed
    {
        get { return maxTurnSpeed; }
    }


    [SerializeField] float damper;

    //leg stepper objects

    [SerializeField] LegStepper[] leftLegs;
    [SerializeField] LegStepper[] rightLegs;

    private bool isSelected = false;
    public bool IsSelected { get { return isSelected; } set { isSelected = value; } }

    CreatureController()
    {
        currentVelocity = new Vector3(0, 0, 0);
        currentAngularVelocity = 0;
    }

    void Awake()
    {
        StartCoroutine(LegUpdateCoroutine());
    }

    void OnEnable()
    {
        StartCoroutine(LegUpdateCoroutine());
    }

    void OnDisable()
    {
        StopCoroutine(LegUpdateCoroutine());
    }

    private void LateUpdate()
    {
        RootMotionUpdate();
    }

    void RootMotionUpdate()
    {
        move();

        orientBodyOffset();
        orientBodyLeftRight();
        orientBodyFrontBack();
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

    // TODO: clean up WASD movemnt to own function.
    public void move()
    {
        float targetAngularVelocity = 0;

        // messy check
        if (isSelected)
            targetAngularVelocity = Mathf.Lerp(maxTurnSpeed * Input.GetAxisRaw("Horizontal"), maxTurnSpeed * Input.GetAxisRaw("Horizontal") * 0.6f, Vector3.Magnitude(currentVelocity));



        currentAngularVelocity = Mathf.Lerp(
        currentAngularVelocity,
        targetAngularVelocity,
        1 - Mathf.Exp(-turnAcceleration * Time.deltaTime));

        // Rotate the transform around the Y axis in world space, 
        root.transform.Rotate(0, Time.deltaTime * currentAngularVelocity, 0, Space.World);


        Vector3 targetVelocity = Vector3.zero;


        // messy check
        if (isSelected)
            targetVelocity = maxMoveSpeed * root.transform.forward * Input.GetAxisRaw("Vertical");

        currentVelocity = Vector3.Lerp(
         currentVelocity,
         targetVelocity,
         1 - Mathf.Exp(-moveAcceleration * Time.deltaTime)
       );

        // Apply the velocity
        root.transform.position += currentVelocity * Time.deltaTime;
    }


    /// <summary>
    /// Iterates over the height (relative to the world Y cordinate) of each leg, and moves the creatures root up.
    /// </summary>
    public void orientBodyOffset()
    {
        float averageHeight = 0;

        for (int i = 0; i < leftLegs.Length; i++)
        {
            averageHeight += leftLegs[i].Y + rightLegs[i].Y;
        }

        if (averageHeight != 0)
            averageHeight /= (leftLegs.Length + rightLegs.Length);

        float offset = averageHeight;
        float heightOffset = distanceFromGround;


        //root.position = new Vector3(root.position.x, heightOffset + offset, root.position.z);
        root.position = new Vector3(root.position.x, Mathf.SmoothDamp(root.position.y,  heightOffset + offset , ref currentVelocity.y, 0.2f), root.position.z);
    }

    /// <summary>
    /// Rotate the creatures root left and right based on 
    /// </summary>
    public void orientBodyLeftRight()
    {

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

        Quaternion test = Quaternion.Euler(root.transform.eulerAngles.x, root.transform.eulerAngles.y, rotationDegrees * -1);

        // lerp the rotation based on damper value
        root.transform.rotation = Quaternion.Slerp(
            root.transform.rotation,
            test,
            1 - Mathf.Exp(-damper * Time.deltaTime)
        );
    }

    /// <summary>
    /// Rotate the creatures root forwards and back based on front legs length and back legs length.
    /// </summary>
    public void orientBodyFrontBack()
    {
        if (leftLegs.Length == 1)
            return;

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

        
        // Quarternion.Slerp takes the most direct rotation to the target. These guards prevent the body flipping upside down.
        if (root.transform.eulerAngles.x > 180)
        {
            root.transform.rotation = Quaternion.Euler(root.transform.eulerAngles.x - 360, root.transform.eulerAngles.y, root.transform.eulerAngles.z); 

        }

        if (root.transform.eulerAngles.z > 180)
        {
            root.transform.rotation = Quaternion.Euler(root.transform.eulerAngles.x, root.transform.eulerAngles.y - 360, root.transform.eulerAngles.z);
        }
    }

}
