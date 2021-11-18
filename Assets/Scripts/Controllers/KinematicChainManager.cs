using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicChainManager : MonoBehaviour
{

    KinematicChain kc;
    GameObject chain;

    public int length;

    public GameObject endEffector;

    public List<Vector3> points;
    public List<Transform> bones;
    public List<float> lengths;

    private void Init()
    {
        length = 2;

    }

    // Start is called before the first frame update
    void Start()
    {
        Init();

        createChain();

        // could be in a function
        endEffector = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        endEffector.name = "End Effector";
        endEffector.transform.position = kc.segments[kc.segments.Count-1].joint;
        var sr = endEffector.GetComponent<Renderer>();
        sr.material.SetColor("_Color", Color.blue);
    }

    // Update is called once per frame
    void Update()
    {

        Vector3[] points;
        points = kc.Solve(endEffector.transform.position);
        for (int i = 0; i < kc.segments.Count; i++)
        {
            chain.transform.GetChild(i).transform.position = points[i];
        }
    }

    public void createChain()
    {
        kc = new KinematicChain(length);
        onChainCreated();
    }

    public void onChainCreated()
    {
        GameObject go = new GameObject();
        go.transform.SetParent(transform, true);

        go.name = "KinematicChain";

        // iterator in a foreach, god...
        int it = 0;

        foreach (Segment limb in kc.segments)
        {
            GameObject j = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            j.name = it == 0 ? "Joint" + (it) + "_origin" : "Joint" + (it);
            j.transform.position = limb.joint;
            j.transform.SetParent(go.transform, true);

            var sr = j.GetComponent<Renderer>();
            sr.material.SetColor("_Color", it == 0 ? Color.green : Color.red);

            it++;
        }

        chain = go;
    }
}
