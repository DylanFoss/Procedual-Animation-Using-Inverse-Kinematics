using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [SerializeField] float maxAngleToTarget;

    Vector3 currentVelocity;

    float currentAngularVelocity;

    [SerializeField] Transform root;

    [SerializeField] Transform target;

    //leg stepper objects

    [SerializeField] LegStepper[] leftLegs;
    [SerializeField] LegStepper[] rightLegs;

    // to be replaced by arrays above

    [SerializeField] LegStepper FRL;
    [SerializeField] LegStepper FLL;

    [SerializeField] LegStepper MRL;
    [SerializeField] LegStepper MLL;

    [SerializeField] LegStepper RRL;
    [SerializeField] LegStepper RLL;

    public AnimationCurve sensitivityCurve;


    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(LegUpdateCoroutine());
    }

    IEnumerator LegUpdateCoroutine()
    {
        while (true)
        {
            do
            {
                //FRL.TryMove();
                //MLL.TryMove();
                //RRL.TryMove();

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
            while (FRL.moving || MLL.moving || RRL.moving);

            do
            {
                //FLL.TryMove();
                //MRL.TryMove();
                //RLL.TryMove();

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
            while (FLL.moving || MRL.moving || RLL.moving);
        }
    }

    //Based on leg placement, how should the body be oriented?
    public void CalculateOrientation()
    {

        //Vector3 up = Vector3.zero;
        //float avgDistance = 0;

        //Vector3 point, a, b, c;

        //LegStepper[] legs = { FLL, MLL, RLL, RRL, MRL, FRL };

        //for (int i = 0; i < leftLegs.Length + rightLegs.Length; i++)
        //{
        //    int temp = i < 3 ? i + 3 : i - 3; 

        //    point = legs[i].transform.position;
        //    avgDistance += transform.InverseTransformPoint(point).y;
        //    a = (transform.position - point).normalized;
        //    b = ((legs[temp].transform.position) - point).normalized;
        //    c = Vector3.Cross(a, b);
        //    up += c * sensitivityCurve.Evaluate(c.magnitude) + (legs[i].transform.position.normalized == Vector3.zero ? transform.forward : legs[i].transform.position.normalized);
        //    //grounded |= legs[i].legGrounded;

        //    Debug.DrawRay(point, a, Color.red, 0);

        //    Debug.DrawRay(point, b, Color.green, 0);

        //    Debug.DrawRay(point, c, Color.blue, 0);

        //}

        //up /= legs.Length;
        //avgDistance /= legs.Length;
        //distanceFromGround = avgDistance;
        //Debug.DrawRay(transform.position, up, Color.red, 0);

        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, up), up), 22.5f * Time.deltaTime);

        //transform.Translate(0, -(-avgDistance + distanceFromGround) * 0.5f, 0, Space.Self);

    }


    void RootMotionUpdate()
    {
        // rotate root to face target

        // change root height based on leg heights

        float averageHeight = 0;

        for (int i = 0; i < leftLegs.Length; i++)
        {
            averageHeight += leftLegs[i].Y + rightLegs[i].Y;
        }

        //float averageHeight = (FRL.transform.position.y + FLL.transform.position.y + MRL.transform.position.y + MLL.transform.position.y + RRL.transform.position.y + RLL.transform.position.y);

        if (averageHeight != 0)
            averageHeight /= 6;

        float offset = averageHeight;

        float heightOffset = distanceFromGround;

        root.position = new Vector3(root.position.x, heightOffset + offset, root.position.z);

        //this code is evil and WILL release daemons into this dimension. Use at own risk.

        //body rotation

        //left right rotation

        //float leftLegHeight = 0;
        //float rightLegHeight = 0;

        //foreach (LegStepper leg in leftLegs)
        //{
        //    leftLegHeight += leg.transform.position.y;
        //}

        //foreach (LegStepper leg in rightLegs)
        //{
        //    rightLegHeight += leg.transform.position.y;
        //}

        //float delta = leftLegHeight - rightLegHeight;

        //float rotationDegrees = delta * 2f;

        //root.transform.rotation = Quaternion.Euler(root.transform.eulerAngles.x, root.transform.eulerAngles.y, rotationDegrees * -1); //new Vector3(root.transform.eulerAngles.x, root.transform.eulerAngles.y, rotationDegrees * -1);

        ////front back rotation


        //float fronttLegHeight = leftLegs[0].transform.position.y + rightLegs[0].transform.position.y;
        //float backLegHeight = leftLegs[leftLegs.Length - 1].transform.position.y + rightLegs[rightLegs.Length - 1].transform.position.y;

        //delta = fronttLegHeight - backLegHeight;

        //rotationDegrees = delta;


        //root.transform.eulerAngles = new Vector3(rotationDegrees * -1, root.transform.eulerAngles.y, root.transform.eulerAngles.z);

        //// cover for body rotations

        //if (root.transform.eulerAngles.x > 180)
        //{
        //    root.transform.rotation = Quaternion.Euler(root.transform.eulerAngles.x - 360, root.transform.eulerAngles.y, root.transform.eulerAngles.z); //root.transform.eulerAngles = new Vector3(root.transform.eulerAngles.x - 360, root.transform.eulerAngles.y, root.transform.eulerAngles.z);

        //}
        //if (root.transform.eulerAngles.z > 180)
        //{
        //    root.transform.rotation = Quaternion.Euler(root.transform.eulerAngles.x, root.transform.eulerAngles.y - 360, root.transform.eulerAngles.z); //root.transform.eulerAngles = new Vector3(root.transform.eulerAngles.x, root.transform.eulerAngles.y - 360, root.transform.eulerAngles.z);
        //}

    }

    // Update is called once per frame
    void Update()
    {
        RootMotionUpdate();
    }
}
