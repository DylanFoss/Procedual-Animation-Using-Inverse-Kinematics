using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LegStepper : MonoBehaviour
{
    //TODO: Overshoot to be based on speed?
    //TODO: Overshoot currently can overshoot into floors/walls. Raycasts and/or velocity checks to correct this?

    [SerializeField] private Transform root;

    [SerializeField] private Transform homeTransform;

    [SerializeField] private Transform rayTransform;

    //private Vector3 rayPoint;

    [SerializeField] private float stepAtDistance;

    [SerializeField] private float moveDuration;

    [SerializeField] float stepOvershootFraction;

    [SerializeField] private int layerMask = 1 << 8;

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


    IEnumerator MoveToHome()
    {
        moving = true;

        Quaternion startRot = transform.rotation;
        Vector3 startPoint = transform.position;

        Quaternion endRot = homeTransform.rotation;

        // overshooting our end postion for more natrual movement

        Vector3 towardHome = (homeTransform.position - transform.position);

        float overshootDistance = stepAtDistance * stepOvershootFraction;
        Vector3 overshootVector = towardHome * overshootDistance;

        overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);

        //Vector3 endPoint = homeTransform.position;
        Vector3 endPoint = homeTransform.position + overshootVector;

        Vector3 centerPoint = (startPoint + endPoint) / 2;
        centerPoint += homeTransform.up * Vector3.Distance(startPoint, endPoint) / 2f;

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

    // Start is called before the first frame update
    public void OnEnable()
    {
       // epic.parent = root;
        //epic.position = homeTransform.position;
        FloorRaycast();
    }

    public void Start()
    {
        //epic.parent = root;
       // epic.position = homeTransform.position;
        FloorRaycast();
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
