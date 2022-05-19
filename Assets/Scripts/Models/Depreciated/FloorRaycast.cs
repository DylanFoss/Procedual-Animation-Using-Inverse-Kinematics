using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FloorRaycast : MonoBehaviour
{
    [SerializeField]
    private int layerMask = 1 << 3;

    private Transform initialTransform;
    private Vector3 rest;

    private void Start()
    {
        initialTransform = transform;


        Ray ray = new Ray(new Vector3(transform.position.x, transform.position.y + 5, transform.position.z), Vector3.down);
        RaycastHit hitInfo;
        Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask);
        rest = hitInfo.point;
        transform.position = rest;

    }

    // Update is called once per frame
    void LateUpdate()
    {


        Ray ray = new Ray(new Vector3 (initialTransform.position.x, initialTransform.position.y + 5, initialTransform.position.z), Vector3.down);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask))
        {
            transform.position = hitInfo.point;
            Debug.DrawLine(ray.origin, hitInfo.point, Color.red);
        }
        else 
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.green);
        }

        //transform.position = initialTransform.position;
    }

    private void OnDrawGizmos()
    {
    }
}
