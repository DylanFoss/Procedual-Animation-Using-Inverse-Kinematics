using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorRaycast : MonoBehaviour
{
    [SerializeField]
    private int layerMask = 1 << 3;

    private Transform lastTransform;

    // Update is called once per frame
    void LateUpdate()
    {
        lastTransform = transform;

        Ray ray = new Ray(new Vector3 (transform.position.x, transform.position.y + 5, transform.position.z), Vector3.down);
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

        transform.position = lastTransform.position;
    }
}
