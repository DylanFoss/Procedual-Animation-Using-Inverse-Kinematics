using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicChain
{

    public int length;
    public List<Segment> segments;

    public List<Vector3> points;
    public List<Transform> bones;
    public List<float> lengths;

    public KinematicChain(int length)
    {

        segments = new List<Segment>();
        this.length = length;

        for (int i = 0; i < length; i++)
        {
            addSegment();
        }

        segments.Add(
                new Segment(0,
                new Vector3(0, 0 + segments.Count * segments[segments.Count - 1].length, 0),
                new Vector3(0, 0.5f + segments[segments.Count - 1].length, 0)));
    }

    public void addSegment()
    {
        if (segments.Count == 0)
        {
            segments.Add(new Segment(3, new Vector3(0, 0, 0), new Vector3(0,0.5f,0)));
        }
        else 
        {
            segments.Add(
                new Segment(3, 
                new Vector3(0, 0 + segments.Count * segments[segments.Count - 1].length, 0), 
                new Vector3(0, 0.5f + segments[segments.Count - 1].length, 0)));
        }
    }

    public int CumLength(int numLimbs)
    {
        int count = 0;

        for (int i = 0; i < numLimbs; i++)
        {
            count += segments[i].length;
        }

        return count;
    }

    const int maxIterations = 100;
    const float minDistance = 0.01f;
    public Vector3[] Solve(Vector3 target) 
    {

        Vector3[] points = new Vector3[segments.Count];
        for (int i = 0; i < segments.Count; i++)
        {
            points[i] = segments[i].joint;
        }


        //TODO: could use pre saved lengths instead of working them out at runtime (should be minor overhead)
        float[] lengths = new float[segments.Count-1];
        for (int i = 0; i < segments.Count-1; i++)
        {
            lengths[i] = (points[i + 1] - points[i]).magnitude;
        }


        Vector3 origin = points[0];

        for (int i = 0; i < maxIterations; i++)
        {
            points = Forwards(points, lengths, target);
            points = Backwards(points, lengths, origin);

            float distanceFromTarget = (points[points.Length - 1] - target).magnitude;
            if (distanceFromTarget <= minDistance)
            {
                return points;
            }

        }

        return points;
    }

    public Vector3[] Forwards(Vector3[] points, float[] lengths , Vector3 target)
    {
        points[points.Length - 1] = target;
        for (int j = points.Length - 2; j >= 0; j--)
        {
            Vector3 dir = (points[j] - points[j + 1]).normalized;
            points[j] = points[j + 1] + dir * lengths[j];
        }
        return points;
    }

    public Vector3[] Backwards(Vector3[] points, float[] lengths, Vector3 origin)
    {
        points[0] = origin;
        for (int j = 1; j < points.Length; j++)
        {
            Vector3 dir = (points[j] - points[j - 1]).normalized;
            points[j] = points[j - 1] + dir * lengths[j - 1];
        }
        return points;
    }
}
