using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKSolver : MonoBehaviour
{
    public int length;

    public GameObject endEffector;

    public int iterations = 100;
    public float minDistance = 0.01f;

    public GameObject[] stuff;

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
