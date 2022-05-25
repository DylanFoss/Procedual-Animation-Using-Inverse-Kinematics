using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LegStepper : MonoBehaviour
{
    //TODO: Overshoot to be based on speed?

    [SerializeField] private CreatureController controller;

    [SerializeField] private Transform homeTransform;

    [SerializeField] private float stepAtDistance;

    [SerializeField] private float moveDuration;

    [SerializeField] float stepHeight = 0.5f;

    [SerializeField] private Transform rayTransform;

    private int layerMask = 8;

    public bool moving;

    public float X
    {
        get { return transform.position.x; }
    }

    public float Y
    {
        get { return transform.position.y; }
    }

    public float DistanceFromHome
    {
        get { return Mathf.Abs((transform.position - homeTransform.position).magnitude); }
    }

    public float StepAtDistance
    {
        get { return stepAtDistance; }
    }

    public void OnEnable()
    {
        FloorRaycast();
    }

    public void Start()
    {
        FloorRaycast();
    }


    IEnumerator MoveToHome()
    {
        moving = true;

        Quaternion startRot = transform.rotation;
        Vector3 startPoint = transform.position;

        Quaternion endRot = homeTransform.rotation;

        // overshooting our end postion for more natrual movement

        Vector3 endPoint = homeTransform.position;
        

        //this branch is very unecessary.
        if (controller.CurrentAngularVelocity > 0)
        {
            endPoint -= Vector3.Normalize(startPoint - endPoint) * (controller.CurrentAngularVelocity / controller.MaxTurnSpeed) * 2;
        }
        else
        {
            endPoint += Vector3.Normalize(startPoint - endPoint) * (controller.CurrentAngularVelocity / controller.MaxTurnSpeed) * 2;
        }

        //Linear velocity overshoot
        Vector3 unitVector = Vector3.Normalize(controller.CurrentVelocity);

        endPoint += unitVector * stepAtDistance;

        //// TODO: Generalise FloorRaycast to take a more generic value instead of this
        Ray ray = new Ray(new Vector3(endPoint.x, endPoint.y + 5, endPoint.z), Vector3.down);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask))
        {
            endPoint = hitInfo.point;
            Debug.DrawLine(ray.origin, hitInfo.point, Color.red);
        }

        ////

        Vector3 centerPoint = (startPoint + endPoint) / 2;
        centerPoint += homeTransform.up * Vector3.Distance(startPoint, endPoint) * stepHeight;

        float timeElapsed = 0;
        do
        {
                // Add time since last frame to the time elapsed
                timeElapsed += Time.deltaTime;

                float normalizedTime = timeElapsed / moveDuration;

                // Interpolate position and rotation
               // transform.position = Vector3.Lerp(startPoint, endPoint, normalizedTime);
                transform.rotation = Quaternion.Slerp(startRot, endRot, normalizedTime);

                // Quadratic bezier curve, nice and easy to set up custom steps for.
                transform.position =
                  Vector3.Lerp(
                    Vector3.Lerp(startPoint, centerPoint, normalizedTime),
                    Vector3.Lerp(centerPoint, endPoint, normalizedTime),
                    normalizedTime
                  );

                transform.rotation = Quaternion.Slerp(startRot, endRot, normalizedTime);

            // Wait for one frame
            yield return null;
        }
        while (timeElapsed < moveDuration) ;

        // Done moving
        moving = false;
    }


    // Update is called once per frame
    public void Update()
    {
        FloorRaycast();
    }

    public void TryMove()
    {
        // If we are already moving, don't start another move
        if (moving) return;

        float distFromHome = Vector3.Distance(transform.position, homeTransform.position);

        // If we are too far off in position or rotation
        if (distFromHome > stepAtDistance)
        {
            // Start the step coroutine
            StartCoroutine(MoveToHome());
        }
    }

    public void FloorRaycast()
    {
        Ray ray = new Ray(new Vector3(rayTransform.position.x, rayTransform.position.y + 5, rayTransform.position.z), Vector3.down);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask))
        {
            homeTransform.position = hitInfo.point;
            Debug.DrawLine(ray.origin, hitInfo.point, Color.red);
        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.green);
        }
    }

    private void OnDrawGizmos()
    {
        Handles.color = Vector3.Distance(transform.position, homeTransform.position) > stepAtDistance ? Color.red : Color.green;

        Handles.DrawLine(transform.position, homeTransform.position);
        Handles.DrawWireDisc(transform.position, new Vector3(0, 1, 0), stepAtDistance);
        Handles.DrawWireDisc(transform.position, new Vector3(0, 0, 1), stepAtDistance);
        Handles.DrawWireDisc(transform.position, new Vector3(1, 0, 0), stepAtDistance);

        Handles.color = Color.blue;

        Handles.DrawWireCube(rayTransform.position, new Vector3(0.5f, 0.5f, 0.5f));

    }
}
