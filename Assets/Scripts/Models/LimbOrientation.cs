using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbOrientation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    public Transform Target;
    //public float RotationSpeed; //unused for now

    //values for internal use
    private Quaternion lookRotation;
    private Vector3 direction;

    // Update is called once per frame
    void Update()
    {

        direction = (Target.position - transform.position).normalized;
        lookRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);
        transform.rotation = lookRotation; //Quaternion.Slerp(transform.rotation, _lookRotation, Time.deltaTime * RotationSpeed);
    }
}
