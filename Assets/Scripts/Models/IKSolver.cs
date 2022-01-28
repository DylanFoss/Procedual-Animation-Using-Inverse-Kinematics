using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IKSolver : MonoBehaviour
{
    public int length;

    public GameObject endEffector;
    public GameObject pole;

    public int iterations = 100;
    public float minDistance = 0.01f;

    protected Transform[] bones;
    protected float[] lengths;
    protected float cumLength; //cumulative
    protected Vector3[] points;

    public void Init()
    {
        // stuff = new GameObject[length + 1];

        if (length == null)
        {
            length = 2;
        }

        bones = new Transform[length + 1];
        points = new Vector3[length + 1];
        lengths = new float[length];

        cumLength = 0;

        //init bones
        var current = transform;
        for (int i = bones.Length - 1; i >= 0; i--)
        {
            bones[i] = current;

            if (i == bones.Length - 1)
            {
            }
            else
            {
                lengths[i] = (bones[i + 1].position - current.position).magnitude; //current.position
                cumLength += lengths[i];
            }

            current = current.parent;
        }
        
       // endEffector = null;
    }

    private void Awake()
    {
        Init();
    }

    // Start is called before the first frame update
    void Start()
    { 
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Solve();
    }

    private void OnValidate()
    {
        Init();
    }

    private void OnDrawGizmos()
    {
        var current = this.transform;
        for (int i = 0; i < length && current != null && current.parent != null; i++)
        {
            var scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
            Handles.color = Color.blue;
            Handles.DrawWireCube(Vector2.up * 0.5f, Vector3.one);
            current = current.parent;
        }

    }

    //TODO: could add check for if it's impossible to reach, then strech rather than waiting through each iteration.
    public void Solve()
    {

        if (endEffector == null) // no end effector? nothing to solve!
            return;

        if (lengths.Length != length) // reinitialise chain if the number of chains differs from the lengths of chains
        {
            Init();
        }

        for (int i = 0; i < bones.Length; i++)
            points[i] = bones[i].position;


        Vector3 origin = points[0];
        Vector3 target = endEffector.transform.position;

        // check if this is impossible to solve
        if (Vector3.Distance(target, points[0]) > cumLength)
        {
            var d = (endEffector.transform.position - points[0]).normalized;

            for (int i = 1; i < points.Length; i++)
                points[i] = points[i - 1] + d * lengths[i-1];
        }
        else
        {
            for (int i = 0; i < iterations; i++)
            {
                Forwards(target);
                Backwards(origin);

                float distanceFromTarget = (points[points.Length - 1] - target).magnitude;
                if (distanceFromTarget <= minDistance)
                {
                    break;
                }

            }

            // code for pole target
            if (pole != null)
            {
                for (int i = 1; i < points.Length - 1; i++)
                {
                    Plane plane = new Plane(points[i + 1] - points[i - 1], points[i - 1]);
                    Vector3 projectedPole = plane.ClosestPointOnPlane(pole.transform.position);
                    Vector3 projectedBone = plane.ClosestPointOnPlane(points[i]);
                    float angle = Vector3.SignedAngle(projectedBone - points[i - 1], projectedPole - points[i - 1], plane.normal);
                    points[i] = Quaternion.AngleAxis(angle, plane.normal) * (points[i] - points[i - 1]) + points[i - 1];
                }
            }
        }

        for (int i = 0; i < points.Length; i++)
        {
            bones[i].position = points[i];
        }

    }

    public void Forwards(Vector3 target)
    {
        points[points.Length - 1] = target;
        for (int i = points.Length - 2; i >= 0; i--)
        {
            Vector3 dir = (points[i] - points[i + 1]).normalized;
            points[i] = points[i + 1] + dir * lengths[i];
        }
    }

    public void Backwards(Vector3 origin)
    {
        points[0] = origin;
        for (int i = 1; i < points.Length; i++)
        {
            Vector3 dir = (points[i] - points[i - 1]).normalized;
            points[i] = points[i - 1] + dir * lengths[i - 1];
        }
    }
}
