using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment
{
    public int length;

    public Vector3 joint;
    public Vector3 position; // this can be implictly worked out by joints

    public Segment(int length, Vector3 joint, Vector3 position)
    {
        this.length = length;
        this.joint = joint;
        this.position = position;
    }
}
