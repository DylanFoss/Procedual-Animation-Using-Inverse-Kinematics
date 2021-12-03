using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LegStepper : MonoBehaviour
{

    [SerializeField] private Transform homeTransform;

    [SerializeField] private float stepAtDistance;

    [SerializeField] private float moveDuration;

    public bool moving;

    IEnumerator MoveToHome()
    {
        moving = true;

        Quaternion startRot = transform.rotation;
        Vector3 startPoint = transform.position;

        Quaternion endRot = homeTransform.rotation;
        Vector3 endPoint = homeTransform.position;

        float timeElapsed = 0;

        do
        {
                // Add time since last frame to the time elapsed
                timeElapsed += Time.deltaTime;

                float normalizedTime = timeElapsed / moveDuration;

                // Interpolate position and rotation
                transform.position = Vector3.Lerp(startPoint, endPoint, normalizedTime);
                transform.rotation = Quaternion.Slerp(startRot, endRot, normalizedTime);

                // Wait for one frame
                yield return null;
        }
        while (timeElapsed < moveDuration) ;

        // Done moving
        moving = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


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

    private void OnDrawGizmos()
    {
        Handles.color = Vector3.Distance(transform.position, homeTransform.position) > stepAtDistance ? Color.red : Color.green;

        Handles.DrawLine(transform.position, homeTransform.position);
        Handles.DrawWireDisc(transform.position, new Vector3(0, 1, 0), stepAtDistance);
      //  Handles.DrawWireDisc(transform.position, new Vector3(0, 0, 1), stepAtDistance);
       // Handles.DrawWireDisc(transform.position, new Vector3(1, 0, 0), stepAtDistance);

    }
}
