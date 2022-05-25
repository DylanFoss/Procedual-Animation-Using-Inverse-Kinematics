using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [SerializeField] Transform focus = default;
    private CreatureController controller = null;

    //Distances

    [SerializeField] float maxDistance = 100f;
    [SerializeField] float minDistance = 20f;
    [SerializeField, Range(20f, 100f)] float distance;
    public float Distance { get { return distance; } set { distance = value; } }
    public float MaxDistance { get { return maxDistance; }}
    public float MinDistance { get { return minDistance; }}


    //Focus
    [SerializeField, Min(0f)] float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)]
    float focusCentering = 0.5f;

    Vector3 focusPoint;

    //Orbiting
    [SerializeField, Range(1f, 360f)]
    float rotationSpeed = 90f;

    Vector2 orbitAngles = new Vector2(45f, 0f);

    private void Awake()
    {
        focusPoint = focus.position;       
    }

    // Update is called once per frame
    void LateUpdate()
    {
        SwitchFocusPoint();
        UpdateFocusPoint();
        ManualRotation();
        ManualZoom();
        Quaternion lookRotation = Quaternion.Euler(orbitAngles);
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    void UpdateFocusPoint()
    {
        Vector3 targetPoint = focus.position;
        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1f;
            if (distance > 0.01f && focusCentering > 0f)
            {
                t = Mathf.Pow(1f - focusCentering, Time.deltaTime);
            }
            if (distance > focusRadius)
            {
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
            }
            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
        else
        {
            focusPoint = targetPoint;
        }
    }

    void ManualRotation()
    {
        Vector2 input = new Vector2(
            -Input.GetAxis("Mouse Y"),
            Input.GetAxis("Mouse X")
        );

        if (Input.GetMouseButton(0))
        {
            const float e = 0.001f;
            if (input.x < -e || input.x > e || input.y < -e || input.y > e)
            {
                orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input*3;
            }
        }
    }

    void ManualZoom()
    {
        if (Input.mouseScrollDelta != Vector2.zero)
        {
            Distance += Mathf.Lerp(Input.mouseScrollDelta.y * 0.5f, Input.mouseScrollDelta.y * 5, Distance / MaxDistance); // faster zoom when far away; 

            if  (Distance > MaxDistance)
                Distance = MaxDistance;

            if (Distance < MinDistance)
                Distance = MinDistance;
        }
    }

    //warning: this code isn't good but I needed it working last minute
    void SwitchFocusPoint()
    {
        if (Input.GetMouseButton(1))
        {
            Ray ray = transform.gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo))
            {
                Debug.Log(hitInfo.transform.gameObject);
                Debug.Log(hitInfo.transform.gameObject.GetComponentInParent<CreatureController>());

                if (hitInfo.transform.gameObject.GetComponentInParent<CreatureController>())
                {
                    if (controller != null)
                        controller.IsSelected = false;

                    controller = hitInfo.transform.gameObject.GetComponentInParent<CreatureController>();
                    focus = controller.root;
                    controller.IsSelected = true;
                }
            }
        }
    }
}
