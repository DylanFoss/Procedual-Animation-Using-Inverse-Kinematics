using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorRaycast : MonoBehaviour
{
    [SerializeField]
    private int layerMask = 1 << 3;

    // Update is called once per frame
    void Update()
    {
        //transform.rotation = new Quaternion(0, 0, -90f, 0);
        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask))
        {
            Debug.DrawLine(ray.origin, hitInfo.point, Color.red);
        }
        else 
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 100, Color.green);
        }
    }
}
