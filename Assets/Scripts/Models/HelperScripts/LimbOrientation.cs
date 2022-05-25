using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbOrientation : MonoBehaviour
{

    public Transform Target;

    //values for internal use
    private Quaternion lookRotation;
    private Vector3 direction;

    // Update is called once per frame
    void Update()
    {

        direction = (Target.position - transform.position).normalized;
        lookRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);
        transform.rotation = lookRotation;
    }
}
